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
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocalName { get; set; } = string.Empty;
        public string Abbreviation { get; set; } = string.Empty;
        public string Vision { get; set; } = string.Empty;
        public string Mission { get; set; } = string.Empty;
        public string Values { get; set; } = string.Empty;
        public Guid? LogoFileId { get; set; }
        public Guid? LeftLetterHeadingLogoFileId { get; set; }
        public Guid? RightLetterHeadingLogoFileId { get; set; }
        public string Moto { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string TinNumber { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string VatRegistrationNumber { get; set; } = string.Empty;
        public long WorkingHrPerMonth { get; set; }
        public PerdimeRate PerdiumRate { get; set; }
        public string Remark { get; set; } = string.Empty;

    }
    public enum PerdimeRate
    {
        Fixed, Salary_Scale
    }
}
