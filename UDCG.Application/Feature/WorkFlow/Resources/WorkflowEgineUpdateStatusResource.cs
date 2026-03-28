using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class WorkflowEgineUpdateStatusResource
    {
        public Guid ReferenceId { get; set; }
        public string Username { get; set; }
        public int WorkflowStatusId { get; set; }
        public string Notes { get; set; }

    }
}
