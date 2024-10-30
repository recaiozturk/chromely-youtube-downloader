namespace Web_chromely_mvc.Models
{
    public class JsonModel
    {
        public string SuccessMessage { get; set; } = "Success";
        public string ErrorMessage { get; set; } = "Failed";
        public string ErrorDescription { get; set; } = "";
        public List<Guid> Ids { get; set; } = default!;
        public List<string>? AudioFormats { get; set; }
        public bool IsValid { get; set; } = false;
        public object? Data { get; set; }
    }
}
