using LocWarden.Core;
using Microsoft.Extensions.Configuration;
using NDesk.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace LocWarden.Console
{
    class Program
    {
        static Dictionary<string, Assembly> loadedAssemblies = new Dictionary<string, Assembly>();

        static Dictionary<string, object> loadedPlugins = new Dictionary<string, object>();

        static void Main(string[] args)
        {
            string configFile = string.Empty;
            bool showHelp = false;
            var options = new OptionSet()
            {
                { "c|config=", "the configuration file.", v => { configFile = v; } },
                { "h|help", "show this message and exit.", v => { showHelp = !string.IsNullOrEmpty(v); } },
            };

            options.Parse(args);

            if (showHelp)
            {
                options.WriteOptionDescriptions(System.Console.Out);
                return;
            }

            if (string.IsNullOrEmpty(configFile))
            {
                System.Console.Write("No config file specified.");
                Environment.Exit(1);
                return;
            }

            if (!File.Exists(configFile))
            {
                System.Console.Write("Config file does not exist.");
                Environment.Exit(2);
                return;
            }

            List<LanguageDefinition> languageDefinitions = new List<LanguageDefinition>();
            List<PluginDefinition> pluginDefinitions = new List<PluginDefinition>();

            try
            {
                var builder = new ConfigurationBuilder()
                    .AddYamlFile(configFile);
                var configuration = builder.Build();
                
                var rawLanguageDefinitions = configuration.GetSection("languages");
                foreach (var language in rawLanguageDefinitions.GetChildren())
                {
                    string isMasterRaw = language["isMaster"];
                    languageDefinitions.Add(
                        new LanguageDefinition()
                        {
                            Name = language.Key,
                            Path = language["path"],
                            IsMaster = string.IsNullOrEmpty(isMasterRaw) ? false : bool.Parse(isMasterRaw),
                        });
                }

                var plugins = configuration.GetSection("plugins");
                foreach (var plugin in plugins.GetChildren())
                {
                    pluginDefinitions.Add(
                        new PluginDefinition()
                        {
                            Name = plugin.Key,
                            Type = plugin["type"],
                            Assembly = plugin["assembly"],
                            Config = new PluginConfigAdapter(plugin),
                        });
                }
            }
            catch (Exception)
            {
                System.Console.Write("Config file could not be processed.");
                Environment.Exit(3);
                return;
            }

            int numberOfImporters = 0;
            PluginDefinition importerPlugin = null;
            ILocalizationImporter importer = null;
            foreach (var plugin in pluginDefinitions)
            {
                Assembly assembly;
                object pluginInstance;

                try
                {
                    if (!loadedAssemblies.ContainsKey(plugin.Assembly))
                    {
                        loadedAssemblies[plugin.Assembly] = Assembly.LoadFrom(plugin.Assembly);
                    }

                    assembly = loadedAssemblies[plugin.Assembly];
                }
                catch
                {
                    System.Console.Write("Cannot load plugin assembly: {0}", plugin.Assembly);
                    Environment.Exit(4);
                    return;
                }

                try
                {
                    if (!loadedPlugins.ContainsKey(plugin.Type))
                    {
                        var type = assembly.GetType(plugin.Type);
                        loadedPlugins[plugin.Type] = Activator.CreateInstance(type);
                    }

                    pluginInstance = loadedPlugins[plugin.Type];
                }
                catch
                {
                    System.Console.Write("Cannot instantiate plugin type: {0}", plugin.Type);
                    Environment.Exit(4);
                    return;
                }

                if (pluginInstance is ILocalizationImporter)
                {
                    numberOfImporters++;
                    importerPlugin = plugin;
                    importer = pluginInstance as ILocalizationImporter;
                }
            }

            if (numberOfImporters != 1)
            {
                System.Console.Write("You must define exactly one importer plugin. Found: {0:N0}.", numberOfImporters);
                Environment.Exit(4);
                return;
            }

            foreach (var languageDefinition in languageDefinitions)
            {
                if (!File.Exists(languageDefinition.Path))
                {
                    System.Console.Write("Cannot open localization file: {0}", languageDefinition.Path);
                    Environment.Exit(4);
                    return;
                }
            }

            // Step 1. Create a language class for file passed in.
            // Load every language using the specified importer.
            var languages = new List<LocalizedLanguage>();
            LocalizedLanguage masterLanguage = null;
            foreach (var languageDefinition in languageDefinitions)
            {
                var language = importer.Import(languageDefinition, importerPlugin.Config);
                if (language.IsMasterLanguage)
                {
                    masterLanguage = language;
                }

                languages.Add(language);
            }

            if (masterLanguage == null)
            {
                System.Console.Write("No master language specified in config file.");
                Environment.Exit(5);
                return;
            }

            // Step 2. Read in every language file and populate corresponding
            // metadata in anticipation of processing.
            int numberOfLanguagesWithErrors = 0;
            int numberOfErrors = 0;
            foreach (var language in languages)
            {
                if (language.IsMasterLanguage)
                {
                    continue;
                }

                CompareLanguages(masterLanguage, language);

                if (language.Errors.Count > 0)
                {
                    System.Console.WriteLine(
                        "Errors encountered in: {0} ({1})",
                        language.Name,
                        language.Path);
                    numberOfLanguagesWithErrors++;
                    numberOfErrors += language.Errors.Count;
                    System.Console.WriteLine("Type|Line Number|Message");
                }

                foreach (var error in language.Errors)
                {
                    System.Console.WriteLine(
                        "{1}|{2}|{0}",
                        error.Message,
                        error.Error,
                        error.LineNumber);
                }

                if (language.Errors.Count > 0)
                {
                    System.Console.WriteLine();
                }
            }

            if (numberOfLanguagesWithErrors > 0)
            {
                System.Console.WriteLine(
                    "Encountered {0:N0} errors across {1:N0} languages.",
                    numberOfErrors,
                    numberOfLanguagesWithErrors);
            }
            else
            {
                System.Console.WriteLine("Languages validated. No errors encountered!");
            }

            // Step 3. Run processor plugins
            List<LocalizationError> pluginErrors = new List<LocalizationError>();
            foreach (var plugin in pluginDefinitions)
            {
                var instance = loadedPlugins[plugin.Type];
                if (instance != importer)
                {
                    var exporter = instance as ILocalizationExporter;
                    var task = exporter.Export(languages, plugin.Config);
                    task.Wait();
                    if (task.Result != null)
                    {
                        pluginErrors.Add(task.Result);
                    }
                }
            }

            if (pluginErrors.Count > 0)
            {
                System.Console.WriteLine("Type|Line Number|Message");
                foreach (var error in pluginErrors)
                {
                    System.Console.WriteLine(
                        "{1}|{2}|{0}",
                        error.Message,
                        error.Error,
                        error.LineNumber);
                }
            }
            else if (pluginDefinitions.Count > 1)
            {
                System.Console.WriteLine("Exporters executed. No errors encountered!");
            }

            System.Console.WriteLine("Press any key to continue...");
            System.Console.ReadKey(true);
        }

        private static void CompareLanguages(LocalizedLanguage master, LocalizedLanguage language)
        {
            // Check 1: Do the two languages have the same keys?
            var masterRows = new HashSet<ILocalizedText>(master.Rows.Values);
            var languageRows = new HashSet<ILocalizedText>(language.Rows.Values);
            foreach (var row in master.Rows.Values)
            {
                languageRows.Remove(row);
            }

            foreach (var row in language.Rows.Values)
            {
                masterRows.Remove(row);
            }

            foreach (var row in masterRows)
            {
                language.Errors.Add(new LocalizationError(
                    LocalizationError.ErrorType.KeysMissing,
                    string.Format("Key missing: {0}", row.Key),
                    row.RowNumber));
            }

            foreach (var row in languageRows)
            {
                language.Errors.Add(new LocalizationError(
                    LocalizationError.ErrorType.KeysAdded,
                    string.Format("Key added: {0}", row.Key),
                    row.RowNumber));
            }

            // Check 2: Are the keys in the same order?
            // Only perform this check if the last one passed as it 
            // will always fail if the last check failed.
            if (language.Errors.Count == 0)
            {
                foreach (var masterRow in master.Rows.Values)
                {
                    var row = language.Rows[masterRow.Key];
                    if (masterRow.RowNumber != row.RowNumber)
                    {
                        language.Errors.Add(new LocalizationError(
                            LocalizationError.ErrorType.KeysNotInOrder,
                            string.Format("Keys not in the same order. Expected: {0}. Observed: {1}.", masterRow.Key, row.Key),
                            row.RowNumber));

                        break;
                    }
                }
            }

            // Check 3: Are all the format args present in the translation?
            foreach (var masterRow in master.Rows.Values)
            {
                if (!language.Rows.ContainsKey(masterRow.Key))
                {
                    continue;
                }

                var row = language.Rows[masterRow.Key];
                if (row.HasOpenFormatArgs)
                {
                    // The translation is bad, just bail.
                    language.Errors.Add(new LocalizationError(
                        LocalizationError.ErrorType.FormatArgsOpen,
                        string.Format("Translation has misformatted arg: {0}", row.Key),
                        row.RowNumber));
                }
                else if (masterRow.FormatArgs.Length > 0)
                {
                    // Compare the args args
                    var masterArgs = new HashSet<string>(masterRow.FormatArgs);
                    var languageArgs = new HashSet<string>(row.FormatArgs);
                    foreach (var arg in masterRow.FormatArgs)
                    {
                        languageArgs.Remove(arg);
                    }

                    foreach (var arg in row.FormatArgs)
                    {
                        masterArgs.Remove(arg);
                    }

                    foreach (var arg in masterArgs)
                    {
                        language.Errors.Add(new LocalizationError(
                            LocalizationError.ErrorType.FormatArgMissing,
                            string.Format("Format arg missing: {0}", row.Key),
                            row.RowNumber));
                    }

                    foreach (var arg in languageArgs)
                    {
                        language.Errors.Add(new LocalizationError(
                            LocalizationError.ErrorType.FormatArgsAdded,
                            string.Format("Translation has extra format args: {0}", row.Key),
                            row.RowNumber));
                    }
                }
                else if (row.FormatArgs.Length > 0)
                {
                    // The translation has args but the master does not
                    language.Errors.Add(new LocalizationError(
                        LocalizationError.ErrorType.FormatArgsAdded,
                        string.Format("Translation has extra format args: {0}", row.Key),
                        row.RowNumber));
                }
            }

            // Check 4. Make sure most of the translations are not identical to master
            float numberOfIdenticalTerms = 0;
            float numberOfTerms = 0;
            foreach (var masterRow in master.Rows.Values)
            {
                if (!language.Rows.ContainsKey(masterRow.Key))
                {
                    continue;
                }

                var row = language.Rows[masterRow.Key];
                numberOfTerms++;
                if (row.Value.Equals(masterRow.Value, StringComparison.InvariantCultureIgnoreCase))
                {
                    numberOfIdenticalTerms++;
                }
            }

            var percentIdentical = numberOfIdenticalTerms / numberOfTerms;
            if (percentIdentical > 0.25f)
            {
                language.Errors.Add(new LocalizationError(
                    LocalizationError.ErrorType.EmptyTerm,
                    string.Format("{0:P} of terms are identical to their untranslated term.", percentIdentical),
                    0));
            }

            // Check 5. Make sure there are no blank entries
            foreach (var row in language.Rows.Values)
            {
                if (string.IsNullOrEmpty(row.Value))
                {
                    language.Errors.Add(new LocalizationError(
                        LocalizationError.ErrorType.EmptyTerm,
                        string.Format("Key {0} is not translated.", row.Key),
                        row.RowNumber));
                }
            }
        }
    }
}
