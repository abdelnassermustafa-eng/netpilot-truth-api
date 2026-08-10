# Chapter 8
# Execution Engine

Version: PF1

---

## Purpose

The Execution Engine is responsible for executing validated operations
against cloud providers.

The Execution Engine is the only backend component permitted to perform
modifying infrastructure operations.

All resource modifications shall pass through this engine.

---

## Responsibilities

The Execution Engine is responsible for:

- receiving validated operations
- evaluating execution plans
- performing impact analysis
- coordinating provider execution
- monitoring execution
- verifying results
- collecting audit information
- returning operation results

---

## Execution Pipeline

Every operation shall execute using the following pipeline.

User Request

↓

Validation

↓

Execution Plan

↓

Impact Analysis

↓

Execution

↓

Verification

↓

Audit

↓

Operation Result

The pipeline is identical for every Resource Type.

---

## Validation Input

The Execution Engine only accepts validated operations.

Validation is performed by the Operation Framework.

The Execution Engine assumes that:

- the operation is supported
- required parameters exist
- authorization has succeeded
- provider selection has completed

---

## Execution Plan

The Execution Plan defines exactly what the engine will execute.

Execution Plans should describe:

- target resources
- requested operation
- execution order
- provider
- expected changes

Execution Plans shall not contain provider SDK objects.

---

## Impact Analysis

Before execution, the Platform should determine the potential impact of
the requested operation.

Impact Analysis may identify:

- dependent resources
- affected relationships
- service interruption
- replacement requirements
- restart requirements
- availability risks

Impact Analysis provides information to both users and future platform
components.

Impact Analysis does not prevent execution.

Validation determines whether execution is allowed.

---

## Provider Execution

Provider Services translate Execution Plans into Provider-specific API
calls.

The Execution Engine never communicates directly with cloud provider
SDKs.

Provider Services isolate Provider implementation details from the
Platform.

---

## Execution Monitoring

Long-running operations should expose execution progress whenever
possible.

Examples include:

- resource creation
- database modification
- instance refresh
- snapshot creation

Execution monitoring improves backend observability.

---

## Verification

Verification confirms that the requested infrastructure state has been
achieved.

Verification should use Discovery whenever practical.

Verification compares:

Expected State

↓

Observed State

Verification determines operational success.

---

## Audit

Every execution shall produce an audit record.

Audit records should contain:

- operation
- execution plan
- execution time
- provider
- verification result
- warnings
- errors

Audit records become part of the permanent operational history.

---

## Operation Result

The Execution Engine returns a structured Operation Result.

Operation Results accurately describe:

- execution outcome
- verification outcome
- warnings
- errors

Successful execution without successful verification should be reported
accordingly.

---

## Backend Rule

Every modifying operation implemented by the Platform shall execute
through the Execution Engine.

Alternative execution paths shall not be introduced.
