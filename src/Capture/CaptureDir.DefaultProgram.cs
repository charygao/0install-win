﻿/*
 * Copyright 2011 Bastian Eicher
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser Public License as Captureed by
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
using System.Globalization;
using System.IO;
using System.Security;
using Common;
using Common.Utils;
using Microsoft.Win32;
using ZeroInstall.Capture.Properties;
using ZeroInstall.Model;
using ZeroInstall.Model.Capabilities;
using Windows = ZeroInstall.DesktopIntegration.Windows;

namespace ZeroInstall.Capture
{
    public partial class CaptureDir
    {
        /// <summary>
        /// Collects data about default programs indicated by a snapshot diff.
        /// </summary>
        /// <param name="snapshotDiff">The elements added between two snapshots.</param>
        /// <param name="commandProvider">Provides best-match command-line to <see cref="Command"/> mapping.</param>
        /// <param name="capabilities">The capability list to add the collected data to.</param>
        /// <param name="appName">Is set to the name of the application as displayed to the user; unchanged if the name was not found.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static void CollectDefaultPrograms(Snapshot snapshotDiff, CommandProvider commandProvider, CapabilityList capabilities, ref string appName)
        {
            #region Sanity checks
            if (snapshotDiff == null) throw new ArgumentNullException("snapshotDiff");
            if (capabilities == null) throw new ArgumentNullException("capabilities");
            if (commandProvider == null) throw new ArgumentNullException("commandProvider");
            #endregion

            // Ambiguity warnings
            if (snapshotDiff.ServiceAssocs.Length > 1)
                Log.Warn(Resources.MultipleDefaultProgramsDetected);

            foreach (var serviceAssoc in snapshotDiff.ServiceAssocs)
            {
                string service = serviceAssoc.Key;
                string client = serviceAssoc.Value;

                using (var clientKey = Registry.LocalMachine.OpenSubKey(Windows.DefaultProgram.RegKeyMachineClients + @"\" + service + @"\" + client))
                {
                    if (clientKey == null) continue;

                    if (string.IsNullOrEmpty(appName)) appName = clientKey.GetValue("", "").ToString();
                    if (string.IsNullOrEmpty(appName)) appName = clientKey.GetValue(Windows.DefaultProgram.RegValueLocalizedName, "").ToString();

                    var defaultProgram = new DefaultProgram
                    {
                        ID = client,
                        Service = service
                    };
                    defaultProgram.Verbs.AddAll(GetVerbs(clientKey, commandProvider));
                    defaultProgram.InstallCommands = GetInstallCommands(clientKey, commandProvider.InstallationDir);
                    capabilities.Entries.Add(defaultProgram);
                }
            }
        }

        #region Install commands
        /// <summary>
        /// Retreives commands the application registered for use by Windows' "Set Program Access and Defaults".
        /// </summary>
        /// <param name="clientKey">The registry key containing the application's registration data.</param>
        /// <param name="installationDir">The fully qualified path to the installation directory.</param>
        /// <exception cref="IOException">Thrown if there was an error accessing the registry.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if read access to the registry was not permitted.</exception>
        /// <exception cref="SecurityException">Thrown if read access to the registry was not permitted.</exception>
        private static InstallCommands GetInstallCommands(RegistryKey clientKey, string installationDir)
        {
            #region Sanity checks
            if (clientKey == null) throw new ArgumentNullException("clientKey");
            if (string.IsNullOrEmpty(installationDir)) throw new ArgumentNullException("installationDir");
            #endregion

            using (var installInfoKey = clientKey.OpenSubKey(Windows.DefaultProgram.RegSubKeyInstallInfo))
            {
                if (installInfoKey == null) return default(InstallCommands);

                string reinstallArgs;
                string reinstall = IsolateCommand(installInfoKey.GetValue(Windows.DefaultProgram.RegValueReinstallCommand, "").ToString(), installationDir, out reinstallArgs);

                string showIconsArgs;
                string showIcons = IsolateCommand(installInfoKey.GetValue(Windows.DefaultProgram.RegValueShowIconsCommand, "").ToString(), installationDir, out showIconsArgs);

                string hideIconsArgs;
                string hideIcons = IsolateCommand(installInfoKey.GetValue(Windows.DefaultProgram.RegValueHideIconsCommand, "").ToString(), installationDir, out hideIconsArgs);

                return new InstallCommands
                {
                    Reinstall = reinstall, ReinstallArgs = reinstallArgs,
                    ShowIcons = showIcons, ShowIconsArgs = showIconsArgs,
                    HideIcons = hideIcons, HideIconsArgs = hideIconsArgs
                };
            }
        }

        /// <summary>
        /// Isolates the path of an executable specified in a command-line relative to a base directory.
        /// </summary>
        /// <param name="commandLine"></param>
        /// <param name="baseDir">The directory relative to which the executable is located.</param>
        /// <param name="additionalArguments">Returns any additional arguments found.</param>
        /// <returns>The path of the executable relative to <paramref name="baseDir"/> without any arguments.</returns>
        private static string IsolateCommand(string commandLine, string baseDir, out string additionalArguments)
        {
            if (!commandLine.StartsWith('"' + baseDir + '\\', true, CultureInfo.InvariantCulture))
            {
                additionalArguments = null;
                return null;
            }
            commandLine = commandLine.Substring(baseDir.Length + 2);

            additionalArguments = StringUtils.GetRightPartAtFirstOccurrence(commandLine, "\" ");
            return StringUtils.GetLeftPartAtFirstOccurrence(commandLine, '"');
        }
        #endregion
    }
}