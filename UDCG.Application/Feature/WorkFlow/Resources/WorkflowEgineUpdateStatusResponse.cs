using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class WorkflowEgineUpdateStatusResponse
    {
        public int? Id { get; set; }
        public int? AdditionalId { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public bool isComplete { get; set; }
    }
}
