
using System;

namespace UCDG.Domain.Entities
{
    public class Documents
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public string UploadType { get; set; }
        public string DocumentExtention { get; set; }
        public byte[] DocumentFile { get; set; }
        public int ApplicationsId { get; set; }
        public Guid DocumentGuid { get; set; }
        public Guid BatchGuid { get; set; }
        //Foreign Keys
        public Applications Applications { get; set; }
    }
}
