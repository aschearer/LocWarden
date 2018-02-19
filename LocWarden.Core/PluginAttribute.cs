using System;

namespace LocWarden.Core
{
    /// <summary>
    /// Add metadata to a plugin which is used as documentation for end users.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class PluginAttribute : Attribute
    {
        public readonly string Name;

        public readonly string Description;

        public PluginAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
        }
    }
}
