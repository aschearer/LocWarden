namespace LocWarden.Core
{
    /// <summary>
    /// Configuration information for a plug-in.
    /// </summary>
    public interface IPluginConfig
    {
        string GetString(string setting);
    }
}
