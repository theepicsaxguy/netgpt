// Copyright (c) 2025 NetGPT. All rights reserved.

using System;
using System.Threading.Tasks;
using NetGPT.Infrastructure.Persistence.Entities;

namespace NetGPT.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);

        Task<User> AddAsync(User user);
    }
}
