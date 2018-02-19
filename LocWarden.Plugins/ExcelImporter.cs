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
    public class ExcelImporter : ILocalizationImporter
    {
        public LocalizedLanguage Import(ILanguageDefinition languageDefinition, IPluginConfig config)
        {
            var language = new LocalizedLanguage(languageDefinition);
            var workbook = new XLWorkbook(language.Path);
            var worksheet = workbook.Worksheet(1);
            bool isHeaderRow = true;
            bool isMasterLanguage = language.IsMasterLanguage;
            int rowNumber = 1;
            foreach (var row in worksheet.Rows())
            {
                if (isHeaderRow)
                {
                    isHeaderRow = false;

                    rowNumber++;
                    continue;
                }

                if (row.IsEmpty())
                {
                    rowNumber++;
                    continue;
                }

                int valueColumn = language.IsMasterLanguage ? 3 : 4;

                language.AddText(
                    key: row.Cell(1).Value.ToString(),
                    description: row.Cell(2).Value.ToString(),
                    value: row.Cell(valueColumn).Value.ToString(),
                    rowNumber: rowNumber);

                rowNumber++;
            }

            return language;
        }
    }
}
