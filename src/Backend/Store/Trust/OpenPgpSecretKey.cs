﻿/*
 * Copyright 2010-2014 Bastian Eicher, Simon E. Silva Lauinger
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
using NanoByte.Common.Utils;

namespace ZeroInstall.Store.Trust
{

    #region Enumerations
    /// <seealso cref="OpenPgpSecretKey.Algorithm"/>
    [SuppressMessage("Microsoft.Design", "CA1027:MarkEnumsWithFlags")]
    public enum OpenPgpAlgorithm
    {
        ///<summary>The algorithm used is unknown.</summary>
        Unknown = 0,

        /// <summary>RSA crypto system</summary>
        Rsa = 1,

        /// <summary>Elgamal crypto system</summary>
        Elgamal = 16,

        /// <summary>DAS crypto system</summary>
        Dsa = 17
    }
    #endregion

    /// <summary>
    /// Represents a secret key stored in a local <see cref="IOpenPgp"/> profile.
    /// </summary>
    public sealed class OpenPgpSecretKey : IEquatable<OpenPgpSecretKey>
    {
        #region Variables
        /// <summary>
        /// A unique identifier string for the key.
        /// </summary>
        public readonly string Fingerprint;

        /// <summary>
        /// A short identifier string for the key.
        /// </summary>
        public readonly string KeyID;

        /// <summary>
        /// The user's name, e-mail address, etc.
        /// </summary>
        public readonly string UserID;

        /// <summary>
        /// The point in time when the key was created in UTC.
        /// </summary>
        public readonly DateTime CreationTime;

        /// <summary>
        /// The encryption algorithm used.
        /// </summary>
        public readonly OpenPgpAlgorithm Algorithm;

        /// <summary>
        /// The length of key in bits.
        /// </summary>
        public readonly int BitLength;
        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new <see cref="IOpenPgp"/> secret key representation.
        /// </summary>
        /// <param name="fingerprint"> A unique identifier string for the key.</param>
        /// <param name="keyID">A short identifier string for the key.</param>
        /// <param name="userID">The user's name, e-mail address, etc.</param>
        /// <param name="creationTime">The point in time when the key was created in UTC.</param>
        /// <param name="algorithm">The encryption algorithm used.</param>
        /// <param name="bitLength">The length of key in bits.</param>
        public OpenPgpSecretKey(string fingerprint, string keyID, string userID, DateTime creationTime, OpenPgpAlgorithm algorithm, int bitLength)
        {
            Fingerprint = fingerprint;
            KeyID = keyID;
            UserID = userID;
            CreationTime = creationTime;
            Algorithm = algorithm;
            BitLength = bitLength;
        }
        #endregion

        #region Factory methods
        /// <summary>
        /// Creates a new <see cref="IOpenPgp"/> secret key representation from console lines.
        /// </summary>
        /// <param name="secLine">A 'sec' console line generated by "gpg --list-secret-keys --with-colons --fixed-list-mode --fingerprint".</param>
        /// <param name="fprLine">A 'fpr' console line generated by "gpg --list-secret-keys --with-colons --fixed-list-mode --fingerprint".</param>
        /// <param name="uidLine">A 'uid' console line generated by "gpg --list-secret-keys --with-colons --fixed-list-mode --fingerprint".</param>
        /// <returns>The parsed secret key representation.</returns>
        /// <exception cref="FormatException">Thrown if <paramref name="secLine"/>, <paramref name="fprLine"/> or <paramref name="uidLine"/> cannot be properly parsed.</exception>
        internal static OpenPgpSecretKey Parse(string secLine, string fprLine, string uidLine)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(secLine)) throw new ArgumentNullException("secLine");
            if (string.IsNullOrEmpty(fprLine)) throw new ArgumentNullException("fprLine");
            if (string.IsNullOrEmpty(uidLine)) throw new ArgumentNullException("uidLine");
            #endregion

            string[] sec = secLine.Split(':');
            if (sec.Length != 16) throw new FormatException("The 'sec' line must contain 16 colon-separated blocks.Was:\n" + secLine);
            if (sec[0] != "sec") throw new FormatException("The 'sec' line must start with \"sec\".");

            string[] fpr = fprLine.Split(':');
            if (fpr.Length != 11) throw new FormatException("The 'fpr' line must contain 11 colon-separated blocks.Was:\n" + fprLine);
            if (fpr[0] != "fpr") throw new FormatException("The 'fpr' line must start with \"fpr\".");

            string[] uid = uidLine.Split(':');
            if (uid.Length != 11) throw new FormatException("The 'uid' line must contain 11 colon-separated blocks.Was:\n" + uidLine);
            if (uid[0] != "uid") throw new FormatException("The 'uid' line must start with \"uid\".");

            return new OpenPgpSecretKey(
                fpr[9], sec[4], uid[9],
                FileUtils.FromUnixTime(long.Parse(sec[5])), (OpenPgpAlgorithm)int.Parse(sec[3]), int.Parse(sec[2]));
        }
        #endregion

        //--------------------//

        #region Conversion
        /// <inheritdoc/>
        public override string ToString()
        {
            return UserID ?? "";
        }
        #endregion

        #region Equality
        /// <inheritdoc/>
        public bool Equals(OpenPgpSecretKey other)
        {
            if (ReferenceEquals(null, other)) return false;

            return Equals(other.Fingerprint, Fingerprint);
        }

        /// <inheritdoc/>
        public static bool operator ==(OpenPgpSecretKey left, OpenPgpSecretKey right)
        {
            return Equals(left, right);
        }

        /// <inheritdoc/>
        public static bool operator !=(OpenPgpSecretKey left, OpenPgpSecretKey right)
        {
            return !Equals(left, right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is OpenPgpSecretKey && Equals((OpenPgpSecretKey)obj);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return (Fingerprint != null ? Fingerprint.GetHashCode() : 0);
        }
        #endregion
    }
}
