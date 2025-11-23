#!/bin/bash

# This script creates the complete backend structure

# Create all directories
mkdir -p src/NetGPT.Domain/{Primitives,Aggregates/ConversationAggregate,ValueObjects,Events,Interfaces,Exceptions}
mkdir -p src/NetGPT.Application/{Commands,Queries,Handlers,DTOs,Services,Interfaces,Validators,Mappings}
mkdir -p src/NetGPT.Infrastructure/{Persistence,Agents,Tools,Services,External}
mkdir -p src/NetGPT.API/{Controllers,Hubs,Middleware,Filters}

echo "Backend structure created successfully"
