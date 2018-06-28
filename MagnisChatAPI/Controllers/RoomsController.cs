using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.DTO.Communication;
using Common.DTO.Messages;
using Common.DTO.Rooms;
using Managers.Interfaces;
using Managers.WebSockets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MagnisChatAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]    
    public class RoomsController : Controller
    {
        private readonly IRoomsManager _roomManager;
        private readonly IMessageManager _messageManager;        
        
        public RoomsController(IRoomsManager roomManager, IMessageManager messageManager)
        {
            _roomManager = roomManager;
            _messageManager = messageManager;           
        }

        private Guid GetUserId()
        {
            return new Guid(User.Claims.Where(c => c.Type == "Id").FirstOrDefault().Value);
        }

        #region Rooms
        [HttpGet]    
        public IActionResult GetAllRooms()
        {
            var response = _roomManager.GetAllRooms();
            return Ok(response);
        }
       
        [HttpGet("My")]        
        public IActionResult GetUserRooms()
        {            
            var response =  _roomManager.GetUserRooms(GetUserId());
            return Ok(response);
        }

        [HttpGet("Other")]
        public IActionResult GetOtherRooms()
        {            
            var response = _roomManager.GetOtherRooms(GetUserId());
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromBody] RoomRequest request)
        {          
            var response = await _roomManager.CreateRoomAsync(request, GetUserId());
            return Ok(response);
        }

        [HttpPut("{id}/Participate")]
        public async Task<IActionResult> ParticipateRoom([FromRoute]Guid id)
        {            
            var response = await _roomManager.ParticipateRoomAsync(id, GetUserId());

            if(response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }

            return Ok(response);
        }
        #endregion
        
        #region Messages
        [HttpPost("{id}/TextMessages")]
        public async Task<IActionResult> SendTextMessage([FromRoute] Guid id, [FromBody]TextMessageRequest request)
        {            
            var response = await _messageManager.SendTextMessageAsync(request, id, GetUserId());

            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }

            return Ok(response);
        }

        [HttpPost("{id}/FileMessages")]
        public async Task<IActionResult> SendFileMessage([FromRoute] Guid id, [FromBody]FileMessageRequest request)
        {            
            var response = await _messageManager.SendFileMessageAsync(request, id, GetUserId());

            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }

            return Ok(response);
        }

        [HttpGet("{id}/Messages")]
        public IActionResult GetMessages([FromRoute] Guid id)
        {
            var response = _messageManager.GetMessages(id);

            if (response.Error != null)
            {
                return StatusCode(response.Error.ErrorCode, response);
            }

            return Ok(response);
        }
        #endregion               
        
    }
}
