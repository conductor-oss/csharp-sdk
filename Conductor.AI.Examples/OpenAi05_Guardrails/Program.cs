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
// OpenAi05 — Guardrails.
//
// A banking assistant that uses function tools to look up balances
// and transfer funds. The Python original wraps the agent with PII
// regex input guardrails and forbidden-phrase output guardrails.
//
// Note: simplified from Java original — input_guardrails / output_guardrails
// are not yet surfaced on the OpenAIAgent builder. The tool surface
// and agent shape are ported faithfully; the intended guardrail policy
// is documented below.
//
// Intended guardrails:
//   Input  — block SSN regex (\b\d{3}-\d{2}-\d{4}\b) and credit-card regex
//            (\b\d{4}[\s-]?\d{4}[\s-]?\d{4}[\s-]?\d{4}\b).
//   Output — block "internal system", "database password", "api key",
//            "secret token".
//
// Requirements:
//   - CONDUCTOR_SERVER_URL=http://localhost:8080/api
//   - CONDUCTOR_AGENT_LLM_MODEL=openai/gpt-4o-mini

using Conductor.AI;
using Conductor.AI.Examples;
using Conductor.AI.OpenAI;

var agent = OpenAIAgent.Builder()
    .Name("banking_assistant")
    .Instructions(
        "You are a secure banking assistant. Help users check account balances " +
        "and transfer funds. Never reveal internal system details.")
    .Model(Settings.LlmModel)
    .Tools(new BankingTools())
    .Build();

await using var runtime = new AgentRuntime(new AgentRuntimeOptions { ServerUrl = Settings.ServerUrl });
var result = await runtime.RunAsync(agent, "What's the balance on account ACC-100?");
result.PrintResult();

internal sealed class BankingTools
{
    [Tool(Name = "get_account_balance", Description = "Look up the balance of a bank account.")]
    public string GetAccountBalance(string account_id) => account_id switch
    {
        "ACC-100" => "$5,230.00",
        "ACC-200" => "$12,750.50",
        "ACC-300" => "$890.25",
        _ => $"Account {account_id} not found",
    };

    [Tool(Name = "transfer_funds", Description = "Transfer funds between accounts.")]
    public string TransferFunds(string from_account, string to_account, double amount)
        => $"Transferred ${amount:F2} from {from_account} to {to_account}.";
}
