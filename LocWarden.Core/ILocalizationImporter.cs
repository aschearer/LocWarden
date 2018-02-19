namespace LocWarden.Core
{
    /// <summary>
    /// Defines a strategy to import localization files.
    /// </summary>
    public interface ILocalizationImporter
    {
        LocalizedLanguage Import(ILanguageDefinition languageDefinition, IPluginConfig config);
    }
}
