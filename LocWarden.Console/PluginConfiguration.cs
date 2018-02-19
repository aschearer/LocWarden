using System.Configuration;

namespace LocWarden.Console
{
    public sealed class Plugin : ConfigurationElement
    {
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["Name"];
            }
        }
    }
}
