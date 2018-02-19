namespace LocWarden.Core
{
    internal sealed class PluginDefinition
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Assembly { get; set; }

        public IPluginConfig Config { get; set; }

        public override string ToString()
        {
            return string.Format("PluginDefinition({0},{1},{2})", this.Name, this.Type, this.Assembly);
        }
    }
}
