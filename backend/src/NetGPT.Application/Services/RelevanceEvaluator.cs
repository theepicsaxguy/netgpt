// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;

namespace NetGPT.Application.Services
{
    public sealed class RelevanceEvaluator : IEvaluator
    {
        public string Name => "RelevanceEvaluator";

        [SuppressMessage("Performance", "CA1827:Count", Justification = "Need the count of overlaps, not just if any")]
        public async Task<EvaluationResult> EvaluateAsync(Conversation conversation, string userMessage, AgentResponse response)
        {
            // Simple heuristic: check if response contains keywords from user message
            IEnumerable<string> userWords = userMessage.ToLower(CultureInfo.InvariantCulture).Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct();
            IEnumerable<string> responseWords = response.Content.ToLower(CultureInfo.InvariantCulture).Split(' ', StringSplitOptions.RemoveEmptyEntries).Distinct();
            var overlap = userWords.Intersect(responseWords).Count();
            var score = userWords.Count() > 0 ? (double)overlap / userWords.Count() : 0.0;

            string feedback = score > 0.5 ? "Response is relevant" : "Response may not be fully relevant";

            return new EvaluationResult(
                Name,
                score,
                feedback,
                new Dictionary<string, object> { ["overlap"] = overlap });
        }
    }
}
