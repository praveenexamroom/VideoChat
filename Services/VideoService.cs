using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Base;
using Twilio.Jwt.AccessToken;
using Twilio.Rest.Conversations.V1;
using Twilio.Rest.Video.V1;
using Twilio.Rest.Video.V1.Room;
using Twilio.Types;
using VideoChat.Abstractions;
using VideoChat.Models;
using VideoChat.Options;
using ParticipantStatus = Twilio.Rest.Video.V1.Room.ParticipantResource.StatusEnum;
using ConversationParticipant = Twilio.Rest.Conversations.V1.Conversation.ParticipantResource;

namespace VideoChat.Services
{
    public class VideoService : IVideoService
    {
        readonly TwilioSettings _twilioSettings;

        public VideoService(Microsoft.Extensions.Options.IOptions<TwilioSettings> twilioOptions)
        {
            _twilioSettings =
                twilioOptions?.Value
             ?? throw new ArgumentNullException(nameof(twilioOptions));

            TwilioClient.Init(_twilioSettings.ApiKey, _twilioSettings.ApiSecret);
        }

        public string GetTwilioJwt(string identity)
        {
            var grants = new HashSet<IGrant>
            {
                new VideoGrant(),
                new ChatGrant {ServiceSid = _twilioSettings.ChatSid},
            };

            return new Token(_twilioSettings.AccountSid,
                         _twilioSettings.ApiKey,
                         _twilioSettings.ApiSecret,
                         identity ?? Guid.NewGuid().ToString(),
                         grants: grants).ToJwt();
        }

        public async Task<IEnumerable<RoomDetails>> GetAllRoomsAsync()
        {
            var rooms = await RoomResource.ReadAsync();
            var tasks = rooms.Select(
                room => GetRoomDetailsAsync(
                    room,
                    ParticipantResource.ReadAsync(
                        room.Sid,
                        ParticipantStatus.Connected)));

            return await Task.WhenAll(tasks);

            static async Task<RoomDetails> GetRoomDetailsAsync(
                RoomResource room,
                Task<ResourceSet<ParticipantResource>> participantTask)
            {
                var participants = await participantTask;
                return new RoomDetails
                {
                    Name = room.UniqueName,
                    MaxParticipants = room.MaxParticipants ?? 0,
                    ParticipantCount = participants.ToList().Count
                };
            }
        }

        public async Task<bool> StartRecording(string roomSid)
        {
            var isRecordingStarted = false;
            var recordingRules = await RecordingRulesResource.UpdateAsync(
                rules: new List<RecordingRule>(){
                            new RecordingRule(RecordingRule.TypeEnum.Include, true, null, null, null)
                },
                pathRoomSid: roomSid
            );

                if(recordingRules.RoomSid == roomSid)
                    isRecordingStarted = true;

                return isRecordingStarted;
            }

        public async Task<bool> StopRecording(string roomSid)
        {
            var isRecordingStopped = false;
            var recordingRules = await RecordingRulesResource.UpdateAsync(
                rules: new List<RecordingRule>(){
                            new RecordingRule(RecordingRule.TypeEnum.Exclude, true, null, null, null)
                },
                pathRoomSid: roomSid
            );

            if (recordingRules.RoomSid == roomSid)
                isRecordingStopped = true;

            return isRecordingStopped;
        }


        public async Task<string> CreateConversation(string roomName)
        {
            var conversation = await ConversationResource.CreateAsync(friendlyName: roomName);
            return conversation.Sid;
        }

        public async Task<string> FetchConversation(string conversationSid)
        {
            var conversation = await ConversationResource.FetchAsync(pathSid: conversationSid);
            return conversation.ChatServiceSid;
        }

        public async Task<string> CreateParticipant(string identity, string pathConversationSid)
        {
            var participant = await ConversationParticipant.CreateAsync(
                identity: identity,
                pathConversationSid: pathConversationSid
            );
            return participant.Sid;
        }
    }
}