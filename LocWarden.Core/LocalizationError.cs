namespace LocWarden.Core
{
    /// <summary>
    /// An error found with the translation.
    /// </summary>
    public sealed class LocalizationError
    {
        /// <summary>
        /// The type of error.
        /// </summary>
        public readonly ErrorType Error;

        /// <summary>
        /// A message describing the error.
        /// </summary>
        public readonly string Message;

        /// <summary>
        /// What line the error occurs on in the translation file.
        /// </summary>
        public readonly int LineNumber;

        public LocalizationError(ErrorType error, string message, int lineNumber)
        {
            this.Error = error;
            this.Message = message;
            this.LineNumber = lineNumber;
        }

        public enum ErrorType
        {
            KeysMissing,
            KeysAdded,
            KeysNotInOrder,
            FormatArgsAdded,
            FormatArgsOpen,
            FormatArgMissing,
            EmptyTerm,
            FileError,
            PluginError,
        }
    }
}
