namespace Web_chromely_mvc.Services
{
    public static  class DownloadProgressTracker
    {
        private static readonly Dictionary<Guid, int> Progress = new();

        public static Guid StartTracking()
        {
            var id = Guid.NewGuid();
            Progress[id] = 0;
            return id;
        }

        public static void UpdateProgress(Guid id, int percent)
        {
            if (Progress.ContainsKey(id))
            {
                Progress[id] = percent;
            }
        }

        public static int GetProgress(Guid id)
        {
            return Progress.TryGetValue(id, out var percent) ? percent : 0;
        }
    }
}
