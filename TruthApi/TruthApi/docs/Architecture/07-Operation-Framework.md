# Chapter 7
# Operation Framework

Version: PF1

---

## Purpose

The Operation Framework defines the backend architecture responsible for
changing infrastructure.

Unlike Discovery, Operations modify infrastructure.

Every backend operation shall execute through this framework.

No resource shall bypass this framework.

---

## Responsibilities

The Operation Framework is responsible for:

- receiving operation requests
- validating requests
- determining resource capabilities
- building execution plans
- executing operations
- verifying results
- recording audit information
- returning operation results

---

## Discovery and Operations

Discovery and Operations are equal backend capabilities.

Discovery answers:

"What exists?"

Operations answer:

"How should infrastructure change?"

Both capabilities operate on the same Platform Resource Model.

Neither framework owns the other.

---

## Operation Architecture

Every operation follows the same backend flow.

Platform API

↓

Operation Service

↓

Validation

↓

Execution Plan

↓

Provider Service

↓

Cloud Provider

↓

Verification

↓

Audit

↓

Operation Result

Every Resource Type shall implement this flow.

---

## Operation Service

The Operation Service coordinates modifying operations.

Responsibilities include:

- selecting providers
- selecting resources
- validating requests
- coordinating execution
- collecting results
- returning operation outcomes

The Operation Service never communicates directly with cloud providers.

---

## Supported Operations

Operations include any action that changes infrastructure.

Examples include:

Create

Update

Delete

Start

Stop

Restart

Resize

Attach

Detach

Associate

Disassociate

Tag

Untag

Provider-specific operations may exist.

The backend architecture remains unchanged.

---

## Validation

Every operation shall be validated before execution.

Validation determines whether an operation is:

- supported
- authorized
- complete
- safe

Validation failures shall prevent execution.

---

## Execution Planning

Every modifying operation shall produce an Execution Plan.

The Execution Plan describes:

- target resources
- requested operation
- execution order
- expected changes

Execution Plans become the contract between user intent and execution.

---

## Provider Services

Provider Services translate Platform Operations into Provider-specific
API calls.

Provider Services isolate cloud-provider implementation details from the
Operation Framework.

---

## Verification

After execution, the Platform shall verify the resulting infrastructure
state whenever practical.

Verification confirms that the requested change was successfully applied.

Execution success alone is not considered sufficient.

---

## Audit

Every operation shall generate an audit record.

Audit records should identify:

- requested operation
- target resource
- execution time
- execution result
- verification result

Audit information supports troubleshooting, governance, and compliance.

---

## Operation Result

Every operation returns a structured result.

Operation Results should include:

- requested operation
- execution status
- verification status
- warnings
- errors

The response should accurately represent the outcome of the operation.

---

## Backend Rule

Every Resource Type added to the Platform shall implement Operations
through this framework.

Alternative operation architectures shall not be introduced.
