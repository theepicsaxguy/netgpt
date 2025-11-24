// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates;

namespace NetGPT.Application.Services
{
    public interface IEvaluator
    {
        string Name { get; }

        Task<EvaluationResult> EvaluateAsync(Conversation conversation, string userMessage, AgentResponse response);
    }

    public record EvaluationResult(
        string EvaluatorName,
        double Score,
        string Feedback,
        Dictionary<string, object> Metadata);
}
