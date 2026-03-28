using System;
using System.Collections.Generic;
using System.Text;

namespace UCDG.Domain.Entities
{
    public class ErrorLogs
    {
      
        public int Id { get; set; }
        public string? Functionality { get; set; }
        public string? ApplicationError { get; set; }
       
        public DateTime DateCreated { get; set; }
    }
}
