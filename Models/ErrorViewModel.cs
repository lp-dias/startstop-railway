namespace StartStop.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }

        // ✅ Propriedade calculada para exibir se há RequestId
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
