using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Diagnostics;
using VideoLibrary;
using Web_chromely_mvc.Hubs;
using Web_chromely_mvc.Models;
using Web_chromely_mvc.Services;

namespace Web_chromely_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<DownloadHub> _hubContext;
        private readonly CustomYoutube _youtubeService;

        public HomeController(CustomYoutube youtubeService, ILogger<HomeController> logger, IHubContext<DownloadHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _youtubeService = youtubeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> DownloadVideo()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> DownloadVideo(string audioUri, Guid downloadId,string title)
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

            return Json(1);

        }

        [HttpPost]
        public async Task<JsonResult> GrabVideoAndAudios(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return Json(new JsonModel { IsValid = false, ErrorMessage = "Invalid Url",ErrorDescription= "please enter valid url" });
            }

            List<Guid> videoIds = new ();
            List<string> formats = new ();

            IEnumerable<YouTubeVideo> videoInfos;

            try
            {
                videoInfos = await _youtubeService.GetAllVideosAsync(url);
            }
            catch (Exception)
            {
                return Json(new JsonModel { IsValid = false, ErrorMessage = "Invalid Url", ErrorDescription = "No Video or Audio Found" });
            }

            var audios = videoInfos.Where(a => a.AdaptiveKind == AdaptiveKind.Audio).ToList();

            foreach ( var audio in audios ) 
            {
                videoIds.Add(Guid.NewGuid());
                formats.Add(audio.AudioFormat.ToString());
            }

            if (audios != null)
                return Json(new JsonModel { IsValid = true,Ids= videoIds, Data = audios ,AudioFormats=formats}) ;
            else
                return Json(new JsonModel { IsValid = false,ErrorMessage="No Video or Audio Found"});


        }
 
        public IActionResult About()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
