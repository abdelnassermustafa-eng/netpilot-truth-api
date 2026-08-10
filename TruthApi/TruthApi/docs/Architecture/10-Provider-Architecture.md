# Chapter 10
# Provider Architecture

Version: PF1

---

## Purpose

The Provider Architecture defines how cloud providers integrate with the
Platform.

The Platform owns the architecture.

Providers implement the architecture.

No provider shall introduce architectural changes to the Platform.

---

## Provider Independence

The Platform shall remain independent of every cloud provider.

Provider-specific implementation details shall remain isolated behind
Provider Services.

The Platform shall not expose Provider SDK objects outside Provider
Services.

---

## Provider Responsibilities

Every Provider is responsible for:

- Discovery
- Operations
- Resource Mapping
- Relationship Discovery
- Provider API communication

The Platform remains responsible for:

- Validation
- Execution Planning
- Impact Analysis
- Verification
- Audit
- Resource Graph

---

## Provider Structure

Every Provider shall implement the same logical structure.

Provider

↓

Services

↓

Resource Types

↓

Resources

Provider implementations shall follow the Platform architecture.

---

## Provider Services

Each Provider Service manages one logical cloud service.

Examples include:

AWS Networking

AWS Compute

AWS Auto Scaling

AWS Load Balancing

Future Providers shall organize Services using the same approach.

---

## Resource Discovery

Every Resource Type supported by a Provider shall implement Discovery
through the Discovery Framework.

Discovery returns Platform Resource Models.

Discovery never exposes Provider SDK models.

---

## Resource Operations

Every Resource Type supported by a Provider shall implement Operations
through the Operation Framework.

Operations shall execute through the Execution Engine.

Operations shall never bypass Platform validation or verification.

---

## Resource Mapping

Provider Resources shall be translated into Platform Resources.

Provider terminology may differ.

Platform terminology remains consistent.

Example

AWS EC2 Instance

↓

Platform Resource

Instance

The Platform understands Resources.

Providers understand Provider-specific objects.

---

## Provider Expansion

Adding a new Provider should require:

- implementing Provider Services
- implementing Discovery
- implementing Operations
- implementing Resource Mapping

The Platform architecture should remain unchanged.

---

## Architectural Stability

Adding new Providers shall extend the Platform.

Adding new Providers shall not redesign the Platform.

---

## Backend Rule

The Platform owns the architecture.

Providers implement the architecture.

This relationship shall remain unchanged throughout the lifetime of the
Platform.
