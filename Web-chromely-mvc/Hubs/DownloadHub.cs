using Microsoft.AspNetCore.SignalR;

namespace Web_chromely_mvc.Hubs
{
    public class DownloadHub: Hub
    {
        public async Task SendProgress(int downloadId, int progress)
        {
            await Clients.All.SendAsync("ReceiveProgress", downloadId, progress);
        }
    }
}
