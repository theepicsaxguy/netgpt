// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using NetGPT.Application.DTOs;
using NetGPT.Infrastructure.Tools;

namespace NetGPT.API.Controllers
{
    /// <summary>
    /// Controller for managing and invoking tools.
    /// </summary>
    [ApiController]
    [Route("api/tools")]
    [Authorize]
    public sealed partial class ToolsController(
        IToolRegistry toolRegistry,
        ILogger<ToolsController> logger) : ControllerBase
    {
        private readonly IToolRegistry toolRegistry = toolRegistry;
        private readonly ILogger<ToolsController> logger = logger;

        /// <summary>
        /// List all registered tools.
        /// </summary>
        [HttpGet]
        public IActionResult ListTools()
        {
            IEnumerable<AIFunction> allTools = toolRegistry.GetAllTools();
            List<ToolInfoDto> toolInfos = allTools.Select(tool => new ToolInfoDto(
                Name: tool.Name,
                Description: tool.Description,
                Parameters: [])).ToList();

            return Ok(toolInfos);
        }

        /// <summary>
        /// Get metadata for a specific tool.
        /// </summary>
        [HttpGet("{name}")]
        public IActionResult GetTool(string name)
        {
            AIFunction? tool = toolRegistry.GetTool(name);
            if (tool == null)
            {
                return NotFound(new { error = $"Tool '{name}' not found" });
            }

            ToolInfoDto toolInfo = new(
                Name: tool.Name,
                Description: tool.Description,
                Parameters: []);

            return Ok(toolInfo);
        }

        /// <summary>
        /// Invoke a tool synchronously.
        /// </summary>
        [HttpPost("{name}/invoke")]
        public async Task<IActionResult> InvokeTool(
            string name,
            [FromBody] ToolInvokeRequest request)
        {
            AIFunction? tool = toolRegistry.GetTool(name);
            if (tool == null)
            {
                return NotFound(new { error = $"Tool '{name}' not found" });
            }

            DateTime startTime = DateTime.UtcNow;
            Stopwatch stopwatch = Stopwatch.StartNew();

            try
            {
                // Create AIFunctionArguments from the request dictionary
                AIFunctionArguments args = new();
                foreach (System.Collections.Generic.KeyValuePair<string, object> kvp in request.Arguments)
                {
                    args[kvp.Key] = kvp.Value;
                }

                // Invoke the tool
                object? result = await tool.InvokeAsync(args);

                stopwatch.Stop();

                ToolInvokeResponse response = new(
                    ToolName: name,
                    Result: result,
                    Success: true,
                    ErrorMessage: null,
                    InvokedAt: startTime,
                    DurationMs: stopwatch.Elapsed.TotalMilliseconds);

                return Ok(response);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                LogToolInvocationError(logger, ex, name);

                ToolInvokeResponse response = new(
                    ToolName: name,
                    Result: null,
                    Success: false,
                    ErrorMessage: ex.Message,
                    InvokedAt: startTime,
                    DurationMs: stopwatch.Elapsed.TotalMilliseconds);

                return BadRequest(response);
            }
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Error invoking tool {ToolName}")]
        private static partial void LogToolInvocationError(ILogger logger, Exception ex, string toolName);
    }
}
