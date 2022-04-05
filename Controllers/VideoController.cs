﻿using System.Threading.Tasks;
using VideoChat.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace VideoChat.Controllers
{
    [ApiController]
    [Route("api/video")]
    public class VideoController : ControllerBase
    {
        readonly IVideoService _videoService;

        public VideoController(IVideoService videoService)
            => _videoService = videoService;

        [HttpGet("token")]
        public IActionResult GetToken()
            => new JsonResult(new { token = _videoService.GetTwilioJwt(User.Identity.Name) });

        [HttpGet("rooms")]
        public async Task<IActionResult> GetRooms()
            => new JsonResult(await _videoService.GetAllRoomsAsync());

        [HttpGet("startrecording")]
        public async Task<IActionResult> StartRecording(string roomSid)
            => new JsonResult(await _videoService.StartRecording(roomSid));

        [HttpGet("stoprecording")]
        public async Task<IActionResult> StopRecording(string roomSid)
            => new JsonResult(await _videoService.StopRecording(roomSid));

        [HttpGet("createconversation")]
        public async Task<IActionResult> CreateConversation(string roomName)
            => new JsonResult(await _videoService.CreateConversation(roomName));
    }
}