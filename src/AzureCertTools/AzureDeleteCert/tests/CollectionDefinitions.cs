// ----------------------------------------------------------------------------
// <copyright company="Michael Koster">
//   Copyright (c) Michael Koster. All rights reserved.
//   Licensed under the MIT License.
// </copyright>
// ----------------------------------------------------------------------------

using System.Diagnostics.CodeAnalysis;
using CertTools.TestCore;
using Xunit;

namespace CertTools.AzureDeleteCert.Tests;

/// <summary>
/// Defines the shared Azure Key Vault test collection for AzureDeleteCert integration tests.
/// </summary>
[CollectionDefinition("KeyVault")]
[SuppressMessage("Maintainability", "CA1515:Consider making public types internal", Justification = "required for xUnit collection definition")]
public sealed class KeyVaultCollection : ICollectionFixture<KeyVaultFixture>;
