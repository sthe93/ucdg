using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDCG.Application.Feature.Application.Resources
{
  public  class MotivationLetterReadModel
{
     public int Id { get; set; }
        public string Filename { get; set; }
        public string UploadType { get; set; }
        public string DocumentExtention { get; set; }
        public byte[] DocumentFile { get; set; }
        public int? ApplicationId { get; set; }
        public int? FundingCallId { get; set; }
        public int? UserId { get; set; }
         public int DocumentId { get; set; }
        public DateTime DateAdded { get; set; }
}


      public  class MotivationLetterResponse
{
     public int Id { get; set; }
        public string Filename { get; set; }
        public string UploadType { get; set; }
        public string DocumentExtention { get; set; }
        public int? ApplicationId { get; set; }
        public int? FundingCallId { get; set; }
        public int? UserId { get; set; }
         public int DocumentId { get; set; }
        public DateTime DateAdded { get; set; }
}


}
