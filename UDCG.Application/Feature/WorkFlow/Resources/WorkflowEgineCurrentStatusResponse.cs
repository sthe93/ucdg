using System;
using System.Collections.Generic;
using System.Text;

namespace UDCG.Application.Feature.WorkFlow.Resources
{
    public class WorkflowEgineCurrentStatusResponse
    {
        public int StepDefinitionId { get; set; }
        public int WorkflowDefinitionId { get; set; }
        public string WorkflowReferenceId { get; set; }
        public int SortId { get; set; }
        public int TurnAroundDays { get; set; }
        public bool IncludeWeekends { get; set; }
        public bool IsInitialStep { get; set; }
        public bool IsActive { get; set; }
        public bool IsComplete { get; set; }
        public string StepDefinitionName { get; set; }
        public int NextStepId { get; set; }
        public object PreviousStepId { get; set; }
        public int NumberOfApprovers { get; set; }
        public int MinimumApprovalNumber { get; set; }
        public int TurnAroundTimeReminder { get; set; }
        public int EscalateTo { get; set; }
        public int PpprovalGroupId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public Guid ReferenceId { get; set; }
        public string ApprovalGroupName { get; set; }
        public int WorkflowInstanceId { get; set; }
        public int WorkflowStatusId { get; set; }
        public string WorkflowStatusName { get; set; }
        public int StepInstanceId { get; set; }
    }
}
