# 1.6.0
Streamlined option naming and improved usability

## Features
- Made ClientID and TenantID optional when using 'WorkloadIdentity'.
- Unified Certificate name option to 'CertificateName' across all Azure tools for consistency.

## Bug Fixes
- None

# 1.5.0
Minor release extending signing certificate support

## Features
- Extended the AzureCreateSigningCert tool to support creating signing certificates locally signed by a KeyVault based CA certificate.

## Bug Fixes
- None

# 1.4.0
Minor release extending CA certificate support

## Features
- Added Client and Server Authentication OIDs to extended key usage in CA certificates by default
- Added a tool to create SSL server certificates signed by a Key Vault based CA certificate

## Bug Fixes
- None

# 1.3.0
Minor release

## Features
- Added support for Entra ID Managed Identity authentication

## Bug Fixes
- None

# 1.2.0
Minor release

## Features
- Retargeting to support Windows 11 24H2 and newer only

## Bug Fixes
- Fixed options mismatch in the command line interface

# 1.1.0
Minor release

## Features
- Added option to specify the expiration date of the certificate
- Added Azure Key Vault support

## Bug Fixes
- none

# 1.0.0
Initial release

## Features
- Added Code Signing Certificate Tools

## Bug Fixes
- none
