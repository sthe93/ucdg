using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class WorkflowInstanceResource
    {
        public Guid Id { get; set; }
        public object AdditionalId { get; set; }
        public int Status { get; set; }
        public string Message { get; set; }
        public bool IsComplete { get; set; }
    }
}
