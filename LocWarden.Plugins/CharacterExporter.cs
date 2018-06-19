using LocWarden.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LocWarden.Plugins
{
    /// <summary>
    /// Exports every unique character found across all the loc files into a single file.
    /// Plus a file for each language.
    /// </summary>
    /// <remarks>
    /// Expects:
    ///     keysListFile: path to a file containing a list of keys, one per a line.
    ///     outputFile: path with format arg to a file to write the results to.
    /// </remarks>
    [Plugin(name: "Character Exporter", description: "Export every character per language and across all languages.")]
    [PluginParameter(name: "includeAlphabet", description: "If true, include a-z, A-Z, 0-9, and basic punctuation.", isOptional: true, type: "bool")]
    [PluginParameter(name: "outputDir", description: "Directory to write results to.")]
    public class CharacterExporter : ILocalizationExporter
    {
        public async Task<LocalizationError> Export(
            IEnumerable<LocalizedLanguage> languages,
            IPluginConfig config)
        {
            LocalizationError result = null;
            var outputDir = string.Empty;

            try
            {
                outputDir = config.GetString("outputDir");
                var outputFile = Path.Combine(outputDir, "{0}-Characters.txt");
                var includeAlphabet = config.GetBool("includeAlphabet", false);

                HashSet<char> uniqueCharacters = new HashSet<char>();
                if (includeAlphabet)
                {
                    // Include common characters if flag is present
                    var commonCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.,:;?!'\"";
                    foreach (var character in commonCharacters)
                    {
                        uniqueCharacters.Add(character);
                    }
                }

                // Gather all the characters in every language
                foreach (var language in languages)
                {
                    // And all the characters per language
                    HashSet<char> uniqueCharactersForLanguage = new HashSet<char>();
                    foreach (var row in language.Rows.Values)
                    {
                        foreach (var character in row.Value)
                        {
                            if (Char.IsWhiteSpace(character))
                            {
                                continue;
                            }

                            uniqueCharactersForLanguage.Add(character);
                            uniqueCharacters.Add(character);
                        }

                        // Write the results for the current language
                        using (var stream = File.OpenWrite(string.Format(outputFile, language.Name)))
                        using (var streamWriter = new StreamWriter(stream))
                        {
                            foreach (var character in uniqueCharactersForLanguage)
                            {
                                streamWriter.Write(character);
                            }

                            await streamWriter.FlushAsync();
                        }
                    }
                }

                // Write the results for all languages
                using (var stream = File.OpenWrite(string.Format(outputFile, "All")))
                using (var streamWriter = new StreamWriter(stream))
                {
                    foreach (var character in uniqueCharacters)
                    {
                        streamWriter.Write(character);
                    }

                    await streamWriter.FlushAsync();
                }
            }
            catch (Exception e)
            {
                if (e is IOException)
                {
                    result = new LocalizationError(
                        LocalizationError.ErrorType.FileError,
                        string.Format("Unable to write to output file: {0}. {1}", outputDir, e.Message),
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
