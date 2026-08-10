# Chapter 4
# Core Domain Model

Version: PF1

---

## Purpose

The Core Domain Model defines the fundamental backend objects used by
the Platform.

Every backend feature shall be expressed using these domain concepts.

The model is independent of any cloud provider.

Cloud providers implement this model rather than redefine it.

---

## Domain Hierarchy

The backend is organized using the following hierarchy.

Platform

↓

Provider

↓

Service

↓

Resource Type

↓

Resource

Every discovered or managed object shall fit into this hierarchy.

---

## Platform

The Platform is the backend system responsible for managing all
supported providers.

The Platform defines the architecture.

It does not contain provider-specific implementation details.

---

## Provider

A Provider supplies infrastructure resources.

Examples include:

AWS

Azure

Google Cloud

Kubernetes

A Provider exposes one or more Services.

---

## Service

A Service groups related Resource Types.

Examples include:

AWS EC2

AWS VPC

AWS ELBv2

AWS Auto Scaling

Services organize functionality.

Services do not change the Platform architecture.

---

## Resource Type

A Resource Type defines a class of infrastructure resources.

Examples include:

Vpc

Subnet

Instance

SecurityGroup

LoadBalancer

TargetGroup

Every Resource Type defines the capabilities supported by its
Resources.

---

## Resource

A Resource is an individual infrastructure object.

Examples include:

VPC-12345

Subnet-abcde

i-0123456789

alb-production

Every Resource has one Resource Type.

Every Resource belongs to one Provider.

Every Resource belongs to one Service.

---

## Resource Identity

Every Resource shall have a stable identity within the Platform.

Provider-specific identifiers remain associated with the Resource but
shall not define the Platform architecture.

The Platform may assign additional identifiers if required by future
capabilities.

---

## Resource State

Every Resource has a current state.

Discovery observes state.

Operations modify state.

Verification confirms state.

State is always associated with the Resource.

---

## Resource Relationships

Resources rarely exist in isolation.

Examples include:

Instance → Subnet

Subnet → VPC

Listener → Load Balancer

Target Group → Load Balancer

The Platform shall preserve these relationships.

Relationships become part of the backend domain model rather than being
treated as implementation details.

---

## Resource Lifecycle

Every Resource progresses through a lifecycle.

Typical stages include:

Creation

↓

Discovery

↓

Operation

↓

Verification

↓

Retirement

Not every Resource supports every lifecycle stage.

The lifecycle is defined by the Resource Type.

---

## Resource Capabilities

Every Resource Type defines the capabilities supported by its
Resources.

Examples include:

Discovery

Create

Update

Delete

Start

Stop

Restart

Resize

Attach

Detach

Capabilities describe what a Resource can do.

They do not describe how those capabilities are implemented.

---

## Backend Consistency

All backend domains shall implement the Core Domain Model.

Networking

Compute

Auto Scaling

Load Balancing

Future services shall extend this model rather than introduce
alternative domain concepts.

---

## Guiding Rule

The Core Domain Model is stable.

As the Platform grows, new providers and services shall integrate by
implementing this model instead of modifying it.
