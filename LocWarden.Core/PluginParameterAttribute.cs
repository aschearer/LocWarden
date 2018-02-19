using System;

namespace LocWarden.Core
{
    /// <summary>
    /// Describe what parameters a plugin expects. Used for runtime documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PluginParameterAttribute : Attribute
    {
        private const string DefaultType = "string";

        public readonly string Name;

        public readonly string Description;

        public readonly string Type;

        public readonly bool IsOptional;

        public PluginParameterAttribute(string name, string description)
        {
            this.Name = name;
            this.Description = description;
            this.Type = DefaultType;
        }

        public PluginParameterAttribute(string name, string description, bool isOptional)
        {
            this.Name = name;
            this.Description = description;
            this.IsOptional = isOptional;
            this.Type = DefaultType;
        }

        public PluginParameterAttribute(string name, string description, string type, bool isOptional)
        {
            this.Name = name;
            this.Description = description;
            this.Type = type;
            this.IsOptional = isOptional;
        }
    }
}
