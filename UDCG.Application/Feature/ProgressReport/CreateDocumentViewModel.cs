

using System;

namespace UDCG.Application.Feature.ProgressReport
{
    public class CreateDocumentViewModel
    {
        public int DocumentId { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public string DocumentExtention { get; set; }
        public byte[] DocumentFile { get; set; }
        public int ProgressReportId { get; set; }
        public string UploadType { get; set; }
        public Guid DocumentGuid { get; set; }
        public Guid BatchGuid { get; set; }
    }
}
