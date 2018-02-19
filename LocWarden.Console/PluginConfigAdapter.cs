using Microsoft.Extensions.Configuration;

namespace LocWarden.Core
{
    internal sealed class PluginConfigAdapter : IPluginConfig
    {
        private IConfigurationSection config;

        public PluginConfigAdapter(IConfigurationSection config)
        {
            this.config = config;
        }

        public string GetString(string setting)
        {
            return this.config[setting];
        }
    }
}
