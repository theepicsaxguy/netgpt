// Copyright (c) 2025 NetGPT. All rights reserved.

using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NetGPT.Application.DTOs;
using NetGPT.Application.Interfaces;

namespace NetGPT.API.Controllers
{
    [ApiController]
    [Route("attachments")]
    [Authorize]
    public sealed class AttachmentsController(IConfiguration configuration, IFileStorageService fileStorageService) : ControllerBase
    {
        private readonly IConfiguration configuration = configuration;
        private readonly IFileStorageService fileStorageService = fileStorageService;

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAttachment(IFormFile file)
        {
            if (file is null)
            {
                return BadRequest("No file provided");
            }

            long maxSize = configuration.GetValue<long>("AppSettings:AttachmentMaxSizeBytes");

            if (file.Length > maxSize)
            {
                return StatusCode(413, new { error = "Attachment exceeds maximum allowed size" });
            }

            using Stream stream = file.OpenReadStream();
            string storageKey = await fileStorageService.UploadAsync(stream, file.FileName, file.ContentType);
            string url = fileStorageService.GetPublicUrl(storageKey);

            FileAttachmentDto dto = new(url, file.FileName, (int)file.Length, file.ContentType);
            return Ok(dto);
        }
    }
}
