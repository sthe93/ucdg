using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.ErrorLogs.Resources
{
    public class ErrorLogsViewModel
    {
        public int Id { get; set; }
        public string Functionality { get; set; }
        public string ApplicationError { get; set; }
        public DateTime DateCreated { get; set; }
        public string ParameterValue { get; set; }
    }
}
