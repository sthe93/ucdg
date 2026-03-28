using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Application.Resources
{
   public class ReadDocumentResource
    {
        public int Id { get; set; }
        public string Filename { get; set; }
        public string UploadType { get; set; }
        public string DocumentExtention { get; set; }
        public int ApplicationId { get; set; }
    }
}
