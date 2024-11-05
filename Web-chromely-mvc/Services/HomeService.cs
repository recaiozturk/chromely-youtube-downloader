using Microsoft.AspNetCore.SignalR;
using VideoLibrary;
using Web_chromely_mvc.Hubs;
using Web_chromely_mvc.Models;
using Web_chromely_mvc.Services;

namespace YoutubeMp3Convertor.Services
{
    public class HomeService:IHomeService
    {
        private readonly IHubContext<DownloadHub> _hubContext;
        private readonly CustomYoutube _youtubeService;

        public HomeService(CustomYoutube youtubeService, IHubContext<DownloadHub> hubContext)
        {
            _hubContext = hubContext;
            _youtubeService = youtubeService;
        }

        public async Task DownloadVideoAsync(string audioUri, Guid downloadId, string title)
        {
            title = title.Replace("/", "-");

            await _youtubeService.CreateDownloadAsync(
                new Uri(audioUri),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{title}.mp3"),
                new Progress<Tuple<long, long>>(async v =>
                {
                    var percent = (int)((v.Item1 * 100) / v.Item2);
                    await _hubContext.Clients.All.SendAsync("ReceiveProgress", downloadId, percent);

                }));
        }

        public async Task<JsonModel> GrabVideoAndAudiosAsync(string url)
        {
            if (string.IsNullOrEmpty(url))
                return new JsonModel { IsValid = false, ErrorMessage = "Invalid Url", ErrorDescription = "please enter valid url" };

            List<Guid> videoIds = new();
            List<string> formats = new();

            IEnumerable<YouTubeVideo> videoInfos;

            try
            {
                videoInfos = await _youtubeService.GetAllVideosAsync(url);
            }
            catch (Exception)
            {
                return new JsonModel { IsValid = false, ErrorMessage = "Invalid Url", ErrorDescription = "No Video or Audio Found" };
            }

            var audios = videoInfos.Where(a => a.AdaptiveKind == AdaptiveKind.Audio).ToList();

            foreach (var audio in audios)
            {
                videoIds.Add(Guid.NewGuid());
                formats.Add(audio.AudioFormat.ToString());
            }

            if (audios != null)
                return new JsonModel { IsValid = true, Ids = videoIds, Data = audios, AudioFormats = formats };
            else
                return new JsonModel { IsValid = false, ErrorMessage = "No Video or Audio Found" };
        }
    }
}
