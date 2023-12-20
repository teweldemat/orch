using funcscript;
using funcscript.core;
using funcscript.error;
using funcscript.model;
using funcscript.sql;
using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace fsstudio
{
    public partial class MainWindow : Form, IFsDataProvider
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private extern static IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);
        public class WorkState
        {
            public String OpenFile = null;
            public Point WindowLocation = new Point(-1, -1);
            public Size WindowSize = new Size(-1, -1);
            public int SplitMainPosition = -1;
            public int SplitRightPosition = -1;
            public int SplitTextPosition = -1;
            public FormWindowState WindowState = FormWindowState.Normal;
            public List<Tuple<String, String>> Users = new List<Tuple<string, string>>();
        }

        const string DATA_FOLDER = "data";
        const string STATE_FILE = "state.json";
        const string FILES_FOLDER = "files";
        private const string CODE_FONT_NAME = "Courier New";
        private const float CODE_FONT_SIZE = 12.0f;
        private const int EXP_NODE_PREVIEW = 25;
        String fileName = null;
        ExpressionSystem exp;
        VariableItem selected = null;
        DefaultFsDataProvider globalProvider;


        void initScintilla()
        {
            textExpression.Language = FastColoredTextBoxNS.Language.JS;

        }
        public MainWindow()
        {
            InitializeComponent();
            initScintilla();

            var imgList = new ImageList();
            imgList.Images.Add(Properties.Resources.standard);
            imgList.Images.Add(Properties.Resources.clear_text);
            imgList.Images.Add(Properties.Resources.collection);
            imgList.Images.Add(Properties.Resources.template);
            variableTree.ImageList = imgList;

            textExpression.TextChanged += TextExpression_TextChanged;
            textExpression.SelectionChanged += TextExpression_SelectionChanged;
            textExpression.KeyDown += TextExpression_KeyDown;
            listFiles.ItemSelectionChanged += ListFiles_ItemSelectionChanged;

            textName.Leave += TextName_Leave;
            textName.Enter += TextName_Enter;
            LoadSavedState();

            Func<string, string> exprFunc = (name) =>
            {
                var current = exp.Variables;
                var names = name.Split('.');
                VariableItem varItem = null;
                for (int i = 0; i < names.Length; i++)
                {
                    varItem = current.FirstOrDefault(x => String.Compare(x.Name, names[i], true) == 0);
                    if (varItem == null)
                        break;
                    if (i < name.Length - 1)
                    {
                        current = varItem.ChildItems;
                    }
                }
                if (varItem == null)
                    return null;
                return varItem.Expression;
            };

            DefaultFsDataProvider.LoadFromAssembly(Assembly.GetAssembly(typeof(FuncScriptSql)));


            globalProvider = new DefaultFsDataProvider(
                    ((KeyValueCollection)new ObjectKvc(new
                    {
                        pi = Math.PI,
                        gpt = new GptFunction(),
                        expr = exprFunc,
                        orch = new OrchFunction(this),
                    })).GetAll()
                    );

        }

        private void TextName_Enter(object sender, EventArgs e)
        {
            buttonAddSub.Enabled = false;
        }

        private void TextName_Leave(object sender, EventArgs e)
        {
            buttonAddSub.Enabled = true;
        }

        class VariableKVC : funcscript.model.KeyValueCollection, funcscript.core.IFsDataProvider
        {
            VariableItem _item;
            IFsDataProvider _parent;
            HashSet<String> callStack = new HashSet<string>();
            public VariableKVC(VariableItem item, IFsDataProvider parent)
            {
                _item = item;
                _parent = parent;
            }
            public override bool ContainsKey(string key)
            {
                return _item.ChildItems.Any(x => x.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
            }

            public override object Get(string key)
            {
                var ch = _item.ChildItems.FirstOrDefault(x => x.Name.Equals(key, StringComparison.CurrentCultureIgnoreCase));
                if (ch == null)
                    return null;
                if (ch.ChildItems.Count == 0)
                    return ch.Evaluate(this);
                return new VariableKVC(ch, this);
            }

            public override IList<KeyValuePair<string, object>> GetAll()
            {
                return _item.ChildItems.Select(ch =>
                {
                    var val = ch.ChildItems.Count == 0 ? FuncScript.Evaluate(_parent, ch.Expression) : new VariableKVC(ch, _parent);
                    return KeyValuePair.Create(ch.Name, val);
                }).ToList();
            }

            public object GetData(string name)
            {
                var ch = _item.ChildItems.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                if (ch == null)
                    return _parent.GetData(name);

                if (callStack.Contains(name))
                    throw new InvalidOperationException($"Circular reference {name}");
                callStack.Add(name);

                object ret;
                if (ch.ChildItems.Count == 0)
                    ret = FuncScript.Evaluate(this, ch.Expression);
                else
                    ret = new VariableKVC(ch, _parent);

                callStack.Remove(name);

                return ret;

            }
        }
        private void TextExpression_KeyDown(object sender, KeyEventArgs e)
        {
        }
        private void ListFiles_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var hasSelection = listFiles.SelectedItems.Count > 0;
            buttonDeleteFile.Enabled = hasSelection;
            if (!hasSelection)
                return;
            SaveCurrent();
            loadFile((string)listFiles.SelectedItems[0].Tag);
        }



        void SelectItem(VariableItem item)
        {
            selected = item;
            textName.Enabled = item != null;
            textName.Text = (item?.Name) ?? "";
            textExpression.Text = (item?.Expression) ?? "";
            comboType.Text = item == null ? VariableType.Standard.ToString() : item.Type.ToString();
        }



        TreeNode createNode(TreeNodeCollection parent, VariableItem item)
        {
            TreeNode node = new TreeNode(item.Name);
            if (item != null && item.ChildItems.Count > 0)
                node.ImageIndex = node.SelectedImageIndex = 2;
            else
                switch (item == null ? VariableType.Standard : item.Type)
                {
                    case VariableType.Standard:
                        node.ImageIndex = node.SelectedImageIndex = 0;
                        break;
                    case VariableType.ClearText:
                        node.ImageIndex = node.SelectedImageIndex = 1;
                        break;
                    default:
                        break;
                }

            node.Tag = item;
            parent.Add(node);
            if (item != null)
            {
                foreach (var ch in item.ChildItems)
                {
                    createNode(node.Nodes, ch);
                }
                if (item.Expanded)
                    node.Expand();
            }
            return node;
        }
        void LoadToForm()
        {
            variableTree.Nodes.Clear();
            foreach (var e in exp.Variables)
            {
                createNode(variableTree.Nodes, e);
            }
            this.selected = null;
            textExpression.Text = "";
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveCurrent();
            var state = new WorkState
            {
                OpenFile = fileName,
                WindowSize = this.ClientSize,
                SplitMainPosition = this.splitMain.SplitterDistance,
                SplitTextPosition = this.splitText.SplitterDistance,
                SplitRightPosition = this.splitRight.SplitterDistance,
                WindowLocation = this.Location,
                WindowState = this.WindowState
            };

            System.IO.File.WriteAllText($"{DATA_FOLDER}\\{STATE_FILE}", Newtonsoft.Json.JsonConvert.SerializeObject(state));
        }
        void LoadSavedState()
        {
            var stateFile = $"{DATA_FOLDER}\\{STATE_FILE}";
            if (!System.IO.Directory.Exists(DATA_FOLDER))
            {
                System.IO.Directory.CreateDirectory(DATA_FOLDER);
                System.IO.Directory.CreateDirectory($"{DATA_FOLDER}\\{FILES_FOLDER}");
            }
            foreach (var f in System.IO.Directory.GetFiles($"{DATA_FOLDER}\\{FILES_FOLDER}").OrderBy(x => x))
            {
                var fi = new System.IO.FileInfo(f);
                var li = new ListViewItem(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
                li.Tag = f;
                listFiles.Items.Add(li);
            }
            WorkState state;
            if (!System.IO.File.Exists(stateFile))
            {
                state = new WorkState();
                System.IO.File.WriteAllText(stateFile, Newtonsoft.Json.JsonConvert.SerializeObject(state));
            }
            else
            {
                try
                {
                    state = Newtonsoft.Json.JsonConvert.DeserializeObject<WorkState>(System.IO.File.ReadAllText(stateFile));
                    if (state.WindowSize.Width != -1)
                    {
                        this.ClientSize = state.WindowSize;
                        if (state.SplitMainPosition != -1)
                            this.splitMain.SplitterDistance = state.SplitMainPosition;
                        if (state.SplitRightPosition != -1)
                            this.splitRight.SplitterDistance = state.SplitRightPosition;
                        if (state.SplitTextPosition != -1)
                            this.splitText.SplitterDistance = state.SplitTextPosition;
                        if (state.WindowLocation.X != -1)
                            this.Location = state.WindowLocation;
                        this.WindowState = state.WindowState;
                    }
                }
                catch
                {
                    state = new WorkState();
                }
            }
            if (state.OpenFile != null)
            {
                foreach (ListViewItem item in this.listFiles.Items)
                {
                    if ((string)item.Tag == state.OpenFile)
                    {
                        item.Selected = true;
                        break;
                    }
                }
            }

        }
        void loadFile(string file)
        {
            if (file == fileName)
                return;
            if (file == null)
                fileName = null;
            else
            {
                try
                {
                    if (!System.IO.File.Exists(file))
                    {
                        fileName = null;
                        return;
                    }
                    fileName = file;

                    exp = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpressionSystem>(
                        System.IO.File.ReadAllText(file));
                    populateExp();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading saved expression.{ex.ToString()}");
                }
            }
        }
        void populateExp()
        {
            if (exp == null)
                exp = new ExpressionSystem();
            LoadToForm();
        }
        private void TextExpression_SelectionChanged(object sender, EventArgs e)
        {
            if (ignoreSelectionChange)
                return;
        }

        bool ignoreExpTextChanged = false;

        private void TextExpression_TextChanged(object sender, EventArgs e)
        {
            if (ignoreExpTextChanged)
                return;
            if (selected != null)
            {
                selected.Expression = textExpression.Text;
            }
        }

        void ProcessEvaluationResult(object res)
        {
            FinishEvaluating();
            var sb = new StringBuilder();
            FuncScript.Format(sb, res, null, false, false);
            var text = sb.ToString();
            labelValue.Text = text;
            if (tabResult.SelectedTab == tabHtmlResult)
            {
                webView.NavigateToString(text);
                htmlTabUpdate = true;
            }
            else
                htmlTabUpdate = false;

        }
        BusyBox busyBox = null;
        bool evaluating = false;
        void ProcessEvaluationError(Exception ex)
        {
            FinishEvaluating();
            String msg = null;
            while (ex != null)
            {
                String thisMessage = null;
                if (ex is SyntaxError)
                {
                    thisMessage = ShowSyntaxError((SyntaxError)ex);
                }
                else if (ex is EvaluationException)
                {
                    thisMessage = ShowEvaluationError((EvaluationException)ex);
                }

                else
                    thisMessage = ex.Message;
                msg = msg == null ? thisMessage : $"{msg}\n{thisMessage}";
                ex = ex.InnerException;
            }
            labelValue.Text = msg;

        }

        void BeginEvaluating()
        {
            this.evaluating = true;
            this.buttonSync.Enabled = false;
        }
        void FinishEvaluating()
        {
            this.evaluating = false;
            this.buttonSync.Enabled = true;
            if (busyBox != null)
            {
                busyBox.Close();
                busyBox = null;
            }
        }
        void SyncParse()
        {
            ignoreExpTextChanged = true;
            try
            {
                var f = textExpression.Text;
                VariableType varType = VariableType.Standard;
                if (selected != null)
                {
                    selected.Expression = f;
                    varType = selected.Type;
                }
                else
                    return;

                if (varType == VariableType.Standard)
                {
                    var serror = new List<FuncScriptParser.SyntaxErrorData>();
                    var parsed = FuncScriptParser.Parse(this, f, out var node, serror);
                }
                else if (varType == VariableType.TextTemplate)
                {
                    var serror = new List<FuncScriptParser.SyntaxErrorData>();
                    var parsed = FuncScriptParser.ParseFsTemplate(this, f, out var node, serror);
                }
                labelValue.Text = "";
                callStack.Clear();
                object res = null;
                var t = new Thread(() =>
                {
                    try
                    {
                        var provider = GetProvider(this, this.exp.Variables, this.selected);
                        res = varType switch
                        {
                            VariableType.Standard => this.selected.Evaluate(provider),
                            VariableType.TextTemplate => this.selected.Evaluate(provider),
                            VariableType.ClearText => f,
                            _ => null
                        };

                        this.Invoke(ProcessEvaluationResult, res);
                    }
                    catch (Exception ex)
                    {
                        this.Invoke(ProcessEvaluationError, ex);
                    }
                });
                this.BeginEvaluating();
                evaluating = true;
                t.Start();
                new System.Threading.Timer((obj) =>
                {
                    this.Invoke(() =>
                    {
                        if (this.evaluating)
                        {
                            busyBox = new BusyBox();
                            busyBox.ShowDialog(this);
                        }
                    });
                }, null, 1000, Timeout.Infinite);

            }
            catch (Exception ex)
            {
                ProcessEvaluationError(ex);

            }
            finally
            {
                ignoreExpTextChanged = false;
                textExpression.Invalidate();
            }
        }




        private String ShowEvaluationError(EvaluationException eval)
        {
            string msg = eval.Message;
            var si = textExpression.SelectionStart;
            var sl = textExpression.SelectionLength;
            try
            {
                textExpression.SelectionStart = eval.Pos;
                textExpression.SelectionLength = eval.Len;
                textExpression.SelectionColor = Color.Red;
            }
            finally
            {
                textExpression.SelectionStart = si;
                textExpression.SelectionLength = sl;
            }
            return msg;
        }
        private string ShowSyntaxError(SyntaxError eval)
        {
            string msg = eval.Message;
            var si = textExpression.SelectionStart;
            var sl = textExpression.SelectionLength;
            try
            {
                foreach (var d in eval.data)
                {
                    msg += "\n" + d.Message;
                    if (d.Loc < textExpression.TextLength)
                    {
                        textExpression.SelectionStart = d.Loc;
                        textExpression.SelectionLength = 1;
                        textExpression.SelectionColor = Color.Red;
                    }
                }
                return msg;
            }
            finally
            {
                textExpression.SelectionStart = si;
                textExpression.SelectionLength = sl;
            }
        }

        bool ignoreSelectionChange = false;

        private int BuildRtf(int lastIndex, FuncScriptParser.ParseNode node, StringBuilder rtf, string exp)
        {
            if (node.Childs != null && node.Childs.Count > 0)
            {
                foreach (var child in node.Childs)
                {
                    lastIndex = BuildRtf(lastIndex, child, rtf, exp);
                }
                return lastIndex;
            }
            else
            {
                if (node.Pos > lastIndex)
                {
                    AppendEscapedText(exp.Substring(lastIndex, node.Pos - lastIndex), rtf, @"\cf1 ");
                }

                string color = ColorForNodeType(node.NodeType);
                AppendEscapedText(exp.Substring(node.Pos, node.Length), rtf, color);

                return node.Pos + node.Length;
            }
        }

        private void AppendEscapedText(string text, StringBuilder rtf, string color)
        {
            rtf.Append(color);
            text = text.Replace(@"\", @"\\");
            text = text.Replace("{", @"\{");
            text = text.Replace("}", @"\}");
            text = text.Replace("\n", @"\par" + "\n");
            rtf.Append(text);
            rtf.Append(@"\cf1 ");  // Reset to default color (Black assumed)
        }

        private string ColorForNodeType(FuncScriptParser.ParseNodeType nodeType)
        {
            switch (nodeType)
            {
                case FuncScriptParser.ParseNodeType.Key:
                    return @"\cf2 ";  // Assuming 2 is DarkGreen in your color table
                case FuncScriptParser.ParseNodeType.Identifier:
                    return @"\cf3 ";  // Assuming 3 is DarkCyan in your color table
                case FuncScriptParser.ParseNodeType.KeyWord:
                    return @"\cf4 ";  // Assuming 4 is Blue in your color table
                case FuncScriptParser.ParseNodeType.LiteralInteger:
                    return @"\cf5 ";  // Assuming 5 is Gray in your color table
                case FuncScriptParser.ParseNodeType.LiteralDouble:
                    return @"\cf6 ";  // Assuming 6 is Brown in your color table
                case FuncScriptParser.ParseNodeType.LiteralString:
                    return @"\cf7 ";  // Assuming 7 is RosyBrown in your color table
                default:
                    return @"\cf1 ";  // Assuming 1 is Black in your color table
            }
        }





        private void buttonAdd_Click(object sender, EventArgs e)
        {
            String name = "no_name";
            var n = 1;

            while (this.exp.Variables.FirstOrDefault(x => string.Compare(x.Name, name, true) == 0) != null)
                name = $"{name}_{n++}";

            var item = new VariableItem
            {
                Name = name,
                Expression = "",
            };
            exp.Variables.Add(item);
            var node = createNode(variableTree.Nodes, item);
            variableTree.SelectedNode = node;
            textName.SelectAll();
            textName.Focus();
        }

        private void textName_Click(object sender, EventArgs e)
        {

        }

        private void UpdateExpressionName()
        {
            if (selected == null)
                return;

            var lower = selected.Name.ToLower();

            // Validate JavaScript variable naming conventions
            var jsVariableNamePattern = @"^[a-zA-Z_$][0-9a-zA-Z_$]*$";
            if (!Regex.IsMatch(textName.Text, jsVariableNamePattern))
            {
                MessageBox.Show("Invalid FuncScript variable name. Variable names must start with a letter, underscore (_), or dollar sign ($). Subsequent characters can be letters, digits, underscores, or dollar signs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            selected.Name = textName.Text;
            if (variableTree.SelectedNode != null)
            {
                variableTree.SelectedNode.Text = selected.Name;
            }
        }


        private void textError_TextChanged(object sender, EventArgs e)
        {

        }

        HashSet<String> callStack = new HashSet<string>();
        public object GetData(string name)
        {
            if (exp == null)
                return null;
            var ch = exp.Variables.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if (ch == null)
                return globalProvider.GetData(name);
            if (callStack.Contains(name))
                throw new InvalidOperationException($"Circular reference {name}");
            callStack.Add(name);

            object ret;
            if (ch.ChildItems.Count == 0)
                ret = ch.Evaluate(this);
            else
                ret = new VariableKVC(ch, this);

            callStack.Remove(name);
            return ret;

        }
        void SaveCurrent()
        {
            if (fileName == null)
                return;
            System.IO.File.WriteAllText(fileName, Newtonsoft.Json.JsonConvert.SerializeObject(exp));
        }
        private void buttonAddFile_Click(object sender, EventArgs e)
        {
            var d = new FileName();
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                SaveCurrent();
                this.fileName = $"{DATA_FOLDER}\\{FILES_FOLDER}\\{d.InputText}.json";
                exp = new ExpressionSystem();
                var li = new ListViewItem(d.InputText);
                li.Tag = this.fileName;
                listFiles.Items.Add(li);
                populateExp();
            }
        }

        private void listFiles_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void listFiles_DoubleClick(object sender, EventArgs e)
        {
            if (listFiles.SelectedItems.Count == 0)
                return;
            var li = listFiles.SelectedItems[0];
            var fi = new System.IO.FileInfo((string)li.Tag);

            var d = new FileName(fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length));
            if (d.ShowDialog(this) == DialogResult.OK)
            {
                var fn = $"{DATA_FOLDER}\\{FILES_FOLDER}\\{d.InputText}.json";
                System.IO.File.Move(fi.FullName, fn);
                li.Text = d.InputText;
                li.Tag = fn;
                this.fileName = fn;
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this item?", "Delete Item", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            var hasSelection = listFiles.SelectedItems.Count > 0;
            if (!hasSelection)
                return;
            System.IO.File.Delete(fileName);
            fileName = null;
            exp = null;
            listFiles.SelectedItems[0].Remove();

        }
        public void DumpException(Exception ex)
        {
            if (ex is AggregateException)
            {
                foreach (var inner in ((AggregateException)ex).InnerExceptions)
                    DumpException(inner);
            }
            else
            {
                var prefix = "";
                this.labelValue.Text = "";
                while (ex != null)
                {
                    this.labelValue.Text += $"{prefix}{ex.ToString()}\n";
                    this.labelValue.Text += $"{ex.StackTrace}\n";
                    ex = ex.InnerException;
                    prefix = ">" + prefix;
                }
            }
        }
        private void buttonAPI_Click(object sender, EventArgs e)
        {
            Login();
        }
        void Login()
        {
            var d = new CallAPI("root", "pass", Guid.Empty);
            d.ShowDialog(this);
            if (d.Error != null)
            {
                DumpException(d.Error);
            }
            else
            {
                this.labelValue.Text = d.Result;
                if (tabResult.SelectedTab == tabHtmlResult)
                {
                    webView.NavigateToString(d.Result);
                    htmlTabUpdate = true;
                }
                else
                    htmlTabUpdate = false;

            }
        }

        private void buttonSync_Click(object sender, EventArgs e)
        {
            SyncParse();
        }

        FuncScriptParser.ParseNode SelectedParseNode = null;

        private void contextTree_Opening(object sender, CancelEventArgs e)
        {
            itemCopyNode.Enabled = SelectedParseNode != null;
            evaluateToolStripMenuItem.Enabled = SelectedParseNode != null;
            evaluateWithAPIToolStripMenuItem.Enabled = SelectedParseNode != null;
        }

        private void itemCopyNode_Click(object sender, EventArgs e)
        {
            if (SelectedParseNode != null)
            {

                //Clipboard.SetText(_onTree.Substring(SelectedParseNode.Pos, SelectedParseNode.Length));
            }
        }

        private void textName_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                UpdateExpressionName();
            }
        }

        private void textName_Leave(object sender, EventArgs e)
        {
            UpdateExpressionName();

        }

        private void evaluateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedParseNode != null)
            {
                //Evaluate(_onTree.Substring(SelectedParseNode.Pos, SelectedParseNode.Length));
            }
        }

        private void evaluateWithAPIToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectedParseNode != null)
            {
                //EvaluateWithAPI(_onTree.Substring(SelectedParseNode.Pos, SelectedParseNode.Length));
            }
        }

        private async void MainWindow_Load(object sender, EventArgs e)
        {
            await webView.EnsureCoreWebView2Async();
        }

        bool htmlTabUpdate = false;
        private void tabResult_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabResult.SelectedTab == tabHtmlResult && !htmlTabUpdate)
            {
                webView.NavigateToString(labelValue.Text);
                htmlTabUpdate = true;
            }
        }

        private void buttonShowTree_Click(object sender, EventArgs e)
        {

            var list = new List<FuncScriptParser.SyntaxErrorData>();
            var exp = textExpression.Text;
            funcscript.core.FuncScriptParser.Parse(globalProvider, exp, out var node, list);
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(labelValue.Text);
                MessageBox.Show("Coppied", "FuncScript");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "FuncScript");
            }
        }

        private void variableTree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SaveCurrent();
            var tag = variableTree.SelectedNode.Tag as VariableItem;
            SelectItem(tag);
        }
        VariableItem FindFirstVariable(VariableItem parent, IList<VariableItem> children, string name)
        {
            foreach (var v in children)
            {
                if (v.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                    return v;
                else
                {
                    var par = FindFirstVariable(v, v.ChildItems, name);
                    if (par != null) return par;
                }
            }
            return null;
        }
        VariableItem GetParentVariable(VariableItem parent, IList<VariableItem> children, VariableItem variable)
        {
            foreach (var v in children)
            {
                if (v == variable)
                    return parent;
                else
                {
                    var par = GetParentVariable(v, v.ChildItems, variable);
                    if (par != null) return par;
                }
            }
            return null;
        }

        IFsDataProvider GetProvider(IFsDataProvider parent, IList<VariableItem> children, VariableItem variable)
        {
            foreach (var v in children)
            {
                if (v == variable)
                    return parent;
                if (v.ChildItems.Count > 0)
                {
                    var par = GetProvider(new VariableKVC(v, parent), v.ChildItems, variable);
                    if (par != null) return par;
                }
            }
            return null;
        }
        private void variableTree_DragDrop(object sender, DragEventArgs e)
        {
            // Retrieve the dragged TreeNode
            TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));

            // Retrieve the target TreeNode based on the drop coordinates
            Point targetPoint = variableTree.PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = variableTree.GetNodeAt(targetPoint);
            if (draggedNode == targetNode)
                return;
            // Move the dragged TreeNode to the target location
            if (targetNode != null && targetNode.Tag != null)
            {
                var targetExp = targetNode.Tag as VariableItem;
                if (draggedNode.Tag != null)
                {
                    var draggedItem = draggedNode.Tag as VariableItem;
                    var parent = GetParentVariable(null, this.exp.Variables, draggedItem);
                    if (targetExp == null)
                    {
                        exp.Variables.Add(draggedItem);
                    }
                    else
                        targetExp.ChildItems.Add(draggedItem);
                    if (parent == null)
                        exp.Variables.Remove(draggedItem);
                    else
                        parent.ChildItems.Remove(draggedItem);

                    draggedNode.Remove();
                    // Add the dragged TreeNode as a child of the target TreeNode
                    targetNode.Nodes.Add(draggedNode);
                    variableTree.SelectedNode = draggedNode;
                    // Remove the dragged TreeNode from its original location

                }

            }
        }

        private void variableTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

        private void variableTree_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Move;
            }
        }

        private void buttonAddSub_Click(object sender, EventArgs e)
        {
            if (this.selected == null)
                return;

            String name = "no_name";
            var n = 1;
            while (this.selected.ChildItems.FirstOrDefault(x => string.Compare(x.Name, name, true) == 0) != null)
                name = $"{name}_{n++}";

            var item = new VariableItem
            {
                Name = name,
                Expression = "",
            };
            this.selected.ChildItems.Add(item);
            variableTree.SelectedNode.ImageIndex = variableTree.SelectedNode.SelectedImageIndex = 2;
            var node = createNode(variableTree.SelectedNode.Nodes, item);
            variableTree.SelectedNode = node;
            textName.SelectAll();
            textName.Focus();


        }
        private void buttonDeleteVariable_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this item?", "Delete Item", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;

            if (this.selected == null)
                return;
            var parent = GetParentVariable(null, this.exp.Variables, this.selected);
            if (parent == null)
            {
                this.exp.Variables.Remove(this.selected);
            }
            else
            {
                parent.ChildItems.Remove(this.selected);
                if (parent.ChildItems.Count == 0)
                    variableTree.SelectedNode.Parent.SelectedImageIndex = parent.Type switch
                    {
                        VariableType.Standard => 0,
                        VariableType.ClearText => 1,
                        _ => 0
                    };
            }
            variableTree.SelectedNode.Remove();
            SelectItem(null);
        }

        private void variableTree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
        }

        private void variableTree_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node.Tag is VariableItem)) return;
            VariableItem item = e.Node.Tag as VariableItem;
            item.Expanded = false;
        }

        private void variableTree_AfterExpand(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node.Tag is VariableItem)) return;
            VariableItem item = e.Node.Tag as VariableItem;
            item.Expanded = true;

        }
        private void comboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.selected != null)
            {
                this.selected.Type = Enum.Parse<VariableType>(comboType.Text);
                variableTree.SelectedNode.ImageIndex = variableTree.SelectedNode.SelectedImageIndex = this.selected.Type switch
                {
                    VariableType.Standard => 0,
                    VariableType.ClearText => 1,
                    VariableType.TextTemplate => 3,
                };
            }
        }
    }
    class NoflickerTree : TreeView
    {
        public NoflickerTree()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
    }
    class NoFilckerRichTextBox : RichTextBox
    {
        public NoFilckerRichTextBox()
        {
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
        }
    }

}