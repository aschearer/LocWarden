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
    /// Exports the designated keys as columns with each language as a row.
    /// </summary>
    /// <remarks>
    /// Expects:
    ///     keysListFile: path to a file containing a list of keys, one per a line.
    ///     outputFile: path to a file to write the results to.
    /// </remarks>
    public class KeyExporter : ILocalizationExporter
    {
        public async Task<LocalizationError> Export(
            IEnumerable<LocalizedLanguage> languages,
            IPluginConfig config)
        {
            LocalizationError result = null;
            var keysListFile = config.GetString("keysListFile");
            var outputFile = config.GetString("outputFile");
            string[] keysToProcess = null;

            try
            {
                keysToProcess = File.ReadAllLines(keysListFile);

                using (var stream = File.OpenWrite(outputFile))
                using (var streamWriter = new StreamWriter(stream))
                {
                    Configuration configuration = new Configuration
                    {
                        Encoding = Encoding.UTF8
                    };

                    CsvWriter writer = new CsvWriter(streamWriter, configuration);
                    LocalizedLanguage masterLanguage = null;

                    // Write the header
                    writer.WriteField("Language");
                    foreach (var key in keysToProcess)
                    {
                        writer.WriteField(key);
                    }

                    writer.NextRecord();

                    // Add a row for each language with the keys as columns.
                    foreach (var language in languages)
                    {
                        if (language.IsMasterLanguage)
                        {
                            masterLanguage = language;
                        }

                        writer.WriteField(language.Name);

                        foreach (var key in keysToProcess)
                        {
                            if (language.Rows.ContainsKey(key))
                            {
                                var row = language.Rows[key];
                                writer.WriteField(row.Value);
                            }
                            else
                            {
                                writer.WriteField(string.Empty);
                            }
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
                    if (keysToProcess == null)
                    {
                        result = new LocalizationError(
                            LocalizationError.ErrorType.FileError,
                            string.Format("Unable to write to output file: {0}. {1}", outputFile, e.Message),
                            0);
                    }
                    else
                    {
                        result = new LocalizationError(
                            LocalizationError.ErrorType.FileError,
                            string.Format("Unable to read key list: {0}. {1}", keysListFile, e.Message),
                            0);
                    }
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
