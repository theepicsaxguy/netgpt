// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading.Tasks;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;

namespace NetGPT.Application.Services
{
    public sealed class ToolCallAccuracyEvaluator : IEvaluator
    {
        public string Name => "ToolCallAccuracyEvaluator";

        public async Task<EvaluationResult> EvaluateAsync(Conversation conversation, string userMessage, AgentResponse response)
        {
            // Simple check: if user message suggests tool use (e.g., contains "search" or "calculate"), check if response mentions tool
            bool needsTool = userMessage.ToLower().Contains("search") || userMessage.ToLower().Contains("calculate");
            bool mentionsTool = response.Content.ToLower().Contains("tool") || response.Content.ToLower().Contains("function");

            double score = needsTool ? (mentionsTool ? 1.0 : 0.0) : 0.5; // Neutral if no tool needed

            string feedback = score == 1.0 ? "Tool call appropriate" : score == 0.0 ? "Tool call missing when needed" : "No tool needed";

            return new EvaluationResult(
                Name,
                score,
                feedback,
                new Dictionary<string, object> { ["needsTool"] = needsTool, ["mentionsTool"] = mentionsTool });
        }
    }
}