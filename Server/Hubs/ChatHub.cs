using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web.Resource;

namespace chat.web.Hubs
{
    //[Authorize]
    //[RequiredScope("message.sendreceive")]
    public class Chat : Hub
    {
        private readonly ILogger<Chat> logger;

        public Chat(ILogger<Chat> logger)
        {
            this.logger = logger;
        }

        public async Task Broadcast(string username, string message)
        {
            await Clients.All.SendAsync("Broadcast", username, message);
        }

        public override async Task OnConnectedAsync()
        {
            var username = GetNameFromTokenClaims(this.Context);
            if(string.IsNullOrEmpty(username))
            {
                logger.LogError("no user object in the context");
                return;
            }
            await Clients.Caller.SendAsync("Greeting", $"Welcome {username} 👋");
            await Clients.All.SendAsync("NewPlayerEntered", $"{username} just joined. Say hi 🙋‍♀️");
        }
        
        public override async Task OnDisconnectedAsync(Exception e)
        {
            await Clients.Others.SendAsync("PlayerLeft", "... is no longer online");
            logger.LogInformation($"Disconnected {e?.Message} {Context.ConnectionId}");
            await base.OnDisconnectedAsync(e);
        }

        private string GetNameFromTokenClaims(HubCallerContext context)
        {
            if(!context.User.Claims.Any())
            {
                return null;
            }
            return context.User.Claims.FirstOrDefault(c => c.Type.Equals("Name", System.StringComparison.InvariantCultureIgnoreCase)).Value; 
        }
    }
}