using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web_chromely_mvc.Models;
using Web_chromely_mvc.Services;
using YoutubeMp3Convertor.Services;

namespace Web_chromely_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService,CustomYoutube youtubeService)
        {
            _homeService = homeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> DownloadVideo(string audioUri, Guid downloadId,string title)
        {
            await _homeService.DownloadVideoAsync(audioUri, downloadId, title);
            return Json(1);

        }

        [HttpPost]
        public async Task<JsonResult> GrabVideoAndAudios(string url)
        {
            return Json(await _homeService.GrabVideoAndAudiosAsync(url));
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
