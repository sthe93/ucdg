namespace UDCG.Application.Common.AppCircle.DocumentStore
{
    public class AppCircleDocument
    {
        public int DocumentId { get; set; }
        public string BatchGuid { get; set; }
        public string DocumentGuid { get; set; }
        public string OriginalDocumentName { get; set; }
        public string DocumentType { get; set; }
        public byte[] DocumentContent { get; set; }
        public string ModifiedBy { get; set; }
    }
}
