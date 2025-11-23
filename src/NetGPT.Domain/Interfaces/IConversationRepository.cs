using NetGPT.Domain.Aggregates;
using NetGPT.Domain.ValueObjects;

namespace NetGPT.Domain.Interfaces;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(ConversationId id, CancellationToken ct = default);
    Task<List<Conversation>> GetByUserIdAsync(UserId userId, int skip, int take, CancellationToken ct = default);
    Task<Conversation> AddAsync(Conversation conversation, CancellationToken ct = default);
    Task UpdateAsync(Conversation conversation, CancellationToken ct = default);
    Task DeleteAsync(ConversationId id, CancellationToken ct = default);
    Task<int> CountByUserIdAsync(UserId userId, CancellationToken ct = default);
}