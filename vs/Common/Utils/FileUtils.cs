﻿/*
 * Copyright 2006-2010 Bastian Eicher, Simon E. Silva Lauinger
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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using Common.Properties;
using Mono.Unix;

namespace Common.Utils
{
    /// <summary>
    /// Provides filesystem-related helper methods.
    /// </summary>
    public static class FileUtils
    {
        #region Hash
        /// <summary>
        /// Computes the hash value of the content of a file.
        /// </summary>
        /// <param name="path">The path of the file to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A hexadecimal string representation of the hash value.</returns>
        public static string ComputeHash(string path, HashAlgorithm algorithm)
        {
            #region Sanity checks
            if (path == null) throw new ArgumentNullException("path");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            #endregion

            using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                return ComputeHash(stream, algorithm);
        }

        /// <summary>
        /// Computes the hash value of the content of a stream.
        /// </summary>
        /// <param name="stream">The stream containing the data to hash.</param>
        /// <param name="algorithm">The hashing algorithm to use.</param>
        /// <returns>A hexadecimal string representation of the hash value.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase", Justification = "The returned characters are only 0-9 and A-F")]
        public static string ComputeHash(Stream stream, HashAlgorithm algorithm)
        {
            #region Sanity checks
            if (stream == null) throw new ArgumentNullException("stream");
            if (algorithm == null) throw new ArgumentNullException("algorithm");
            #endregion
            
            return BitConverter.ToString(algorithm.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }
        #endregion

        #region Time
        /// <summary>
        /// Converts a <see cref="DateTime"/> into the number of seconds since the Unix epoch (1970-1-1).
        /// </summary>
        public static long UnixTime(DateTime time)
        {
            TimeSpan timepan = (time - new DateTime(1970, 1, 1));
            return (long)timepan.TotalSeconds;
        }

        /// <summary>
        /// Determines the accuracy with which the filesystem underlying a specific directory can store file-changed times.
        /// </summary>
        /// <param name="path">The path of the directory to check.</param>
        /// <returns>The accuracy in number of seconds. (i.e. 0 = perfect, 1 = may be off by up to one second)</returns>
        /// <exception cref="DirectoryNotFoundException">Thrown if the specified directory doesn't exist.</exception>
        /// <exception cref="IOException">Thrown if writing to the directory fails.</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to write to the directory.</exception>
        public static int DetermineTimeAccuracy(string path)
        {
            // Prepare a file name and fake change time
            var referenceTime = new DateTime(2000, 1, 1, 0, 0, 1); // 1 second past mid-night on 1st of January 2000
            string tempFile = Path.Combine(path, Path.GetRandomFileName());

            File.WriteAllText(tempFile, @"a");
            File.SetLastWriteTimeUtc(tempFile, referenceTime);
            var resultTime = File.GetLastWriteTimeUtc(tempFile);
            File.Delete(tempFile);

            return Math.Abs((resultTime - referenceTime).Seconds);
        }
        #endregion

        #region Temp
        /// <summary>
        /// Creates a uniquely named, empty temporary directory on disk and returns the full path of that directory.
        /// </summary>
        /// <returns>The full path of the temporary directory.</returns>
        /// <exception cref="IOException">Thrown if an IO error occurred, such as no unique temporary directory name is available.</exception>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Delivers a new value on each call")]
        public static string GetTempDirectory()
        {
            string tempPath = Path.GetTempFileName();
            File.Delete(tempPath);
            Directory.CreateDirectory(tempPath);
            return tempPath;
        }
        #endregion

        #region Copy
        /// <summary>
        /// Copies the content of a directory to a new location preserving the original file and directory modification times.
        /// </summary>
        /// <param name="sourcePath">The path of source directory. Must exist!</param>
        /// <param name="destinationPath">The path of the target directory. Must not exist!</param>
        /// <param name="overwrite">Overwrite exisiting files and directories at the <paramref name="destinationPath"/>.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="sourcePath"/> and <paramref name="destinationPath"/> are equal.</exception>
        /// <exception cref="DirectoryNotFoundException">Thrown if <paramref name="sourcePath"/> does not exist.</exception>
        /// <exception cref="IOException">Thrown if <paramref name="destinationPath"/> already exists and <paramref name="overwrite"/> is <see langword="false"/>.</exception>
        public static void CopyDirectory(string sourcePath, string destinationPath, bool overwrite)
        {
            #region Sanity checks
            if (string.IsNullOrEmpty(sourcePath)) throw new ArgumentNullException("sourcePath");
            if (string.IsNullOrEmpty(destinationPath)) throw new ArgumentNullException("destinationPath");
            if (sourcePath == destinationPath) throw new ArgumentException(Resources.SourceDestinationEqual);
            if (!Directory.Exists(sourcePath)) throw new DirectoryNotFoundException(Resources.SourceDirNotExist);
            if (!overwrite && Directory.Exists(destinationPath)) throw new IOException(Resources.DestinationDirExist);
            #endregion

            if (!Directory.Exists(destinationPath)) Directory.CreateDirectory(destinationPath);

            foreach (string entry in Directory.GetFileSystemEntries(sourcePath))
            {
                string destinationFilePath = Path.Combine(destinationPath, Path.GetFileName(entry) ?? "");
                if (Directory.Exists(entry))
                {
                    // Recurse into sub-direcories
                    CopyDirectory(entry, destinationFilePath, overwrite);
                }
                else
                {
                    // Copy individual files
                    File.Copy(entry, destinationFilePath, overwrite);
                }
            }

            // Set directory write time as last step, since file changes within the dir will reset the value
            Directory.SetLastWriteTimeUtc(destinationPath, Directory.GetLastWriteTimeUtc(sourcePath));
        }
        #endregion

        #region Permssions
        /// <summary>
        /// Uses whatever means the current platform provides to prevent further write access to a directory (read-only attribute, ACLs, Unix octals, etc.).
        /// </summary>
        /// <remarks>May do nothing if the platform doesn't provide any known protection mechanisms.</remarks>
        /// <param name="path">The directory to protect.</param>
        /// <exception cref="UnauthorizedAccessException">Thrown if you have insufficient rights to apply the write protection.</exception>
        public static void WriteProtection(string path)
        {
            var dirInfo = new DirectoryInfo(path);

            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    // ToDo: Set Unix octals
                    break;

                case PlatformID.Win32Windows:
                    // ToDo: Run for each contained file: "fileInfo.IsReadOnly = true;"
                    break;

                case PlatformID.Win32NT:
                    DirectorySecurity security = dirInfo.GetAccessControl();
                    security.AddAccessRule(new FileSystemAccessRule(new SecurityIdentifier("S-1-1-0"), FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny));
                    dirInfo.SetAccessControl(security);
                    break;
            }
        }
        #endregion

        #region Unix
        /// <summary>
        /// <see langword="true"/> if the current operating system is a Unix-like system (e.g. Linux or MacOS X).
        /// </summary>
        public static bool IsUnix
        { get { return Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX; } }

        /// <summary>
        /// Checks whether a file is a regular file (i.e. not a device file, symbolic link, etc.).
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a regular file; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files.</remarks>
        public static bool IsRegularFile(string path)
        {
            // ToDo: Detect special files on Windows

            if (!File.Exists(path)) return false;
            if (!IsUnix) return true;
            return new UnixFileInfo(path).IsRegularFile;
        }

        /// <summary>
        /// Checks whether a file is a Unix symbolic link.
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to a symbolic link; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files. Will always return <see langword="false"/> on non-Unix-like systems.</remarks>
        public static bool IsSymlink(string path, out string contents, out long length)
        {
            bool result = File.Exists(path) && IsUnix && UnixFileSystemInfo.GetFileSystemEntry(path).IsSymbolicLink;
            
            if (result)
            {
                var symlinkInfo = new UnixSymbolicLinkInfo(path);
                length = symlinkInfo.Length;
                contents = symlinkInfo.ContentsPath;
            }
            else
            {
                contents = null;
                length = 0;
            }

            return result;
        }

        /// <summary>A combination of bit flags to grant everyone executing permissions.</summary>
        private const FileAccessPermissions AllExecutePermission = FileAccessPermissions.UserExecute | FileAccessPermissions.GroupExecute | FileAccessPermissions.OtherExecute;
        
        /// <summary>
        /// Checks whether a file is marked as Unix-executable.
        /// </summary>
        /// <return><see lang="true"/> if <paramref name="path"/> points to an executable; <see lang="false"/> otherwise.</return>
        /// <remarks>Will return <see langword="false"/> for non-existing files. Will always return <see langword="false"/> on non-Unix-like systems.</remarks>
        public static bool IsExecutable(string path)
        {
            if (!File.Exists(path) || !IsUnix) return false;

            // Check if any execution rights are set
            var fileInfo = new UnixFileInfo(path);
            return ((fileInfo.FileAccessPermissions & AllExecutePermission) > 0);
        }

        /// <summary>
        /// Marks a file as Unix-executable or not Unix-executable. Only works on Unix-like systems!
        /// </summary>
        /// <param name="path">The file to mark as executable or not executable.</param>
        /// <param name="executable"><see lang="true"/> to mark the file as executable, <see lang="true"/> to mark it as not executable.</param>
        /// <exception cref="FileNotFoundException">Thrown if <paramref name="path"/> points to a file that does not exist or cannot be accessed.</exception>
        /// <exception cref="PlatformNotSupportedException">Thrown if this method is called on a non-Unix-like system.</exception>
        public static void SetExecutable(string path, bool executable)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("", path);
            if (!IsUnix) throw new PlatformNotSupportedException();

            // Set or unset all execution rights
            var fileInfo = new UnixFileInfo(path);
            if (executable) fileInfo.FileAccessPermissions = fileInfo.FileAccessPermissions | AllExecutePermission;
            else fileInfo.FileAccessPermissions = fileInfo.FileAccessPermissions & ~AllExecutePermission;
        }
        #endregion
    }
}
