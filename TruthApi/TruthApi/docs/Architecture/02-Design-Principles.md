# Chapter 2
# Design Principles

Version: PF1

---

## Purpose

This chapter defines the engineering principles governing the backend
architecture of the Universal Cloud Management Platform.

Every backend component shall conform to these principles.

These principles exist to ensure that the platform evolves in a
consistent direction without requiring architectural redesign as new
providers, services, and resources are added.

---

## Principle 1
## Discovery and Operations are Equal

Discovery and Operations are first-class capabilities of the platform.

Neither capability is considered an extension of the other.

Every resource supported by the platform shall eventually implement both
capabilities.

Discovery is responsible for understanding infrastructure.

Operations are responsible for changing infrastructure.

The architecture shall always allow both capabilities to evolve together.

---

## Principle 2
## One Resource Model

Every resource shall have a single backend resource model.

Both Discovery and Operations shall use the same model.

Operations shall never introduce duplicate models representing the same
resource.

---

## Principle 3
## Separation of Responsibilities

The backend shall separate responsibilities into distinct components.

Examples include:

- Discovery
- Operations
- Validation
- Planning
- Execution
- Verification
- Audit

Each component shall have one clearly defined responsibility.

---

## Principle 4
## Provider Independence

The platform architecture shall not depend on AWS-specific concepts.

AWS is the first provider implemented.

Future providers shall integrate through the same architectural
contracts without requiring changes to the platform core.

---

## Principle 5
## Extend Before Modify

New capabilities should be added through extension rather than by
modifying existing components whenever practical.

Extension points shall be preferred over conditional logic.

---

## Principle 6
## Consistency

Resources implementing similar behavior shall follow the same
architectural patterns.

A developer familiar with one resource should immediately understand
another resource within the same domain.

Consistency is preferred over cleverness.

---

## Principle 7
## Validation Before Execution

Every operation shall be validated before execution.

Validation shall determine whether an operation is:

- valid
- safe
- supported
- authorized

Operations failing validation shall not execute.

---

## Principle 8
## Planning Before Execution

Every modifying operation shall generate an execution plan before
interacting with the cloud provider.

Execution plans become the contract between user intent and provider
implementation.

---

## Principle 9
## Verification After Execution

Successful execution does not guarantee successful change.

Every operation shall verify the resulting infrastructure state.

Verification confirms that the intended change was actually applied.

---

## Principle 10
## Auditability

Every modifying operation shall be auditable.

The backend shall record sufficient information to determine:

- what changed
- when it changed
- who initiated the change
- whether the change succeeded
- how the resulting state was verified

---

## Principle 11
## Architectural Stability

As additional AWS services are implemented after Platform Foundation
PF1, they shall implement the existing backend architecture rather than
introduce new architectural patterns.

The backend architecture should become increasingly stable as platform
capabilities grow.

---

## Guiding Rule

Before implementing any new backend feature, ask:

"Does this strengthen the existing architecture, or does it introduce a
new architectural pattern?"

If a new pattern is introduced, its necessity should be justified before
implementation.
