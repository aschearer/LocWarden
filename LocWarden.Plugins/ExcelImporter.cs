using ClosedXML.Excel;
using LocWarden.Core;

namespace LocWarden.Plugins
{
    /// <summary>
    /// Imports a language stored in an Excel file.
    /// </summary>
    /// <remarks>
    /// The following format is assumed:
    ///   * Localized text is in the first worksheet.
    ///   * There's a single header row.
    ///   * The first column is the key.
    ///   * The second column is a description.
    ///   * For the master, the third column is the value text.
    ///   * For translated languages, the third column is the master text.
    ///   * For translated languages, the fourth column is the value text.
    ///   * Empty rows should be skipped.
    /// </remarks>
    [Plugin(name: "Excel Importer", description: "Imports languages stored as Excel files.")]
    [PluginParameter(name: "numberOfHeaderRows", description: "How many rows should be skipped from the beginning. Default is 1.", isOptional: true, type: "number")]
    [PluginParameter(name: "keyColumn", description: "Column index for loc key. Default is 0.", isOptional: true, type: "number")]
    [PluginParameter(name: "descriptionColumn", description: "Column index for loc description. Default is 1.", isOptional: true, type: "number")]
    [PluginParameter(name: "valueColumnForMaster", description: "Column index for loc value in master language. Default is 2.", isOptional: true, type: "number")]
    [PluginParameter(name: "valueColumnForNormal", description: "Column index for loc value in non-master languages. Default is 3.", isOptional: true, type: "number")]
    public class ExcelImporter : ILocalizationImporter
    {
        public LocalizedLanguage Import(ILanguageDefinition languageDefinition, IPluginConfig config)
        {
            var language = new LocalizedLanguage(languageDefinition);
            var workbook = new XLWorkbook(language.Path);
            var worksheet = workbook.Worksheet(1);
            int headerRowsRemaining = config.GetInt("numberOfHeaderRows", 1);
            int keyColumn = 1 + config.GetInt("keyColumn", 0);
            int descriptionColumn = 1 + config.GetInt("descriptionColumn", 1);
            int valueMasterColumn = 1 + config.GetInt("valueColumnForMaster", 2);
            int valueNormalColumn = 1 + config.GetInt("valueColumnForNormal", 3);

            bool isMasterLanguage = language.IsMasterLanguage;
            int rowNumber = 1;
            foreach (var row in worksheet.Rows())
            {
                if (headerRowsRemaining > 0)
                {
                    headerRowsRemaining--;

                    rowNumber++;
                    continue;
                }

                if (row.IsEmpty())
                {
                    rowNumber++;
                    continue;
                }

                int valueColumn = language.IsMasterLanguage ? valueMasterColumn : valueNormalColumn;

                language.AddText(
                    key: row.Cell(keyColumn).Value.ToString(),
                    description: row.Cell(descriptionColumn).Value.ToString(),
                    value: row.Cell(valueColumn).Value.ToString(),
                    rowNumber: rowNumber);

                rowNumber++;
            }

            return language;
        }
    }
}
