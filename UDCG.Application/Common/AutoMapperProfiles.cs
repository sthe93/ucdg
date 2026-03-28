using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;
using UCDG.Domain.Entities;
using UDCG.Application.Feature.Application;
using UDCG.Application.Feature.Application.Resources;
using UDCG.Application.Feature.DocumentSignOff.Resources;
using UDCG.Application.Feature.Emails;
using UDCG.Application.Feature.ErrorLogs.Resources;
using UDCG.Application.Feature.FundingCalls.Resources;
using UDCG.Application.Feature.Project.Resources;
using UDCG.Application.Feature.Roles.Resources;

namespace UDCG.Application.Common
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            // CreateMap<ReadRoleResource, Role>();
            CreateMap<Role, ReadRoleResource>();

            CreateMap<UserStoreUser, ReadUserViewModelResource>();
            CreateMap<ReadUserViewModelResource, UserStoreUser>();

            CreateMap<User, ReadUserViewModelResource>();
            CreateMap<ReadUserViewModelResource, User>();

            //  CreateMap<ReadQualificationResource, Qualification>().ForMember(o => o.UserId, opts => opts.Ignore());
            CreateMap<UserQualificationViewModel, Qualification>();
            CreateMap<Qualification, UserQualificationViewModel>();
            CreateMap<ApplicationDetailsViewModel, Applications>();
            CreateMap<Applications, ApplicationDetailsViewModel>();


            CreateMap<Applications, ApplicationDetailsViewModel>()
                .ForMember(dest => dest.ApplicationStatusId, opt => opt.Ignore())
                .ForMember(dest => dest.UserId, opt => opt.Ignore())
                .ForMember(dest => dest.FundingCallDetailsId, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.Ignore())
                .ForMember(dest => dest.EndDate, opt => opt.Ignore())
                .ForMember(dest => dest.SupportRequired, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentOption, opt => opt.Ignore())
                .ForMember(dest => dest.FinancialSupport, opt => opt.Ignore())
                .ForMember(dest => dest.CareerFinancialSupport, opt => opt.Ignore())
                .ForMember(dest => dest.CareerTeachingRelief, opt => opt.Ignore())
                .ForMember(dest => dest.FundingCallDetailName, opt => opt.Ignore()) // Assuming FundingCalls has a Name property

                .ForMember(dest => dest.ProjectId, opt => opt.Ignore()) // Assuming this needs to be populated elsewhere
                .ForMember(dest => dest.ProjectName, opt => opt.Ignore()) // Assuming this needs to be populated elsewhere
                .ForMember(dest => dest.IsAcknowledge, opt => opt.Ignore()); // Assuming this needs to be populated elsewhere


            CreateMap<Applications, ReadApplicationResource>();
            CreateMap<ApplicationStatus, ReadApplicationStatusResouce>();
            CreateMap<FundingCallStatus, ReadFundingCallStatusResource>();
            CreateMap<ProjectCycles, ReadProjectCyclesResource>();

            CreateMap<FundingCalls, ReadFundingCallsResource>();
            CreateMap<Documents, ReadDocumentResource>();
            CreateMap<Comments, AppplicationCommentVeiwModel>(); 
            CreateMap<DocumentSignOffs, ReadDocumentSignOffViewModel>();
            CreateMap<ErrorLogs, ErrorLogsViewModel>();
            CreateMap<MessageRecipientDetails, MessageRecipientDetailsViewModel>();



              // Ensure all mappings are explicitly defined
        CreateMap<Applications, ReadApplicationResource>()
            .ForMember(dest => dest.FundingCalls, opt => opt.MapFrom(src => src.FundingCalls));

        CreateMap<FundingCalls, ReadFundingCallsResource>()
            .ForMember(dest => dest.FundingCallProjects, opt => opt.MapFrom(src => src.FundingCallProjects));

        CreateMap<Projects, ProjectsV2>(); // Ensure AutoMapper understands this transformation

        // If there are other complex objects, define their mappings too
        CreateMap<UserStoreUser, ReadUserResource>();
       
            
        }
    }
}
