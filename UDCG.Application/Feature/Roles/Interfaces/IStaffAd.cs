using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UDCG.Application.Common;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;
using UDCG.Application.Feature.Roles.Resources;

namespace UDCG.Application.Feature.Roles.Interfaces
{
    public interface IStaffAd
    {
        ApiIntegrationCircleModel Options { get; set; }
        public string Environment { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        Task<bool> Authenticate(string username, string password);
        Task<string> GetStaffNumber(string username);
        EmployeeHODModel GetEmployeeHODDetails(string staffNumber);

        ReadUserViewModelResource GetUserOracleProfile(string staffNumber, string username);
        EmployeeBioModel GetEmployeeDeanInfo(string deanStaffNumber);

        Task<string> GetStaffUsername(string stafnumber);

        ReadUserViewModelResource GetEmployeeInfoByIdNumber(string IdNumber);
        Task<bool> ValidateUsername();

        List<EmployeeBioModel> GetMecList();
        Task<Dictionary<string, string>> GetEmployeeNamesByStaffNumbersAsync(IEnumerable<string> staffNumbers);
        Task<Dictionary<string, OracleOrgSnapshot>> GetOrgHierarchyByStaffNumbersAsync(IEnumerable<string> staffNumbers);
    }
}
