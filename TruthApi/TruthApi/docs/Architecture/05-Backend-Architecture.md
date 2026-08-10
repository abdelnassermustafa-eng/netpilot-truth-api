# Chapter 5
# Backend Architecture

Version: PF1

---

## Purpose

This chapter defines the logical backend architecture of the Platform.

The backend is composed of independent architectural components.

Each component has one clearly defined responsibility.

Communication between components shall occur through well-defined
backend contracts.

---

## Backend Layers

The backend is organized into the following logical layers.

Platform API

↓

Discovery

↓

Operations

↓

Provider Services

↓

Cloud Provider

Each layer has a single responsibility.

Higher layers shall not bypass lower layers.

---

## Platform API

The Platform API exposes backend capabilities.

The API does not communicate directly with cloud providers.

The API delegates work to backend components.

---

## Discovery Layer

The Discovery Layer is responsible for observing infrastructure.

Discovery never modifies resources.

Discovery returns resource inventories.

Discovery understands relationships between resources.

---

## Operations Layer

The Operations Layer is responsible for changing infrastructure.

Every modifying action shall pass through this layer.

Operations never bypass backend validation.

---

## Provider Services

Provider Services implement cloud-provider behavior.

Examples include:

AWS Networking

AWS Compute

AWS Auto Scaling

AWS Load Balancing

Provider Services translate Platform requests into Provider API calls.

---

## Cloud Provider

Cloud providers expose native APIs.

Examples include:

AWS SDK

Azure SDK

Google Cloud SDK

The Platform interacts with providers only through Provider Services.

---

## Architectural Boundaries

Discovery and Operations are independent backend capabilities.

Neither layer owns the other.

Both layers operate on the same Resource Model.

Both layers use the same Provider Services.

---

## Backend Flow

Discovery Flow

Platform API

↓

Discovery

↓

Provider Service

↓

Cloud Provider

↓

Inventory

Operations Flow

Platform API

↓

Operations

↓

Provider Service

↓

Cloud Provider

↓

Verification

↓

Operation Result

---

## Shared Domain Model

Discovery and Operations always share:

- Provider
- Service
- Resource Type
- Resource
- Relationships

No duplicate domain models shall exist.

---

## Backend Stability

Future AWS services shall integrate into the existing backend
architecture.

New services shall not introduce new backend layers.

The architecture grows by extending existing components rather than
adding alternative execution paths.

---

## Guiding Rule

The backend architecture shall remain stable as supported providers,
services, and resources continue to grow.
