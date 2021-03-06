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

using System;
using System.IO;
using ZeroInstall.Store;

namespace ZeroInstall.Publish.EntryPoints
{
    /// <summary>
    /// A script written in PHP.
    /// </summary>
    public sealed class PhpScript : InterpretedScript
    {
        /// <inheritdoc/>
        internal override bool Analyze(DirectoryInfo baseDirectory, FileInfo file)
        {
            if (!base.Analyze(baseDirectory, file)) return false;
            return
                file.Extension.StartsWith(@".php", StringComparison.OrdinalIgnoreCase) ||
                HasShebang(file, "php");
        }

        /// <inheritdoc/>
        protected override FeedUri InterpreterInterface => new FeedUri("http://0install.de/feeds/PHP.xml");
    }
}
