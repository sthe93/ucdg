using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StaffADService;
using UCDG.Infrastructure.Helpers;
using UDCG.Application.Common;
using UDCG.Application.Feature.ApplicationsDashboard.Resources;
using UDCG.Application.Feature.Roles.Interfaces;
using UDCG.Application.Feature.Roles.Resources;
using UDCG.Application.Feature.Users.Services;
using UDCG.Application.Interface;

namespace UCDG.Infrastructure.ExternalServices
{
    public class StaffAd : IStaffAd
    {
        private readonly StaffADService.ServiceSoap _staffService;

        private readonly IRequestSet _request;
        private readonly IAppLoggerSevice _appLoggerSevice;

        public ApiIntegrationCircleModel Options { get; set; }
        public string Environment { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }


        public StaffAd(IRequestSet request, IAppLoggerSevice appLoggerSevice, IOptions<ApiIntegrationCircleModel> options)
        {
            _staffService = new ServiceSoapClient(ServiceSoapClient.EndpointConfiguration.ServiceSoap12);
            _request = request;
            _appLoggerSevice = appLoggerSevice;
            Options = options.Value;
        }

        public async Task<bool> Authenticate(string username, string password)
        {
            if (Environment == EnvironmentConstants.Production)
            {
                var result =  await _staffService.validADUserAsync(username, password);

                if (result.ToLower()  == Message.Success.ToLower())
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }

            return true;

        }
        public async Task<bool> ValidateUsername()
        {

            try
            {
                var result = await _staffService.getPersonnelNrAsync(Username);
                if (result == Message.DoNotExist)
                    return false;
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

        public async Task<string> GetStaffNumber(string username)
        {
            string results = "";
            try
            {
                results = await _staffService.getPersonnelNrAsync(username);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                results = "ERROR: " + e.Message;
            }
            if (results == "DNE")
                return null;
            return results;
        }
        public async Task<string> GetStaffUsername(string stafnumber)
        {
            string results = "";
            try
            {
                results = await _staffService.getUserNameFromPersonnelNrAsync(stafnumber);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                results = "ERROR: " + e.Message;
            }
            if (results == "DNE")
                return null;
            return results;
        }

        public EmployeeHODModel GetEmployeeHODDetails(string staffNumber)
        {

            using (var client = new HttpClient())
            {
                var url = Base.Url();

                var response = client.GetAsync(url + "StudentQualification/StudentAppeal?studentNumber=" + staffNumber).Result;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var responseData = response?.Content.ReadAsStringAsync().Result;
                    var data = JsonConvert.DeserializeObject<EmployeeHODModel>(responseData);
                    if (data.EMPLOYEE_NUMBER != null)
                        return data;
                    return null;
                }
                return null;
            }

        }

        public ReadUserViewModelResource GetEmployeeInfoByIdNumber(string IdNumber)
        {

            using (var client = new HttpClient())
            {
                string url = Base.Url();

                HttpResponseMessage response = client.GetAsync(url + "StaffDetails?identifier=" + IdNumber).Result;

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    string responseData = response?.Content.ReadAsStringAsync().Result;

                    EmployeeBioModel resultsBio = JsonConvert.DeserializeObject<EmployeeBioModel>(responseData);

                    if (resultsBio.EmployeE_NUMBER != null)
                    {
                        ReadUserViewModelResource userViewModel = new ReadUserViewModelResource()
                        {

                            FirstName = resultsBio.FirsT_NAME,
                            Surname = resultsBio.LasT_NAME,
                            Nationality = resultsBio.Citizenship,
                            IdPassportNumber = resultsBio.identifier,
                            DateOfBirth = resultsBio.DatE_OF_BIRTH.Value,
                            EmailAddress = resultsBio.EmaiL_ADDRESS,
                            AlternativeEmailAddress = resultsBio.EmaiL_ADDRESS,
                            CellPhoneNumber = resultsBio.CelL_PHONE,
                            TelephoneNumber = resultsBio.WorK_TELEPHONE_NUMBER,
                            IsProfileCompleted = false,
                            Campus = resultsBio.Campus,
                            Title = resultsBio.Title,
                            StaffNumber = resultsBio.EmployeE_NUMBER,
                            Position = resultsBio.PositioN_NAME,
                            IsAcademic = resultsBio.IS_ACADEMIC != "false",
                            Gender = resultsBio.Gender,
                            Department = resultsBio.DepartmenT_NAME,
                            Race = resultsBio.Race,
                            Faculty = resultsBio.FacultY_DIVISION == null ? "Not Supplied by Oracle" : resultsBio.FacultY_DIVISION.ToString(),
                            HOD = resultsBio.LinE_MANAGER_FIRST_NAME + " " + resultsBio.LinE_MANAGER_SURNAME,
                            HODStaffNUmber = resultsBio.SupervisoR_EMP_NAME,
                            HodPersonType = resultsBio == null ? "Not Supplied by Oracle" : resultsBio.PersoN_TYPE,
                            ViceDeanPersonType = resultsBio == null ? "Not Supplied by Oracle" : resultsBio.PersoN_TYPE

                        };

                        return userViewModel;
                    }
                    else
                    {
                        ReadUserViewModelResource user = new ReadUserViewModelResource();
                        return user;
                    }
                }
                return null;
            }

        }

        public EmployeeBioModel GetEmployeeDeanInfo(string deanStaffNumber)
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var deanInfoRes = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + deanStaffNumber, HttpVerb.Get, null);
            var deanInfo = JsonConvert.DeserializeObject<EmployeeBioModel>(deanInfoRes.ToString());
            return deanInfo;
        }



        public List<EmployeeBioModel> GetMecList()
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var mecListRes = _request.ExecuteAsJson("get-mec-list", HttpVerb.Get, null);
            var mecListInfo = JsonConvert.DeserializeObject<List<EmployeeBioModel>>(mecListRes.ToString());
            return mecListInfo;
        }


        public ReadUserViewModelResource GetUserOracleProfile(string staffNumber, string username)
        {

            _request.BaseUrl =  Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;


            var get_elg_members = string.Empty;

            var hrEmpDetails = string.Empty;
            var userBio = "";
            try
            {

                userBio = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + staffNumber, HttpVerb.Get, null);
            }
            catch (Exception e)
            {
                _appLoggerSevice.LogMessage("Exception InnerException: " + e.InnerException + "Exception meassage: " + e.Message);
                  return null;
            }

            if (string.IsNullOrEmpty(userBio))
                return null;
            //throw new System.Exception("User has no biographical information provided from Oracle");


            var json = JObject.Parse(userBio);
            //var aa = JToken.Parse(userBio);


            var userQual = _request.ExecuteAsJson("get-employee-qual/employee-number/" + staffNumber, HttpVerb.Get, null);


            // var results = GetEmployeeHODDetails(staffNumber);


            var resultsBio = JsonConvert.DeserializeObject<EmployeeBioModel>(json.ToString());

               if (resultsBio == null)
                return null;

            var deanInfoRes = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + resultsBio.SupervisoR_EMP_NAME, HttpVerb.Get, null);

            var HodInfoRes = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + resultsBio.SupervisoR_EMP_NAME, HttpVerb.Get, null);

            if(string.IsNullOrEmpty(HodInfoRes))
                return null;
            var resultsHodBio = JsonConvert.DeserializeObject<EmployeeBioModel>(HodInfoRes.ToString());

            if (resultsHodBio == null)
                return null;


            var ViceDeanInfoRes = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + resultsHodBio.SupervisoR_EMP_NAME, HttpVerb.Get, null);
            var resultsViceDeanBio = JsonConvert.DeserializeObject<EmployeeBioModel>(ViceDeanInfoRes.ToString());

            var deanInfo = JsonConvert.DeserializeObject<EmployeeBioModel>(deanInfoRes.ToString());
            var userViewModel = new ReadUserViewModelResource()
            {
                Username = username,
                FirstName = resultsBio.FirsT_NAME,
                Surname = resultsBio.LasT_NAME,
                Nationality = resultsBio.Citizenship,
                IdPassportNumber = resultsBio.NI_PI,
                DateOfBirth = resultsBio.DatE_OF_BIRTH.Value,
                EmailAddress = username + "@uj.ac.za",
                AlternativeEmailAddress = resultsBio.EmaiL_ADDRESS,
                CellPhoneNumber = resultsBio.CelL_PHONE,
                TelephoneNumber = resultsBio.WorK_TELEPHONE_NUMBER,
                IsProfileCompleted = false,
                Campus = resultsBio.Campus,
                Title = resultsBio.Title,
                StaffNumber = resultsBio.EmployeE_NUMBER,
                Position = resultsBio.PositioN_NAME,
                IsAcademic = resultsBio.IS_ACADEMIC != "false",
                Gender = resultsBio.Gender,
                Department = resultsBio.DepartmenT_NAME,
                Race = resultsBio.Race,
                Disability = "Not Supplied by Oracle",
                Faculty = resultsBio.FacultY_DIVISION == null ? "Not Supplied by Oracle" : resultsBio.FacultY_DIVISION.ToString(),
                HOD = resultsBio.LinE_MANAGER_FIRST_NAME + " " + resultsBio.LinE_MANAGER_SURNAME,
                ViceDean = deanInfo == null ? "Not Supplied by Oracle" : deanInfo.LinE_MANAGER_FIRST_NAME + " " + deanInfo.LinE_MANAGER_SURNAME,
                HODStaffNUmber = resultsBio.SupervisoR_EMP_NAME,
                ViceDeanStaffNUmber = deanInfo == null ? "Not Supplied by Oracle" : deanInfo.SupervisoR_EMP_NAME,
                HodPersonType = resultsHodBio == null ? "Not Supplied by Oracle" : resultsHodBio.PersoN_TYPE,
                ViceDeanPersonType = resultsViceDeanBio == null ? "Not Supplied by Oracle" : resultsViceDeanBio.PersoN_TYPE


            };

            var resultsQual = JsonConvert.DeserializeObject<List<EmployeeQualModel>>(userQual);
            var userQualifications = new List<UserQualificationViewModel>();
            foreach (var qual in resultsQual)
            {
                var uq = new UserQualificationViewModel()
                {
                    Name = string.IsNullOrEmpty(qual.Title) ? "Not Supplied by Oracle" : qual.Title,
                    QualificationType = string.IsNullOrEmpty(qual.QualificatioN_TYPE) ? "Not Supplied by Oracle" : qual.QualificatioN_TYPE,
                    InstitutionName = string.IsNullOrEmpty(qual.InstitutioN_NAME) ? "Not Supplied by Oracle" : qual.InstitutioN_NAME
                };
                userQualifications.Add(uq);
            }
            userViewModel.Qualifications = userQualifications;

            return userViewModel;
        }

        public async Task<Dictionary<string, string>> GetEmployeeNamesByStaffNumbersAsync(IEnumerable<string> staffNumbers)
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var staffNoRaw in staffNumbers.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var staffNo = staffNoRaw.Trim();

                try
                {
                    var bioJson = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + staffNo, HttpVerb.Get, null);
                    if (bioJson == null) continue;

                    var bio = JsonConvert.DeserializeObject<EmployeeBioModel>(bioJson.ToString());
                    if (bio == null) continue;

                    var fullName = $"{bio.FirsT_NAME} {bio.LasT_NAME}".Trim();
                    if (!string.IsNullOrWhiteSpace(fullName))
                        result[staffNo] = fullName;
                }
                catch
                {
                   
                }
            }

            return result;
        }

        public async Task<Dictionary<string, OracleOrgSnapshot>> GetOrgHierarchyByStaffNumbersAsync(IEnumerable<string> staffNumbers)
        {
            _request.BaseUrl = Options.BaseUrl;
            _request.AuthUrl = Options.AuthUrl;
            _request.Username = Options.Username;
            _request.Password = Options.Password;
            _request.IsFormUrlEncoded = false;

            var result = new Dictionary<string, OracleOrgSnapshot>(StringComparer.OrdinalIgnoreCase);

            // Normalize + distinct
            var list = staffNumbers
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            // NOTE: your IRequestSet is sync; wrap each call in Task.Run to avoid blocking,
            // or keep it synchronous if this runs in a background/admin action.
            foreach (var staffNo in list)
            {
                try
                {
                    // 1) Applicant bio
                    var bioJson = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + staffNo, HttpVerb.Get, null);
                    if (bioJson == null) continue;

                    var bio = JsonConvert.DeserializeObject<EmployeeBioModel>(bioJson.ToString());
                    if (bio == null) continue;

                    var hodStaffNo = bio.SupervisoR_EMP_NAME?.Trim();

                    // 2) HOD bio (to get their supervisor = "vice dean" in your chain)
                    string? viceDeanStaffNo = null;
                    if (!string.IsNullOrWhiteSpace(hodStaffNo))
                    {
                        var hodJson = _request.ExecuteAsJson("get-employee-Bio/employee-number/" + hodStaffNo, HttpVerb.Get, null);
                        if (hodJson != null)
                        {
                            var hodBio = JsonConvert.DeserializeObject<EmployeeBioModel>(hodJson.ToString());
                            viceDeanStaffNo = hodBio?.SupervisoR_EMP_NAME?.Trim();
                        }
                    }

                    result[staffNo] = new OracleOrgSnapshot
                    {
                        StaffNo = staffNo,
                        LineManagerStaffNo = hodStaffNo,
                        ViceDeanStaffNo = viceDeanStaffNo
                    };
                }
                catch
                {
                    
                }
            }

            return result;
        }

    }
}
