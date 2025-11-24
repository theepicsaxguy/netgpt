using System.Collections.Generic;

namespace NetGPT.Application.DTOs;

public record ErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? ValidationErrors = null);
