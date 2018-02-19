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

        public int GetInt(string setting, int defaultValue)
        {
            int result = defaultValue;
            try
            {
                result = int.Parse(config[setting]);
            }
            catch
            {
            }

            return result;
        }

        public string GetString(string setting)
        {
            return this.config[setting];
        }
    }
}
