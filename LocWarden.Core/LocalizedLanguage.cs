using System;
using System.Collections.Generic;

namespace LocWarden.Core
{
    /// <summary>
    /// Represents a language to be validated.
    /// </summary>
    public sealed class LocalizedLanguage : IEquatable<LocalizedLanguage>
    {
        private ILanguageDefinition definition;

        /// <summary>
        /// The name of the language.
        /// </summary>
        public string Name
        {
            get { return this.definition.Name; }
        }

        /// <summary>
        /// The path to the language's file.
        /// </summary>
        public string Path
        {
            get { return this.definition.Path; }
        }

        /// <summary>
        /// Whether the language is the master language. The master language is 
        /// the source of truth when validating languages.
        /// </summary>
        public bool IsMasterLanguage
        {
            get { return this.definition.IsMaster; }
        }

        /// <summary>
        /// Map of text Key's to Text.
        /// </summary>
        public readonly Dictionary<string, ILocalizedText> Rows = new Dictionary<string, ILocalizedText>();

        /// <summary>
        /// List of errors encountered during validation for this language.
        /// </summary>
        public readonly List<LocalizationError> Errors = new List<LocalizationError>();

        public void AddText(string key, string description, string value, int rowNumber)
        {
            this.Rows.Add(key, new LocalizedText(key, description, value, rowNumber));
        }

        public LocalizedLanguage(ILanguageDefinition definition)
        {
            this.definition = definition;
        }

        public bool Equals(LocalizedLanguage other)
        {
            return this.Name == other.Name;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var localizedText = obj as LocalizedLanguage;
            return localizedText != null && localizedText.Name == this.Name;
        }

        public override string ToString()
        {
            return string.Format("Language({0})", this.Name);
        }
    }
}
