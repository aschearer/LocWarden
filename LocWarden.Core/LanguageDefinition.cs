namespace LocWarden.Core
{
    internal sealed class LanguageDefinition : ILanguageDefinition
    {
        public string Name { get; set; }

        public string Path { get; set; }

        public bool IsMaster { get; set; }

        public override string ToString()
        {
            return string.Format("LanguageDefinition({0},{1},{2})", this.Name, this.Path, this.IsMaster);
        }
    }
}
