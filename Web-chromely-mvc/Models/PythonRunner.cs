using Python.Runtime;
using System;
using System.Diagnostics;

namespace Web_chromely_mvc.Models
{
    public class PythonRunner
    {
        public  async Task<string> RunPythonScriptAsync( string urls)
        {


            // Python dizininizi belirtin
            string pythonHome = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Python313");

            string pythonDllPath = Path.Combine(pythonHome, "python311.dll"); // Python sürümünüze göre güncelleyin
            //Environment.SetEnvironmentVariable("PYTHONNET_PYDLL", pythonDllPath);

            Runtime.PythonDLL = @"C:\Users\90531\AppData\Local\Programs\Python\Python313\python313.dll";

            // Python ortamını ayarlayın
            //Environment.SetEnvironmentVariable("PYTHONHOME", pythonHome);
            //Environment.SetEnvironmentVariable("PYTHONPATH", pythonHome);

            try
            {
                // Python'u başlatın
                PythonEngine.Initialize();

                using (Py.GIL()) // Global Interpreter Lock (GIL) alın
                {
                    dynamic pytube = Py.Import("pytube");
                    dynamic YouTube = pytube.YouTube(urls);

                    string entry = YouTube.title.ToString();
                    string videoPath = $"{entry}.mp4";
                    string audioPath = $"{entry}.mp3";

                    // Video ve ses dosyasını indir
                    dynamic video_download = YouTube.streams.get_highest_resolution();
                    dynamic audio_download = YouTube.streams.get_audio_only();

                    video_download.download(videoPath);
                    audio_download.download(audioPath);

                    return $"Video found: {entry}\nDownloaded Video: {videoPath}\nDownloaded Audio: {audioPath}";
                }
            }
            catch (Exception e)
            {

                throw;
            }
            //
           
        }
    }
}
