using CsvHelper;
using LocWarden.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Configuration = CsvHelper.Configuration.Configuration;

namespace LocWarden.Plugins
{
    /// <summary>
    /// Exports a CSV file containing the localization data in a format
    /// suitable for import by the I2 plug-in for Unity.
    /// </summary>
    [Plugin(name: "Csv Exporter", description: "Exports languages in a single CSV.")]
    [PluginParameter(name: "outputFile", description: "File to write CSV data to.")]
    public class CsvExporter : ILocalizationExporter
    {
        public async Task<LocalizationError> Export(
            IEnumerable<LocalizedLanguage> languages,
            IPluginConfig config)
        {
            LocalizationError result = null;
            var outputFile = config.GetString("outputFile");

            try
            {
                using (var stream = File.OpenWrite(outputFile))
                using (var streamWriter = new StreamWriter(stream))
                {
                    Configuration configuration = new Configuration
                    {
                        Encoding = Encoding.UTF8
                    };

                    CsvWriter writer = new CsvWriter(streamWriter, configuration);
                    LocalizedLanguage masterLanguage = null;

                    writer.WriteField("Key");
                    writer.WriteField("Type");
                    writer.WriteField("Desc");
                    foreach (var language in languages)
                    {
                        if (language.IsMasterLanguage)
                        {
                            masterLanguage = language;
                        }

                        writer.WriteField(language.Name);
                    }

                    writer.NextRecord();

                    foreach (var row in masterLanguage.Rows.Values)
                    {
                        writer.WriteField(row.Key);
                        writer.WriteField("Text");
                        writer.WriteField(row.Description);
                        foreach (var language in languages)
                        {
                            writer.WriteField(language.Rows[row.Key].Value);
                        }

                        writer.NextRecord();
                    }

                    await writer.FlushAsync();
                }
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is IOException)
                {
                    result = new LocalizationError(
                        LocalizationError.ErrorType.FileError,
                        string.Format("Unable to write to output file: {0}. {1}", outputFile, e.Message),
                        0);
                }
                else
                {
                    result = new LocalizationError(
                        LocalizationError.ErrorType.PluginError,
                        string.Format("Plugin failed: {0}", e.Message),
                        0);
                }
            }

            return result;
        }
    }
}
