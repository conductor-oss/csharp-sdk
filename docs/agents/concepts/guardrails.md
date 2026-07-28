# Guardrails

Guardrails validate input or output and can retry, raise, fix, or escalate to a
human. `Position` is `Input` or `Output`; `OnFail` is `Retry`, `Raise`, `Fix`, or
`Human`.

## `[Guardrail]` methods + `GuardrailRegistry.FromInstance`

```csharp
internal sealed class PiiGuardrails
{
    [Guardrail(Position = Position.Output, OnFail = OnFail.Retry, MaxRetries = 3)]
    public GuardrailResult NoPii(string content)
    {
        if (CcPattern.IsMatch(content) || SsnPattern.IsMatch(content))
            return new GuardrailResult(false, "Redact card numbers and SSNs before responding.");
        return new GuardrailResult(true);
    }
}

var agent = new Agent("support_agent")
{
    Guardrails = GuardrailRegistry.FromInstance(new PiiGuardrails()),
};
```

## Regex guardrail

`mode: "block"` fails on a match; `"allow"` fails when nothing matches.

```csharp
var noEmails = RegexGuardrail.Create(
    pattern:    @"[\w.+\-]+@[\w\-]+\.[\w.\-]+",
    mode:       "block",
    name:       "no_email_addresses",
    message:    "Response must not contain email addresses.",
    position:   Position.Output,
    onFail:     OnFail.Retry,
    maxRetries: 3);
```

## LLM guardrail

A model judges content against a policy and returns `{passed, reason}`:

```csharp
var safety = LLMGuardrail.Create(
    model:  "anthropic/claude-sonnet-4-6",
    policy: "Reject medical/legal advice presented as fact, guarantees, or PII.",
    name:   "content_safety",
    position: Position.Output,
    onFail:   OnFail.Retry);
```

## Scoping to a single tool

```csharp
var t = someToolDef.WithGuardrails(noEmails);
```

This applies the guardrail to that tool's input or output rather than the agent's.

## Observing guardrail outcomes

When streaming, guardrail activity surfaces as `EventType.GuardrailPass` and
`EventType.GuardrailFail` events — see
[streaming-hitl.md](streaming-hitl.md).

A guardrail with `OnFail = OnFail.Human` escalates to a human approval step, which
behaves like any other pause; see
[streaming-hitl.md](streaming-hitl.md#human-in-the-loop).

## Reference

`GuardrailAttribute`, `GuardrailDef`, `GuardrailResult`, and the factory
signatures are tabulated in [reference/api.md](../reference/api.md#guardrails).
