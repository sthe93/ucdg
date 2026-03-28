using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UCDG.Persistence.Enums;
using UDCG.Application.Common.AppCircle.DocumentStore;
using UDCG.Application.Feature.Emails.Interface;
using UDCG.Application.Feature.ProgressReport;
using UDCG.Application.Feature.ProgressReport.Interface;
using UDCG.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class ProgressReportRepository : IProgressReportRepository
    {
        private readonly UCDGDbContext _context;
        private readonly UserStoreDbContext _userStoreDbContext;
        private readonly IEmailSentRepository _emailSentRepository;
        private readonly IAppCircleService _appCircleService;

        public ProgressReportRepository(UCDGDbContext context, IEmailSentRepository emailSentRepository, IAppCircleService appCircleService, UserStoreDbContext userStoreDbContext)
        {
            _context = context;
            _emailSentRepository = emailSentRepository;
            _appCircleService = appCircleService;
            _userStoreDbContext = userStoreDbContext;
        }


        public async Task<ProgressReports> Add(ProgressReportDetailsViewModel model)
        {
            try
            {
                if (model.ApplicationId != 0)
                {
                    Applications application = await _context.Applications.FirstOrDefaultAsync(u => u.Id == model.ApplicationId);
                    ProgressReportStatus progressReportStatus = await _context.ProgressReportStatus.FirstOrDefaultAsync(u => u.Status.ToLower().Trim() == ProgressReportStatusEnum.New.GetDescription().ToLower().Trim());

                    if (application != null)
                    {
                        ProgressReports progressReports = new ProgressReports();

                        progressReports.Application = application;
                        progressReports.ProgressReportStatus = progressReportStatus;
                        progressReports.IsComplete = false;
                        progressReports.CreatedDate = DateTime.Now;
                        progressReports.IsQualificationInPrgress = false;
                        progressReports.IsQualificationGraduated = false;
                        progressReports.IsReliefAppointment = false;
                        progressReports.IsResearchPublication = false;
                        progressReports.IsResearchProject = false;
                        progressReports.IsCollaborativeProject = false;

                        var results = await _context.ProgressReports.AddAsync(progressReports);

                        await _context.SaveChangesAsync();

                        return progressReports;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        //public async Task<List<ProgressReportComments>> GetCommentsByReportId(int reportId)
        //{

        //    if (reportId != 0)
        //    {

        //        List<ProgressReportComments> comments = await _context.ProgressReportComments.Include(c => c.ProgressReports).Include(c => c.User).Where(f => f.ProgressReports.Id == reportId).ToListAsync();

        //        return comments;

        //    }
        //    return null;

        //}

        public async Task<List<ProgressReportComments>> GetCommentsByReportId(int reportId)
        {
            if (reportId == 0) return new List<ProgressReportComments>();

            var comments = await _context.ProgressReportComments
                .Where(c => c.ProgressReportsId == reportId)
                .Include(c => c.ProgressReports)
                .Include(c => c.User) // legacy user join
                .AsNoTracking()
                .ToListAsync();

            // 2) Collect UserStore ids
            var usIds = comments
                .Where(c => c.UserStoreUserId != null)
                .Select(c => c.UserStoreUserId.Value)
                .Distinct()
                .ToList();

            // 3) Fetch users from UserStore
            var usUsers = usIds.Count == 0
                ? new Dictionary<int, UserStoreUser>()
                : await _userStoreDbContext.Users
                    .Where(u => usIds.Contains(u.UserId))
                    .AsNoTracking()
                    .ToDictionaryAsync(u => u.UserId);

            foreach (var c in comments)
            {
                var userId = c.UserStoreUserId ?? c.UserId; // Prefer UserStoreUserId, fallback to legacy UserId
                if (usUsers.TryGetValue((int)userId, out var us))
                {
                    c.AddedBy = us.Username;
                    c.DisplayName = $"{us.Name ?? ""} {us.Surname ?? ""}".Trim();
                }
                else
                {
                    c.AddedBy = c.User?.Username;
                    c.DisplayName = $"{c.User?.FirstName ?? ""} {c.User?.Surname ?? ""}".Trim();
                }
            }

            return comments;
        }

        public async Task<List<ProgressReportComments>> GetCommentsListByApplicationsId(int applicationsId)
        {

            if (applicationsId != 0)
            {

                List<ProgressReportComments> comments = await _context.ProgressReportComments.Include(c => c.ProgressReports).Include(c => c.User).Where(f => f.ProgressReports.Application.Id == applicationsId).ToListAsync();
                return comments;

            }
            return null;

        }

        public async Task<ProgressReports> Update(ProgressReportDetailsViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.Include(c => c.Application).FirstOrDefaultAsync(u => u.Id == model.Id);
                    if (progressReport != null)
                    {
                        if (model.StepId == 2)
                        {
                            progressReport.IsQualificationInPrgress = model.IsQualificationInPrgress;
                            progressReport.QualificationName = model.QualificationName;
                            progressReport.QualificationInPrgressFieldOfStudy = model.QualificationInPrgressFieldOfStudy;
                            progressReport.QualificationInPrgressTitleofThesis = model.QualificationInPrgressTitleofThesis;
                            progressReport.QualificationInPrgressInstitution = model.QualificationInPrgressInstitution;
                            progressReport.QualificationInPrgressGraduationYear = model.QualificationInPrgressGraduationYear;
                        }

                        if (model.StepId == 3)
                        {
                            progressReport.IsQualificationGraduated = model.IsQualificationGraduated;
                            progressReport.QualificationGraduatedName = model.QualificationGraduatedName;
                            progressReport.QualificationGraduatedFieldOfStudy = model.QualificationGraduatedFieldOfStudy;
                            progressReport.QualificationGraduatedTitleofThesis = model.QualificationGraduatedTitleofThesis;
                            progressReport.QualificationGraduatedInstitution = model.QualificationGraduatedInstitution;
                            progressReport.QualificationGraduatedYear = model.QualificationGraduatedYear;
                        }


                        if (model.StepId == 4)
                        {
                            progressReport.IsReliefAppointment = model.IsReliefAppointment;
                        }

                        if (model.StepId == 5)
                        {
                            progressReport.IsResearchPublication = model.IsResearchPublication;
                            progressReport.ResearchAccreditedJournal = model.ResearchAccreditedJournal;
                            progressReport.ResearchAccreditedChapter = model.ResearchAccreditedChapter;
                            progressReport.ResearchAccreditedBook = model.ResearchAccreditedBook;
                            progressReport.ResearchAccreditedConference = model.ResearchAccreditedConference;
                        }

                        if (model.StepId == 6)
                        {
                            progressReport.IsResearchProject = model.IsResearchProject;
                            progressReport.ResearchProjectSupport = model.ResearchProjectSupport;
                            progressReport.Activities = model.Activities;
                            progressReport.Outputs = model.Outputs;
                            progressReport.Outcome = model.Outcome;
                        }

                        if (model.StepId == 7)
                        {
                            progressReport.IsCollaborativeProject = model.IsCollaborativeProject;
                            progressReport.CollaborativeProjectSupported = model.CollaborativeProjectSupported;
                            progressReport.CollaborativeActivities = model.CollaborativeActivities;
                            progressReport.CollaborativeOutputs = model.CollaborativeOutputs;
                            progressReport.CollaborativeOutcome = model.CollaborativeOutcome;
                        }

                        if (model.StepId == 8)
                        {
                            ProgressReportStatus newStatus = await _context.ProgressReportStatus.FirstOrDefaultAsync(u => u.Status == ProgressReportStatusEnum.New.GetDescription());

                            progressReport.IsComplete = true;
                            if (newStatus != null)
                            {
                                progressReport.ProgressReportStatus = newStatus;
                            }
                        }

                        var results = _context.ProgressReports.Update(progressReport);

                        await _context.SaveChangesAsync();

                        model.ApplicationId = progressReport.Application.Id;
                        model.Id = progressReport.Id;

                        switch (model.StepId)
                        {
                            case 8:
                                await _emailSentRepository.SendSubmissionReportDueEmail(model);
                                await _emailSentRepository.SendFundingAdminReviewEmailBody(model);
                                _emailSentRepository.DeActivateProgressReportSubmissionReminderV2(model.ApplicationId);
                                break;
                        }

                        return progressReport;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception msg)
            {
                return null;
            }
        }

        public async Task<bool> GetReportStatus(int applicationsId)
        {

            List<ProgressReports> res = await _context.ProgressReports.Include(o => o.Application).Where(u => u.Application.Id == applicationsId && u.IsComplete == true).ToListAsync();

            if (res.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> GetViewReportStatus(int applicationsId)
        {

            List<ProgressReports> res = await _context.ProgressReports.Include(o => o.Application).Where(u => u.Application.Id == applicationsId && u.ProgressReportStatus.Status == ProgressReportStatusEnum.RFI.GetDescription()).ToListAsync();

            if (res.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<ProgressReportDetailsViewModel> DownloadCompleteProgressReport(int reportId)
        {
            try
            {
                if (reportId != 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).Include(c => c.Application.Applicant).Where(c => c.Id == reportId).FirstOrDefaultAsync();

                    if (progressReport != null)
                    {
                        ProgressReportDetailsViewModel progressReportViewModel = new ProgressReportDetailsViewModel();

                        progressReportViewModel.Id = progressReport.Id;
                        progressReportViewModel.IsComplete = progressReport.IsComplete;
                        progressReportViewModel.ApplicationId = progressReport.Application.Id;
                        progressReportViewModel.ReferenceNumber = progressReport.Application.ReferenceNumber;
                        progressReportViewModel.ProgressReportStatusId = progressReport.ProgressReportStatus.ProgressReportStatusId;
                        progressReportViewModel.Status = progressReport.ProgressReportStatus.Status;
                        progressReportViewModel.IsQualificationInPrgress = progressReport.IsQualificationInPrgress;
                        progressReportViewModel.QualificationName = progressReport.QualificationName;
                        progressReportViewModel.QualificationInPrgressFieldOfStudy = progressReport.QualificationInPrgressFieldOfStudy;
                        progressReportViewModel.QualificationInPrgressTitleofThesis = progressReport.QualificationInPrgressTitleofThesis;
                        progressReportViewModel.QualificationInPrgressInstitution = progressReport.QualificationInPrgressInstitution;
                        progressReportViewModel.QualificationInPrgressGraduationYear = progressReport.QualificationInPrgressGraduationYear;
                        progressReportViewModel.IsQualificationGraduated = progressReport.IsQualificationGraduated;
                        progressReportViewModel.QualificationGraduatedName = progressReport.QualificationGraduatedName;
                        progressReportViewModel.QualificationGraduatedFieldOfStudy = progressReport.QualificationGraduatedFieldOfStudy;
                        progressReportViewModel.QualificationGraduatedTitleofThesis = progressReport.QualificationGraduatedTitleofThesis;
                        progressReportViewModel.QualificationGraduatedInstitution = progressReport.QualificationGraduatedInstitution;
                        progressReportViewModel.QualificationGraduatedYear = progressReport.QualificationGraduatedYear;
                        progressReportViewModel.IsReliefAppointment = progressReport.IsReliefAppointment;
                        progressReportViewModel.IsResearchPublication = progressReport.IsResearchPublication;
                        progressReportViewModel.ResearchAccreditedJournal = progressReport.ResearchAccreditedJournal;
                        progressReportViewModel.ResearchAccreditedChapter = progressReport.ResearchAccreditedChapter;
                        progressReportViewModel.ResearchAccreditedBook = progressReport.ResearchAccreditedBook;
                        progressReportViewModel.ResearchAccreditedConference = progressReport.ResearchAccreditedConference;
                        progressReportViewModel.IsResearchProject = progressReport.IsResearchProject;
                        progressReportViewModel.ResearchProjectSupport = progressReport.ResearchProjectSupport;
                        progressReportViewModel.Activities = progressReport.Activities;
                        progressReportViewModel.Outputs = progressReport.Outputs;
                        progressReportViewModel.Outcome = progressReport.Outcome;
                        progressReportViewModel.IsCollaborativeProject = progressReport.IsCollaborativeProject;
                        progressReportViewModel.CollaborativeProjectSupported = progressReport.CollaborativeProjectSupported;
                        progressReportViewModel.CollaborativeActivities = progressReport.CollaborativeActivities;
                        progressReportViewModel.CollaborativeOutputs = progressReport.CollaborativeOutputs;
                        progressReportViewModel.CollaborativeOutcome = progressReport.CollaborativeOutcome;
                        progressReportViewModel.StaffNumber = progressReport.Application.Applicant.StaffNumber;
                        progressReportViewModel.Title = progressReport.Application.Applicant.Title;
                        progressReportViewModel.FirstName = progressReport.Application.Applicant.FirstName;
                        progressReportViewModel.Surname = progressReport.Application.Applicant.Surname;
                        progressReportViewModel.ApprovedAmount = progressReport.Application.ApprovedAmount;
                        progressReportViewModel.ApplicantCategory = progressReport.Application.ApplicantCategory;

                        return progressReportViewModel;
                    }
                }
                return null;

            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<ProgressReportDetailsViewModel> GetProgressReportByApplicationId(int applicationId)
        {
            try
            {
                if (applicationId != 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).Include(c => c.Application.FundingCalls).Include(c => c.Application.Applicant).Where(c => c.Application.Id == applicationId).FirstOrDefaultAsync();

                    if (progressReport != null)
                    {
                        ProgressReportDetailsViewModel progressReportViewModel = new ProgressReportDetailsViewModel();

                        progressReportViewModel.Id = progressReport.Id;
                        progressReportViewModel.IsComplete = progressReport.IsComplete;
                        progressReportViewModel.ApplicationId = applicationId;
                        progressReportViewModel.ReferenceNumber = progressReport.Application.ReferenceNumber;
                        progressReportViewModel.ProgressReportStatusId = progressReport.ProgressReportStatus.ProgressReportStatusId;
                        progressReportViewModel.Status = progressReport.ProgressReportStatus.Status;
                        progressReportViewModel.IsQualificationInPrgress = progressReport.IsQualificationInPrgress;
                        progressReportViewModel.QualificationName = progressReport.QualificationName;
                        progressReportViewModel.QualificationInPrgressFieldOfStudy = progressReport.QualificationInPrgressFieldOfStudy;
                        progressReportViewModel.QualificationInPrgressTitleofThesis = progressReport.QualificationInPrgressTitleofThesis;
                        progressReportViewModel.QualificationInPrgressInstitution = progressReport.QualificationInPrgressInstitution;
                        progressReportViewModel.QualificationInPrgressGraduationYear = progressReport.QualificationInPrgressGraduationYear;
                        progressReportViewModel.IsQualificationGraduated = progressReport.IsQualificationGraduated;
                        progressReportViewModel.QualificationGraduatedName = progressReport.QualificationGraduatedName;
                        progressReportViewModel.QualificationGraduatedFieldOfStudy = progressReport.QualificationGraduatedFieldOfStudy;
                        progressReportViewModel.QualificationGraduatedTitleofThesis = progressReport.QualificationGraduatedTitleofThesis;
                        progressReportViewModel.QualificationGraduatedInstitution = progressReport.QualificationGraduatedInstitution;
                        progressReportViewModel.QualificationGraduatedYear = progressReport.QualificationGraduatedYear;
                        progressReportViewModel.IsReliefAppointment = progressReport.IsReliefAppointment;
                        progressReportViewModel.IsResearchPublication = progressReport.IsResearchPublication;
                        progressReportViewModel.ResearchAccreditedJournal = progressReport.ResearchAccreditedJournal;
                        progressReportViewModel.ResearchAccreditedChapter = progressReport.ResearchAccreditedChapter;
                        progressReportViewModel.ResearchAccreditedBook = progressReport.ResearchAccreditedBook;
                        progressReportViewModel.ResearchAccreditedConference = progressReport.ResearchAccreditedConference;
                        progressReportViewModel.IsResearchProject = progressReport.IsResearchProject;
                        progressReportViewModel.ResearchProjectSupport = progressReport.ResearchProjectSupport;
                        progressReportViewModel.Activities = progressReport.Activities;
                        progressReportViewModel.Outputs = progressReport.Outputs;
                        progressReportViewModel.Outcome = progressReport.Outcome;
                        progressReportViewModel.IsCollaborativeProject = progressReport.IsCollaborativeProject;
                        progressReportViewModel.CollaborativeProjectSupported = progressReport.CollaborativeProjectSupported;
                        progressReportViewModel.CollaborativeActivities = progressReport.CollaborativeActivities;
                        progressReportViewModel.CollaborativeOutputs = progressReport.CollaborativeOutputs;
                        progressReportViewModel.CollaborativeOutcome = progressReport.CollaborativeOutcome;
                        progressReportViewModel.StaffNumber = progressReport.Application.Applicant.StaffNumber;
                        progressReportViewModel.Title = progressReport.Application.Applicant.Title;
                        progressReportViewModel.FirstName = progressReport.Application.Applicant.FirstName;
                        progressReportViewModel.Surname = progressReport.Application.Applicant.Surname;
                        progressReportViewModel.ApprovedAmount = progressReport.Application.ApprovedAmount;
                        progressReportViewModel.ApplicantCategory = progressReport.Application.ApplicantCategory;
                        progressReportViewModel.FundingCallStartDate = progressReport.Application.FundingCalls.OpeningDate;

                        return progressReportViewModel;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception msg)
            {
                return null;
            }
        }

        public async Task<ProgressReportDetailsViewModel> GetCompletedProgressReportByApplicationId(int applicationId)
        {
            try
            {
                if (applicationId != 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).Include(c => c.Application.FundingCalls).Include(c => c.Application.Applicant).Where(c => c.Application.Id == applicationId).FirstOrDefaultAsync();

                    if (progressReport != null)
                    {
                        ProgressReportDetailsViewModel progressReportViewModel = new ProgressReportDetailsViewModel();

                        progressReportViewModel.Id = progressReport.Id;
                        progressReportViewModel.IsComplete = progressReport.IsComplete;
                        progressReportViewModel.ApplicationId = applicationId;
                        progressReportViewModel.ReferenceNumber = progressReport.Application.ReferenceNumber;
                        progressReportViewModel.ProgressReportStatusId = progressReport.ProgressReportStatus.ProgressReportStatusId;
                        progressReportViewModel.Status = progressReport.ProgressReportStatus.Status;
                        progressReportViewModel.IsQualificationInPrgress = progressReport.IsQualificationInPrgress;
                        progressReportViewModel.QualificationName = progressReport.QualificationName;
                        progressReportViewModel.QualificationInPrgressFieldOfStudy = progressReport.QualificationInPrgressFieldOfStudy;
                        progressReportViewModel.QualificationInPrgressTitleofThesis = progressReport.QualificationInPrgressTitleofThesis;
                        progressReportViewModel.QualificationInPrgressInstitution = progressReport.QualificationInPrgressInstitution;
                        progressReportViewModel.QualificationInPrgressGraduationYear = progressReport.QualificationInPrgressGraduationYear;
                        progressReportViewModel.IsQualificationGraduated = progressReport.IsQualificationGraduated;
                        progressReportViewModel.QualificationGraduatedName = progressReport.QualificationGraduatedName;
                        progressReportViewModel.QualificationGraduatedFieldOfStudy = progressReport.QualificationGraduatedFieldOfStudy;
                        progressReportViewModel.QualificationGraduatedTitleofThesis = progressReport.QualificationGraduatedTitleofThesis;
                        progressReportViewModel.QualificationGraduatedInstitution = progressReport.QualificationGraduatedInstitution;
                        progressReportViewModel.QualificationGraduatedYear = progressReport.QualificationGraduatedYear;
                        progressReportViewModel.IsReliefAppointment = progressReport.IsReliefAppointment;
                        progressReportViewModel.IsResearchPublication = progressReport.IsResearchPublication;
                        progressReportViewModel.ResearchAccreditedJournal = progressReport.ResearchAccreditedJournal;
                        progressReportViewModel.ResearchAccreditedChapter = progressReport.ResearchAccreditedChapter;
                        progressReportViewModel.ResearchAccreditedBook = progressReport.ResearchAccreditedBook;
                        progressReportViewModel.ResearchAccreditedConference = progressReport.ResearchAccreditedConference;
                        progressReportViewModel.IsResearchProject = progressReport.IsResearchProject;
                        progressReportViewModel.ResearchProjectSupport = progressReport.ResearchProjectSupport;
                        progressReportViewModel.Activities = progressReport.Activities;
                        progressReportViewModel.Outputs = progressReport.Outputs;
                        progressReportViewModel.Outcome = progressReport.Outcome;
                        progressReportViewModel.IsCollaborativeProject = progressReport.IsCollaborativeProject;
                        progressReportViewModel.CollaborativeProjectSupported = progressReport.CollaborativeProjectSupported;
                        progressReportViewModel.CollaborativeActivities = progressReport.CollaborativeActivities;
                        progressReportViewModel.CollaborativeOutputs = progressReport.CollaborativeOutputs;
                        progressReportViewModel.CollaborativeOutcome = progressReport.CollaborativeOutcome;
                        progressReportViewModel.StaffNumber = progressReport.Application.Applicant.StaffNumber;
                        progressReportViewModel.Title = progressReport.Application.Applicant.Title;
                        progressReportViewModel.FirstName = progressReport.Application.Applicant.FirstName;
                        progressReportViewModel.Surname = progressReport.Application.Applicant.Surname;
                        progressReportViewModel.ApprovedAmount = progressReport.Application.ApprovedAmount;
                        progressReportViewModel.ApplicantCategory = progressReport.Application.ApplicantCategory;
                        progressReportViewModel.FundingCallStartDate = progressReport.Application.FundingCalls.OpeningDate;

                        return progressReportViewModel;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception msg)
            {
                return null;
            }
        }

        public async Task<List<CreateDocumentViewModel>> DocumentUpload(List<CreateDocumentViewModel> model)
        {
            try
            {
                List<CreateDocumentViewModel> savedDocs = new List<CreateDocumentViewModel>();

                if (model.Count > 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.FirstOrDefaultAsync(u => u.Id == model[0].ProgressReportId);

                    if (progressReport != null)
                    {
                        foreach (var doc in model)
                        {
                            ProgressReportDocuments documents = new ProgressReportDocuments();

                            documents.DocumentExtention = doc.DocumentExtention;
                            documents.Filename = doc.Filename;
                            documents.DocumentFile = doc.DocumentFile;
                            documents.UploadType = doc.UploadType;
                            documents.ProgressReport = progressReport;

                            var results = await _context.ProgressReportDocuments.AddAsync(documents);
                            _context.SaveChanges();

                        }

                        await _context.SaveChangesAsync();


                        List<ProgressReportDocuments> availableDocs = await _context.ProgressReportDocuments.Include(c => c.ProgressReport).Where(u => u.ProgressReport.Id == progressReport.Id && u.UploadType.ToLower().Trim() == model[0].UploadType.ToLower().Trim()).ToListAsync();

                        foreach (var item in availableDocs)
                        {
                            var modelUploaded = new CreateDocumentViewModel
                            {
                                DocumentId = item.DocumentId,
                                Filename = item.Filename,
                                DocumentExtention = item.DocumentExtention,
                                DocumentFile = item.DocumentFile,
                                UploadType = item.UploadType,
                                ProgressReportId = item.ProgressReport.Id
                            };

                            savedDocs.Add(modelUploaded);
                        }
                        return savedDocs;
                    }
                }

                return null;

            }
            catch (Exception msg)
            {
                return null;
            }
        }

        public async Task<List<CreateDocumentViewModel>> GetPrgressReportDocumentsByApplicationId(int applicationsId)
        {
            try
            {
                List<CreateDocumentViewModel> documents = new List<CreateDocumentViewModel>();

                ProgressReports progressReports = await _context.ProgressReports.Include(c => c.Application).Where(u => u.Application.Id == applicationsId).FirstOrDefaultAsync();

                List<ProgressReportDocuments> doc = await _context.ProgressReportDocuments.Include(c => c.ProgressReport).Where(u => u.ProgressReport.Id == progressReports.Id).ToListAsync();

                foreach (var item in doc)
                {
                    var model = new CreateDocumentViewModel
                    {
                        DocumentId = item.DocumentId,
                        Filename = item.Filename,
                        DocumentExtention = item.DocumentExtention,
                        DocumentFile = item.DocumentFile,
                        UploadType = item.UploadType,
                        ProgressReportId = item.ProgressReport.Id
                    };

                    documents.Add(model);

                }

                return documents;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }

        }

        public byte[] GetProgressReportDocument(int documentId)
        {
            var res = _context.ProgressReportDocuments.FirstOrDefault(o => o.DocumentId == documentId);
            if (res != null)
                return res.DocumentFile;

            return null;
        }

        public async Task<byte[]> GetProgressReportDocumentV2(int documentId)
        {
            var res = _context.ProgressReportDocuments.FirstOrDefault(o => o.DocumentId == documentId);
            if (res != null)
                if (res.DocumentFile != null)
                {
                    return res.DocumentFile;
                }
                else
                {
                    var result = await _appCircleService.GetDocument(res.DocumentGuid.ToString());
                    if (result != null)
                    {
                        return result.DocumentContent;
                    }
                }
            return null;
        }

        public async Task<List<CreateDocumentViewModel>> DeleteProgressReportDocument(int documentId)
        {
            try
            {
                List<CreateDocumentViewModel> documents = new List<CreateDocumentViewModel>();

                ProgressReportDocuments doc = await _context.ProgressReportDocuments.Include(c => c.ProgressReport).Where(u => u.DocumentId == documentId).FirstOrDefaultAsync();
                int progressReportId = doc.ProgressReport.Id;
                string uploadType = doc.UploadType;

                _context.ProgressReportDocuments.Remove(doc);
                _context.SaveChanges();

                List<ProgressReportDocuments> availableDocs = await _context.ProgressReportDocuments.Include(c => c.ProgressReport).Where(u => u.ProgressReport.Id == progressReportId && u.UploadType.Trim().ToLower() == uploadType.Trim().ToLower()).ToListAsync();

                foreach (var item in availableDocs)
                {
                    var model = new CreateDocumentViewModel
                    {
                        DocumentId = item.DocumentId,
                        Filename = item.Filename,
                        DocumentExtention = item.DocumentExtention,
                        DocumentFile = item.DocumentFile,
                        UploadType = item.UploadType,
                        ProgressReportId = item.ProgressReport.Id
                    };

                    documents.Add(model);

                }

                return documents;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }

        }

        public async Task<List<Applications>> GetProgressReportPending()
        {
            try
            {
                List<ProgressReports> progressReportsList = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).ToListAsync();

                List<Applications> applicationsList = new List<Applications>();


                List<Applications> res = await _context.Applications
                            .Include(o => o.ApplicationStatus)
                            .Include(o => o.Applicant)
                            .Include(o => o.FundingCalls)
                            .Where(o => o.ApplicationStatus.Status.ToLower().Trim() == ApplicationStatusEnum.ApprovedbySIADirector.GetDescription().ToLower().Trim()).ToListAsync();


                foreach (var item in progressReportsList)
                {
                    res.Remove(item.Application);

                }

                return res.OrderBy(c => c.ApplicationEndDate).ToList();

            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }

        }

        public async Task<List<ProgressReportDetailsViewModel>> GetProgressReportSubmitted()
        {
            try
            {
                List<ProgressReportDetailsViewModel> progressReportsList = new List<ProgressReportDetailsViewModel>();
                List<ProgressReports> progressReport = await getProgressReports();

                //  List<ProgressReports> progressReport = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).Include(c => c.Application.Applicant).ToListAsync();

                if (progressReport.Count() > 0)
                {
                    foreach (var report in progressReport)
                    {
                        ProgressReportDetailsViewModel progressReportViewModel = new ProgressReportDetailsViewModel();

                        progressReportViewModel.Id = report.Id;
                        progressReportViewModel.IsComplete = report.IsComplete;
                        progressReportViewModel.CreatedDate = report.CreatedDate;
                        progressReportViewModel.ApplicationId = report.Application.Id;
                        progressReportViewModel.ReferenceNumber = report.Application.ReferenceNumber;
                        progressReportViewModel.ProgressReportStatusId = report.ProgressReportStatus.ProgressReportStatusId;
                        progressReportViewModel.Status = report.ProgressReportStatus.Status;
                        progressReportViewModel.IsQualificationInPrgress = report.IsQualificationInPrgress;
                        progressReportViewModel.QualificationName = report.QualificationName;
                        progressReportViewModel.QualificationInPrgressFieldOfStudy = report.QualificationInPrgressFieldOfStudy;
                        progressReportViewModel.QualificationInPrgressTitleofThesis = report.QualificationInPrgressTitleofThesis;
                        progressReportViewModel.QualificationInPrgressInstitution = report.QualificationInPrgressInstitution;
                        progressReportViewModel.QualificationInPrgressGraduationYear = report.QualificationInPrgressGraduationYear;
                        progressReportViewModel.IsQualificationGraduated = report.IsQualificationGraduated;
                        progressReportViewModel.QualificationGraduatedName = report.QualificationGraduatedName;
                        progressReportViewModel.QualificationGraduatedFieldOfStudy = report.QualificationGraduatedFieldOfStudy;
                        progressReportViewModel.QualificationGraduatedTitleofThesis = report.QualificationGraduatedTitleofThesis;
                        progressReportViewModel.QualificationGraduatedInstitution = report.QualificationGraduatedInstitution;
                        progressReportViewModel.QualificationGraduatedYear = report.QualificationGraduatedYear;
                        progressReportViewModel.IsReliefAppointment = report.IsReliefAppointment;
                        progressReportViewModel.IsResearchPublication = report.IsResearchPublication;
                        progressReportViewModel.ResearchAccreditedJournal = report.ResearchAccreditedJournal;
                        progressReportViewModel.ResearchAccreditedChapter = report.ResearchAccreditedChapter;
                        progressReportViewModel.ResearchAccreditedBook = report.ResearchAccreditedBook;
                        progressReportViewModel.ResearchAccreditedConference = report.ResearchAccreditedConference;
                        progressReportViewModel.IsResearchProject = report.IsResearchProject;
                        progressReportViewModel.ResearchProjectSupport = report.ResearchProjectSupport;
                        progressReportViewModel.Activities = report.Activities;
                        progressReportViewModel.Outputs = report.Outputs;
                        progressReportViewModel.Outcome = report.Outcome;
                        progressReportViewModel.IsCollaborativeProject = report.IsCollaborativeProject;
                        progressReportViewModel.CollaborativeProjectSupported = report.CollaborativeProjectSupported;
                        progressReportViewModel.CollaborativeActivities = report.CollaborativeActivities;
                        progressReportViewModel.CollaborativeOutputs = report.CollaborativeOutputs;
                        progressReportViewModel.CollaborativeOutcome = report.CollaborativeOutcome;
                        progressReportViewModel.StaffNumber = report.Application.Applicant?.StaffNumber;
                        progressReportViewModel.Title = report.Application.Applicant?.Title;
                        progressReportViewModel.FirstName = report.Application.Applicant?.FirstName;
                        progressReportViewModel.Surname = report.Application.Applicant?.Surname;
                        progressReportViewModel.ApprovedAmount = report.Application.ApprovedAmount;
                        progressReportViewModel.ApplicantCategory = report.Application.ApplicantCategory;

                        progressReportsList.Add(progressReportViewModel);
                    }

                    return progressReportsList;
                }

                return null;
            }
            catch (Exception msg)
            {
                return null;
            }
        }

        private async Task<List<ProgressReports>> getProgressReports()
        {
            //try
            //{
            //    var result = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).Include(c => c.Application.User).ToListAsync();
            //    return result;
            //}
            //catch (InvalidOperationException ex)
            //{
            //    if (ex.Message.Contains("c.Application.User")) {
            //        var result = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application).Include(c => c.Application.LegacyUser).ToListAsync();
            //        return result;
            //    }
            //    throw;
            //}

            var result = await _context.ProgressReports.Include(c => c.ProgressReportStatus).Include(c => c.Application)
                .ThenInclude(a => a.Applicant).ToListAsync();

            return result;
        }


        public async Task<ProgressReports> FinalizeProgressReport(ProgressReportDetailsViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.Include(c => c.Application).FirstOrDefaultAsync(u => u.Id == model.Id);

                    if (progressReport != null)
                    {
                        ProgressReportStatus finalizedStatus = await _context.ProgressReportStatus.FirstOrDefaultAsync(u => u.Status == ProgressReportStatusEnum.Finalize.GetDescription());

                        if (finalizedStatus != null)
                        {
                            progressReport.ProgressReportStatus = finalizedStatus;
                        }

                        var results = _context.ProgressReports.Update(progressReport);

                        await _context.SaveChangesAsync();

                        model.Id = progressReport.Id;
                        model.ApplicationId = progressReport.Application.Id;

                        await _emailSentRepository.SendFundingReportFinalizedEmail(model);

                        return progressReport;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception msg)
            {
                return null;
            }
        }

        public async Task<ProgressReports> RFIProgressReport(ProgressReportDetailsViewModel model)
        {
            try
            {
                if (model.Id != 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.Include(c => c.Application).FirstOrDefaultAsync(u => u.Id == model.Id);

                    if (progressReport != null)
                    {
                        ProgressReportStatus RFIStatus = await _context.ProgressReportStatus.FirstOrDefaultAsync(u => u.Status == ProgressReportStatusEnum.RFI.GetDescription());

                        if (RFIStatus != null)
                        {
                            progressReport.ProgressReportStatus = RFIStatus;
                            progressReport.IsComplete = false;
                        }

                        var results = _context.ProgressReports.Update(progressReport);

                        await _context.SaveChangesAsync();

                        model.Id = progressReport.Id;
                        model.ApplicationId = progressReport.Application.Id;

                        await _emailSentRepository.SendApplicantRfiEmail(model);

                        return progressReport;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception msg)
            {
                return null;
            }
        }

        public async Task<ProgressReportComments> AddComments(ProgressReportCommentsViewModel model)
        {
            try
            {
                if (model.ProgressReportId != 0)
                {
                    ProgressReports progressReports = await _context.ProgressReports.FirstOrDefaultAsync(u => u.Id == model.ProgressReportId);

                    User user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);

                    if (progressReports != null)
                    {
                        ProgressReportComments progressReportComment = new ProgressReportComments();

                        progressReportComment.ProgressReports = progressReports;
                        progressReportComment.User = user;
                        progressReportComment.Comment = model.Comment;

                        var results = await _context.ProgressReportComments.AddAsync(progressReportComment);

                        await _context.SaveChangesAsync();

                        return progressReportComment;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<ProgressReportComments> AddCommentsV2(ProgressReportCommentsViewModel model)
        {
            try
            {
                if (model.ProgressReportId != 0)
                {
                    ProgressReports progressReports = await _context.ProgressReports.FirstOrDefaultAsync(u => u.Id == model.ProgressReportId);

                    var user = await _userStoreDbContext.Users.FirstOrDefaultAsync(u => u.UserId == model.UserId);

                    if (progressReports != null)
                    {
                        ProgressReportComments progressReportComment = new ProgressReportComments();

                        progressReportComment.ProgressReports = progressReports;
                        progressReportComment.UserStoreUserId = user.UserId;
                        progressReportComment.Comment = model.Comment;

                        var results = await _context.ProgressReportComments.AddAsync(progressReportComment);

                        await _context.SaveChangesAsync();

                        return progressReportComment;
                    }

                    return null;
                }

                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        #region appstore documents
        public async Task<List<CreateDocumentViewModel>> DocumentUploadV2(List<CreateDocumentViewModel> model)
        {
            try
            {
                List<CreateDocumentViewModel> savedDocs = new List<CreateDocumentViewModel>();

                if (model.Count > 0)
                {
                    ProgressReports progressReport = await _context.ProgressReports.FirstOrDefaultAsync(u => u.Id == model[0].ProgressReportId);

                    if (progressReport != null)
                    {
                        using (_context)
                        {
                            using (var transaction = await _context.Database.BeginTransactionAsync())
                            {
                                try
                                {
                                    var batchGuid = Guid.NewGuid();
                                    foreach (var doc in model)
                                    {
                                        ProgressReportDocuments document = new ProgressReportDocuments();

                                        document.DocumentExtention = doc.DocumentExtention;
                                        document.Filename = doc.Filename;
                                        document.DocumentFile = null; // Set to null since the file content will be stored in the external document store
                                        document.UploadType = doc.UploadType;
                                        document.ProgressReport = progressReport;
                                        document.BatchGuid = batchGuid;
                                        document.DocumentGuid = Guid.NewGuid();

                                        var results = await _context.ProgressReportDocuments.AddAsync(document);
                                        var savedItem = await _context.SaveChangesAsync();

                                        var modelUploaded = new CreateDocumentViewModel
                                        {
                                            DocumentId = document.DocumentId,
                                            Filename = doc.Filename,
                                            DocumentExtention = doc.DocumentExtention,
                                            DocumentFile = doc.DocumentFile,
                                            UploadType = doc.UploadType,
                                            ProgressReportId = progressReport.Id,
                                            BatchGuid = document.BatchGuid,
                                            DocumentGuid = document.DocumentGuid
                                        };
                                        savedDocs.Add(modelUploaded);
                                    }

                                    var result = await _appCircleService.UploadDocuments([.. savedDocs.Select(d => new DocumentCreationModel
                                    {
                                        DocumentGuid = d.DocumentGuid,
                                        BatchGuid = d.BatchGuid,
                                        OriginalDocumentName = d.Filename,
                                        DocumentContent = d.DocumentFile,
                                        IsArchived = false
                                    })]);
                                    if (!result.IsSuccessStatusCode)
                                    {
                                        throw new Exception("Failed to upload documents to external document store");
                                    }

                                    // Commit if all operations succeed
                                    await transaction.CommitAsync();
                                }
                                catch (Exception ex)
                                {
                                    // Rollback if anything fails
                                    await transaction.RollbackAsync();
                                    throw;
                                }
                            }
                        }
                        return savedDocs;
                    }
                }

                return null;

            }
            catch (Exception msg)
            {
                return null;
            }
        }
        #endregion
    }
}
