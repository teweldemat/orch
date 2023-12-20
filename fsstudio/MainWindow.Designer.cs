namespace fsstudio
{
    partial class MainWindow
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            splitMain = new SplitContainer();
            listFiles = new ListView();
            toolStrip2 = new ToolStrip();
            buttonAddFile = new ToolStripButton();
            buttonDeleteFile = new ToolStripButton();
            splitRight = new SplitContainer();
            splitText = new SplitContainer();
            toolStrip1 = new ToolStrip();
            textName = new ToolStripTextBox();
            buttonDelete = new ToolStripButton();
            buttonAddSub = new ToolStripButton();
            buttonAdd = new ToolStripButton();
            buttonSync = new ToolStripButton();
            buttonAPI = new ToolStripButton();
            buttonCopy = new ToolStripButton();
            comboType = new ToolStripComboBox();
            tabResult = new TabControl();
            tabTexResult = new TabPage();
            labelValue = new RichTextBox();
            tabHtmlResult = new TabPage();
            webView = new Microsoft.Web.WebView2.WinForms.WebView2();
            variableTree = new TreeView();
            contextTree = new ContextMenuStrip(components);
            itemCopyNode = new ToolStripMenuItem();
            evaluateToolStripMenuItem = new ToolStripMenuItem();
            evaluateWithAPIToolStripMenuItem = new ToolStripMenuItem();
            textExpression = new FastColoredTextBoxNS.FastColoredTextBox();
            ((System.ComponentModel.ISupportInitialize)splitMain).BeginInit();
            splitMain.Panel1.SuspendLayout();
            splitMain.Panel2.SuspendLayout();
            splitMain.SuspendLayout();
            toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitRight).BeginInit();
            splitRight.Panel1.SuspendLayout();
            splitRight.Panel2.SuspendLayout();
            splitRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitText).BeginInit();
            splitText.Panel1.SuspendLayout();
            splitText.Panel2.SuspendLayout();
            splitText.SuspendLayout();
            toolStrip1.SuspendLayout();
            tabResult.SuspendLayout();
            tabTexResult.SuspendLayout();
            tabHtmlResult.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)webView).BeginInit();
            contextTree.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)textExpression).BeginInit();
            SuspendLayout();
            // 
            // splitMain
            // 
            splitMain.Dock = DockStyle.Fill;
            splitMain.FixedPanel = FixedPanel.Panel1;
            splitMain.Location = new Point(0, 0);
            splitMain.Margin = new Padding(2);
            splitMain.Name = "splitMain";
            // 
            // splitMain.Panel1
            // 
            splitMain.Panel1.Controls.Add(listFiles);
            splitMain.Panel1.Controls.Add(toolStrip2);
            // 
            // splitMain.Panel2
            // 
            splitMain.Panel2.Controls.Add(splitRight);
            splitMain.Size = new Size(1175, 466);
            splitMain.SplitterDistance = 230;
            splitMain.SplitterWidth = 3;
            splitMain.TabIndex = 0;
            // 
            // listFiles
            // 
            listFiles.Dock = DockStyle.Fill;
            listFiles.FullRowSelect = true;
            listFiles.Location = new Point(0, 27);
            listFiles.Margin = new Padding(2);
            listFiles.Name = "listFiles";
            listFiles.Size = new Size(230, 439);
            listFiles.TabIndex = 1;
            listFiles.UseCompatibleStateImageBehavior = false;
            listFiles.View = View.List;
            listFiles.SelectedIndexChanged += listFiles_SelectedIndexChanged;
            listFiles.DoubleClick += listFiles_DoubleClick;
            // 
            // toolStrip2
            // 
            toolStrip2.ImageScalingSize = new Size(24, 24);
            toolStrip2.Items.AddRange(new ToolStripItem[] { buttonAddFile, buttonDeleteFile });
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(230, 27);
            toolStrip2.TabIndex = 0;
            toolStrip2.Text = "toolStrip2";
            // 
            // buttonAddFile
            // 
            buttonAddFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonAddFile.Image = (Image)resources.GetObject("buttonAddFile.Image");
            buttonAddFile.ImageTransparentColor = Color.Magenta;
            buttonAddFile.Name = "buttonAddFile";
            buttonAddFile.Size = new Size(33, 24);
            buttonAddFile.Text = "[+]";
            buttonAddFile.Click += buttonAddFile_Click;
            // 
            // buttonDeleteFile
            // 
            buttonDeleteFile.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonDeleteFile.Image = (Image)resources.GetObject("buttonDeleteFile.Image");
            buttonDeleteFile.ImageTransparentColor = Color.Magenta;
            buttonDeleteFile.Name = "buttonDeleteFile";
            buttonDeleteFile.Size = new Size(29, 24);
            buttonDeleteFile.Text = "[-]";
            buttonDeleteFile.Click += buttonDelete_Click;
            // 
            // splitRight
            // 
            splitRight.Dock = DockStyle.Fill;
            splitRight.FixedPanel = FixedPanel.Panel2;
            splitRight.Location = new Point(0, 0);
            splitRight.Margin = new Padding(2);
            splitRight.Name = "splitRight";
            // 
            // splitRight.Panel1
            // 
            splitRight.Panel1.Controls.Add(splitText);
            // 
            // splitRight.Panel2
            // 
            splitRight.Panel2.Controls.Add(variableTree);
            splitRight.Size = new Size(942, 466);
            splitRight.SplitterDistance = 627;
            splitRight.SplitterWidth = 3;
            splitRight.TabIndex = 2;
            // 
            // splitText
            // 
            splitText.Dock = DockStyle.Fill;
            splitText.FixedPanel = FixedPanel.Panel2;
            splitText.Location = new Point(0, 0);
            splitText.Margin = new Padding(2);
            splitText.Name = "splitText";
            splitText.Orientation = Orientation.Horizontal;
            // 
            // splitText.Panel1
            // 
            splitText.Panel1.Controls.Add(textExpression);
            splitText.Panel1.Controls.Add(toolStrip1);
            // 
            // splitText.Panel2
            // 
            splitText.Panel2.Controls.Add(tabResult);
            splitText.Size = new Size(627, 466);
            splitText.SplitterDistance = 272;
            splitText.SplitterWidth = 3;
            splitText.TabIndex = 1;
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(24, 24);
            toolStrip1.Items.AddRange(new ToolStripItem[] { textName, buttonDelete, buttonAddSub, buttonAdd, buttonSync, buttonAPI, buttonCopy, comboType });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(627, 32);
            toolStrip1.TabIndex = 1;
            toolStrip1.Text = "toolStrip1";
            // 
            // textName
            // 
            textName.Name = "textName";
            textName.Size = new Size(241, 32);
            textName.Leave += textName_Leave;
            textName.KeyDown += textName_KeyDown;
            textName.Click += textName_Click;
            // 
            // buttonDelete
            // 
            buttonDelete.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonDelete.Image = (Image)resources.GetObject("buttonDelete.Image");
            buttonDelete.ImageTransparentColor = Color.Magenta;
            buttonDelete.Name = "buttonDelete";
            buttonDelete.Size = new Size(32, 29);
            buttonDelete.Text = "[X]";
            buttonDelete.Click += buttonDeleteVariable_Click;
            // 
            // buttonAddSub
            // 
            buttonAddSub.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonAddSub.Image = (Image)resources.GetObject("buttonAddSub.Image");
            buttonAddSub.ImageTransparentColor = Color.Magenta;
            buttonAddSub.Name = "buttonAddSub";
            buttonAddSub.Size = new Size(43, 29);
            buttonAddSub.Text = "[++]";
            buttonAddSub.Click += buttonAddSub_Click;
            // 
            // buttonAdd
            // 
            buttonAdd.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonAdd.Image = (Image)resources.GetObject("buttonAdd.Image");
            buttonAdd.ImageTransparentColor = Color.Magenta;
            buttonAdd.Name = "buttonAdd";
            buttonAdd.Size = new Size(33, 29);
            buttonAdd.Text = "[+]";
            buttonAdd.Click += buttonAdd_Click;
            // 
            // buttonSync
            // 
            buttonSync.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonSync.Image = (Image)resources.GetObject("buttonSync.Image");
            buttonSync.ImageTransparentColor = Color.Magenta;
            buttonSync.Name = "buttonSync";
            buttonSync.Size = new Size(69, 29);
            buttonSync.Text = "Evaluate";
            buttonSync.Click += buttonSync_Click;
            // 
            // buttonAPI
            // 
            buttonAPI.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonAPI.Image = (Image)resources.GetObject("buttonAPI.Image");
            buttonAPI.ImageTransparentColor = Color.Magenta;
            buttonAPI.Name = "buttonAPI";
            buttonAPI.Size = new Size(35, 29);
            buttonAPI.Text = "API";
            buttonAPI.Click += buttonAPI_Click;
            // 
            // buttonCopy
            // 
            buttonCopy.DisplayStyle = ToolStripItemDisplayStyle.Text;
            buttonCopy.Image = (Image)resources.GetObject("buttonCopy.Image");
            buttonCopy.ImageTransparentColor = Color.Magenta;
            buttonCopy.Name = "buttonCopy";
            buttonCopy.Size = new Size(91, 29);
            buttonCopy.Text = "Copy Result";
            buttonCopy.Click += buttonCopy_Click;
            // 
            // comboType
            // 
            comboType.DropDownStyle = ComboBoxStyle.DropDownList;
            comboType.Items.AddRange(new object[] { "Standard", "ClearText", "TextTemplate" });
            comboType.Name = "comboType";
            comboType.Size = new Size(121, 28);
            comboType.SelectedIndexChanged += comboType_SelectedIndexChanged;
            // 
            // tabResult
            // 
            tabResult.Controls.Add(tabTexResult);
            tabResult.Controls.Add(tabHtmlResult);
            tabResult.Dock = DockStyle.Fill;
            tabResult.Location = new Point(0, 0);
            tabResult.Margin = new Padding(2);
            tabResult.Name = "tabResult";
            tabResult.SelectedIndex = 0;
            tabResult.Size = new Size(627, 191);
            tabResult.TabIndex = 1;
            tabResult.SelectedIndexChanged += tabResult_SelectedIndexChanged;
            // 
            // tabTexResult
            // 
            tabTexResult.Controls.Add(labelValue);
            tabTexResult.Location = new Point(4, 29);
            tabTexResult.Margin = new Padding(2);
            tabTexResult.Name = "tabTexResult";
            tabTexResult.Padding = new Padding(2);
            tabTexResult.Size = new Size(619, 158);
            tabTexResult.TabIndex = 0;
            tabTexResult.Text = "Text";
            tabTexResult.UseVisualStyleBackColor = true;
            // 
            // labelValue
            // 
            labelValue.BackColor = Color.White;
            labelValue.Dock = DockStyle.Fill;
            labelValue.Font = new Font("Arial Narrow", 10F, FontStyle.Regular, GraphicsUnit.Point);
            labelValue.Location = new Point(2, 2);
            labelValue.Margin = new Padding(2);
            labelValue.Name = "labelValue";
            labelValue.ReadOnly = true;
            labelValue.Size = new Size(615, 154);
            labelValue.TabIndex = 0;
            labelValue.Text = "";
            // 
            // tabHtmlResult
            // 
            tabHtmlResult.Controls.Add(webView);
            tabHtmlResult.Location = new Point(4, 29);
            tabHtmlResult.Margin = new Padding(2);
            tabHtmlResult.Name = "tabHtmlResult";
            tabHtmlResult.Padding = new Padding(2);
            tabHtmlResult.Size = new Size(618, 159);
            tabHtmlResult.TabIndex = 1;
            tabHtmlResult.Text = "HTML";
            tabHtmlResult.UseVisualStyleBackColor = true;
            // 
            // webView
            // 
            webView.CreationProperties = null;
            webView.DefaultBackgroundColor = Color.White;
            webView.Dock = DockStyle.Fill;
            webView.Location = new Point(2, 2);
            webView.Margin = new Padding(2);
            webView.Name = "webView";
            webView.Size = new Size(614, 155);
            webView.TabIndex = 0;
            webView.ZoomFactor = 1D;
            // 
            // variableTree
            // 
            variableTree.AllowDrop = true;
            variableTree.Dock = DockStyle.Fill;
            variableTree.Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
            variableTree.FullRowSelect = true;
            variableTree.HideSelection = false;
            variableTree.Location = new Point(0, 0);
            variableTree.Name = "variableTree";
            variableTree.Size = new Size(312, 466);
            variableTree.TabIndex = 1;
            variableTree.AfterCollapse += variableTree_AfterCollapse;
            variableTree.BeforeExpand += variableTree_BeforeExpand;
            variableTree.AfterExpand += variableTree_AfterExpand;
            variableTree.ItemDrag += variableTree_ItemDrag;
            variableTree.AfterSelect += variableTree_AfterSelect;
            variableTree.DragDrop += variableTree_DragDrop;
            variableTree.DragEnter += variableTree_DragEnter;
            // 
            // contextTree
            // 
            contextTree.ImageScalingSize = new Size(24, 24);
            contextTree.Items.AddRange(new ToolStripItem[] { itemCopyNode, evaluateToolStripMenuItem, evaluateWithAPIToolStripMenuItem });
            contextTree.Name = "contextTree";
            contextTree.Size = new Size(193, 76);
            contextTree.Opening += contextTree_Opening;
            // 
            // itemCopyNode
            // 
            itemCopyNode.Name = "itemCopyNode";
            itemCopyNode.Size = new Size(192, 24);
            itemCopyNode.Text = "Copy Expression";
            itemCopyNode.Click += itemCopyNode_Click;
            // 
            // evaluateToolStripMenuItem
            // 
            evaluateToolStripMenuItem.Name = "evaluateToolStripMenuItem";
            evaluateToolStripMenuItem.Size = new Size(192, 24);
            evaluateToolStripMenuItem.Text = "Evaluate";
            evaluateToolStripMenuItem.Click += evaluateToolStripMenuItem_Click;
            // 
            // evaluateWithAPIToolStripMenuItem
            // 
            evaluateWithAPIToolStripMenuItem.Name = "evaluateWithAPIToolStripMenuItem";
            evaluateWithAPIToolStripMenuItem.Size = new Size(192, 24);
            evaluateWithAPIToolStripMenuItem.Text = "Evaluate with API";
            evaluateWithAPIToolStripMenuItem.Click += evaluateWithAPIToolStripMenuItem_Click;
            // 
            // textExpression
            // 
            textExpression.AutoCompleteBracketsList = (new char[] { '(', ')', '{', '}', '[', ']', '"', '"', '\'', '\'' });
            textExpression.AutoScrollMinSize = new Size(31, 18);
            textExpression.BackBrush = null;
            textExpression.CharHeight = 18;
            textExpression.CharWidth = 10;
            textExpression.DisabledColor = Color.FromArgb(100, 180, 180, 180);
            textExpression.Dock = DockStyle.Fill;
            textExpression.Font = new Font("Courier New", 9.75F, FontStyle.Regular, GraphicsUnit.Point);
            textExpression.IsReplaceMode = false;
            textExpression.Location = new Point(0, 32);
            textExpression.Name = "textExpression";
            textExpression.Paddings = new Padding(0);
            textExpression.SelectionColor = Color.FromArgb(60, 0, 0, 255);
            textExpression.ServiceColors = (FastColoredTextBoxNS.ServiceColors)resources.GetObject("textExpression.ServiceColors");
            textExpression.Size = new Size(627, 240);
            textExpression.TabIndex = 2;
            textExpression.Zoom = 100;
            // 
            // MainWindow
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1175, 466);
            Controls.Add(splitMain);
            DoubleBuffered = true;
            Margin = new Padding(2);
            Name = "MainWindow";
            Text = "FuncScript";
            Load += MainWindow_Load;
            splitMain.Panel1.ResumeLayout(false);
            splitMain.Panel1.PerformLayout();
            splitMain.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitMain).EndInit();
            splitMain.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            splitRight.Panel1.ResumeLayout(false);
            splitRight.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitRight).EndInit();
            splitRight.ResumeLayout(false);
            splitText.Panel1.ResumeLayout(false);
            splitText.Panel1.PerformLayout();
            splitText.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitText).EndInit();
            splitText.ResumeLayout(false);
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            tabResult.ResumeLayout(false);
            tabTexResult.ResumeLayout(false);
            tabHtmlResult.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)webView).EndInit();
            contextTree.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)textExpression).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private SplitContainer splitMain;
        private SplitContainer splitText;
        private SplitContainer splitRight;
        private ToolStrip toolStrip1;
        private ToolStripTextBox textName;
        private ToolStripButton buttonAdd;
        private ListView listFiles;
        private ToolStrip toolStrip2;
        private ToolStripButton buttonAddFile;
        private ToolStripButton buttonDeleteFile;
        private ToolStripButton buttonAPI;
        private RichTextBox labelValue;
        private ToolStripButton buttonSync;
        private ContextMenuStrip contextTree;
        private ToolStripMenuItem itemCopyNode;
        private ToolStripMenuItem evaluateToolStripMenuItem;
        private ToolStripMenuItem evaluateWithAPIToolStripMenuItem;
        private TabControl tabResult;
        private TabPage tabTexResult;
        private TabPage tabHtmlResult;
        private Microsoft.Web.WebView2.WinForms.WebView2 webView;
        private ToolStripButton buttonCopy;
        private TreeView variableTree;
        private ToolStripButton buttonDelete;
        private ToolStripButton buttonAddSub;
        private ToolStripComboBox comboType;
        private FastColoredTextBoxNS.FastColoredTextBox textExpression;
    }
}