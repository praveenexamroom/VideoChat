using System.Collections.Generic;
using System.Threading.Tasks;
using VideoChat.Models;

namespace VideoChat.Abstractions
{
    public interface IVideoService
    {
        string GetTwilioJwt(string identity);
        Task<IEnumerable<RoomDetails>> GetAllRoomsAsync();
        Task<bool> StartRecording(string roomSid);
        Task<bool> StopRecording(string roomSid);
        Task<string> CreateConversation(string roomName);
    }
}
