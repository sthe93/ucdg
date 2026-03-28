using System.ComponentModel.DataAnnotations;

namespace UCDG.Domain.Entities
{
    public class CostCentres
    {
        [Key]
        public int CostCentreId { get; set; }
        public string Name { get; set; }
        public string CostCentreCode { get; set; } 
    }
}
