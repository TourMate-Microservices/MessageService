﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TourMate.MessageService.Repositories.Models;
using TourMate.MessageService.Services.IServices;
using TourMate.MessageService.Repositories.RequestModels;


namespace TourMate.MessageService.Api.Controllers
{
    [Authorize]
    [Route("api/files")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IFileService fileService;
        public FileController(IFileService fileService)
        {
            this.fileService = fileService;
        }
        [HttpGet]
        public async Task<ActionResult<FileStorage>> GetFile(string id)
        {
            var file = await fileService.GetFileAsync(id);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }
        [HttpPost]
        public async Task<IActionResult> UploadFile(FileUploadModel data)
        {
            var res = await fileService.UploadFile(data.Convert());
            return res ? Ok() : BadRequest("File upload failed");
        }
        [HttpGet("message/{messageId}")]
        public async Task<ActionResult<FileStorage>> GetFileOfMessage(int messageId)
        {
            var file = await fileService.GetFileOfMessage(messageId);
            if (file == null)
            {
                return NotFound();
            }
            return Ok(file);
        }
    }
}
