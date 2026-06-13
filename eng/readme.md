# 'eng' directory
## Directory Structure
### 'actions'
- This directory is being used by GitHub Actions only and holds custom actions.
### 'IaC'
- This directory holds Bicep files for infrastructure as code (IaC) deployments. The Bicep files in this directory include:
  - `main.bicep`: This is the main Bicep file that orchestrates the deployment of all resources. It references other Bicep files to create the necessary infrastructure for the project.
  - `resource-group.bicep`: This file defines the resource group that will be used for the project, including its location and tags.
  - `resource-group-applylock.bicep`: This file applies a lock to the resource group to prevent accidental deletion.
