# Chapter 3
# Platform Dictionary

Version: PF1

---

## Purpose

This chapter defines the official terminology used throughout the
backend architecture.

Every architectural document, source file, API, and implementation
should use these definitions consistently.

Terms defined here shall have one meaning throughout the platform.

---

## Platform

The backend system responsible for discovering, understanding,
operating, and governing cloud resources.

The Platform defines the architecture.

Cloud providers implement the architecture.

---

## Provider

A cloud or infrastructure system supported by the Platform.

Examples include:

- AWS
- Azure
- Google Cloud
- Kubernetes
- VMware

A Provider implements Platform contracts.

---

## Service

A logical collection of related resources offered by a Provider.

Examples include:

AWS EC2

AWS VPC

AWS ELBv2

AWS Auto Scaling

Services organize resources.

They do not define architecture.

---

## Resource

A manageable infrastructure object.

Examples include:

- VPC
- Subnet
- EC2 Instance
- Security Group
- Load Balancer
- Target Group

Every resource shall eventually support both:

- Discovery
- Operations

---

## Resource Type

The definition describing a class of resources.

Examples include:

Vpc

Subnet

Instance

LoadBalancer

TargetGroup

Individual resources are instances of a Resource Type.

---

## Discovery

The backend capability responsible for obtaining the current state of
resources from a Provider.

Discovery never changes infrastructure.

Discovery answers:

- What exists?
- What is its current state?
- How are resources related?

---

## Operation

The backend capability responsible for changing infrastructure.

Operations may include:

- Create
- Update
- Delete
- Start
- Stop
- Restart
- Resize
- Attach
- Detach
- Associate
- Disassociate

Every modifying action performed by the Platform is an Operation.

---

## Inventory

A collection of resources returned by Discovery.

An Inventory represents the observed state of infrastructure at a
specific point in time.

---

## Relationship

A connection between two or more resources.

Examples include:

Instance belongs to Subnet.

Subnet belongs to VPC.

Listener belongs to Load Balancer.

Target Group belongs to Load Balancer.

Relationships are fundamental to understanding infrastructure.

---

## Validation

The process of determining whether an Operation is valid before
execution.

Validation does not change infrastructure.

---

## Execution Plan

A structured description of how an Operation will be performed.

Execution Plans are produced after successful validation and before
execution.

---

## Execution

The process of applying an Execution Plan to a Provider.

Execution performs the intended infrastructure change.

---

## Verification

The process of confirming that an executed Operation produced the
expected infrastructure state.

Execution success alone is not considered sufficient.

Operations should be verified whenever practical.

---

## Audit

The permanent record describing an Operation.

Audit information should identify:

- the requested operation
- the target resource
- execution time
- execution result
- verification result

---

## Backend Contract

An architectural agreement defining how backend components interact.

Contracts provide stable interfaces while allowing implementations to
evolve.

---

## Guiding Rule

When introducing a new architectural concept, first determine whether
it already exists in this dictionary.

If an existing term accurately describes the concept, reuse it.

If a new term is required, define it here before introducing it into
the architecture.
