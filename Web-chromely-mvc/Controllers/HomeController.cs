using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VideoLibrary;
using Web_chromely_mvc.Models;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using Web_chromely_mvc.Services;
using Microsoft.AspNetCore.SignalR;
using Web_chromely_mvc.Hubs;

namespace Web_chromely_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHubContext<DownloadHub> _hubContext;

        public HomeController(ILogger<HomeController> logger, IHubContext<DownloadHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
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
            try
            {
                var youtube = new CustomYouTube();
                //var videoInfos = await youtube.GetAllVideosAsync(url);
                //var audio = videoInfos.FirstOrDefault(a => a.AudioBitrate == 128 && a.AudioFormat == AudioFormat.Aac);

                //var audio = await youtube.GetVideoAsync(audioUri);

                await youtube.CreateDownloadAsync(
                    new Uri(audioUri),
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{title}.mp3"),
                    new Progress<Tuple<long, long>>(async v =>
                    {
                        var percent = (int)((v.Item1 * 100) / v.Item2);
                        await _hubContext.Clients.All.SendAsync("ReceiveProgress", downloadId, percent);

                    }));
            }
            catch (Exception e)
            {
               
                _logger.LogError(e, "Download failed.");
                return Json(0);
            }

            return Json(1);
        }

        [HttpPost]
        public async Task<JsonResult> GrabVideoAndAudios(string url)
        {
            List<Guid> videoIds = new List<Guid>();

            var youtube = new CustomYouTube();
            var videoInfos = await youtube.GetAllVideosAsync(url);

            var audios = videoInfos.Where(a => a.AdaptiveKind == AdaptiveKind.Audio).ToList();

            foreach ( var audio in audios ) 
            {
                videoIds.Add(Guid.NewGuid());
            }


            if (audios != null)
                return Json(new JsonModel { IsValid = true,Ids= videoIds, Data = audios }) ;
            else
                return Json(new JsonModel { IsValid = false,ErrorMessage="No Video or Audio Found"});


        }

        [HttpGet]
        public IActionResult GetDownloadProgress(Guid downloadId)
        {
            var progress = DownloadProgressTracker.GetProgress(downloadId);
            return Json(new { progress });
        }

        //        [HttpPost]
        //        public async Task<IActionResult> DownloadVideoAsync(string url)
        //        {
        //            try
        //            {
        //                var youtube = new CustomYouTube();
        //                //our tests
        //                var videoInfos = youtube.GetAllVideosAsync(url).GetAwaiter().GetResult();

        //                var audio=videoInfos.ToList().FirstOrDefault(a=>a.AudioBitrate==128 && a.AudioFormat==AudioFormat.Aac);

        //                youtube
        //                    .CreateDownloadAsync(
        //                    new Uri(audio.Uri),
        //                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), $"{audio.Title}.mp3")
        //,
        //                    new Progress<Tuple<long, long>>((Tuple<long, long> v) =>
        //                    {
        //                        var percent = (int)((v.Item1 * 100) / v.Item2);
        //                        _logger.LogInformation(string.Format("Downloading.. ( % {0} ) {1} / {2} MB\r", percent, (v.Item1 / (double)(1024 * 1024)).ToString("N"), (v.Item2 / (double)(1024 * 1024)).ToString("N")));
        //                    }))
        //                    .GetAwaiter().GetResult();

        //            }
        //            catch (Exception e)
        //            {

        //                throw;
        //            }
        //            return View();
        //        }

        class CustomHandler
        {
            public HttpMessageHandler GetHandler()
            {
                CookieContainer cookieContainer = new CookieContainer();
                cookieContainer.Add(new Cookie("CONSENT", "YES+cb", "/", "youtube.com"));
                return new HttpClientHandler
                {
                    UseCookies = true,
                    CookieContainer = cookieContainer
                };

            }
        }
        class CustomYouTube : YouTube
        {
            private long chunkSize = 10_485_760;
            private long _fileSize = 0L;
            private HttpClient _client = new HttpClient();
            protected override HttpClient MakeClient(HttpMessageHandler handler)
            {
                return base.MakeClient(handler);
            }
            protected override HttpMessageHandler MakeHandler()
            {
                return new CustomHandler().GetHandler();
            }
            public async Task CreateDownloadAsync(Uri uri, string filePath, IProgress<Tuple<long, long>> progress)
            {
                var totalBytesCopied = 0L;
                _fileSize = await GetContentLengthAsync(uri.AbsoluteUri) ?? 0;
                if (_fileSize == 0)
                {
                    throw new Exception("File has no any content !");
                }
                using (Stream output = System.IO.File.OpenWrite(filePath))
                {
                    var segmentCount = (int)Math.Ceiling(1.0 * _fileSize / chunkSize);
                    for (var i = 0; i < segmentCount; i++)
                    {
                        var from = i * chunkSize;
                        var to = (i + 1) * chunkSize - 1;
                        var request = new HttpRequestMessage(HttpMethod.Get, uri);
                        request.Headers.Range = new RangeHeaderValue(from, to);
                        using (request)
                        {
                            // Download Stream
                            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                            if (response.IsSuccessStatusCode)
                                response.EnsureSuccessStatusCode();
                            var stream = await response.Content.ReadAsStreamAsync();
                            //File Steam
                            var buffer = new byte[81920];
                            int bytesCopied;
                            do
                            {
                                bytesCopied = await stream.ReadAsync(buffer, 0, buffer.Length);
                                output.Write(buffer, 0, bytesCopied);
                                totalBytesCopied += bytesCopied;
                                progress.Report(new Tuple<long, long>(totalBytesCopied, _fileSize));
                            } while (bytesCopied > 0);
                        }
                    }
                }
            }
            private async Task<long?> GetContentLengthAsync(string requestUri, bool ensureSuccess = true)
            {
                using (var request = new HttpRequestMessage(HttpMethod.Head, requestUri))
                {
                    var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                    if (ensureSuccess)
                        response.EnsureSuccessStatusCode();
                    return response.Content.Headers.ContentLength;
                }
            }
        }

        public IActionResult Privacy()
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
