using Microsoft.Extensions.Localization;
using System.Globalization;

namespace orch.common
{
    public interface IExtendedStringLocalizer : IStringLocalizer
    {
        CultureInfo Culture { get; set; }
        LocalizedString this[string name, string fallback] { get; }
    }
}
