// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NetGPT.Application.Interfaces;
using NetGPT.Infrastructure.Persistence.Entities;

namespace NetGPT.Infrastructure.Persistence.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext context;

        public UserRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await context.Set<User>().FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<User> AddAsync(User user)
        {
            await context.Set<User>().AddAsync(user);
            return user;
        }
    }
}
