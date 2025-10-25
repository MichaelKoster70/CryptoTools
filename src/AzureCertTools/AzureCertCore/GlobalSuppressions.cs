// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Major Code Smell", "S125:Sections of code should not be commented out", Justification = "Is a comment", Scope = "member", Target = "~M:CertTools.AzureCertCore.KeyVaultX509SignatureGenerator.GetSignatureAlgorithmIdentifier(System.Security.Cryptography.HashAlgorithmName)~System.Byte[]")]
[assembly: SuppressMessage("Design", "CA1056:URI-like properties should not be strings", Justification = "<Pending>", Scope = "member", Target = "~P:CertTools.AzureCertCore.OptionsBase.KeyVaultUri")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member", Target = "~M:CertTools.AzureCertCore.OptionsExtensions.PrintError(System.String)")]
