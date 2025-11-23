#!/bin/bash

# Create remaining Query Handlers
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

    public async Task<Result<ConversationResponse>> Handle(GetConversationQuery request, CancellationToken cancellationToken)
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

    public async Task<Result<PaginatedResponse<ConversationResponse>>> Handle(GetConversationsQuery request, CancellationToken cancellationToken)
    {
        var conversations = await _repository.GetByUserIdAsync(request.UserId, request.Page, request.PageSize, cancellationToken);
        var totalCount = await _repository.CountByUserIdAsync(request.UserId, cancellationToken);
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var responses = conversations.Select(_mapper.ToResponse).ToList();

        return new PaginatedResponse<ConversationResponse>(
            responses,
            request.Page,
            request.PageSize,
            totalCount,
            totalPages);
    }
}
EOF

cat > src/NetGPT.Application/Handlers/GetMessagesHandler.cs << 'EOF'
using MediatR;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Application.Queries;
using NetGPT.Domain.Interfaces;
using NetGPT.Domain.Primitives;

namespace NetGPT.Application.Handlers;

public sealed class GetMessagesHandler : IRequestHandler<GetMessagesQuery, Result<List<MessageResponse>>>
{
    private readonly IConversationRepository _repository;
    private readonly IConversationMapper _mapper;

    public GetMessagesHandler(IConversationRepository repository, IConversationMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<Result<List<MessageResponse>>> Handle(GetMessagesQuery request, CancellationToken cancellationToken)
    {
        var conversation = await _repository.GetByIdAsync(request.ConversationId, cancellationToken);
        if (conversation is null)
        {
            return Result.Failure<List<MessageResponse>>(
                new Error("Conversation.NotFound", "Conversation not found"));
        }

        if (conversation.UserId != request.UserId)
        {
            return Result.Failure<List<MessageResponse>>(
                new Error("Conversation.Unauthorized", "Unauthorized access"));
        }

        var messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(_mapper.ToMessageResponse)
            .ToList();

        return messages;
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

# Create Mapper Interface and Implementation
cat > src/NetGPT.Application/Interfaces/IConversationMapper.cs << 'EOF'
using NetGPT.Application.DTOs;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Application.Interfaces;

public interface IConversationMapper
{
    ConversationResponse ToResponse(Conversation conversation);
    MessageResponse ToMessageResponse(Message message);
}
EOF

cat > src/NetGPT.Application/Mappings/ConversationMapper.cs << 'EOF'
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Application.Mappings;

public sealed class ConversationMapper : IConversationMapper
{
    public ConversationResponse ToResponse(Conversation conversation)
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

    public MessageResponse ToMessageResponse(Message message)
    {
        return new MessageResponse(
            message.Id,
            message.Content.Role.ToString(),
            message.Content.Text,
            message.Content.Attachments?.Select(a => new FileAttachmentDto(
                a.FileName,
                a.FileUrl,
                a.ContentType,
                a.FileSizeBytes)).ToList(),
            message.Metadata != null
                ? new MessageMetadataDto(
                    message.Metadata.TokenCount,
                    message.Metadata.ResponseTime.TotalMilliseconds,
                    message.Metadata.ModelUsed,
                    message.Metadata.ToolsInvoked)
                : null,
            message.CreatedAt);
    }
}
EOF

# Create Validators
cat > src/NetGPT.Application/Validators/CreateConversationValidator.cs << 'EOF'
using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public sealed class CreateConversationValidator : AbstractValidator<CreateConversationCommand>
{
    public CreateConversationValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        
        When(x => x.Configuration != null, () =>
        {
            RuleFor(x => x.Configuration!.Temperature)
                .InclusiveBetween(0f, 2f)
                .When(x => x.Configuration!.Temperature.HasValue);
            
            RuleFor(x => x.Configuration!.MaxTokens)
                .InclusiveBetween(1, 128000)
                .When(x => x.Configuration!.MaxTokens.HasValue);
        });
    }
}
EOF

cat > src/NetGPT.Application/Validators/SendMessageValidator.cs << 'EOF'
using FluentValidation;
using NetGPT.Application.Commands;

namespace NetGPT.Application.Validators;

public sealed class SendMessageValidator : AbstractValidator<SendMessageCommand>
{
    public SendMessageValidator()
    {
        RuleFor(x => x.ConversationId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Content)
            .NotEmpty()
            .MaximumLength(32000);
    }
}
EOF

# Create missing Application interfaces
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
        CancellationToken cancellationToken = default);
}
EOF

cat > src/NetGPT.Application/Interfaces/IStreamingService.cs << 'EOF'
namespace NetGPT.Application.Interfaces;

public interface IStreamingService
{
    Task StreamResponseAsync(
        Guid conversationId,
        Guid messageId,
        IAsyncEnumerable<string> chunks,
        CancellationToken cancellationToken = default);
}
EOF

# Create EF Core Configurations
cat > src/NetGPT.Infrastructure/Persistence/Configurations/ConversationConfiguration.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetGPT.Domain.Aggregates.ConversationAggregate;

namespace NetGPT.Infrastructure.Persistence.Configurations;

public sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("Conversations");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.UserId).IsRequired();
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.TokensUsed).IsRequired();
        
        builder.OwnsOne(c => c.AgentConfiguration, config =>
        {
            config.Property(a => a.ModelName).HasColumnName("ModelName").HasMaxLength(50);
            config.Property(a => a.Temperature).HasColumnName("Temperature");
            config.Property(a => a.MaxTokens).HasColumnName("MaxTokens");
            config.Property(a => a.TopP).HasColumnName("TopP");
            config.Property(a => a.FrequencyPenalty).HasColumnName("FrequencyPenalty");
            config.Property(a => a.PresencePenalty).HasColumnName("PresencePenalty");
        });
        
        builder.HasMany(typeof(Message))
            .WithOne()
            .HasForeignKey("ConversationId")
            .OnDelete(DeleteBehavior.Cascade);
        
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
        builder.ToTable("Messages");
        
        builder.HasKey(m => m.Id);
        
        builder.Property(m => m.ConversationId).IsRequired();
        
        builder.OwnsOne(m => m.Content, content =>
        {
            content.Property(c => c.Text).HasColumnName("Text").IsRequired();
            content.Property(c => c.Role).HasColumnName("Role").IsRequired();
            content.OwnsMany(c => c.Attachments, attachment =>
            {
                attachment.ToTable("MessageAttachments");
                attachment.Property(a => a.FileName).HasMaxLength(255);
                attachment.Property(a => a.FileUrl).HasMaxLength(1000);
                attachment.Property(a => a.ContentType).HasMaxLength(100);
                attachment.Property(a => a.FileSizeBytes);
            });
        });
        
        builder.OwnsOne(m => m.Metadata, metadata =>
        {
            metadata.Property(m => m.TokenCount).HasColumnName("TokenCount");
            metadata.Property(m => m.ResponseTime).HasColumnName("ResponseTimeMs");
            metadata.Property(m => m.ModelUsed).HasColumnName("ModelUsed").HasMaxLength(50);
            metadata.Property(m => m.ToolsInvoked).HasColumnName("ToolsInvoked");
        });
        
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.CreatedAt);
    }
}
EOF

# Create ApplicationDbContext
cat > src/NetGPT.Infrastructure/Persistence/ApplicationDbContext.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Primitives;
using NetGPT.Infrastructure.Persistence.Configurations;

namespace NetGPT.Infrastructure.Persistence;

public sealed class ApplicationDbContext : DbContext
{
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new ConversationConfiguration());
        modelBuilder.ApplyConfiguration(new MessageConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<Entity>()
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                var events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        // TODO: Publish domain events via MediatR
        
        return result;
    }
}
EOF

# Create Repository Implementation
cat > src/NetGPT.Infrastructure/Persistence/Repositories/ConversationRepository.cs << 'EOF'
using Microsoft.EntityFrameworkCore;
using NetGPT.Domain.Aggregates.ConversationAggregate;
using NetGPT.Domain.Interfaces;

namespace NetGPT.Infrastructure.Persistence.Repositories;

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

    public async Task<List<Conversation>> GetByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
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

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var conversation = await _context.Conversations.FindAsync(new object[] { id }, cancellationToken);
        if (conversation != null)
        {
            _context.Conversations.Remove(conversation);
        }
    }

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Conversations
            .Where(c => c.UserId == userId)
            .CountAsync(cancellationToken);
    }
}
EOF

# Create UnitOfWork
cat > src/NetGPT.Infrastructure/Persistence/Repositories/UnitOfWork.cs << 'EOF'
using Microsoft.EntityFrameworkCore.Storage;
using NetGPT.Domain.Interfaces;

namespace NetGPT.Infrastructure.Persistence.Repositories;

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

echo "All remaining files created"
