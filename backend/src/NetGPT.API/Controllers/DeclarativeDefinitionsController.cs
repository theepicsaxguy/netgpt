// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Entities;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Declarative;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("api/declarative/definitions")]

    /// <summary>
    /// Controller for managing declarative definitions.
    /// </summary>
    public sealed partial class DeclarativeDefinitionsController(
        IDefinitionRepository repo,
        IDeclarativeLoader loader,
        IAgentOrchestrator orchestrator,
        ILogger<DeclarativeDefinitionsController> logger) : ControllerBase
    {
        private readonly IDefinitionRepository repo = repo;
        private readonly IDeclarativeLoader loader = loader;
        private readonly IAgentOrchestrator orchestrator = orchestrator;
        private readonly ILogger<DeclarativeDefinitionsController> logger = logger;

        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> Create([FromBody] CreateDefinitionRequest request)
        {
            if (request is null)
            {
                return BadRequest("Request body is required");
            }

            // Basic YAML parse validation to provide early feedback about syntax
            IDeserializer deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            try
            {
                _ = deserializer.Deserialize<object>(request.ContentYaml);
            }
            catch (YamlException yex)
            {
                // Return parser location and message
                return BadRequest(new { message = yex.Message, line = yex.Start.Line + 1, column = yex.Start.Column + 1 });
            }

            DefinitionEntity def = new()
            {
                Name = request.Name,
                Kind = request.Kind,
                ContentYaml = request.ContentYaml,
                CreatedBy = User?.Identity?.Name ?? "api",
            };

            try
            {
                _ = await loader.LoadAsync(def);
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new { error = ex.Message });
            }

            try
            {
                DefinitionEntity created = await repo.CreateAsync(def);
                DefinitionDto dto = new(created.Id, created.Name, created.Kind, created.Version, created.CreatedBy, created.CreatedAtUtc);
                return CreatedAtAction(nameof(Get), new { id = created.Id }, dto);
            }
            catch (InvalidOperationException inv)
            {
                // Repository-level validation (e.g., raw secrets) -> 422
                return UnprocessableEntity(new { error = inv.Message });
            }
            catch (Exception ex)
            {
                LogCreateError(logger, ex, request.Name);
                throw;
            }
        }

        [HttpGet]
        public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            IEnumerable<DefinitionEntity> items = await repo.ListLatestAsync(page, pageSize);
            IEnumerable<DefinitionDto> dtos = items.Select(d => new DefinitionDto(d.Id, d.Name, d.Kind, d.Version, d.CreatedBy, d.CreatedAtUtc));
            return Ok(dtos);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Get([FromRoute] Guid id)
        {
            DefinitionEntity? def = await repo.GetByIdAsync(id);
            if (def == null)
            {
                return NotFound();
            }

            DefinitionDto dto = new(def.Id, def.Name, def.Kind, def.Version, def.CreatedBy, def.CreatedAtUtc);
            return Ok(dto);
        }

        [HttpPost("{id:guid}/execute")]
        [Authorize]
        public async Task<IActionResult> Execute([FromRoute] Guid id, [FromBody] ExecuteRequest? request)
        {
            DefinitionEntity? def = await repo.GetByIdAsync(id);
            if (def == null)
            {
                return NotFound();
            }

            Guid executionId = Guid.NewGuid();
            DateTime startTime = DateTime.UtcNow;
            try
            {
                IAgentExecutable exec = await loader.LoadAsync(def);

                Result<AgentResponse> result = await orchestrator.ExecuteDefinitionAsync(def, exec, request?.Input ?? string.Empty);

                DateTime endTime = DateTime.UtcNow;

                if (result.IsSuccess)
                {
                    LogExecutionCompleted(logger, def.Id, def.Version, executionId, startTime, endTime, "success");
                    return Ok(new { definitionId = def.Id, version = def.Version, executionId, startTime, endTime, result = result.Value });
                }
                else
                {
                    LogExecutionFailed(logger, def.Id, def.Version, executionId, startTime, endTime);
                    return UnprocessableEntity(new { definitionId = def.Id, executionId, error = result.Error.Message });
                }
            }
            catch (InvalidOperationException inv)
            {
                LogExecutionFailedInvalidOp(logger, inv, def.Id, def.Version, executionId);
                return UnprocessableEntity(new { definitionId = def.Id, executionId, error = inv.Message });
            }
            catch (Exception ex)
            {
                LogExecutionError(logger, ex, def.Id, def.Version, executionId);
                return StatusCode(500, new { definitionId = def.Id, executionId, error = ex.Message });
            }
        }

        [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to create declarative definition {Name}")]
        private static partial void LogCreateError(ILogger logger, Exception? ex, string name);

        [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Declarative execution completed: definitionId={DefinitionId} version={Version} executionId={ExecutionId} start={Start} end={End} outcome={Outcome}")]
        private static partial void LogExecutionCompleted(ILogger logger, Guid definitionId, int version, Guid executionId, DateTime start, DateTime end, string outcome);

        [LoggerMessage(EventId = 3, Level = LogLevel.Warning, Message = "Declarative execution failed: definitionId={DefinitionId} version={Version} executionId={ExecutionId} start={Start} end={End} outcome=failure")]
        private static partial void LogExecutionFailed(ILogger logger, Guid definitionId, int version, Guid executionId, DateTime start, DateTime end);

        [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Declarative execution failed for {DefinitionId} v{Version} executionId={ExecutionId}")]
        private static partial void LogExecutionFailedInvalidOp(ILogger logger, Exception? ex, Guid definitionId, int version, Guid executionId);

        [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Declarative execution error for {DefinitionId} v{Version} executionId={ExecutionId}")]
        private static partial void LogExecutionError(ILogger logger, Exception? ex, Guid definitionId, int version, Guid executionId);

        public record CreateDefinitionRequest(string Name, string Kind, string ContentYaml);

        public record DefinitionDto(Guid Id, string Name, string Kind, int Version, string CreatedBy, DateTime CreatedAtUtc);

        public record ExecuteRequest(string? Input);
    }
}
