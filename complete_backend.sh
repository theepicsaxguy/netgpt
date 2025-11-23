#!/bin/bash

# Application Interfaces
cat > src/NetGPT.Application/Interfaces/IAgentOrchestrator.cs << 'EOF'
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Agents;

namespace NetGPT.Application.Interfaces;

public interface IAgentOrchestrator
{
    Task<Result<AgentResponse>> ExecuteAsync(
        Conversation conversation,
        string userMessage,
        CancellationToken cancellationToken);
}
EOF

cat > src/NetGPT.Application/Interfaces/IConversationMapper.cs << 'EOF'
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Interfaces;

public interface IConversationMapper
{
    Result<ConversationResponse> ToResponse(Conversation conversation);
    Result<MessageResponse> ToMessageResponse(Message message);
}
EOF

cat > src/NetGPT.Application/Services/ConversationMapper.cs << 'EOF'
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Services;

public sealed class ConversationMapper : IConversationMapper
{
    public Result<ConversationResponse> ToResponse(Conversation conversation)
    {
        return new ConversationResponse(
            conversation.Id,
            conversation.Title,
            conversation.Status.ToString(),
            conversation.CreatedAt,
            conversation.UpdatedAt,
            conversation.Messages.Count,
            conversation.TokensUsed,
            new AgentConfigurationDto(
                conversation.AgentConfiguration.ModelName,
                conversation.AgentConfiguration.Temperature,
                conversation.AgentConfiguration.MaxTokens,
                conversation.AgentConfiguration.TopP,
                conversation.AgentConfiguration.FrequencyPenalty,
                conversation.AgentConfiguration.PresencePenalty));
    }

    public Result<MessageResponse> ToMessageResponse(Message message)
    {
        var attachments = message.Content.Attachments?
            .Select(a => new FileAttachmentDto(a.FileName, a.FileUrl, a.ContentType, a.FileSizeBytes))
            .ToList();

        MessageMetadataDto? metadata = null;
        if (message.Metadata != null)
        {
            metadata = new MessageMetadataDto(
                message.Metadata.TokenCount,
                message.Metadata.ResponseTime.TotalMilliseconds,
                message.Metadata.ModelUsed,
                message.Metadata.ToolsInvoked);
        }

        return new MessageResponse(
            message.Id,
            message.Content.Role.ToString(),
            message.Content.Text,
            attachments,
            metadata,
            message.CreatedAt);
    }
}
EOF

# Query Handlers
cat > src/NetGPT.Application/Handlers/GetConversationHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class GetConversationHandler : IRequestHandler<GetConversationQuery, Result<ConversationResponse>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetConversationHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<ConversationResponse>> Handle(
        GetConversationQuery request,
        CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        
        if (conversation is null)
        {
            return Result.Failure<ConversationResponse>(
                new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (conversation.UserId != request.UserId)
        {
            return Result.Failure<ConversationResponse>(
                new Error("Conversation.Unauthorized", "Unauthorized access"));
        }

        return _mapper.ToResponse(conversation);
    }
}
EOF

cat > src/NetGPT.Application/Handlers/GetConversationsHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class GetConversationsHandler : IRequestHandler<GetConversationsQuery, Result<PaginatedResponse<ConversationResponse>>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetConversationsHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<PaginatedResponse<ConversationResponse>>> Handle(
        GetConversationsQuery request,
        CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByUserIdAsync(
            request.UserId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var total = await _repository.CountByUserIdAsync(request.UserId, cancellationToken);

        var responses = conversations
            .Select(c => _mapper.ToResponse(c).Value)
            .ToList();

        return new PaginatedResponse<ConversationResponse>(
            responses,
            request.Page,
            request.PageSize,
            total,
            (int)Math.Ceiling(total / (double)request.PageSize));
    }
}
EOF

cat > src/NetGPT.Application/Handlers/DeleteConversationHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.Commands;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class DeleteConversationHandler : IRequestHandler<DeleteConversationCommand, Result>
{
    private readonly IConversationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteConversationHandler(IConversationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(DeleteConversationCommand request, CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        
        if (conversation is null)
        {
            return Result.Failure(new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (conversation.UserId != request.UserId)
        {
            return Result.Failure(new Error("Conversation.Unauthorized", "Unauthorized access"));
        }

        await _repository.DeleteAsync(request.ConversationId, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
EOF

# Complete Infrastructure DbContext
cat > src/NetGPT.Infrastructure/Persistence/ApplicationDbContext.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<Conversation> Conversations { get; set; } = null!;
    public DbSet<Message> Messages { get; set; } = null!;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
EOF

cat > src/NetGPT.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        
        builder.OwnsOne(c => c.AgentConfiguration, config =>
        {
            config.Property(a => a.ModelName).HasMaxLength(50);
        });

        builder.HasMany<Message>()
            .WithOne()
            .HasForeignKey(m => m.ConversationId);

        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.CreatedAt);
    }
}
EOF

cat > src/NetGPT.Infrastructure/Persistence/Configurations/MessageConfiguration.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Infrastructure.Persistence.Configurations;

public sealed class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);
        builder.Property(m => m.ConversationId).IsRequired();

        builder.OwnsOne(m => m.Content, content =>
        {
            content.Property(c => c.Text).IsRequired();
            content.Property(c => c.Role).IsRequired();
        });

        builder.OwnsOne(m => m.Metadata);
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.CreatedAt);
    }
}
EOF

cat > src/NetGPT.Infrastructure/Repositories/ConversationRepository.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Persistence;

namespace NetGPT.Infrastructure.Repositories;

public sealed class ConversationRepository : IConversationRepository
{
    private readonly ApplicationDbContext _context;

    public ConversationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Conversation>> GetByUserIdAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<Conversation> AddAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        await _context.Conversations.AddAsync(conversation, cancellationToken);
        return conversation;
    }

    public Task UpdateAsync(Conversation conversation, CancellationToken cancellationToken = default)
    {
        _context.Conversations.Update(conversation);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conversation = _context.Conversations.Find(id);
        if (conversation != null)
        {
            _context.Conversations.Remove(conversation);
        }
        return Task.CompletedTask;
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .CountAsync(cancellationToken);
    }
}
EOF

cat > src/NetGPT.Infrastructure/Repositories/UnitOfWork.cs << 'EOF'
using Microsoft.EntityFrameworkCore.Storage;
using NetGPT.Domain.Interfaces;
using NetGPT.Infrastructure.Persistence;

namespace NetGPT.Infrastructure.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
EOF

echo "Complete backend structure created"
