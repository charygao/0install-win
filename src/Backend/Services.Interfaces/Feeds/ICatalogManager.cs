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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using JetBrains.Annotations;
using ZeroInstall.Store;
using ZeroInstall.Store.Model;
using ZeroInstall.Store.Trust;

namespace ZeroInstall.Services.Feeds
{
    /// <summary>
    /// Provides access to remote and local <see cref="Catalog"/>s. Handles downloading, signature verification and caching.
    /// </summary>
    public interface ICatalogManager
    {
        /// <summary>
        /// Loads the last result of <see cref="GetOnline"/>.
        /// </summary>
        /// <returns>A <see cref="Catalog"/>; <c>null</c> if there is no cached data.</returns>
        /// <exception cref="IOException">A problem occured while reading the cache file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to the cache file was not permitted.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "File system access")]
        [CanBeNull]
        Catalog GetCached();

        /// <summary>
        /// Downloads and merges all <see cref="Catalog"/>s specified by the configuration files.
        /// </summary>
        /// <returns>A <see cref="Catalog"/>.</returns>
        /// <exception cref="IOException">A problem occured while reading a local catalog file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a local catalog file was not permitted.</exception>
        /// <exception cref="WebException">A problem occured while fetching a remote catalog file.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        /// <exception cref="SignatureException">The signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
        [NotNull, SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Performs network IO and has side-effects")]
        Catalog GetOnline();

        /// <summary>
        /// Downloads and parses a remote catalog file. Mainly for internal use.
        /// </summary>
        /// <param name="source">The URL to download the catalog file from.</param>
        /// <returns>The parsed <see cref="Catalog"/>.</returns>
        /// <exception cref="WebException">A file could not be downloaded from the internet.</exception>
        /// <exception cref="SignatureException">The signature data of a remote catalog file could not be verified.</exception>
        /// <exception cref="InvalidDataException">A problem occurs while deserializing the XML data.</exception>
        [NotNull]
        Catalog DownloadCatalog([NotNull] FeedUri source);

        /// <summary>
        /// Adds a new source to download <see cref="Catalog"/> files from.
        /// </summary>
        /// <param name="uri">The URI of the source to add.</param>
        /// <returns><c>true</c> if the source was add; <c>false</c> if the source was already in the list.</returns>
        /// <exception cref="IOException">There was a problem accessing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
        bool AddSource([NotNull] FeedUri uri);

        /// <summary>
        /// Removes an existing source of <see cref="Catalog"/> files.
        /// </summary>
        /// <param name="uri">The URI of the source to remove.</param>
        /// <returns><c>true</c> if the source was removed; <c>false</c> if the source was not in the current list.</returns>
        /// <exception cref="IOException">There was a problem accessing a configuration file.</exception>
        /// <exception cref="UnauthorizedAccessException">Access to a configuration file was not permitted.</exception>
        /// <exception cref="UriFormatException">An invalid catalog source is specified in the configuration file.</exception>
        bool RemoveSource([NotNull] FeedUri uri);
    }
}
