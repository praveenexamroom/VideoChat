using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenTokSDK;
using VideoChat.Options;

namespace api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SessionController : ControllerBase
    {
        public IOptions<TokboxSettings> Config { get; }
        public Session Session { get; protected set; }
        public OpenTok OpenTok { get; protected set; }
        private readonly IOptions<TokboxSettings> _config;

        public SessionController(IOptions<TokboxSettings> config)
        {
            _config = config;
        }

        [HttpPost("CreateSession")]
        public ActionResult<string> CreateSession(string username, string location = "", MediaMode mediaMode = MediaMode.ROUTED, ArchiveMode archiveMode = ArchiveMode.ALWAYS)
        {


            OpenTokService();
            string sessionId = OpenTok.CreateSession(location, mediaMode, archiveMode).Id;
            return sessionId;

        }
        [HttpPost("GetToken")]
        public ActionResult<string> GetToken(string username, string sessionId, Role role = Role.PUBLISHER)
        {
            double inOneWeek = (DateTime.UtcNow.Add(TimeSpan.FromDays(7)).Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            OpenTokService();
            string token = OpenTok.GenerateToken(sessionId, role, expireTime: inOneWeek);
            return token;
        }

        [HttpPost("StartArchive")]
        public ActionResult<Guid> StartArchive(string sessionId, string name = "", bool hasVideo = true, bool hasAudio = true, OutputMode outputMode = OutputMode.COMPOSED, string resolution = null, ArchiveLayout layout = null)
        {
            OpenTokService();
            return OpenTok.StartArchive(sessionId, name, hasAudio, hasVideo, outputMode, resolution, layout).Id;

        }

        [HttpPost("StopArchive")]
        public Archive StopArchive(string archiveId)
        {
            OpenTokService();
            return OpenTok.StopArchive(archiveId);

        }

        [HttpPost("GetArchive")]
        public ActionResult<string> GetArchive(string archiveId)
        {
            OpenTokService();
            Archive archive = OpenTok.GetArchive(archiveId);
            return archive.Url;
        }
        private void OpenTokService()
        {
            int apiKey = 0;
            string apiSecret = null;

            try
            {
                string apiKeyString = _config.Value.API_KEY;
                apiSecret = _config.Value.API_SECRET;
                apiKey = Convert.ToInt32(apiKeyString);
            }

            catch (Exception ex)
            {
                Console.Write(ex);

            }

            finally
            {
                if (apiKey == 0 || apiSecret == null)
                {
                    Console.WriteLine(
                        "The OpenTok API Key and API Secret were not set in the application configuration. " +
                        "Set the values in App.config and try again. (apiKey = {0}, apiSecret = {1})", apiKey, apiSecret);
                    Console.ReadLine();
                    Environment.Exit(-1);
                }
            }

            this.OpenTok = new OpenTok(apiKey, apiSecret);
        }
    }
}