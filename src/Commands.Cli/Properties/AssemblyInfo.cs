﻿using System;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Permissions;

// Assembly info
[assembly: AssemblyTitle("Zero Install Command CLI")]
[assembly: AssemblyDescription("A command-line interface for Zero Install, for installing and launching applications, managing caches, etc.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("0install.de")]
[assembly: AssemblyProduct("Zero Install")]
[assembly: AssemblyCopyright("Copyright © Bastian Eicher 2010-2011")]
[assembly: NeutralResourcesLanguage("en")]

// Version information
[assembly: AssemblyVersion("0.54.3")]
[assembly: AssemblyFileVersion("0.54.3")]

// Security settings
[assembly: FileIOPermission(SecurityAction.RequestMinimum, Unrestricted = true)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode = true)]
[assembly: CLSCompliant(true)]
[assembly: ComVisible(false)]
