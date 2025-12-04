# GitHub Copilot Instructions for CryptoTools

This file provides guidance to GitHub Copilot Agent when working with code in this repository.

## Project Overview

CryptoTools is a .NET 8-based toolset for creating X.509 certificates for development and testing purposes. The project includes:
- Local certificate creation tools (CertTools)
- Azure Key Vault-based certificate creation tools (AzureCertTools)

## Repository Structure

```
src/
├── CertTools/              # Local certificate tools
│   ├── CertCore/          # Shared certificate logic
│   ├── CreateRootCert/    # Root CA certificate creation
│   └── CreateSigningCert/ # Code signing certificate creation
└── AzureCertTools/         # Azure Key Vault certificate tools
    ├── AzureCertCore/     # Shared Azure certificate logic
    ├── AzureCreateRootCert/
    ├── AzureCreateIntermediateCert/
    ├── AzureCreateSigningCert/
    └── AzureCreateSslServerCert/
```

## Technology Stack

- **Language**: C# with .NET 8
- **Cryptography**: 4096-bit RSA with SHA384
- **Azure Integration**: Azure Key Vault, Azure Identity SDK
- **Authentication**: Service Principal, Managed Identity, Workload Identity Federation
- **Build System**: .NET CLI (dotnet restore, dotnet build, dotnet publish)
- **CI/CD**: GitHub Actions

## Build Instructions

### Prerequisites
- Windows 11 x64 24H2 or newer (for development)
- Visual Studio 2022 17.8+ with .NET 8 SDK
- .NET 8 Runtime

### Build Commands

```powershell
# Restore dependencies for CertTools
dotnet restore src/CertTools/CertTools.sln

# Build CertTools
dotnet build src/CertTools/CertTools.sln --configuration Release

# Restore dependencies for AzureCertTools
dotnet restore src/AzureCertTools/AzureCertTools.sln

# Build AzureCertTools
dotnet build src/AzureCertTools/AzureCertTools.sln --configuration Release

# Publish individual tools (example)
dotnet publish src/CertTools/CreateRootCert/CreateRootCert.csproj --configuration Release
```

### CI/CD Workflow
- CI builds run on `feature/**` and `bugfix/**` branches
- Release builds run on `main` branch and tags
- Builds use GitVersion for semantic versioning
- Artifacts are signed when publishing releases

## Development Guidelines

### Code Conventions
- Follow standard .NET naming conventions
- Use PascalCase for public members
- Use camelCase for private fields with `_` prefix
- Command-line tools use `--ParameterName` style arguments

### Security Best Practices
- Never hard-code secrets or credentials
- Use Azure Key Vault for production certificates
- Always validate certificate parameters before creation
- Implement proper certificate expiration handling
- Use strong RSA key sizes (4096 bits minimum)

### Azure Integration
- Support multiple authentication methods: Service Principal, Interactive, Workload Identity
- Workload Identity requires environment variables:
  - `AZURE_CLIENT_ID`
  - `AZURE_TENANT_ID`
  - `AZURE_FEDERATED_TOKEN_FILE`
- Required Azure Key Vault permissions:
  - `Microsoft.KeyVault/vaults/keys/sign/action`
  - `Microsoft.KeyVault/vaults/certificates/read`
  - `Microsoft.KeyVault/vaults/certificates/create/action`

### Testing
- Test certificate creation with various parameters
- Test Azure authentication methods separately
- Validate certificate properties after creation
- Test error handling for invalid inputs

## Common Tasks

### Adding a New Certificate Tool
1. Create new project in appropriate solution (CertTools or AzureCertTools)
2. Reference CertCore or AzureCertCore for shared logic
3. Implement command-line parsing with clear parameter names
4. Add appropriate error handling and validation
5. Update README.md with usage instructions
6. Add publish configuration to CI/CD workflow

### Modifying Certificate Creation Logic
- Core certificate logic lives in CertCore/AzureCertCore projects
- Validate cryptographic parameters meet security standards
- Test with different certificate types and configurations
- Consider backward compatibility with existing certificates

### Azure Key Vault Integration Changes
- Use Azure.Identity SDK for authentication
- Support all three authentication methods consistently
- Handle Azure SDK exceptions appropriately
- Test with actual Azure Key Vault when possible

## Project-Specific Notes

- This is a **Windows-only** project due to certificate store dependencies in CertTools
- Azure tools can potentially run cross-platform but are primarily tested on Windows
- Version numbers follow semantic versioning via GitVersion
- All tools are self-contained single-file executables when published
- Code signing is applied to release builds using Sectigo timestamp server

## References

- [Azure Workload Identity Federation](https://learn.microsoft.com/en-us/entra/workload-id/workload-identity-federation)
- [Azure Identity SDK for .NET](https://learn.microsoft.com/en-us/dotnet/api/azure.identity.workloadidentitycredential)
- [X.509 Certificate Standards](https://datatracker.ietf.org/doc/html/rfc5280)
