﻿/*
 * Copyright 2010-2014 Bastian Eicher
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

using System.Windows.Forms;
using JetBrains.Annotations;
using NanoByte.Common.Controls;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    /// <summary>
    /// A wizard guiding the user through creating a new <see cref="Feed"/>.
    /// </summary>
    public sealed partial class NewFeedWizard : Wizard
    {
        private SignedFeed _signedFeed;
        private readonly FeedBuilder _feedBuilder;

        /// <summary>
        /// Runs the wizard.
        /// </summary>
        /// <param name="openPgp">Used to get a list of <see cref="OpenPgpSecretKey"/>s.</param>
        /// <param name="owner">The parent window the displayed window is modal to; can be <see langword="null"/>.</param>
        /// <returns>The feed generated by the wizard; <see langword="null"/> if the user canceled.</returns>
        [CanBeNull]
        public static SignedFeed Run([NotNull] IOpenPgp openPgp, [CanBeNull] IWin32Window owner = null)
        {
            using (var wizard = new NewFeedWizard(openPgp))
            {
                wizard.ShowDialog(owner);
                return wizard._signedFeed;
            }
        }

        private NewFeedWizard(IOpenPgp openPgp)
        {
            InitializeComponent();
            _feedBuilder = new FeedBuilder();

            // Pages
            var downloadPage = new DownloadPage(_feedBuilder);
            var archiveExtractPage = new ArchiveExtractPage(_feedBuilder);
            var installerPage = new InstallerPage();
            var entryPointPage = new EntryPointPage(_feedBuilder);
            var detailsPage = new DetailsPage(_feedBuilder);
            var iconPage = new IconPage(_feedBuilder);
            var securityPage = new SecurityPage(_feedBuilder, openPgp);
            var donePage = new DonePage();

            // Flows
            downloadPage.Archive += () => PushPage(archiveExtractPage);
            archiveExtractPage.Next += () => PushPage(entryPointPage);
            downloadPage.SingleFile += () => PushPage(detailsPage);
            downloadPage.Installer += () => PushPage(installerPage);
            entryPointPage.Next += () => PushPage(detailsPage);
            detailsPage.Next += () => PushPage(iconPage);
            iconPage.Next += () => PushPage(securityPage);
            securityPage.Next += () => PushPage(donePage);
            donePage.Finish += () =>
            {
                _feedBuilder.GenerateCommands();
                _signedFeed = _feedBuilder.Build();
                Close();
            };
            installerPage.Exit += Close;

            PushPage(downloadPage);
        }
    }
}
