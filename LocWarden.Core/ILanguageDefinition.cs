namespace LocWarden.Core
{
    /// <summary>
    /// Defines a language file to be imported and processed.
    /// </summary>
    public interface ILanguageDefinition
    {
        /// <summary>
        /// The name of the language.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The path to the language file.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Whether the language file is the "master" file which is used
        /// as the source of truth during validation.
        /// </summary>
        bool IsMaster { get; }
    }
}
