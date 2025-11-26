// Copyright (c) 2025 NetGPT. All rights reserved.

using System;

namespace NetGPT.Infrastructure.Services
{
    public interface IPasswordHasher
    {
        void CreateHash(string password, out byte[] hash, out byte[] salt);
        bool Verify(string password, byte[] hash, byte[] salt);
    }
}
