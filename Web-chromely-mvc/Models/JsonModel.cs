namespace Web_chromely_mvc.Models
{
    public class JsonModel
    {
        public string SuccessMessage { get; set; } = "Success";
        public string ErrorMessage { get; set; } = "Failed";
        public List<Guid> Ids { get; set; } = default!;
        public bool IsValid { get; set; } = false;
        public object? Data { get; set; }
    }
}
