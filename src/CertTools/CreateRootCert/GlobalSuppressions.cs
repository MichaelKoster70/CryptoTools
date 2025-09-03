// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Instantiated via CommandLineParser reflection.", Scope = "type", Target = "~T:CertTools.CreateRootCert.Options")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "by Design", Scope = "member", Target = "~M:CertTools.CreateRootCert.Program.Main(System.String[])")]
