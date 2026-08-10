# Chapter 6
# Discovery Framework

Version: PF1

---

## Purpose

The Discovery Framework defines the backend architecture responsible for
observing infrastructure without modifying it.

Discovery provides the Platform with an accurate representation of the
current infrastructure state.

Discovery is a read-only capability.

---

## Responsibilities

The Discovery Framework is responsible for:

- Discovering resources
- Collecting resource metadata
- Building inventories
- Discovering relationships
- Returning a consistent backend model

Discovery shall never modify infrastructure.

---

## Discovery Architecture

The Discovery Framework consists of the following components.

Platform API

↓

Discovery Service

↓

Discoverers

↓

Provider Services

↓

Cloud Provider

Each component has one responsibility.

---

## Discovery Service

The Discovery Service coordinates discovery.

Responsibilities include:

- selecting providers
- selecting regions
- executing discovery
- coordinating discoverers
- collecting inventories
- returning unified results

The Discovery Service does not communicate directly with cloud
providers.

---

## Discoverer

A Discoverer is responsible for one Resource Type.

Examples include:

VpcDiscoverer

SubnetDiscoverer

InstanceDiscoverer

LoadBalancerDiscoverer

TargetGroupDiscoverer

Every Discoverer is responsible for one Resource Type only.

---

## Provider Services

Provider Services communicate with the native cloud SDK.

Provider Services isolate Provider-specific implementation details from
the Discovery Framework.

---

## Discovery Flow

Discovery follows the same sequence for every Resource Type.

Platform API

↓

Discovery Service

↓

Discoverer

↓

Provider Service

↓

Cloud Provider

↓

Resource Mapping

↓

Inventory

↓

Platform API Response

---

## Resource Mapping

Discoverers translate Provider-specific objects into Platform Resource
Models.

The remainder of the backend shall never depend upon Provider SDK
objects.

---

## Regional Discovery

Discovery may execute across multiple regions.

Regional discovery should execute independently whenever practical.

Results shall be merged into a single Inventory.

Failure in one region should not prevent successful discovery in other
regions whenever possible.

---

## Partial Discovery

Discovery should tolerate partial failures.

When partial failures occur:

- successful resources shall still be returned
- warnings shall be collected
- failures shall be reported
- discovery shall continue whenever practical

---

## Parallel Discovery

Independent Discoverers should execute in parallel whenever practical.

Parallel execution should not change backend behavior.

Parallelism is an implementation optimization rather than an
architectural requirement.

---

## Inventory

Discovery returns Inventories.

An Inventory represents the infrastructure state at one point in time.

Inventories shall contain Platform Resource Models only.

---

## Relationships

Discovery is responsible for identifying relationships between
Resources.

Relationships become part of the Platform domain model.

They are not implementation details.

---

## Backend Rule

Every new Resource Type added to the Platform shall implement Discovery
through this framework.

Alternative discovery architectures shall not be introduced.
