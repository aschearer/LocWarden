using System.Collections.Generic;

namespace LocWarden.Core
{
    internal sealed class LocalizedText : ILocalizedText
    {
        private string[] formatArgs;

        public LocalizedText(string key, string description, string value, int rowNumber)
        {
            this.Key = key;
            this.Description = description;
            this.Value = value;
            this.RowNumber = rowNumber;
        }

        public string Key { get; set; }

        public string Description { get; set; }

        public string Value { get; set; }

        public int RowNumber { get; set; }

        public string[] FormatArgs
        {
            get
            {
                if (this.formatArgs == null)
                {
                    List<string> args = new List<string>();
                    var text = this.Value;
                    int argStart = text.IndexOf(Constants.ArgStart);
                    int argStop = text.IndexOf(Constants.ArgStop);
                    while (argStart >= 0)
                    {
                        if (argStop < 0)
                        {
                            // We have a problem
                            this.HasOpenFormatArgs = true;
                            break;
                        }

                        args.Add(text.Substring(argStart, argStop - argStart + Constants.ArgStop.Length));

                        text = text.Substring(argStop + 1);
                        argStart = text.IndexOf(Constants.ArgStart);
                        argStop = text.IndexOf(Constants.ArgStop);
                    }

                    this.formatArgs = args.ToArray();
                }

                return this.formatArgs;
            }
        }

        public bool HasOpenFormatArgs { get; set; }

        public bool Equals(ILocalizedText other)
        {
            return this.Key == other.Key;
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var localizedText = obj as LocalizedText;
            return localizedText != null && localizedText.Key == this.Key;
        }

        public override string ToString()
        {
            return string.Format("Row({0})", this.Key);
        }
    }
}
