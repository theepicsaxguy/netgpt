using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetGPT.Application.Commands;
using NetGPT.Application.DTOs;
using NetGPT.Application.Queries;

namespace NetGPT.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public sealed class ConversationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ConversationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateConversation(
        [FromBody] CreateConversationRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new CreateConversationCommand(userId, request.Title, request.Configuration);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetConversation(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetConversationQuery(id, userId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Message });
    }

    [HttpGet]
    public async Task<IActionResult> GetConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var query = new GetConversationsQuery(userId, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new SendMessageCommand(id, userId, request.Content, request.Attachments);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : BadRequest(new { error = result.Error.Message });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteConversation(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var command = new DeleteConversationCommand(id, userId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess
            ? NoContent()
            : BadRequest(new { error = result.Error.Message });
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Get from JWT claims
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}
