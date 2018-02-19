using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocWarden.Core
{
    public interface ILocalizationExporter
    {
        Task<LocalizationError> Export(
            IEnumerable<LocalizedLanguage> languages,
            IPluginConfig config);
    }
}
