# GitHub Copilot Instructions for CryptoTools

## Project Overview
This repository contains .NET 10 based crypto tools for creating x.509 based code signing certificates for development and testing purposes.

## Technology Stack
- **Framework**: .NET 10
- **Language**: C#
- **IDE**: Visual Studio 2026 with .NET 10 SDK
- **Runtime**: .NET 10 Runtime
- **Platform**: Windows 11 x64 24H2 or newer
- **Cryptography**: 4096-bit RSA with SHA384
- **Azure Integration**: Azure Key Vault, Azure Identity SDK
- **Authentication**: Service Principal, Managed Identity, Workload Identity Federation
- **CI/CD**: GitHub Actions

## Prerequisites
To work on this project, you need:
- Windows 11 x64 24H2 or newer
- Visual Studio 2026 with .NET 10 SDK
- .NET 10 Runtime installed

## Build Instructions
1. Clone the repository
2. Open the solution in Visual Studio 2026 in the [src](../src) folder
3. Build the solution using .NET 10 SDK

## Code Style Guidelines
- Follow standard C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

## Project Structure
- `src/CertTools` - Local certificate tools
- `src/AzureCertTools` - Azure Key Vault certificate tools
- `.github/workflows` - CI/CD pipeline definitions

## Testing
- Ensure all changes build successfully with .NET 10 SDK
- Test locally before committing
- Verify certificate creation functionality works as expected

## Azure Integration
The tools support Azure Key Vault integration with workload identity federation. When working with Azure features, ensure:
- Proper authentication mechanisms are in place
- Required Key Vault permissions are documented
- Support for WorkloadIdentity, Interactive, and ClientSecret authentication methods
