﻿/*
 * Copyright 2010-2016 Bastian Eicher
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

using JetBrains.Annotations;
using NanoByte.Common.Tasks;
using ZeroInstall.Commands.Properties;
using ZeroInstall.Store;

namespace ZeroInstall.Commands.CliCommands
{
    partial class CatalogMan
    {
        private class Remove : CatalogSubCommand
        {
            #region Metadata
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public new const string Name = "remove";

            protected override string Description => Resources.DescriptionCatalogRemove;

            protected override string Usage => "URI";

            protected override int AdditionalArgsMin => 1;

            protected override int AdditionalArgsMax => 1;

            public Remove([NotNull] ICommandHandler handler) : base(handler)
            {}
            #endregion

            public override ExitCode Execute()
            {
                var uri = new FeedUri(AdditionalArgs[0]);

                if (CatalogManager.RemoveSource(uri))
                    return ExitCode.OK;
                else
                {
                    Handler.OutputLow(Resources.CatalogSources, string.Format(Resources.CatalogNotRegistered, uri.ToStringRfc()));
                    return ExitCode.NoChanges;
                }
            }
        }
    }
}