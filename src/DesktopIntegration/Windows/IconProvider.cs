﻿/*
 * Copyright 2010-2011 Bastian Eicher
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
using System.Net;
using System.Security.Cryptography;
using Common;
using Common.Storage;
using Common.Tasks;
using Common.Utils;
using ZeroInstall.Model;
using ZeroInstall.Store.Feeds;

namespace ZeroInstall.DesktopIntegration.Windows
{
    /// <summary>
    /// Utility class for getting permanent icon copies.
    /// </summary>
    public static class IconProvider
    {
        /// <summary>
        /// Retreives a Windows icon via the <see cref="IIconCache"/> and stores a permanent copy of it.
        /// </summary>
        /// <param name="icon">The icon to retreive.</param>
        /// <param name="systemWide">Apply the configuration system-wide instead of just for the current user.</param>
        /// <param name="handler">A callback object used when the the user is to be informed about the progress of long-running operations such as downloads.</param>
        /// <returns>The path to the icon file.</returns>
        /// <exception cref="UserCancelException">Thrown if the user canceled the task.</exception>
        /// <exception cref="IOException">Thrown if a problem occurs while writing to the filesystem or registry.</exception>
        /// <exception cref="WebException">Thrown if a problem occured while downloading additional data (such as icons).</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if write access to the filesystem or registry is not permitted.</exception>
        public static string GetIconPath(Icon icon, bool systemWide, ITaskHandler handler)
        {
            #region Sanity checks
            if (handler == null) throw new ArgumentNullException("handler");
            #endregion

            string iconDirPath = Locations.GetIntegrationDirPath("0install.net", systemWide, "desktop-integration", "icons");
            string iconFilePath = Path.Combine(iconDirPath, StringUtils.Hash(icon.Location.ToString(), SHA256.Create()) + ".ico");

            // Return an existing icon or get a new one from the cache
            if (!File.Exists(iconFilePath))
                File.Copy(IconCacheProvider.CreateDefault().GetIcon(icon.Location, handler), iconFilePath, true);
            return iconFilePath;
        }
    }
}