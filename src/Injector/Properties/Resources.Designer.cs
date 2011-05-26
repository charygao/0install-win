﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.225
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ZeroInstall.Injector.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("ZeroInstall.Injector.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The output of the external solver could not be processed..
        /// </summary>
        internal static string ExternalSolverOutputErrror {
            get {
                return ResourceManager.GetString("ExternalSolverOutputErrror", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The feed &apos;{0}&apos; is not cached and Zero Install is currently in off-line mode..
        /// </summary>
        internal static string FeedNotCachedOffline {
            get {
                return ResourceManager.GetString("FeedNotCachedOffline", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No interface was specified..
        /// </summary>
        internal static string MissingInterfaceID {
            get {
                return ResourceManager.GetString("MissingInterfaceID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No commands in the selection..
        /// </summary>
        internal static string NoCommands {
            get {
                return ResourceManager.GetString("NoCommands", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to At least one implementation must be passed..
        /// </summary>
        internal static string NoImplementationsPassed {
            get {
                return ResourceManager.GetString("NoImplementationsPassed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No selected version.
        /// </summary>
        internal static string NoSelectedVersion {
            get {
                return ResourceManager.GetString("NoSelectedVersion", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to (not cached).
        /// </summary>
        internal static string NotCached {
            get {
                return ResourceManager.GetString("NotCached", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The interface &apos;{0}&apos; doesn&apos;t start with &apos;http:&apos; and doesn&apos;t exist as a file &apos;{1}&apos; either..
        /// </summary>
        internal static string NotInterfaceID {
            get {
                return ResourceManager.GetString("NotInterfaceID", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value is not a valid domain name..
        /// </summary>
        internal static string NotValidDomain {
            get {
                return ResourceManager.GetString("NotValidDomain", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There was a problem loading configuration values from &apos;{0}&apos;..
        /// </summary>
        internal static string ProblemLoadingConfig {
            get {
                return ResourceManager.GetString("ProblemLoadingConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Not compatible with current system.
        /// </summary>
        internal static string SelectionCandidateNoteIncompatibleSystem {
            get {
                return ResourceManager.GetString("SelectionCandidateNoteIncompatibleSystem", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Non-executable source implementation.
        /// </summary>
        internal static string SelectionCandidateNoteSource {
            get {
                return ResourceManager.GetString("SelectionCandidateNoteSource", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The solver encountered an unexpected problem..
        /// </summary>
        internal static string SolverProblem {
            get {
                return ResourceManager.GetString("SolverProblem", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The working directory has already been changed by a previous command..
        /// </summary>
        internal static string WokringDirDuplicate {
            get {
                return ResourceManager.GetString("WokringDirDuplicate", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The working directory contains an invalid paths (potentially a security risk)..
        /// </summary>
        internal static string WorkingDirInvalidPath {
            get {
                return ResourceManager.GetString("WorkingDirInvalidPath", resourceCulture);
            }
        }
    }
}
