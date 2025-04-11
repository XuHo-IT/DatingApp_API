using Microsoft.AspNetCore.SignalR;

namespace API.SignaIR
{
    public class PrecenseHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

    }
}
