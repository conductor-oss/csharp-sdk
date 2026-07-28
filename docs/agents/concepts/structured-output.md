# Structured output

Set `Agent.OutputType` to a C# type. The server enforces the JSON schema and the
typed object lands in `result.Output["result"]` as JSON.

```csharp
internal record WeatherReport(
    [property: JsonPropertyName("city")]           string City,
    [property: JsonPropertyName("temperature")]    double Temperature,
    [property: JsonPropertyName("condition")]      string Condition,
    [property: JsonPropertyName("recommendation")] string Recommendation);

var agent = new Agent("weather_reporter")
{
    Model      = "anthropic/claude-sonnet-4-6",
    Tools      = ToolRegistry.FromInstance(new WeatherTools()),
    OutputType = typeof(WeatherReport),
};

var result = await runtime.RunAsync(agent, "What's the weather in NYC?");

if (result.Output?.TryGetValue("result", out var raw) == true && raw is not null)
{
    var jsonStr = raw is JsonElement je
        ? (je.ValueKind == JsonValueKind.String ? je.GetString() : je.GetRawText())
        : raw.ToString();
    var report = JsonSerializer.Deserialize<WeatherReport>(jsonStr!, AgentspanJson.Options);
}
```

Use `AgentBuilder` with `.WithOutputType<T>()`, or the field directly.

> `AgentspanJson.Options` is the SDK's shared `JsonSerializerOptions` (camelCase,
> snake_case enums) — handy when deserializing agent output yourself. The type name
> predates the Conductor rebrand and is retained for API compatibility; see
> [../../upgrading.md](../../upgrading.md).

## Why the unwrapping dance

`Output["result"]` may arrive as a JSON string or as an already-parsed
`JsonElement`, depending on how the server serialized the final task output. The
`ValueKind == JsonValueKind.String` check above handles both without guessing.

## Schema derivation

The schema is derived from the type's properties. Use `[JsonPropertyName]` to pin
wire names rather than relying on the default naming policy — the schema the server
enforces is the one derived from these names, so a mismatch shows up as a validation
failure rather than a silently empty field.

## Reference

`OutputType` and the `AgentResult` shape are tabulated in
[reference/api.md](../reference/api.md).
