// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Application.Interfaces;
using NetGPT.Domain.Entities;

namespace NetGPT.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository(ApplicationDbContext context) : IUserRepository
    {
        private readonly ApplicationDbContext context = context;

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await context.Set<User>().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> AddAsync(User user)
        {
            _ = await context.Set<User>().AddAsync(user);
            return user;
        }
    }
}
