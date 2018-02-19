using System;

namespace LocWarden.Core
{
    /// <summary>
    /// Represents a row in a localization file.
    /// </summary>
    public interface ILocalizedText : IEquatable<ILocalizedText>
    {
        /// <summary>
        /// A unique key which identifies this text. It should be the same in all languages.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// A description for text. This is only used in the master language.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// The raw text which will appear in game.
        /// </summary>
        string Value { get; }

        /// <summary>
        /// The row in which this text appears in the source localization file. The first row is 1.
        /// </summary>
        int RowNumber { get; }

        /// <summary>
        /// Returns a list of format args present in the text's Value.
        /// </summary>
        string[] FormatArgs { get; }

        /// <summary>
        /// True when the text has format args but one of them is not properly formatted.
        /// </summary>
        bool HasOpenFormatArgs { get; }
    }
}
