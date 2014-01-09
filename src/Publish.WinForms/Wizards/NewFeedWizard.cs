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

using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Common;
using Common.Controls;
using Common.Tasks;
using ZeroInstall.Model;
using ZeroInstall.Publish.EntryPoints;
using ZeroInstall.Publish.Properties;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Publish.WinForms.Wizards
{
    /// <summary>
    /// A wizard guiding the user through creating a new <see cref="Feed"/>.
    /// </summary>
    public sealed partial class NewFeedWizard : Wizard
    {
        private SignedFeed _signedFeed;
        private readonly FeedBuilder _feedBuilder = new FeedBuilder(new GuiTaskHandler());

        /// <summary>
        /// Runs the wizard.
        /// </summary>
        /// <param name="openPgp">Used to get a list of <see cref="OpenPgpSecretKey"/>s.</param>
        /// <param name="owner">The parent window the displayed window is modal to; may be <see langword="null"/>.</param>
        /// <returns>The feed generated by the wizard; <see langword="null"/> if the user cancelled.</returns>
        public static SignedFeed Run(IOpenPgp openPgp, IWin32Window owner)
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

            #region Pages
            var sourcePage = new SourcePage();

            var archivePage = new DownloadRetrievalMethodPage("Archive");
            var archiveOnlinePage = new DownloadRetrievalMethodOnlinePage<Archive>("Archive", _feedBuilder);
            var archiveLocalPage = new DownloadRetrievalMethodLocalPage<Archive>("Archive", _feedBuilder);
            var archiveExtractPage = new ArchiveExtractPage(_feedBuilder);

            var singleFilePage = new DownloadRetrievalMethodPage("Single Executable");
            var singleFileOnlinePage = new DownloadRetrievalMethodOnlinePage<SingleFile>("Single Executable", _feedBuilder);
            var singleFileLocalPage = new DownloadRetrievalMethodLocalPage<SingleFile>("Single Executable", _feedBuilder);

            var setupPage = new SetupPage();

            var entryPointPage = new EntryPointPage(_feedBuilder);
            var detailsPage = new DetailsPage(_feedBuilder);
            var iconPage = new IconPage(_feedBuilder);
            var securityPage = new SecurityPage(_feedBuilder, openPgp);
            var donePage = new DonePage();
            #endregion

            #region Flows
            sourcePage.Archive += () => PushPage(archivePage);
            sourcePage.SingleFile += () => PushPage(singleFilePage);
            sourcePage.Setup += () => PushPage(setupPage);

            archivePage.Online += () => PushPage(archiveOnlinePage);
            archivePage.Local += () => PushPage(archiveLocalPage);
            archiveOnlinePage.Next += () => PushPage(archiveExtractPage);
            archiveLocalPage.Next += () => PushPage(archiveExtractPage);
            archiveExtractPage.Next += () => PushPage(entryPointPage);

            singleFilePage.Online += () => PushPage(singleFileOnlinePage);
            singleFilePage.Local += () => PushPage(singleFileLocalPage);

            Action fileSelected = () =>
            {
                _feedBuilder.ImplementationDirectory = _feedBuilder.TemporaryDirectory;
                _feedBuilder.Candidate = Detection.ListCandidates(new DirectoryInfo(_feedBuilder.ImplementationDirectory)).FirstOrDefault();
                if (_feedBuilder.Candidate == null) Msg.Inform(this, Resources.UnknownExecutableType, MsgSeverity.Warn);
                else PushPage(detailsPage);
            };
            singleFileOnlinePage.Next += fileSelected;
            singleFileLocalPage.Next += fileSelected;

            entryPointPage.Next += () => PushPage(detailsPage);
            detailsPage.Next += () =>
            {
                var windowsExe = _feedBuilder.Candidate as WindowsExe;
                if (windowsExe == null) PushPage(securityPage);
                else
                {
                    iconPage.SetIcon(windowsExe.ExtractIcon());
                    PushPage(iconPage);
                }
            };
            iconPage.Next += () => PushPage(securityPage);
            securityPage.Next += () => PushPage(donePage);
            donePage.Finish += () =>
            {
                _signedFeed = _feedBuilder.Build();
                Close();
            };
            #endregion

            PushPage(sourcePage);
        }
    }
}
