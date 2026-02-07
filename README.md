# NetPilot Truth API

A lightweight, enterprise-ready .NET Web API that provides a **read-only, truth-first view** of system and cloud environments.

This project is the foundational service for the **NetPilot platform**, designed around a simple but critical engineering principle:

> **Visibility and validation must come before automation.**

---

## Purpose

Modern cloud and infrastructure environments are powerful but complex:

- Configuration is spread across multiple services
- Logs, metrics, and events live in different systems
- Engineers switch between tools to understand issues
- Small mistakes can cause large outages

The NetPilot Truth API provides a **safe, structured, and consistent interface** to inspect environments without making changes.

---

## Phase 1 — Current Capabilities

This repository currently contains the **enterprise API foundation**, including:

- Controller-based architecture
- Service-layer logic separation
- Dependency Injection
- Configuration-driven behavior
- Structured logging
- Standardized API response model
- Global exception handling

### Phase 2 — Secure API Core

### JWT authentication
- Role-based access control
- Audit logging
- Secure endpoints

## Architecture Philosophy

This project follows a truth-first engineering model:

Truth → Validation → Automation

- Instead of immediately changing infrastructure, the system:
- Inspects the environment
- Validates configurations
- Explains the current state
- Only then enables safe automation

### Phase 3 — Cloud Validation Engine
- AWS environment inspection
- Configuration validation rules
- Drift detection
- Safe read-only analysis

## Planned Development Roadmap

### Phase 4 — Desktop Troubleshooting Tool
- Engineer-focused desktop UI
- Log and event inspection
- Validation summaries

### Phase 5 — TechMaster Modern Platform
- Interactive educational interface
- Real system simulations
- Visual troubleshooting workflows
- Technology Stack

### Getting Started
Prerequisites
- .NET SDK 10 or later

Run locally
dotnet build
dotnet run

Then test:

curl http://localhost:5029/api/v1/health

Project Structure
TruthApi/
├── Controllers/
├── Services/
├── Models/
├── Config/
├── Program.cs
└── appsettings.json

### Long-Term Vision

This repository will evolve into the core inspection and validation service behind:
- NetPilot cloud observability tools
- Desktop engineering utilities
- Educational platforms
- Enterprise automation systems

## Author
Nasser M Abdelghan
Senior network and automation engineer with over two decades of experience in:
- Large-scale networking
- Cloud infrastructure
- Automation and observability
- Enterprise systems

### Example endpoint

GET /api/v1/health


Example response:

```json
{
  "success": true,
  "data": {
    "status": "healthy",
    "service": "TruthApi"
  },
  "timestamp": "2026-02-07T00:00:00Z"
}






