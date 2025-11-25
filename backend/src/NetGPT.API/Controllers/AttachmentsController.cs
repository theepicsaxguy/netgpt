// Copyright (c) 2025 NetGPT. All rights reserved.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetGPT.Application.Interfaces;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("api/v1/attachments")]
    [Authorize]
    public sealed class AttachmentsController(IConfiguration configuration, IFileStorageService fileStorageService) : ControllerBase
    {
        private readonly IConfiguration configuration = configuration;
        private readonly IFileStorageService fileStorageService = fileStorageService;

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            long maxSize = configuration.GetValue<long>("AppSettings:AttachmentMaxSizeBytes");

            if (file is not null && file.Length > maxSize)
            {
                return StatusCode(413, new { error = "Attachment exceeds maximum allowed size" });
            }

            var result = await fileStorageService.SaveAsync(file);
            return Ok(result);
        }
    }
}
