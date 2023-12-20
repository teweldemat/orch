using orch.utils.web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.ef.workflow.entities
{
    internal class DALTaskNoteContentItem
    {
        public Guid NoteId { get; set; }
        public Guid TranId { get; set; }
        public int SeqNo { get; set; }
        public Guid FileId { get; set; }
        public DALTaskNote Note { get; set; }
    }
}
