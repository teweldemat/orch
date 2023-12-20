using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.utils.web
{
    public class OContentItem
    {
        public Guid FileId { get; set; }
        public byte[] Hash { get; set; }
        public String Reference { get; set; }
        public String Description { get; set; }
        public string FileName { get; set; }
        public String Mime { get; set; }

    }
}
