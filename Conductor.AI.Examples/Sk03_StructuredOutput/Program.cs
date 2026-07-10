/*
 * Copyright 2024 Conductor Authors.
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
 * an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations under the License.
 */
// Sk03 — SK plugin that returns a structured record.
//
// The tool returns a typed object; the bridge serializes it to JSON so the
// LLM can ground its final answer on the structured fields.
//
// Requirements:
//   - AGENTSPAN_SERVER_URL=http://localhost:6767/api
//   - AGENTSPAN_LLM_MODEL=openai/gpt-4o-mini

using System.ComponentModel;
using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.SemanticKernel;
using Microsoft.SemanticKernel;

namespace Conductor.AI.Examples.Sk03;

public record StockQuote(string Symbol, decimal Price, decimal ChangePct);

public sealed class StockPlugin
{
    [KernelFunction, Description("Get a stock quote for the given ticker symbol.")]
    public StockQuote GetQuote([Description("ticker symbol, e.g. AAPL")] string symbol) =>
        symbol.ToUpperInvariant() switch
        {
            "AAPL" => new StockQuote("AAPL", 192.30m, 1.4m),
            "MSFT" => new StockQuote("MSFT", 421.55m, -0.6m),
            "GOOG" => new StockQuote("GOOG", 173.12m, 0.3m),
            _ => new StockQuote(symbol.ToUpperInvariant(), 0m, 0m),
        };
}

public static class Program
{
    public static async Task Main()
    {
        var agent = SemanticKernelAgent.From(
            name: "sk_quotes",
            model: Settings.LlmModel,
            instructions: "Quote the symbol, price and percent change exactly as returned by the tool.",
            new StockPlugin());

        await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
        var result = await runtime.RunAsync(agent, "What are AAPL and MSFT trading at, and what's the move?");
        result.PrintResult();
    }
}
