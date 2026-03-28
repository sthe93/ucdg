using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.Application.Resources
{
    public class ApplicationApproversViewModel
    {
        public int ApproverUserId { get; set; }
        public string? EmailAddress { get; set; }
        public string FullTittle { get; set; }
        public string Surname { get; set; }
        public string Fistname { get; set; }
        public string FullName { get; set; }
        public string RoleName { get; set; }
        public string StaffNumber { get; set; }
    }
}
