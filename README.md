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






