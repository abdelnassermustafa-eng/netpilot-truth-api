namespace TruthApi.Models.Platform.Operations;

/// <summary>
/// Represents one permanent, queryable infrastructure state transition.
///
/// The aggregate owns the complete operation lifecycle:
///
/// Request
/// Current state
/// Desired state
/// Validation
/// Execution plan
/// Progress
/// Execution
/// Observed state
/// Verification
/// Result
/// Timeline
/// </summary>
public sealed class Operation
{
    private readonly List<OperationTimelineEntry> _timeline = [];

    private Operation(
        OperationDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        OperationId = document.OperationId;
        Provider = document.Provider;
        Service = document.Service;
        ResourceType = document.ResourceType;
        ResourceId = document.ResourceId;
        OperationName = document.OperationName;
        Region = document.Region;
        Parameters = document.Parameters;
        RequestedBy = document.RequestedBy;
        RequestedAt = document.RequestedAt;
        CreatedAt = document.CreatedAt;
        UpdatedAt = document.UpdatedAt;
        StartedAt = document.StartedAt;
        CompletedAt = document.CompletedAt;
        Status = document.Status;
        Version = document.Version;
        CurrentState = document.CurrentState;
        DesiredState = document.DesiredState;
        ObservedState = document.ObservedState;
        Validation = document.Validation;
        Plan = document.Plan;
        Approval = document.Approval;
        Progress = document.Progress;
        Verification = document.Verification;
        Result = document.Result;

        _timeline.AddRange(document.Timeline);
    }

    private Operation(OperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        OperationId = request.OperationId;
        Provider = request.Provider;
        Service = request.Service;
        ResourceType = request.ResourceType;
        ResourceId = request.ResourceId;
        OperationName = request.Operation;
        Region = request.Region;
        Parameters = request.Parameters;
        RequestedBy = request.RequestedBy;
        RequestedAt = request.RequestedAt;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
        Status = OperationStatus.Pending;

        AddTimelineEntry(
            "OperationCreated",
            "The infrastructure operation was created.");
    }

    public string OperationId { get; }

    public string Provider { get; }

    public string Service { get; }

    public string ResourceType { get; }

    public string? ResourceId { get; }

    public string OperationName { get; }

    public string Region { get; }

    public IReadOnlyDictionary<string, string> Parameters { get; }

    public string RequestedBy { get; }

    public DateTimeOffset RequestedAt { get; }

    public DateTimeOffset CreatedAt { get; }

    public DateTimeOffset UpdatedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public OperationStatus Status { get; private set; }

    public long Version { get; private set; }

    public OperationStateSnapshot? CurrentState { get; private set; }

    public OperationStateSnapshot? DesiredState { get; private set; }

    public OperationStateSnapshot? ObservedState { get; private set; }

    public OperationValidationResult? Validation { get; private set; }

    public ExecutionPlan? Plan { get; private set; }

    public OperationApprovalRecord? Approval
    { get; private set; }

    public bool RequiresConfirmation =>
        Plan?.RequiresConfirmation == true;

    public OperationProgress Progress { get; private set; } =
        new();

    public OperationVerificationResult? Verification
    { get; private set; }

    public OperationResult? Result { get; private set; }

    public IReadOnlyList<OperationTimelineEntry> Timeline =>
        _timeline.AsReadOnly();

    public bool IsTerminal =>
        Status is
            OperationStatus.ValidationFailed or
            OperationStatus.Succeeded or
            OperationStatus.PartiallySucceeded or
            OperationStatus.Failed or
            OperationStatus.Rejected or
            OperationStatus.Cancelled;

    public static Operation Create(OperationRequest request)
    {
        return new Operation(request);
    }

    public static Operation Rehydrate(
        OperationDocument document)
    {
        return new Operation(document);
    }

    public OperationDocument ToDocument()
    {
        return new OperationDocument
        {
            OperationId = OperationId,
            Provider = Provider,
            Service = Service,
            ResourceType = ResourceType,
            ResourceId = ResourceId,
            OperationName = OperationName,
            Region = Region,
            Parameters = Parameters,
            RequestedBy = RequestedBy,
            RequestedAt = RequestedAt,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt,
            StartedAt = StartedAt,
            CompletedAt = CompletedAt,
            Status = Status,
            Version = Version,
            CurrentState = CurrentState,
            DesiredState = DesiredState,
            ObservedState = ObservedState,
            Validation = Validation,
            Plan = Plan,
            Approval = Approval,
            Progress = Progress,
            Verification = Verification,
            Result = Result,
            Timeline = Timeline.ToList()
        };
    }

    public void CaptureCurrentState(
        OperationStateSnapshot snapshot)
    {
        EnsureNotTerminal();
        ArgumentNullException.ThrowIfNull(snapshot);

        CurrentState = snapshot;

        Touch(
            "CurrentStateCaptured",
            "Discovery captured the current infrastructure state.");
    }

    public void DefineDesiredState(
        OperationStateSnapshot snapshot)
    {
        EnsureNotTerminal();
        ArgumentNullException.ThrowIfNull(snapshot);

        DesiredState = snapshot;

        Touch(
            "DesiredStateDefined",
            "The requested infrastructure state was defined.");
    }

    public void BeginValidation()
    {
        TransitionTo(
            OperationStatus.Validating,
            "ValidationStarted",
            "Operation validation started.");
    }

    public void ApplyValidation(
        OperationValidationResult validation)
    {
        EnsureStatus(OperationStatus.Validating);
        ArgumentNullException.ThrowIfNull(validation);

        Validation = validation;

        if (validation.IsValid)
        {
            TransitionTo(
                OperationStatus.Validated,
                "ValidationSucceeded",
                "Operation validation succeeded.");

            return;
        }

        TransitionTo(
            OperationStatus.ValidationFailed,
            "ValidationFailed",
            "Operation validation failed.");

        CompletedAt = DateTimeOffset.UtcNow;
    }

    public void BeginPlanning()
    {
        EnsureStatus(OperationStatus.Validated);

        TransitionTo(
            OperationStatus.Planning,
            "PlanningStarted",
            "Execution planning started.");
    }

    public void ApplyPlan(ExecutionPlan plan)
    {
        EnsureStatus(OperationStatus.Planning);
        ArgumentNullException.ThrowIfNull(plan);

        if (!string.Equals(
                plan.OperationId,
                OperationId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "The execution plan belongs to another operation.");
        }

        Plan = plan;
        Progress = new OperationProgress
        {
            CompletedSteps = 0,
            TotalSteps = plan.Steps.Count,
            Message = "Execution plan created."
        };

        TransitionTo(
            plan.RequiresConfirmation
                ? OperationStatus.AwaitingConfirmation
                : OperationStatus.Planned,
            "PlanCreated",
            "The execution plan was created.");
    }

    public void ApprovePlan(
        string approvedBy,
        string comment = "")
    {
        EnsureStatus(OperationStatus.AwaitingConfirmation);
        ArgumentException.ThrowIfNullOrWhiteSpace(approvedBy);

        Approval = new OperationApprovalRecord
        {
            Decision = OperationApprovalDecision.Approved,
            DecidedBy = approvedBy.Trim(),
            DecidedAt = DateTimeOffset.UtcNow,
            Comment = comment.Trim()
        };

        TransitionTo(
            OperationStatus.Planned,
            "PlanApproved",
            $"The execution plan was approved by " +
            $"{Approval.DecidedBy}.",
            new Dictionary<string, string>
            {
                ["decision"] = "Approved",
                ["decidedBy"] = Approval.DecidedBy,
                ["comment"] = Approval.Comment
            });
    }

    public void RejectPlan(
        string rejectedBy,
        string comment)
    {
        EnsureStatus(OperationStatus.AwaitingConfirmation);
        ArgumentException.ThrowIfNullOrWhiteSpace(rejectedBy);
        ArgumentException.ThrowIfNullOrWhiteSpace(comment);

        Approval = new OperationApprovalRecord
        {
            Decision = OperationApprovalDecision.Rejected,
            DecidedBy = rejectedBy.Trim(),
            DecidedAt = DateTimeOffset.UtcNow,
            Comment = comment.Trim()
        };

        Complete(
            OperationStatus.Rejected,
            $"The execution plan was rejected by " +
            $"{Approval.DecidedBy}: {Approval.Comment}",
            errors:
            [
                $"Operation rejected: {Approval.Comment}"
            ]);
    }

    public void BeginExecution()
    {
        EnsureStatus(OperationStatus.Planned);

        StartedAt ??= DateTimeOffset.UtcNow;

        TransitionTo(
            OperationStatus.Executing,
            "ExecutionStarted",
            "Infrastructure state transition started.");
    }

    public void UpdateProgress(
        int completedSteps,
        string currentStep,
        string message = "")
    {
        EnsureStatus(OperationStatus.Executing);

        var totalSteps = Plan?.Steps.Count ?? 0;

        if (completedSteps < 0 ||
            completedSteps > totalSteps)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedSteps));
        }

        Progress = new OperationProgress
        {
            CompletedSteps = completedSteps,
            TotalSteps = totalSteps,
            CurrentStep = currentStep,
            Message = message
        };

        Touch(
            "ProgressUpdated",
            string.IsNullOrWhiteSpace(message)
                ? $"Execution progress: {Progress.Percentage}%."
                : message,
            new Dictionary<string, string>
            {
                ["completedSteps"] =
                    completedSteps.ToString(),
                ["totalSteps"] =
                    totalSteps.ToString(),
                ["percentage"] =
                    Progress.Percentage.ToString(),
                ["currentStep"] =
                    currentStep
            });
    }

    public void BeginVerification(
        OperationStateSnapshot observedState)
    {
        EnsureStatus(OperationStatus.Executing);
        ArgumentNullException.ThrowIfNull(observedState);

        ObservedState = observedState;

        TransitionTo(
            OperationStatus.Verifying,
            "VerificationStarted",
            "Discovery observed the resulting infrastructure state.");
    }

    public void ApplyVerification(
        OperationVerificationResult verification)
    {
        EnsureStatus(OperationStatus.Verifying);
        ArgumentNullException.ThrowIfNull(verification);

        Verification = verification;

        if (verification.IsVerified)
        {
            Complete(
                OperationStatus.Succeeded,
                "Verification succeeded. Discovery confirmed the " +
                "requested infrastructure state.");

            return;
        }

        Complete(
            OperationStatus.Failed,
            "Verification failed. Observed infrastructure state " +
            "does not match the requested state.");
    }

    public void Fail(
        string error,
        IReadOnlyList<string>? warnings = null)
    {
        if (IsTerminal)
        {
            return;
        }

        Complete(
            OperationStatus.Failed,
            error,
            warnings,
            [error]);
    }

    public void Cancel(string reason)
    {
        if (IsTerminal)
        {
            return;
        }

        Complete(
            OperationStatus.Cancelled,
            reason,
            errors: [reason]);
    }

    private void Complete(
        OperationStatus status,
        string message,
        IReadOnlyList<string>? warnings = null,
        IReadOnlyList<string>? errors = null)
    {
        if (status is not (
            OperationStatus.Succeeded or
            OperationStatus.PartiallySucceeded or
            OperationStatus.Failed or
            OperationStatus.Rejected or
            OperationStatus.Cancelled))
        {
            throw new ArgumentOutOfRangeException(
                nameof(status),
                "The supplied status is not terminal.");
        }

        Status = status;
        CompletedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CompletedAt.Value;
        Version++;

        Result = new OperationResult
        {
            OperationId = OperationId,
            PlanId = Plan?.PlanId ?? "",
            Status = status,
            Validation = Validation,
            Verification = Verification,
            Warnings = warnings ?? Array.Empty<string>(),
            Errors = errors ?? Array.Empty<string>(),
            CompletedAt = CompletedAt.Value
        };

        AddTimelineEntry(
            "OperationCompleted",
            message);
    }

    private void TransitionTo(
        OperationStatus status,
        string eventType,
        string message,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        EnsureNotTerminal();

        Status = status;
        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;

        AddTimelineEntry(
            eventType,
            message,
            metadata);
    }

    private void Touch(
        string eventType,
        string message,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        EnsureNotTerminal();

        UpdatedAt = DateTimeOffset.UtcNow;
        Version++;

        AddTimelineEntry(
            eventType,
            message,
            metadata);
    }

    private void AddTimelineEntry(
        string eventType,
        string message,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        _timeline.Add(
            new OperationTimelineEntry
            {
                OccurredAt = DateTimeOffset.UtcNow,
                Status = Status,
                EventType = eventType,
                Message = message,
                Metadata = metadata ??
                    new Dictionary<string, string>(
                        StringComparer.OrdinalIgnoreCase)
            });
    }

    private void EnsureStatus(OperationStatus expected)
    {
        EnsureNotTerminal();

        if (Status != expected)
        {
            throw new InvalidOperationException(
                $"Operation {OperationId} is currently {Status}; " +
                $"expected {expected}.");
        }
    }

    private void EnsureNotTerminal()
    {
        if (IsTerminal)
        {
            throw new InvalidOperationException(
                $"Operation {OperationId} is already complete with " +
                $"status {Status}.");
        }
    }
}
