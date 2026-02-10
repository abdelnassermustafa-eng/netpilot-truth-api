![.NET](https://img.shields.io/badge/.NET-10.0-blue)
![Platform](https://img.shields.io/badge/platform-macOS%20%7C%20Windows%20%7C%20Linux-lightgrey)
![License](https://img.shields.io/badge/license-MIT-green)
![Release](https://img.shields.io/github/v/release/abdelnassermustafa-eng/netpilot-truth-api)

# NetPilot Truth Platform

A truth-first inspection and validation platform for cloud and infrastructure environments.

The NetPilot Truth Platform is built around a simple engineering principle:

> **Visibility and validation must come before automation.**

Instead of blindly applying changes, the system first:

1. Inspects the environment
2. Validates configuration
3. Explains the real state
4. Then enables safe, controlled actions

---

## Project Components

This repository contains two main components:

### 1. TruthApi (Backend)
A secure, read-only .NET Web API that:

- Inspects infrastructure and cloud environments
- Provides validation results
- Exposes structured, consistent API responses
- Enforces authentication and role-based access

### 2. TruthDoctor (Desktop Tool)
A cross-platform engineering desktop application that:

- Connects to the TruthApi
- Authenticates securely
- Retrieves validation results
- Displays structured environment health data
- Provides a safe, read-only troubleshooting interface

---

## Current Capabilities (v1.0)

### TruthApi
- Controller-based architecture
- Service-layer separation
- Dependency Injection
- Configuration-driven behavior
- Structured logging
- Standardized API response model
- Global exception handling
- JWT authentication
- Role-based access control
- Secure validation endpoints

### TruthDoctor Desktop Tool
- Login with API authentication
- Secure token-based session
- Automatic validation retrieval
- Structured validation window
- Cross-platform builds:
  - macOS (ARM & Intel)
  - Linux
  - Windows

---

## Architecture Philosophy

The system follows a **truth-first model**:
Truth → Validation → Automation

This ensures:

- Safer operations
- Clear diagnostics
- Reduced risk of destructive actions
- Better understanding before changes

---

## Getting Started

### Prerequisites

- .NET SDK 10 or later

Check your installation:

dotnet --version

---

## Running from Source

Clone the repository:

git clone https://github.com/abdelnassermustafa-eng/netpilot-truth-api.git
cd netpilot-truth-api

Build the solution:

dotnet build

Run the API:

cd TruthApi
dotnet run

Test the API:

curl http://localhost:5029/api/v1/health

Expected response:

{
  "success": true,
  "data": {
    "status": "healthy",
    "service": "TruthApi"
  }
}

---

## Running the Desktop Tool from Source

Open a new terminal:

cd TruthDoctor
dotnet run

Login using your API credentials.
After successful login, the validation window will appear automatically.

---

## Screenshots

### Login Window
![Login](docs/screenshots/login.png)

### Validation Window
![Validation](docs/screenshots/validation.png)

Truth → Validation → Automation

This ensures:

- Safer operations
- Clear diagnostics
- Reduced risk of destructive actions
- Better understanding before changes

---

## Getting Started

### Prerequisites

- .NET SDK 10 or later

Check your installation:

dotnet --version

---

## Running from Source

Clone the repository:

git clone https://github.com/abdelnassermustafa-eng/netpilot-truth-api.git
cd netpilot-truth-api

Build the solution:

dotnet build

Run the API:

cd TruthApi
dotnet run

Test the API:

curl http://localhost:5029/api/v1/health

Expected response:

{
  "success": true,
  "data": {
    "status": "healthy",
    "service": "TruthApi"
  }
}

---

## Running the Desktop Tool from Source

Open a new terminal:

cd TruthDoctor
dotnet run

Login using your API credentials.
After successful login, the validation window will appear automatically.

---

## Screenshots

### Login Window
![Login](docs/screenshots/login.png)

### Validation Window
![Validation](docs/screenshots/validation.png)

---

## Installation Packages

Prebuilt installers are available in the **Releases** section of the repository.

Download the appropriate file for your operating system:

### macOS
- File: `TruthDoctor-<version>.dmg`
- Open the DMG
- Drag **TruthDoctor.app** into the Applications folder
- Launch from Applications

### Windows
- File: `TruthDoctor-<version>-win-x64.msi`
- Double-click the MSI installer
- Follow the installation wizard
- Launch TruthDoctor from the Start Menu

### Linux
- File: `TruthDoctor-<version>-linux-x64.AppImage`
- Make it executable:

chmod +x TruthDoctor-*.AppImage

- Run it:

./TruthDoctor-*.AppImage

---

## Project Roadmap

### Phase 1 — Truth API Foundation (Completed)
- Controller-based architecture
- Structured responses
- Health endpoint
- Dependency injection
- Logging and configuration

### Phase 2 — Secure API Core (Completed)
- JWT authentication
- Role-based access control
- Audit-ready design

### Phase 3 — Cloud Validation Engine (Completed)
- Network validation endpoint
- Structured validation results
- Read-only inspection model

### Phase 4 — Desktop Tool (Completed)
- Avalonia-based cross-platform UI
- Secure login
- Validation result viewer
- Session lifecycle handling

### Phase 5 — Packaging & Distribution (Completed)
- macOS DMG installer
- Windows MSI installer
- Linux AppImage
- GitHub Releases automation

---

## Next Phases (Planned)

### Phase 6 — Advanced Validation
- Multi-domain validation (network, compute, storage)
- Severity-based issue grouping
- Recommendations engine

### Phase 7 — Observability Integration
- Logs, metrics, and events ingestion
- Real-time environment summaries

### Phase 8 — Safe Automation
- Read-only → guided actions
- Approval-based remediation
- Change previews before execution

---

## Project Structure

DotNetProjects/
├── TruthApi/ # Read-only validation API
├── TruthDoctor/ # Desktop troubleshooting tool
├── Controllers/
├── Services/
├── Models/
└── DotNetProjects.sln


---

## Author

**Nasser M. Abdelghani**  
Senior Network & Automation Engineer

Over 20 years of experience in:

- Large-scale networking
- Cloud infrastructure
- Automation and DevOps
- Observability platforms
- Enterprise system design

---

## License

This project is released under the MIT License.

