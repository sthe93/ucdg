using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace UDCG.Application.Common.AppCircle.DocumentStore
{
    public class DocumentCreationModel
    {
        public int DocumentId { get; set; }
        public Guid DocumentGuid { get; set; }
        public Guid BatchGuid { get; set; }
        public string OriginalDocumentName { get; set; }
        [ForeignKey("DocumentTypeId")]
        public byte[] DocumentContent { get; set; }
        public Nullable<int> DocumentTypeId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsArchived { get; set; }
    }
}
