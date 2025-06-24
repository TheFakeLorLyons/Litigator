using AutoMapper;
using Litigator.DataAccess.Entities;
using Litigator.Models.DTOs.Case;
using Litigator.Models.DTOs.Deadline;
using Litigator.Models.DTOs.Document;

namespace Litigator.Models.Mapping
{
    public class LitigatorMappingProfile : Profile
    {
        public LitigatorMappingProfile()
        {
            // Case mappings
            CreateMap<Case, CaseDTO>()
                .ForMember(dest => dest.ClientName, opt => opt.MapFrom(src => src.Client != null ? src.Client.ClientName : string.Empty))
                .ForMember(dest => dest.AttorneyFirstName, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.FirstName : null))
                .ForMember(dest => dest.AttorneyLastName, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.LastName : null))
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court != null ? src.Court.CourtName : null))
                .ForMember(dest => dest.OpenDeadlines, opt => opt.MapFrom(src => src.Deadlines != null ? src.Deadlines.Count(d => !d.IsCompleted) : 0));

            // Deadline mappings
            CreateMap<Deadline, DeadlineDTO>()
                .ForMember(dest => dest.CaseNumber, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseNumber : null))
                .ForMember(dest => dest.CaseTitle, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseTitle : null));

            CreateMap<DeadlineCreateDTO, Deadline>()
                .ForMember(dest => dest.DeadlineId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CompletedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore());

            CreateMap<DeadlineUpdateDTO, Deadline>()
                .ForMember(dest => dest.DeadlineId, opt => opt.Ignore())
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore());

            // Document mappings
            CreateMap<Document, DocumentDTO>()
                .ForMember(dest => dest.CaseNumber, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseNumber : null))
                .ForMember(dest => dest.CaseTitle, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseTitle : null));

            CreateMap<DocumentCreateDTO, Document>()
                .ForMember(dest => dest.DocumentId, opt => opt.Ignore())
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore());

            CreateMap<DocumentUpdateDTO, Document>()
                .ForMember(dest => dest.DocumentId, opt => opt.Ignore())
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // Don't update file path through update DTO
                .ForMember(dest => dest.FileSize, opt => opt.Ignore()) // Don't update file size through update DTO
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore());
        }
    }
}