﻿/*
 * Copyright 2010-2013 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Reflection;
using Common.Info;
using Common.Utils;
using ZeroInstall.Model;

namespace ZeroInstall.Publish.EntryPoints
{
    public enum DotNetRuntimeVersion
    {
        V20,
        V30,
        V35,
        V40,
        V45
    }

    public enum DotNetRuntimeType
    {
        Any,
        MicrosoftOnly,
        MonoOnly
    }

    public sealed class DotNetExe : Candidate
    {
        /// <inheritdoc/>
        internal override bool Analyze(FileInfo file)
        {
            #region Sanity checks
            if (file == null) throw new ArgumentNullException("file");
            #endregion

            if (!base.Analyze(file)) return false;
            if (!StringUtils.EqualsIgnoreCase(file.Extension, ".exe")) return false;

            try
            {
                var assembly = Assembly.ReflectionOnlyLoadFrom(file.FullName);
                Parse(assembly);
                // TODO: Unload assembly
                return true;
            }
                #region Error handling
            catch (IOException)
            {
                return false;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
            #endregion
        }

        private void Parse(Assembly assembly)
        {
            RuntimeVersion = GetRuntimeVersion(assembly.ImageRuntimeVersion);
            var appInfo = AppInfo.Load(assembly);
            Name = appInfo.Name;
            Description = appInfo.Description;
            Version = new ImplementationVersion(appInfo.Version);
        }

        private static DotNetRuntimeVersion GetRuntimeVersion(string version)
        {
            switch (version)
            {
                case "v2.0.50727":
                    return DotNetRuntimeVersion.V20;
                case "v3.0":
                    return DotNetRuntimeVersion.V30;
                case "v3.5":
                    return DotNetRuntimeVersion.V35;
                default:
                case "v4.0.30319":
                    return DotNetRuntimeVersion.V40;
            }
        }

        public DotNetRuntimeVersion RuntimeVersion { get; set; }

        public DotNetRuntimeType RuntimeType { get; set; }

        public bool NeedsMonoPath { get; set; }

        public override Runner Runner
        {
            get
            {
                return null;
            }
        }

        #region Equality
        private bool Equals(DotNetExe other)
        {
            return
                base.Equals(other) &&
                RuntimeVersion == other.RuntimeVersion &&
                RuntimeType == other.RuntimeType &&
                NeedsMonoPath == other.NeedsMonoPath;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is DotNetExe && Equals((DotNetExe)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)RuntimeVersion;
                hashCode = (hashCode * 397) ^ (int)RuntimeType;
                hashCode = (hashCode * 397) ^ NeedsMonoPath.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}