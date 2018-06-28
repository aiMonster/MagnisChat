using Common.DTO.Messages;
using Managers.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagnisChatAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    public class FilesController : Controller
    {        
        private readonly IFileManager _fileManager;

        public FilesController(IFileManager fileManager)
        {            
            _fileManager = fileManager;
        }

        [HttpGet("{id}")]
        public IActionResult GetFile([FromRoute]Guid id)
        {
            var response = _fileManager.GetFile(id);

            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }

            return Ok(response);
        }
               
        [HttpPost("{id}")]
        public async Task<IActionResult> UploadFilePart([FromRoute] Guid id, [FromBody]FilePartDTO file)
        {
            var response = await _fileManager.UploadPartFile(file, id);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }
            return Ok(response);
        }
        
        [HttpGet("{id}/DownloadPart/{part}")]
        public async Task<IActionResult> DownloadFilePart([FromRoute] Guid id, int part)
        {
            var response = await _fileManager.DownloadPartFile(id, part);
            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }
            return Ok(response);
        }
    }
}
