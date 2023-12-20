using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace orch.core.model
{
    public abstract class OrganizationDataProps:ChangeProps
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string LocalName { get; set; }
        public string Abbreviation { get; set; }
        public string Vision { get; set; }
        public string Mission { get; set; }
        public string Values { get; set; }
        public Guid? LogoFileId { get; set; }
        public Guid? LeftLetterHeadingLogoFileId { get; set; }
        public Guid? RightLetterHeadingLogoFileId { get; set; }
        public string Moto { get; set; }
        public string Address { get; set; }
        public string TinNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string VatRegistrationNumber { get; set; }
        public long WorkingHrPerMonth { get; set; }
        public PerdimeRate PerdiumRate { get; set; }
        public string Remark { get; set; }

    }
    public enum PerdimeRate
    {
        Fixed, Salary_Scale
    }
}
