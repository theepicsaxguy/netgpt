
namespace NetGPT.Application.DTOs
{
    public record CreateConversationRequest(
        string? Title = null,
        AgentConfigurationDto? Configuration = null);
}
