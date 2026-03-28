using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Application.Resources
{
    public class AppplicationCommentVeiwModel
    {

        public int Id { get; set; }
        public int ApplicationsId { get; set; }
        
        public int UserId { get; set; }
  
        public string Comment { get; set; }
    }
}
