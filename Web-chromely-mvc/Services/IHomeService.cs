using Web_chromely_mvc.Models;

namespace YoutubeMp3Convertor.Services
{
    public interface IHomeService
    {
        Task DownloadVideoAsync(string audioUri, Guid downloadId, string title);
        Task<JsonModel> GrabVideoAndAudiosAsync(string url);
    }
}
