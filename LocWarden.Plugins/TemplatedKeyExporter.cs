using LocWarden.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace LocWarden.Plugins
{
    /// <summary>
    /// Takes a template file and exports a copy per language with 
    /// parameters replaced with localized text.
    /// </summary>
    /// <remarks>
    /// This is useful if you want to collate a number of parameters 
    /// together. For example, if you want to create text for your
    /// Steam page using multiple localized terms you can create
    /// a template appropriate for Steam using the desired terms then
    /// quickly copy and paste the generated text into Steam.
    /// 
    /// Expects:
    ///   templateFile: path to a file with keys in {[format args]}.
    ///   outputFile: path to a file to write results to.
    /// </remarks>
    public class TemplatedKeyExporter : ILocalizationExporter
    {
        public async Task<LocalizationError> Export(
            IEnumerable<LocalizedLanguage> languages,
            IPluginConfig config)
        {
            LocalizationError result = null;
            var templateFile = config.GetString("templateFile");
            var outputFile = config.GetString("outputFile");

            string[] keysToProcess;
            string template = string.Empty;
            try
            {
                template = File.ReadAllText(templateFile);

                List<string> args = new List<string>();
                var text = template;
                int argStart = text.IndexOf(Constants.ArgStart);
                int argStop = text.IndexOf(Constants.ArgStop);
                while (argStart >= 0)
                {
                    if (argStop < 0)
                    {
                        // We have a problem
                        throw new FormatException();
                    }

                    args.Add(text.Substring(argStart + Constants.ArgStart.Length, argStop - argStart - Constants.ArgStart.Length));

                    text = text.Substring(argStop + 1);
                    argStart = text.IndexOf(Constants.ArgStart);
                    argStop = text.IndexOf(Constants.ArgStop);
                }

                keysToProcess = args.ToArray();

                using (var stream = File.OpenWrite(outputFile))
                using (var streamWriter = new StreamWriter(stream))
                {
                    var writer = new StringBuilder();

                    foreach (var language in languages)
                    {
                        var templateForLanguage = template;
                        writer.AppendLine("Language: " + language.Name);
                        foreach (var key in keysToProcess)
                        {
                            ILocalizedText row;
                            if (language.Rows.ContainsKey(key))
                            {
                                row = language.Rows[key];
                                templateForLanguage = templateForLanguage.Replace(Constants.ArgStart + key + Constants.ArgStop, row.Value);
                            }
                            else
                            {
                                System.Console.WriteLine("Key missing: {0}", key);
                            }
                        }

                        writer.Append(templateForLanguage);
                        writer.AppendLine();
                    }

                    await streamWriter.WriteAsync(writer.ToString());
                }
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is IOException)
                {
                    if (string.IsNullOrEmpty(template))
                    {
                        result = new LocalizationError(
                            LocalizationError.ErrorType.FileError,
                            string.Format("Unable to read template file: {0}. {1}", templateFile, e.Message),
                            0);
                    }
                    else
                    {
                        result = new LocalizationError(
                            LocalizationError.ErrorType.FileError,
                            string.Format("Unable to write to output file: {0}. {1}", outputFile, e.Message),
                            0);
                    }
                }
                else if (e is FormatException)
                {
                    result = new LocalizationError(
                        LocalizationError.ErrorType.PluginError,
                        "Template file has malformed format args.",
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