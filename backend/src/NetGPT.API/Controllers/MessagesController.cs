using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using NetGPT.Application.Queries;

namespace NetGPT.API.Controllers;

[ApiController]
[Route("api/v1/conversations/{conversationId}/messages")]
public sealed class MessagesController : ControllerBase
{
    private readonly IMediator _mediator;

    public MessagesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMessages(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var query = new GetMessagesQuery(conversationId, userId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.IsSuccess
            ? Ok(result.Value)
            : NotFound(new { error = result.Error.Message });
    }

    private Guid GetCurrentUserId()
    {
        // TODO: Get from JWT claims
        return Guid.Parse("00000000-0000-0000-0000-000000000001");
    }
}
