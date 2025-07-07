using AutoMapper;
using Litigator.DataAccess.Entities;
using Litigator.DataAccess.ValueObjects;
using Litigator.Models.DTOs.ClassDTOs;
using Litigator.Models.DTOs.Shared;

namespace Litigator.Models.Mapping
{
    public class LitigatorMappingProfile : Profile
    {
        public LitigatorMappingProfile()
        {
            // Case mappings
            CreateMap<Case, CaseDTO>()
                .ForMember(dest => dest.ClientFirstName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name.First : string.Empty))
                .ForMember(dest => dest.ClientLastName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name.Last : string.Empty))
                .ForMember(dest => dest.AttorneyFirstName, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.Name.First : null))
                .ForMember(dest => dest.AttorneyLastName, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.Name.Last : null))
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court != null ? src.Court.CourtName : null))
                .ForMember(dest => dest.OpenDeadlines, opt => opt.MapFrom(src => src.Deadlines != null ? src.Deadlines.Count(d => !d.IsCompleted) : 0));

            CreateMap<Case, CaseDetailDTO>()
                .ForMember(dest => dest.ClientFirstName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name.First : string.Empty))
                .ForMember(dest => dest.ClientLastName, opt => opt.MapFrom(src => src.Client != null ? src.Client.Name.Last : string.Empty))
                .ForMember(dest => dest.ClientEmail, opt => opt.MapFrom(src => src.Client != null ? src.Client.Email : null))
                .ForMember(dest => dest.ClientPhone, opt => opt.MapFrom(src => src.Client != null && src.Client.PrimaryPhone != null ? src.Client.PrimaryPhone.Display : null))
                .ForMember(dest => dest.AttorneyFirstName, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.Name.First : null))
                .ForMember(dest => dest.AttorneyLastName, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.Name.Last : null))
                .ForMember(dest => dest.AttorneyEmail, opt => opt.MapFrom(src => src.AssignedAttorney != null ? src.AssignedAttorney.Email : null))
                .ForMember(dest => dest.CourtName, opt => opt.MapFrom(src => src.Court != null ? src.Court.CourtName : null))
                .ForMember(dest => dest.CourtAddress, opt => opt.MapFrom(src => src.Court != null && src.Court.Address != null ? src.Court.Address.ToString() : null))
                .ForMember(dest => dest.TotalDeadlines, opt => opt.MapFrom(src => src.Deadlines != null ? src.Deadlines.Count : 0))
                .ForMember(dest => dest.OpenDeadlines, opt => opt.MapFrom(src => src.Deadlines != null ? src.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now) : 0))
                .ForMember(dest => dest.OverdueDeadlines, opt => opt.MapFrom(src => src.Deadlines != null ? src.Deadlines.Count(d => !d.IsCompleted && d.DeadlineDate < DateTime.Now) : 0))
                .ForMember(dest => dest.TotalDocuments, opt => opt.MapFrom(src => src.Documents != null ? src.Documents.Count : 0))
                .ForMember(dest => dest.NextDeadlineDate, opt => opt.MapFrom<NextDeadlineDateResolver>())
                .ForMember(dest => dest.NextDeadlineDescription, opt => opt.MapFrom<NextDeadlineDescriptionResolver>());

            CreateMap<CaseCreateDTO, Case>()
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedAttorney, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedJudge, opt => opt.Ignore())
                .ForMember(dest => dest.Court, opt => opt.Ignore())
                .ForMember(dest => dest.Attorneys, opt => opt.Ignore())
                .ForMember(dest => dest.Clients, opt => opt.Ignore())
                .ForMember(dest => dest.Deadlines, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());

            CreateMap<CaseUpdateDTO, Case>()
                .ForMember(dest => dest.CaseId, opt => opt.Ignore())
                .ForMember(dest => dest.Client, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedAttorney, opt => opt.Ignore())
                .ForMember(dest => dest.AssignedJudge, opt => opt.Ignore())
                .ForMember(dest => dest.Court, opt => opt.Ignore())
                .ForMember(dest => dest.Attorneys, opt => opt.Ignore())
                .ForMember(dest => dest.Clients, opt => opt.Ignore())
                .ForMember(dest => dest.Deadlines, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore());
            //Court mappings
            CreateMap<Court, CourtDTO>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Display : null))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address != null ? src.Address.SingleLine : null))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")));

            CreateMap<Court, CourtDetailDTO>()
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => src.Phone != null ? src.Phone.Display : null))
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address != null ? src.Address.SingleLine : null))
                .ForMember(dest => dest.AddressDetails, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.PhoneDetails, opt => opt.MapFrom(src => src.Phone))
                .ForMember(dest => dest.ChiefJudge, opt => opt.MapFrom(src => src.ChiefJudge != null ? src.ChiefJudge.Name.Display : null))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")))
                .ForMember(dest => dest.ClosedCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Closed")))
                .ForMember(dest => dest.LastCaseDate, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().FilingDate : (DateTime?)null))
                .ForMember(dest => dest.MostRecentCaseTitle, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().CaseTitle : null));

            CreateMap<CreateCourtDTO, Court>()
                .ForMember(dest => dest.CourtId, opt => opt.Ignore())
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Phone) ? new PhoneNumber { Number = src.Phone } : null))
                .ForMember(dest => dest.ChiefJudge, opt => opt.Ignore())
                .ForMember(dest => dest.Cases, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            CreateMap<UpdateCourtDTO, Court>()
                .ForMember(dest => dest.Address, opt => opt.MapFrom(src => src.Address))
                .ForMember(dest => dest.Phone, opt => opt.MapFrom(src => !string.IsNullOrWhiteSpace(src.Phone) ? new PhoneNumber { Number = src.Phone } : null))
                .ForMember(dest => dest.ChiefJudge, opt => opt.Ignore())
                .ForMember(dest => dest.Cases, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            // Attorney mappings
            CreateMap<Attorney, AttorneyDTO>()
                .ForMember(dest => dest.AttorneyId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Display))
                .ForMember(dest => dest.BarNumber, opt => opt.MapFrom(src => src.BarNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone != null ? src.PrimaryPhone.Display : null))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")));

            CreateMap<Attorney, AttorneyDetailDTO>()
                .ForMember(dest => dest.AttorneyId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name.Display))
                .ForMember(dest => dest.ProfessionalName, opt => opt.MapFrom(src => src.Name.Professional))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name.Full))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.First))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Name.Last))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.Name.Middle))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name.Title))
                .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.Name.Suffix))
                .ForMember(dest => dest.PreferredName, opt => opt.MapFrom(src => src.Name.Preferred))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone != null ? src.PrimaryPhone.Display : null))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src => src.PrimaryAddress != null ? src.PrimaryAddress.SingleLine : null))
                .ForMember(dest => dest.AllAddresses, opt => opt.MapFrom(src => src.GetAllAddresses()))
                .ForMember(dest => dest.AllPhones, opt => opt.MapFrom(src => src.GetAllPhoneNumbers()))
                .ForMember(dest => dest.Clients, opt => opt.MapFrom(src => src.Clients))
                .ForMember(dest => dest.BarNumber, opt => opt.MapFrom(src => src.BarNumber))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")))
                .ForMember(dest => dest.ClosedCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Closed")))
                .ForMember(dest => dest.LastCaseDate, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().FilingDate : (DateTime?)null))
                .ForMember(dest => dest.MostRecentCaseTitle, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().CaseTitle : null))
                .ForMember(dest => dest.TotalCaseValue, opt => opt.MapFrom(src => src.Cases.Sum(c => c.EstimatedValue)));

            CreateMap<AttorneyDetailDTO, Attorney>()
                .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.AttorneyId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new PersonName
                {
                    First = src.FirstName,
                    Last = src.LastName,
                    Middle = src.MiddleName,
                    Title = src.Title,
                    Suffix = src.Suffix,
                    Preferred = src.PreferredName
                }))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.PrimaryAddress) ? ParseAddressFromString(src.PrimaryAddress) : null))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.PrimaryPhone) ? new PhoneNumber { Number = src.PrimaryPhone } : null))
                .ForMember(dest => dest.BarNumber, opt => opt.MapFrom(src => src.BarNumber))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Cases, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalAddresses, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalPhones, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            // Judge mappings
            CreateMap<Judge, JudgeDTO>()
                .ForMember(dest => dest.JudgeId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Display))
                .ForMember(dest => dest.BarNumber, opt => opt.MapFrom(src => src.BarNumber))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone != null ? src.PrimaryPhone.Display : null))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")));

            CreateMap<Judge, JudgeDetailDTO>()
                .ForMember(dest => dest.JudgeId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name.Display))
                .ForMember(dest => dest.ProfessionalName, opt => opt.MapFrom(src => src.Name.Professional))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name.Full))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.First))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Name.Last))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.Name.Middle))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name.Title))
                .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.Name.Suffix))
                .ForMember(dest => dest.PreferredName, opt => opt.MapFrom(src => src.Name.Preferred))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone != null ? src.PrimaryPhone.Display : null))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src => src.PrimaryAddress != null ? src.PrimaryAddress.SingleLine : null))
                .ForMember(dest => dest.AllAddresses, opt => opt.MapFrom(src => src.GetAllAddresses()))
                .ForMember(dest => dest.AllPhones, opt => opt.MapFrom(src => src.GetAllPhoneNumbers()))
                .ForMember(dest => dest.BarNumber, opt => opt.MapFrom(src => src.BarNumber))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")))
                .ForMember(dest => dest.ClosedCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Closed")))
                .ForMember(dest => dest.LastCaseDate, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().FilingDate : (DateTime?)null))
                .ForMember(dest => dest.MostRecentCaseTitle, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().CaseTitle : null))
                .ForMember(dest => dest.TotalCaseValue, opt => opt.MapFrom(src => src.Cases.Sum(c => c.EstimatedValue)));

            CreateMap<JudgeDetailDTO, Judge>()
                .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.JudgeId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new PersonName
                {
                    First = src.FirstName,
                    Last = src.LastName,
                    Middle = src.MiddleName,
                    Title = src.Title,
                    Suffix = src.Suffix,
                    Preferred = src.PreferredName
                }))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.PrimaryAddress) ? ParseAddressFromString(src.PrimaryAddress) : null))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.PrimaryPhone) ? new PhoneNumber { Number = src.PrimaryPhone } : null))
                .ForMember(dest => dest.BarNumber, opt => opt.MapFrom(src => src.BarNumber))
                .ForMember(dest => dest.Specialization, opt => opt.MapFrom(src => src.Specialization))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Cases, opt => opt.Ignore())
                .ForMember(dest => dest.Courts, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalAddresses, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalPhones, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            // Client mappings
            CreateMap<Client, ClientDTO>()
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.Display))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone != null ? src.PrimaryPhone.Display : null))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src => src.PrimaryAddress != null ? src.PrimaryAddress.SingleLine : null))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")));

            CreateMap<Client, ClientDetailDTO>()
                .ForMember(dest => dest.ClientId, opt => opt.MapFrom(src => src.SystemId))
                .ForMember(dest => dest.DisplayName, opt => opt.MapFrom(src => src.Name.Display))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.Name.Full))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.Name.First))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.Name.Last))
                .ForMember(dest => dest.MiddleName, opt => opt.MapFrom(src => src.Name.Middle))
                .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Name.Title))
                .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src => src.Name.Suffix))
                .ForMember(dest => dest.PreferredName, opt => opt.MapFrom(src => src.Name.Preferred))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src => src.PrimaryPhone != null ? src.PrimaryPhone.Display : null))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src => src.PrimaryAddress != null ? src.PrimaryAddress.SingleLine : null))
                .ForMember(dest => dest.AllAddresses, opt => opt.MapFrom(src => src.GetAllAddresses()))
                .ForMember(dest => dest.AllPhones, opt => opt.MapFrom(src => src.GetAllPhoneNumbers()))
                .ForMember(dest => dest.Attorneys, opt => opt.MapFrom(src => src.Attorneys))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => src.ModifiedDate))
                .ForMember(dest => dest.TotalCases, opt => opt.MapFrom(src => src.Cases.Count))
                .ForMember(dest => dest.ActiveCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Active")))
                .ForMember(dest => dest.ClosedCases, opt => opt.MapFrom(src => src.Cases.Count(c => c.Status == "Closed")))
                .ForMember(dest => dest.LastCaseDate, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().FilingDate : (DateTime?)null))
                .ForMember(dest => dest.MostRecentCaseTitle, opt => opt.MapFrom(src => src.Cases.Any() ? src.Cases.OrderByDescending(c => c.FilingDate).First().CaseTitle : null));

            CreateMap<ClientDetailDTO, Client>()
                .ForMember(dest => dest.SystemId, opt => opt.MapFrom(src => src.ClientId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new PersonName
                {
                    First = src.FirstName,
                    Last = src.LastName,
                    Middle = src.MiddleName,
                    Title = src.Title,
                    Suffix = src.Suffix,
                    Preferred = src.PreferredName
                }))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PrimaryAddress, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.PrimaryAddress) ? ParseAddressFromString(src.PrimaryAddress) : null))
                .ForMember(dest => dest.PrimaryPhone, opt => opt.MapFrom(src =>
                    !string.IsNullOrWhiteSpace(src.PrimaryPhone) ? new PhoneNumber { Number = src.PrimaryPhone } : null))
                .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.Cases, opt => opt.Ignore())
                .ForMember(dest => dest.Attorneys, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalAddresses, opt => opt.Ignore())
                .ForMember(dest => dest.AdditionalPhones, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            // Deadline mappings
            CreateMap<Deadline, DeadlineDTO>()
                .ForMember(dest => dest.CaseNumber, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseNumber : null))
                .ForMember(dest => dest.CaseTitle, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseTitle : null));

            CreateMap<DeadlineCreateDTO, Deadline>()
                .ForMember(dest => dest.DeadlineId, opt => opt.Ignore())
                .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
                .ForMember(dest => dest.CompletedDate, opt => opt.MapFrom(src => (DateTime?)null))
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .ForMember(dest => dest.Case, opt => opt.Ignore());

            CreateMap<DeadlineUpdateDTO, Deadline>()
                .ForMember(dest => dest.DeadlineId, opt => opt.Ignore())
                .ForMember(dest => dest.CaseId, opt => opt.Ignore()) // Don't allow changing case
                .ForMember(dest => dest.Case, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.Notes, opt => opt.Ignore())
                .AfterMap((src, dest) =>
                {
                    // If marking as completed but no completed date provided, set to now
                    if (src.IsCompleted && !src.CompletedDate.HasValue)
                    {
                        dest.CompletedDate = DateTime.Now;
                    }
                    // If marking as not completed, clear the completed date
                    else if (!src.IsCompleted)
                    {
                        dest.CompletedDate = null;
                    }
                });

            // Document mappings
            CreateMap<Document, DocumentDTO>()
                .ForMember(dest => dest.CaseNumber, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseNumber : null))
                .ForMember(dest => dest.CaseTitle, opt => opt.MapFrom(src => src.Case != null ? src.Case.CaseTitle : null));

            CreateMap<DocumentCreateDTO, Document>()
                .ForMember(dest => dest.DocumentId, opt => opt.Ignore())
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore()) // Set in service
                .ForMember(dest => dest.Case, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore()); // Not in DTO

            CreateMap<DocumentUpdateDTO, Document>()
                .ForMember(dest => dest.DocumentId, opt => opt.Ignore())
                .ForMember(dest => dest.FilePath, opt => opt.Ignore()) // Not updatable
                .ForMember(dest => dest.UploadDate, opt => opt.Ignore()) // Not updatable
                .ForMember(dest => dest.FileSize, opt => opt.Ignore()) // Not updatable
                .ForMember(dest => dest.CaseId, opt => opt.Ignore()) // Not updatable
                .ForMember(dest => dest.Case, opt => opt.Ignore())
                .ForMember(dest => dest.Description, opt => opt.Ignore());


            // Shared DTO mappings
            CreateMap<Address, AddressDTO>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => AddressType.Primary)) // Default type
                .ForMember(dest => dest.SingleLine, opt => opt.MapFrom(src => src.SingleLine))
                .ForMember(dest => dest.MultiLine, opt => opt.MapFrom(src => src.MultiLine));

            CreateMap<AddressDTO, Address>()
                .ForMember(dest => dest.SingleLine, opt => opt.Ignore())
                .ForMember(dest => dest.MultiLine, opt => opt.Ignore())
                .AfterMap((src, dest) => dest.SetPopulated()); // Ensure populated flag is set

            CreateMap<PhoneNumber, PhoneNumberDTO>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => PhoneType.Primary)) // Default type
                .ForMember(dest => dest.Formatted, opt => opt.MapFrom(src => src.Formatted))
                .ForMember(dest => dest.Display, opt => opt.MapFrom(src => src.Display));

            CreateMap<PhoneNumberDTO, PhoneNumber>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.Number))
                .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.Extension))
                .ForMember(dest => dest.Formatted, opt => opt.Ignore())
                .ForMember(dest => dest.Display, opt => opt.Ignore())
                .AfterMap((src, dest) => dest.SetPopulated()); // Ensure populated flag is set

            // PersonAddress and PersonPhoneNumber mappings
            CreateMap<PersonAddress, AddressDTO>()
                .ForMember(dest => dest.Line1, opt => opt.MapFrom(src => src.Address.Line1))
                .ForMember(dest => dest.Line2, opt => opt.MapFrom(src => src.Address.Line2))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.Address.City))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.Address.State))
                .ForMember(dest => dest.PostalCode, opt => opt.MapFrom(src => src.Address.PostalCode))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Address.Country))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.SingleLine, opt => opt.MapFrom(src => src.Address.SingleLine))
                .ForMember(dest => dest.MultiLine, opt => opt.MapFrom(src => src.Address.MultiLine));

            CreateMap<PersonPhoneNumber, PhoneNumberDTO>()
                .ForMember(dest => dest.Number, opt => opt.MapFrom(src => src.PhoneNumber.Number))
                .ForMember(dest => dest.Extension, opt => opt.MapFrom(src => src.PhoneNumber.Extension))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Formatted, opt => opt.MapFrom(src => src.PhoneNumber.Formatted))
                .ForMember(dest => dest.Display, opt => opt.MapFrom(src => src.PhoneNumber.Display));
        }

        public class NextDeadlineDateResolver : IValueResolver<Case, CaseDetailDTO, DateTime?>
        {
            public DateTime? Resolve(Case source, CaseDetailDTO destination, DateTime? destMember, ResolutionContext context)
            {
                if (source.Deadlines == null || !source.Deadlines.Any())
                    return null;

                return source.Deadlines
                    .Where(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                    .OrderBy(d => d.DeadlineDate)
                    .Select(d => d.DeadlineDate)
                    .FirstOrDefault();
            }
        }

        public class NextDeadlineDescriptionResolver : IValueResolver<Case, CaseDetailDTO, string?>
        {
            public string? Resolve(Case source, CaseDetailDTO destination, string? destMember, ResolutionContext context)
            {
                if (source.Deadlines == null || !source.Deadlines.Any())
                    return null;

                return source.Deadlines
                    .Where(d => !d.IsCompleted && d.DeadlineDate >= DateTime.Now)
                    .OrderBy(d => d.DeadlineDate)
                    .Select(d => d.Description)
                    .FirstOrDefault();
            }
        }

        // Helper method for parsing address from string
        private static Address? ParseAddressFromString(string addressString)
        {
            if (string.IsNullOrWhiteSpace(addressString))
                return null;

            var parts = addressString.Split(',').Select(p => p.Trim()).ToArray();
            var address = new Address
            {
                Line1 = parts.Length > 0 ? parts[0] : null,
                City = parts.Length > 1 ? parts[1] : null,
                State = parts.Length > 2 ? parts[2] : null,
                PostalCode = parts.Length > 3 ? parts[3] : null
            };

            address.SetPopulated();
            return address;
        }
    }
}