

using System;
using System.ComponentModel.DataAnnotations;

namespace UCDG.Domain.Entities
{
    public class ProgressReportDocuments
    {
        [Key]
        public int DocumentId { get; set; }
        public string Filename { get; set; }
        public string ContentType { get; set; }
        public string DocumentExtention { get; set; }
        public byte[] DocumentFile { get; set; } 
        public string UploadType { get; set; }
        public Guid DocumentGuid { get; set; }
        public Guid BatchGuid { get; set; }
        public ProgressReports ProgressReport { get; set; }
    }
}
