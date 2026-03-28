//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.ComponentModel.DataAnnotations.Schema;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace UCDG.Domain.Entities
//{
//    internal class LinkUserMotivationLetter
//    {
//    }
//}



namespace UCDG.Domain.Entities
{
    using System;
    using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("LinkUserMotivationLetter")]
public class LinkUserMotivationLetter
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int FundingCallId { get; set; }

    [Required]
    public int UserId { get; set; }

    public int DocumentId { get; set; }

    // Nullable ApplicationId as requested
    public int? ApplicationId { get; set; }

        public DateTime DateAdded { get; set; }  

        public string Filename { get; set; }
        public string UploadType { get; set; }
        public string DocumentExtention { get; set; }
        public byte[] DocumentFile { get; set; }
       
}
}