// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

// This file is used by Code Analysis to maintain SuppressMessage attributes that are applied to this project.
// Project-level suppressions either have no target or are given a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Style", "IDE0130:Namespace does not match folder structure", Scope = "namespace", Target = "~N:CertTools.TestCore")]
[assembly: SuppressMessage("Design", "CA1031:Do not catch general exception types", Scope = "member", Target = "~M:CertTools.TestCore.KeyVaultFixture.System#IAsyncDisposable#DisposeAsync~System.Threading.Tasks.ValueTask")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Scope = "member", Target = "~M:CertTools.TestCore.KeyVaultFixture.CreateClientSecretCredential~Azure.Core.TokenCredential")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Scope = "member", Target = "~M:CertTools.TestCore.KeyVaultFixture.CreateWorkloadIdentityCredential~Azure.Core.TokenCredential")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Scope = "member", Target = "~M:CertTools.TestCore.KeyVaultFixture.CreateStandardKeyVaultUri~System.Uri")]
[assembly: SuppressMessage("Performance", "CA1822:Mark members as static", Scope = "member", Target = "~M:CertTools.TestCore.KeyVaultFixture.CreatePremiumKeyVaultUri~System.Uri")]
