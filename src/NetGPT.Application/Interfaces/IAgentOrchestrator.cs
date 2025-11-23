using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Agents;

namespace NetGPT.Application.Interfaces;

public interface IAgentOrchestrator
{
    Task<Result<AgentResponse>> ExecuteAsync(
        Conversation conversation,
        string userMessage,
        CancellationToken cancellationToken = default);
}
