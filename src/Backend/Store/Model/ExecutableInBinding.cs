﻿using System.ComponentModel;
using System.Xml.Serialization;
using JetBrains.Annotations;
using ZeroInstall.Store.Model.Design;

namespace ZeroInstall.Store.Model
{
    /// <summary>
    /// Make a chosen <see cref="Implementation"/> available as an executable at runtime.
    /// </summary>
    [XmlType("executable-in-binding", Namespace = Feed.XmlNamespace)]
    public abstract class ExecutableInBinding : Binding
    {
        /// <summary>
        /// The name of the <see cref="Command"/> in the <see cref="Implementation"/> to launch; leave <c>null</c> for <see cref="Store.Model.Command.NameRun"/>.
        /// </summary>
        [Description("The name of the command in the implementation to launch; leave empty for 'run'.")]
        [TypeConverter(typeof(CommandNameConverter))]
        [XmlAttribute("command"), DefaultValue(""), CanBeNull]
        public string Command { get; set; }

        #region Equality
        /// <inheritdoc/>
        protected bool Equals(ExecutableInBinding other)
        {
            if (other == null) return false;
            return base.Equals(other) && Command == other.Command;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            unchecked
            {
                int result = base.GetHashCode();
                result = (result * 397) ^ Command?.GetHashCode() ?? 0;
                return result;
            }
        }
        #endregion
    }
}
