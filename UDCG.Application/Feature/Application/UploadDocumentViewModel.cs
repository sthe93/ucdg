using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Application
{
    public class UploadDocumentViewModel
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public string UploadType { get; set; }
        public string DocumentExtention { get; set; }
        public byte[] DocumentFile { get; set; }
        public int ApplicationId { get; set; }
        public Guid DocumentGuid { get; set; }
        public Guid BatchGuid { get; set; }
    }
}
