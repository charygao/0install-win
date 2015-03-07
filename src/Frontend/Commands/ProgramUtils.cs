﻿/*
 * Copyright 2010-2015 Bastian Eicher
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
using NanoByte.Common;
using NanoByte.Common.Native;
using NanoByte.Common.Net;
using NanoByte.Common.Storage;
using ZeroInstall.DesktopIntegration;
using ZeroInstall.Store.Implementations;

namespace ZeroInstall.Commands
{
    /// <summary>
    /// Provides utiltity methods for application entry points.
    /// </summary>
    public static class ProgramUtils
    {
        /// <summary>
        /// Common initialization code to be called by every Frontend executable after start.
        /// </summary>
        public static void Startup()
        {
            if (WindowsUtils.IsWindows && !Locations.IsPortable && !StoreUtils.PathInAStore(Locations.InstallBase))
            {
                try
                {
                    RegistryUtils.SetSoftwareString("Zero Install", "InstallLocation", Locations.InstallBase);
                }
                catch (IOException)
                {}
                catch (UnauthorizedAccessException)
                {}
            }

            NetUtils.ApplyProxy();
            if (!WindowsUtils.IsWindows7) NetUtils.TrustCertificates(SyncIntegrationManager.DefaultServerPublicKey);
        }
    }
}
