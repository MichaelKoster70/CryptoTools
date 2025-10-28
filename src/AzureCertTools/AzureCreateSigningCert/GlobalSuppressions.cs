// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via CommandLineParser reflection.", Scope = "type", Target = "~T:CertTools.AzureCreateSigningCert.Options")]
[assembly: SuppressMessage("CodeSmell", "S1075:Refactor your code not to use hardcoded absolute paths or URIs", Justification = "Hardcoded URI is required for authentication redirect.", Scope = "member", Target = "~M:CertTools.AzureCreateSigningCert.Program.Main(System.String[])")]
[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "By design Windows only", Scope = "member", Target = "~M:CertTools.AzureCreateSigningCert.CertificateWorker.LocalCreateSigningCertificateAsync(System.String,System.String,System.String,System.String,System.Uri,Azure.Core.TokenCredential,System.Int32)~System.Threading.Tasks.Task")]
