// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Infrastructure.Persistence.Entities;

namespace NetGPT.Infrastructure.Persistence.Repositories
{
    public sealed class RefreshTokenRepository(ApplicationDbContext context)
    {
        private readonly ApplicationDbContext context = context;

        public async Task AddAsync(RefreshToken token)
        {
            await context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
        {
            return await context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public Task RevokeAsync(RefreshToken token, Guid? replacedBy = null)
        {
            token.RevokedAt = DateTime.UtcNow;
            token.ReplacedByTokenId = replacedBy;
            _ = context.RefreshTokens.Update(token);
            return Task.CompletedTask;
        }

        public async Task RevokeAllForUserAsync(Guid userId)
        {
            List<RefreshToken> tokens = await context.RefreshTokens.Where(t => t.UserId == userId && t.RevokedAt == null).ToListAsync();
            foreach (RefreshToken? t in tokens)
            {
                t.RevokedAt = DateTime.UtcNow;
            }

            context.RefreshTokens.UpdateRange(tokens);
        }

        public Task<int> SaveChangesAsync()
        {
            return context.SaveChangesAsync();
        }
    }
}
