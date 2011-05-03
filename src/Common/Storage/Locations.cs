﻿/*
 * Copyright 2006-2011 Bastian Eicher
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.IO;
using Common.Utils;

namespace Common.Storage
{
    /// <summary>
    /// Provides easy access to platform-specific common directories for storing settings and application data.
    /// </summary>
    /// <remarks>
    /// Uses <see cref="Environment.SpecialFolder"/> on Windows and the freedesktop.org basedir spec (XDG) on Linux.
    /// See http://freedesktop.org/wiki/Standards/basedir-spec
    /// </remarks>
    public static class Locations
    {
        #region Helpers
        /// <summary>
        /// Returns the value of an environment variable or a default value if it isn't set.
        /// </summary>
        /// <param name="variable">The name of the environment variable to retrieve.</param>
        /// <param name="defaultValue">The default value to return if the environment variable was not set.</param>
        /// <returns>The value of the environment variable or <paramref name="defaultValue"/>.</returns>
        private static string GetEnvironmentVariable(string variable, string defaultValue)
        {
            string value = Environment.GetEnvironmentVariable(variable);
            return (string.IsNullOrEmpty(value)) ? defaultValue : value;
        }
        #endregion

        #region Variables
        /// <summary>
        /// The directory the application binaries are located in without a trailing directory separator charachter.
        /// </summary>
        public static readonly string InstallBase = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);

        /// <summary>
        /// The directory used for storing files if <see cref="IsPortable"/> is <see langword="true"/>. Usuall the same as <see cref="InstallBase"/>
        /// </summary>
        public static readonly string PortableBase = InstallBase;

        /// <summary>
        /// Indicates whether the application is currently operating in portable mode.
        /// </summary>
        /// <remarks>
        ///   <para>Portable mode is activated by placing a file named "_portable" int the application's base directory.</para>
        ///   <para>When portable mode is active files are stored and loaded from the application's base directory instead of the user profile and sysem directories.</para>
        /// </remarks>
        public static readonly bool IsPortable = File.Exists(Path.Combine(PortableBase, "_portable"));
        #endregion

        #region Properties

        #region Per-user directories
        /// <summary>
        /// The home directory of the current user.
        /// </summary>
        public static string HomeDir
        { get { return Environment.GetFolderPath(Environment.SpecialFolder.Personal); } }

        /// <summary>
        /// The directory to store per-user settings that can roam across different machines.
        /// </summary>
        /// <remarks>On Windows this is <c>%appdata%</c>, on Linux it usually is <c>~/.config</c>.</remarks>
        public static string UserConfigDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CONFIG_HOME", Path.Combine(HomeDir, ".config"));

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }
            }
        }

        /// <summary>
        /// The directory to store per-user data files that do not roam across different machines.
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.local/share</c>.</remarks>
        public static string UserDataDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_DATA_HOME", Path.Combine(HomeDir, ".local/share"));

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
            }
        }

        /// <summary>
        /// The directory to store per-user cache data that does not roam across different machines.
        /// </summary>
        /// <remarks>On Windows this is <c>%localappdata%</c>, on Linux it usually is <c>~/.cache</c>.</remarks>
        public static string UserCacheDir
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CACHE_HOME", Path.Combine(HomeDir, ".cache"));

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                }
            }
        }
        #endregion

        #region System-wide directories
        /// <summary>
        /// The directories to store system-wide settings that can roam across different machines.
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/etc/xdg</c>.</remarks>
        public static string SystemConfigDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_CONFIG_DIRS", "/etc/xdg");

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                }
            }
        }

        /// <summary>
        /// The directories to store system-wide data files that do not roam across different machines.
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/usr/local/share:/usr/share</c>.</remarks>
        public static string SystemDataDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Use XDG specification
                        return GetEnvironmentVariable("XDG_DATA_DIRS", "/usr/local/share:/usr/share");

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                }
            }
        }

        /// <summary>
        /// The directories to store system-wide cache data that does not roam across different machines.
        /// </summary>
        /// <returns>Directories separated by <see cref="Path.PathSeparator"/> sorted by decreasing importance.</returns>
        /// <remarks>On Windows this is <c>CommonApplicationData</c>, on Linux it usually is <c>/var/cache</c>.</remarks>
        public static string SystemCacheDirs
        {
            get
            {
                switch (Environment.OSVersion.Platform)
                {
                    case PlatformID.MacOSX:
                        // ToDo: Use MacOS X-specific locations instead of POSIX subsytem

                    case PlatformID.Unix:
                        // Unoffical extension of the XDG specification
                        return GetEnvironmentVariable("XDG_CACHE_DIRS", "/var/cache");

                    default:
                    case PlatformID.Win32Windows:
                    case PlatformID.Win32NT:
                        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                }
            }
        }
        #endregion

        #endregion

        //--------------------//

        #region Paths
        /// <summary>
        /// Determines a path for storing a configuration resource that can roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="resource">The file or directory name of the resource to be stored.</param>
        /// <param name="isDirectory"><see langword="true"/> if the <paramref name="resource"/> is an entire directory instead of a single file.</param>
        /// <returns>A fully qualified path to use to store the resource.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <remarks>Any directories that are a part of the <paramref name="resource"/> are guaranteed to exist. Files are not.</remarks>
        public static string GetSaveConfigPath(string appName, string resource, bool isDirectory)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException("resource");
            #endregion

            string path = IsPortable
                ? StringUtils.PathCombine(PortableBase, "config", resource)
                : StringUtils.PathCombine(UserConfigDir, appName, resource);

            // Ensure the directory part of the path exists
            string dirPath = isDirectory ? path : (Path.GetDirectoryName(path) ?? path);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            return path;
        }

        /// <summary>
        /// Determines a list of paths for loading a configuration resource that can roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="resource">The file or directory name of the resource to be stored.</param>
        /// <param name="isDirectory"><see langword="true"/> if the <paramref name="resource"/> is an entire directory instead of a single file.</param>
        /// <returns>
        /// A list of fully qualified paths to use to load the resource sorted by decreasing importance.
        /// This list will always reflect the current state in the filesystem and can not be modified! It may be empty.
        /// </returns>
        /// <remarks>The returned paths are guaranteed to exist.</remarks>
        public static IEnumerable<string> GetLoadConfigPaths(string appName, string resource, bool isDirectory)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException("resource");
            #endregion

            string path;
            if (IsPortable)
            {
                // Check in portable base directory
                path = StringUtils.PathCombine(PortableBase, "config", resource);
                if ((!isDirectory && File.Exists(path)) || (isDirectory && Directory.Exists(path))) yield return path;
            }
            else
            {
                // Check in user profile
                path = StringUtils.PathCombine(UserConfigDir, appName, resource);
                if ((!isDirectory && File.Exists(path)) || (isDirectory && Directory.Exists(path))) yield return path;

                // Check in system directories
                foreach (var dirPath in SystemConfigDirs.Split(Path.PathSeparator))
                {
                    path = StringUtils.PathCombine(dirPath, appName, resource);
                    if ((!isDirectory && File.Exists(path)) || (isDirectory && Directory.Exists(path))) yield return path;
                }
            }
        }

        /// <summary>
        /// Determines a path for storing a data resource that does not roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="resource">The file or directory name of the resource to be stored.</param>
        /// <param name="isDirectory"><see langword="true"/> if the <paramref name="resource"/> is an entire directory instead of a single file.</param>
        /// <returns>A fully qualified path to use to store the resource.</returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <remarks>Any directories that are a part of the <paramref name="resource"/> are guaranteed to exist. Files are not.</remarks>
        public static string GetSaveDataPath(string appName, string resource, bool isDirectory)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException("resource");
            #endregion

            string path = IsPortable
                ? StringUtils.PathCombine(PortableBase, "data", resource)
                : StringUtils.PathCombine(UserDataDir, appName, resource);

            // Ensure the directory part of the path exists
            string dirPath = isDirectory ? path : (Path.GetDirectoryName(path) ?? path);
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            return path;
        }

        /// <summary>
        /// Determines a list of paths for loading a data resource that does not roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="resource">The file or directory name of the resource to be stored.</param>
        /// <param name="isDirectory"><see langword="true"/> if the <paramref name="resource"/> is an entire directory instead of a single file.</param>
        /// <returns>
        /// A list of fully qualified paths to use to load the resource sorted by decreasing importance.
        /// This list will always reflect the current state in the filesystem and can not be modified! It may be empty.
        /// </returns>
        /// <remarks>The returned paths are guaranteed to exist.</remarks>
        public static IEnumerable<string> GetLoadDataPaths(string appName, string resource, bool isDirectory)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException("resource");
            #endregion

            string path;
            if (IsPortable)
            {
                // Check in portable base directory
                path = StringUtils.PathCombine(PortableBase, "data", resource);
                if ((!isDirectory && File.Exists(path)) || (isDirectory && Directory.Exists(path))) yield return path;
            }
            else
            {
                // Check in user profile
                path = StringUtils.PathCombine(UserDataDir, appName, resource);
                if ((!isDirectory && File.Exists(path)) || (isDirectory && Directory.Exists(path))) yield return path;

                // Check in system directories
                foreach (var dirPath in SystemDataDirs.Split(Path.PathSeparator))
                {
                    path = StringUtils.PathCombine(dirPath, appName, resource);
                    if ((!isDirectory && File.Exists(path)) || (isDirectory && Directory.Exists(path))) yield return path;
                }
            }
        }

        /// <summary>
        /// Determines a list of paths for cache directories that do not roam across different machines.
        /// </summary>
        /// <param name="appName">The name of application. Used as part of the path, unless <see cref="IsPortable"/> is <see langword="true"/>.</param>
        /// <param name="resource">The directory name of the resource to be stored.</param>
        /// <returns>
        /// A list of fully qualified directory path sorted by decreasing importance.
        /// This list will always reflect the current state in the filesystem and can not be modified! It is never empty.
        /// </returns>
        /// <exception cref="IOException">Thrown if a problem occurred while creating a directory.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if creating a directory is not permitted.</exception>
        /// <remarks>The returned directories are guaranteed to exist. At least the first directory is usually writable.</remarks>
        public static IEnumerable<string> GetCachePath(string appName, string resource)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(appName)) throw new ArgumentNullException("appName");
            if (string.IsNullOrEmpty(resource)) throw new ArgumentNullException("resource");
            #endregion

            string path;
            if (IsPortable)
            {
                // Create in portable base directory
                path = StringUtils.PathCombine(PortableBase, "cache", resource);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                yield return path;
            }
            else
            {
                // Create in user profile
                path = StringUtils.PathCombine(UserCacheDir, appName, resource);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                yield return path;

                // Check in system directories
                foreach (var dirPath in SystemCacheDirs.Split(Path.PathSeparator))
                {
                    path = StringUtils.PathCombine(dirPath, appName, resource);
                    if (Directory.Exists(path)) yield return path;
                }
            }
        }
        #endregion
    }
}
