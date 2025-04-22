using Microsoft.AspNetCore.SignalR;

namespace RMSProjectAPI.Hubs
{
    public class OrderHub : Hub
    {
        public override Task OnConnectedAsync()
        {
            Console.WriteLine("Connected!!!");
            return base.OnConnectedAsync();
        }
    }
}
