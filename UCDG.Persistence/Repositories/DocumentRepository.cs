using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using UCDG.Domain.Entities;
using UDCG.Application.Common.AppCircle.DocumentStore;
using UDCG.Application.Feature.Application;
using UDCG.Application.Feature.Application.Interface;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.ProgressReport;
using UDCG.Application.Interface;

namespace UCDG.Persistence.Repositories
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly UCDGDbContext _context;
        private readonly IAppCircleService _appCircleService;

        public DocumentRepository(UCDGDbContext context, IAppCircleService appCircleService)
        {
            _context = context;
            _appCircleService = appCircleService;
        }

        public async Task<List<UploadDocumentViewModel>> Add(List<UploadDocumentViewModel> model)
        {
            try
            {
                // List<UploadDocumentViewModel> savedDocs = new List<UploadDocumentViewModel>();

                Applications applications = await _context.Applications.FirstOrDefaultAsync(u => u.Id == model[0].ApplicationId);

                foreach (var doc in model)
                {

                    Documents documents = new Documents();

                    documents.DocumentExtention = doc.DocumentExtention;
                    documents.Filename = doc.Filename;
                    documents.DocumentFile = doc.DocumentFile;
                    documents.UploadType = doc.UploadType;
                    documents.Applications = applications;

                    await _context.Documents.AddAsync(documents);
                    _context.SaveChanges();

                }

                await _context.SaveChangesAsync();

                var savedDocs = await _context.Documents
                      .Where(u => u.ApplicationsId == applications.Id && u.UploadType.ToLower().Trim() == model[0].UploadType.ToLower().Trim())
                      .Select(item => new UploadDocumentViewModel
                      {
                          Id = item.Id,
                          Filename = item.Filename,
                          DocumentExtention = item.DocumentExtention,
                          //DocumentFile = item.DocumentFile,
                          UploadType = item.UploadType,
                          ApplicationId = item.ApplicationsId
                      })
                  .ToListAsync();


                //List<Documents> availableDocs = await _context.Documents.Where(u => u.ApplicationId == applications.Id && u.UploadType.ToLower().Trim() == model[0].UploadType.ToLower().Trim()).ToListAsync();

                //foreach (var item in availableDocs)
                //{
                //    var modelUploaded = new UploadDocumentViewModel
                //    {
                //        Id = item.Id,
                //        Filename = item.Filename,
                //        DocumentExtention = item.DocumentExtention,
                //        DocumentFile = item.DocumentFile,
                //        UploadType = item.UploadType,
                //        ApplicationId = item.Applications.Id
                //    };

                //    savedDocs.Add(modelUploaded);
                //}
                return savedDocs;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<List<UploadDocumentViewModel>> AddV2(List<UploadDocumentViewModel> model)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                List<UploadDocumentViewModel> savedDocs = new List<UploadDocumentViewModel>();
                try
                {
                    Applications application = await _context.Applications.FirstOrDefaultAsync(u => u.Id == model[0].ApplicationId);
                    var batchGuid = Guid.NewGuid();
                    foreach (var doc in model)
                    {

                        Documents document = new Documents();

                        document.DocumentExtention = doc.DocumentExtention;
                        document.Filename = doc.Filename;
                        document.DocumentFile = null;
                        document.UploadType = doc.UploadType;
                        document.Applications = application;
                        document.BatchGuid = batchGuid;
                        document.DocumentGuid = Guid.NewGuid();

                        await _context.Documents.AddAsync(document);
                        await _context.SaveChangesAsync();

                        var modelUploaded = new UploadDocumentViewModel
                        {
                            Id = document.Id,
                            Filename = doc.Filename,
                            DocumentExtention = doc.DocumentExtention,
                            DocumentFile = doc.DocumentFile,
                            UploadType = doc.UploadType,
                            ApplicationId = application.Id,
                            BatchGuid = document.BatchGuid,
                            DocumentGuid = document.DocumentGuid
                        };
                        savedDocs.Add(modelUploaded);
                    }

                    await _context.SaveChangesAsync();

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
                catch (Exception Msg)
                {
                    transaction.Rollback();
                    throw new NotImplementedException(Msg.ToString());
                }
                return savedDocs;
            }
        }

        public async Task<List<UploadDocumentViewModel>> GetDocumentsByApplicationId(int applicationsId)
        {
            try
            {
                // List<UploadDocumentViewModel> documents = new List<UploadDocumentViewModel>();
                var documents = await _context.Documents
                    .Where(u => u.ApplicationsId == applicationsId)
                    .Select(item => new UploadDocumentViewModel
                    {
                        Id = item.Id,
                        Filename = item.Filename,
                        DocumentExtention = item.DocumentExtention,
                        //DocumentFile = item.DocumentFile,
                        UploadType = item.UploadType,
                        ApplicationId = item.ApplicationsId
                    })
                .ToListAsync();

                //foreach (var item in doc)
                //{
                //    var model = new UploadDocumentViewModel
                //    {
                //        Id = item.Id,
                //        Filename = item.Filename,
                //        DocumentExtention = item.DocumentExtention,
                //        DocumentFile = item.DocumentFile,
                //        UploadType = item.UploadType,
                //        ApplicationId = item.Applications.Id
                //    };

                //    documents.Add(model);

                //}

                return documents;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }

        }

        public async Task<List<Documents>> GetDocumentsListByApplicationsId(int applicationsId)
        {
            //   var docList = new List<Documents>();
            var res = await _context.Documents.Where(u => u.ApplicationsId == applicationsId)
                .Select(o => new Documents { Id = o.Id, Filename = o.Filename, UploadType = o.UploadType, DocumentExtention = o.DocumentExtention })
                .ToListAsync();
            //foreach (var doc in res)
            //{
            //    var docObj = new Documents()
            //    {
            //        DocumentExtention = doc.DocumentExtention,
            //        Id = doc.Id,
            //        UploadType = doc.UploadType,
            //        Filename = doc.Filename,
            //    };
            //    docList.Add(docObj);
            //}
            return res;
        }
        public async Task<int> GetApplicationsDocumentsCountById(int applicationsId)
        {
            //  var docList = new List<Documents>();
            var res = await _context.Documents.Where(u => u.ApplicationsId == applicationsId).Select(o => new { o.Id, o.Filename, o.UploadType, o.DocumentExtention }).CountAsync();
            //if (res != null)
            return res;

            //  return 0;
        }

        public byte[] GetDocumentById(int documentId)
        {
            var res = _context.Documents.FirstOrDefault(o => o.Id == documentId);
            if (res != null)
                return res.DocumentFile;

            return null;
        }

        public async Task<List<UploadDocumentViewModel>> DeleteDocumentById(int documentId)
        {
            try
            {
                //  List<UploadDocumentViewModel> documents = new List<UploadDocumentViewModel>();

                Documents doc = await _context.Documents.Where(u => u.Id == documentId).FirstOrDefaultAsync();
                int applicationId = doc.ApplicationsId;
                string uploadType = doc.UploadType;

                _context.Documents.Remove(doc);
                _context.SaveChanges();

                var documents = await _context.Documents
                    .Where(u => u.ApplicationsId == applicationId && u.UploadType.Trim().ToLower() == uploadType.Trim().ToLower())
                       .Select(item => new UploadDocumentViewModel
                       {
                           Id = item.Id,
                           Filename = item.Filename,
                           DocumentExtention = item.DocumentExtention,
                           //DocumentFile = item.DocumentFile,
                           UploadType = item.UploadType,
                           ApplicationId = item.ApplicationsId
                       })
                   .ToListAsync();

                //foreach (var item in availableDocs)
                //{
                //    var model = new UploadDocumentViewModel
                //    {
                //        Id = item.Id,
                //        Filename = item.Filename,
                //        DocumentExtention = item.DocumentExtention,
                //        DocumentFile = item.DocumentFile,
                //        UploadType = item.UploadType,
                //        ApplicationId = item.Applications.Id
                //    };

                //    documents.Add(model);

                //}

                return documents;
            }
            catch (Exception ex)
            {
                throw new NotImplementedException(ex.Message.ToString());
            }

        }


        public async Task<List<MotivationLetterReadModel>> CreateMotivationalLetterDoc(List<MotivationLetterReadModel> model)
        {
            try
            {

                foreach (var doc in model)
                {

                    LinkUserMotivationLetter documents = new LinkUserMotivationLetter();
                    documents.DocumentExtention = doc.DocumentExtention;
                    documents.Filename = doc.Filename;
                    documents.DocumentFile = doc.DocumentFile;
                    documents.UploadType = doc.UploadType;


                    await _context.LinkUserMotivationLetter.AddAsync(documents);
                    _context.SaveChanges();

                }

                await _context.SaveChangesAsync();

                var savedDocs = await _context.LinkUserMotivationLetter
                      .Where(u => u.Filename.ToLower().Trim() == model[0].Filename.ToLower().Trim() && u.UploadType.ToLower().Trim() == model[0].UploadType.ToLower().Trim())
                      .Select(item => new MotivationLetterReadModel
                      {
                          Id = item.Id,
                          Filename = item.Filename,
                          DocumentExtention = item.DocumentExtention,
                          DocumentFile = item.DocumentFile,
                          UploadType = item.UploadType

                      })
                  .ToListAsync();

                return savedDocs;
            }
            catch (Exception Msg)
            {
                throw new NotImplementedException(Msg.ToString());
            }
        }

        public async Task<byte[]> GetDocumentByIdV2(int documentId)
        {
            var res = _context.Documents.FirstOrDefault(o => o.Id == documentId);
            if (res != null)
            {
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
            return res.DocumentFile;
        }
    }
}
