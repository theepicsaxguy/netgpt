#!/bin/bash

# Create Tool Registry and Plugin System
cat > src/NetGPT.Infrastructure/Tools/IToolRegistry.cs << 'EOF'
using Microsoft.Extensions.AI;

namespace NetGPT.Infrastructure.Tools;

public interface IToolRegistry
{
    void RegisterTool(AIFunction tool);
    IEnumerable<AIFunction> GetAllTools();
    AIFunction? GetTool(string name);
}
EOF

cat > src/NetGPT.Infrastructure/Tools/ToolRegistry.cs << 'EOF'
using Microsoft.Extensions.AI;
using System.Collections.Concurrent;

namespace NetGPT.Infrastructure.Tools;

public sealed class ToolRegistry : IToolRegistry
{
    private readonly ConcurrentDictionary<string, AIFunction> _tools = new();

    public void RegisterTool(AIFunction tool)
    {
        _tools.TryAdd(tool.Metadata.Name, tool);
    }

    public IEnumerable<AIFunction> GetAllTools() => _tools.Values;

    public AIFunction? GetTool(string name) =>
        _tools.TryGetValue(name, out var tool) ? tool : null;
}
EOF

# Create Web Search Tool Plugin
cat > src/NetGPT.Infrastructure/Tools/WebSearchToolPlugin.cs << 'EOF'
using System.ComponentModel;

namespace NetGPT.Infrastructure.Tools;

public sealed class WebSearchToolPlugin
{
    [Description("Search the web for current information")]
    public async Task<string> SearchWeb(
        [Description("The search query")] string query)
    {
        // Implement actual web search (e.g., using Bing API, Google Custom Search)
        await Task.Delay(100); // Simulate API call
        return $"Search results for: {query}\\n1. Example result 1\\n2. Example result 2";
    }
}
EOF

# Create Code Execution Tool Plugin
cat > src/NetGPT.Infrastructure/Tools/CodeExecutionToolPlugin.cs << 'EOF'
using System.ComponentModel;

namespace NetGPT.Infrastructure.Tools;

public sealed class CodeExecutionToolPlugin
{
    [Description("Execute Python code in a sandboxed environment")]
    public async Task<string> ExecutePython(
        [Description("Python code to execute")] string code)
    {
        // Implement sandboxed code execution
        await Task.Delay(100); // Simulate execution
        return $"Execution result: Code executed successfully";
    }

    [Description("Execute JavaScript code")]
    public async Task<string> ExecuteJavaScript(
        [Description("JavaScript code to execute")] string code)
    {
        await Task.Delay(100);
        return $"Execution result: {code.Length} characters executed";
    }
}
EOF

# Create File Processing Tool Plugin
cat > src/NetGPT.Infrastructure/Tools/FileProcessingToolPlugin.cs << 'EOF'
using System.ComponentModel;

namespace NetGPT.Infrastructure.Tools;

public sealed class FileProcessingToolPlugin
{
    [Description("Extract text from a PDF file")]
    public async Task<string> ExtractPdfText(
        [Description("URL or path to PDF file")] string fileUrl)
    {
        await Task.Delay(100);
        return "Extracted text from PDF...";
    }

    [Description("Analyze an image and describe its contents")]
    public async Task<string> AnalyzeImage(
        [Description("URL to image")] string imageUrl)
    {
        await Task.Delay(100);
        return "Image analysis: The image contains...";
    }
}
EOF

# Create API project
cat > src/NetGPT.API/NetGPT.API.csproj << 'EOF'
<Project Sdk="Microsoft.NET.Sdk.Web">

  <ItemGroup>
    <ProjectReference Include="..\NetGPT.Application\NetGPT.Application.csproj" />
    <ProjectReference Include="..\NetGPT.Infrastructure\NetGPT.Infrastructure.csproj" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="Microsoft.AspNetCore.SignalR" Version="1.1.0" />
  </ItemGroup>

</Project>
EOF

# Create Configuration
cat > src/NetGPT.Infrastructure/Configuration/OpenAISettings.cs << 'EOF'
namespace NetGPT.Infrastructure.Configuration;

public sealed class OpenAISettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = "gpt-4o";
    public int MaxTokens { get; set; } = 4000;
}
EOF

# Create Conversations Controller
cat > src/NetGPT.API/Controllers/ConversationsController.cs << 'EOF'
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
EOF

echo "Tools and API created"
