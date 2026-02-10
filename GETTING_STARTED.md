# TruthDoctor – Getting Started Guide

TruthDoctor is a cross-platform desktop client for validating and troubleshooting cloud network environments.

It connects securely to the Truth API, performs validation checks, and displays the results in a simple, safe, read-only interface.

---

# 1. Features and Capabilities

## Core Capabilities
- Secure login to the Truth API
- Cloud network validation
- Real-time validation results
- Inventory view of:
  - VPCs
  - Subnets
- Pass/Fail status for validation rules
- Read-only safe operations

## Design Goals
- Safe by default (no destructive actions)
- Clear validation results
- Simple and fast desktop interface
- Cross-platform support

Supported platforms:
- macOS (Intel and Apple Silicon)
- Windows (x64)
- Linux (x64)

---

# 2. Clone and Run from Source

## Requirements
- .NET SDK 10.0 or later

Check installed version:

dotnet --version

---

## Clone the repository

git clone https://github.com/
<your-username>/netpilot-truth-api.git
cd netpilot-truth-api/DotNetProjects/TruthDoctor

---

## Build the project

dotnet build

---

## Run the application

dotnet run

The TruthDoctor login window should appear.

---

# 3. Install and Run (Prebuilt Releases)

Download the latest release from:


https://github.com/<your-username>/netpilot-truth-api/releases

---

## macOS

### Download
File:

TruthDoctor-v1.0.0.dmg

### Install
1. Double-click the `.dmg` file
2. Drag **TruthDoctor.app** to the **Applications** folder
3. Open **Applications**
4. Launch **TruthDoctor**

If macOS blocks the app:
- Right-click the app
- Select **Open**
- Confirm the security dialog

---

## Windows

### Download
File:

TruthDoctor-v1.0.0-win-x64.msi

### Install
1. Double-click the `.msi` file
2. Follow the installer steps
3. Launch **TruthDoctor** from the Start Menu

---

## Linux

### Download
File:

TruthDoctor-x86_64.AppImage

### Run
Open a terminal in the download folder:


chmod +x TruthDoctor-x86_64.AppImage
./TruthDoctor-x86_64.AppImage

No installation is required.

---

# 4. Connecting to the API

By default, TruthDoctor connects to:

http://localhost:5029

Make sure the Truth API is running before logging in.

---

# 5. Development Workflow

From the TruthDoctor project folder:

## Build

dotnet run

Release files will appear in:

releases/

---

# 6. Project Structure

DotNetProjects/
├── TruthApi → Backend API
├── TruthDoctor → Desktop client
└── build-release.sh

---

# 7. Version

Current version:

TruthDoctor v1.0.0

---

# 8. Author

**Nasser Abdelghani**  
NetPilot
