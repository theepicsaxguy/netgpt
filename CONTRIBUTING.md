# Contributing to NetGPT

## Adding New Features

### 1. Add New Tool/Plugin
Create plugin class in `src/NetGPT.Infrastructure/Tools/`:
```csharp
public class NewToolPlugin
{
    [Description("Tool description")]
    public async Task<string> ToolMethod(
        [Description("Parameter")] string param)
    {
        throw new NotImplementedException();
    }
}
```

Register in `Program.cs`:
```csharp
var tools = AIFunctionFactory.Create(new NewToolPlugin());
foreach (var tool in tools)
{
    registry.RegisterTool(tool);
}
```

### 2. Add New Command
1. Create command in `Application/Commands/`
2. Create handler in `Application/Handlers/`
3. Create validator in `Application/Validators/`

### 3. Add New Query
1. Create query in `Application/Queries/`
2. Create handler in `Application/Handlers/`

## Code Standards
- Max 200 lines per file
- Follow DDD, SOLID, SRP, SOC
- Use Result pattern for error handling
- All public APIs must have XML comments
