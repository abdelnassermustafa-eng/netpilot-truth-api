# Chapter 9
# Resource Graph

Version: PF1

---

## Purpose

The Resource Graph provides the Platform with a complete understanding
of infrastructure relationships.

The Platform manages infrastructure as a connected graph rather than as
independent resources.

Every discovered resource becomes part of the Resource Graph.

---

## Why a Resource Graph

Cloud infrastructure is composed of connected resources.

A resource rarely exists in isolation.

Examples include:

VPC

↓

Subnet

↓

Network Interface

↓

EC2 Instance

↓

Target Group

↓

Load Balancer

Understanding these relationships is essential for both Discovery and
Operations.

---

## Discovery and the Resource Graph

Discovery is responsible for constructing the Resource Graph.

Discovery identifies:

- resources
- resource types
- ownership
- dependencies
- associations

Every Discovery operation updates the Resource Graph.

---

## Operations and the Resource Graph

Operations use the Resource Graph to understand infrastructure before
making changes.

The Resource Graph allows Operations to determine:

- affected resources
- dependencies
- execution order
- potential impact

Operations never assume resources are independent.

---

## Resource Relationships

Relationships describe how Resources are connected.

Examples include:

Contains

Owns

Attached To

Associated With

Routes To

Depends On

References

Uses

Every relationship has a defined meaning.

---

## Relationship Direction

Relationships are directional.

Examples

VPC

Contains

Subnet

Subnet

Contains

Instance

Load Balancer

Uses

Target Group

Target Group

Targets

Instance

Relationship direction is significant.

---

## Resource Identity

Every Resource appears only once within the Resource Graph.

Relationships reference Resources.

Relationships never duplicate Resources.

---

## Graph Updates

Discovery updates the graph by observing current infrastructure.

Operations update the graph after successful verification.

The Resource Graph always represents the latest verified infrastructure
state.

---

## Impact Analysis

Before executing an Operation, the Platform consults the Resource Graph.

Impact Analysis determines:

- dependent resources
- affected relationships
- execution sequence
- potential service interruption

The Resource Graph provides information.

It does not approve or deny Operations.

---

## Future Growth

The Resource Graph shall support future Providers without architectural
changes.

Provider-specific relationship types should be mapped into Platform
relationship definitions whenever practical.

---

## Backend Rule

Every Resource discovered by the Platform shall become part of the
Resource Graph.

Every Operation shall consult the Resource Graph before execution.

The Resource Graph becomes the authoritative representation of
infrastructure relationships within the Platform.
