using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Litigator.Models.DTOs.ECF
{
    // Base ECF Message
    public abstract class ECFMessage
    {
        public string MessageId { get; set; } = Guid.NewGuid().ToString();
        public DateTime MessageTimestamp { get; set; } = DateTime.UtcNow;
        public required string MessageType { get; set; }
        public required string CourtLocationCode { get; set; }
        public required string CaseTrackingId { get; set; }
    }

    // Core ECF Message Types
    public class RecordDocketingMessage : ECFMessage
    {
        public RecordDocketingMessage()
        {
            MessageType = "RecordDocketing";
            FilingParty = new FilingParty();
            DocumentStamp = new DocumentStamp();
            FilingType = string.Empty;
            PaymentInfo = new PaymentInfo();
            CourtLocationCode = string.Empty;
            CaseTrackingId = string.Empty;
        }

        public FilingParty FilingParty { get; set; }
        public List<DocumentMetadata> Documents { get; set; } = new();
        public DocumentStamp DocumentStamp { get; set; }
        public string FilingType { get; set; }
        public decimal FilingFee { get; set; }
        public PaymentInfo PaymentInfo { get; set; }
    }

    public class ServiceReceiptMessage : ECFMessage
    {
        public ServiceReceiptMessage()
        {
            MessageType = "ServiceReceipt";
            OriginalMessageId = string.Empty;
            ReceiptStatus = string.Empty;
            ConfirmationNumber = string.Empty;
            CourtLocationCode = string.Empty;
            CaseTrackingId = string.Empty;
        }

        public string OriginalMessageId { get; set; }
        public string ReceiptStatus { get; set; } // Accepted, Rejected, Pending
        public List<string> ValidationErrors { get; set; } = new();
        public string ConfirmationNumber { get; set; }
        public DateTime ProcessedDate { get; set; }
    }

    public class ServiceInformationQueryMessage : ECFMessage
    {
        public ServiceInformationQueryMessage()
        {
            MessageType = "ServiceInformationQuery";
            QueryType = string.Empty;
            DocumentId = string.Empty;
            DateRange = new DateRange();
            CourtLocationCode = string.Empty;
            CaseTrackingId = string.Empty;
        }

        public string QueryType { get; set; } // DocumentStatus, ServiceHistory, etc.
        public string DocumentId { get; set; }
        public DateRange DateRange { get; set; }
    }

    public class ServiceInformationResponseMessage : ECFMessage
    {
        public ServiceInformationResponseMessage()
        {
            MessageType = "ServiceInformationResponse";
            QueryMessageId = string.Empty;
            CourtLocationCode = string.Empty;
            CaseTrackingId = string.Empty;
        }

        public string QueryMessageId { get; set; }
        public List<ServiceEvent> ServiceEvents { get; set; } = new();
        public List<DocumentStatus> DocumentStatuses { get; set; } = new();
    }

    public class ReviewFilingCallbackMessage : ECFMessage
    {
        public ReviewFilingCallbackMessage()
        {
            MessageType = "ReviewFilingCallback";
            OriginalFilingId = string.Empty;
            ReviewStatus = string.Empty;
            ReviewerName = string.Empty;
            CourtLocationCode = string.Empty;
            CaseTrackingId = string.Empty;
        }

        public string OriginalFilingId { get; set; }
        public string ReviewStatus { get; set; } // Approved, Rejected, NeedsCorrection
        public string ReviewerName { get; set; }
        public List<string> ReviewComments { get; set; } = new();
        public DateTime ReviewDate { get; set; }
    }

    // Supporting Models
    public class FilingParty
    {
        public FilingParty()
        {
            PartyId = string.Empty;
            PartyType = string.Empty;
            Name = string.Empty;
            AttorneyBarNumber = string.Empty;
            FirmName = string.Empty;
            Address = new Address();
            ContactInfo = new ContactInfo();
        }

        public string PartyId { get; set; }
        public string PartyType { get; set; } // Plaintiff, Defendant, Attorney, etc.
        public string Name { get; set; }
        public string AttorneyBarNumber { get; set; }
        public string FirmName { get; set; }
        public Address Address { get; set; }
        public ContactInfo ContactInfo { get; set; }
    }

    public class DocumentMetadata
    {
        public DocumentMetadata()
        {
            DocumentId = string.Empty;
            DocumentType = string.Empty;
            DocumentTitle = string.Empty;
            FileName = string.Empty;
            MimeType = string.Empty;
            HashValue = string.Empty;
        }

        public string DocumentId { get; set; }
        public string DocumentType { get; set; }
        public string DocumentTitle { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public string MimeType { get; set; }
        public string HashValue { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsConfidential { get; set; }
        public List<string> RedactionFlags { get; set; } = new();
    }

    public class DocumentStamp
    {
        public DocumentStamp()
        {
            StampId = string.Empty;
            StampType = string.Empty;
            ClerkName = string.Empty;
            CourtSeal = string.Empty;
        }

        public string StampId { get; set; }
        public DateTime StampDate { get; set; }
        public string StampType { get; set; }
        public string ClerkName { get; set; }
        public string CourtSeal { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new();
    }

    public class ServiceEvent
    {
        public ServiceEvent()
        {
            EventId = string.Empty;
            ServiceMethod = string.Empty;
            ServiceStatus = string.Empty;
            RecipientName = string.Empty;
            RecipientType = string.Empty;
            ServiceAddress = new Address();
            DeliveryConfirmation = string.Empty;
        }

        public string EventId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceMethod { get; set; } // Electronic, Mail, InPerson, Publication
        public string ServiceStatus { get; set; } // Served, Failed, Pending
        public string RecipientName { get; set; }
        public string RecipientType { get; set; }
        public Address ServiceAddress { get; set; }
        public string DeliveryConfirmation { get; set; }
    }

    public class DocumentStatus
    {
        public DocumentStatus()
        {
            DocumentId = string.Empty;
            Status = string.Empty;
            StatusReason = string.Empty;
        }

        public string DocumentId { get; set; }
        public string Status { get; set; } // Filed, Pending, Rejected, Sealed
        public DateTime StatusDate { get; set; }
        public string StatusReason { get; set; }
        public List<string> ApplicableRules { get; set; } = new();
    }

    public class PaymentInfo
    {
        public PaymentInfo()
        {
            PaymentMethod = string.Empty;
            TransactionId = string.Empty;
            PaymentStatus = string.Empty;
        }

        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentStatus { get; set; }
    }

    public class Address
    {
        public Address()
        {
            Street1 = string.Empty;
            Street2 = string.Empty;
            City = string.Empty;
            State = string.Empty;
            ZipCode = string.Empty;
        }

        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; } = "US";
    }

    public class ContactInfo
    {
        public ContactInfo()
        {
            Email = string.Empty;
            Phone = string.Empty;
            Fax = string.Empty;
            PreferredContactMethod = string.Empty;
        }

        public string Email { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string PreferredContactMethod { get; set; }
    }

    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    // Cancellation Support
    public class CancellationRequest
    {
        public CancellationRequest()
        {
            OriginalMessageId = string.Empty;
            CancellationReason = string.Empty;
            RequestedBy = string.Empty;
        }

        public string OriginalMessageId { get; set; }
        public string CancellationReason { get; set; }
        public DateTime CancellationDate { get; set; }
        public string RequestedBy { get; set; }
        public bool RequiresCourtApproval { get; set; }
    }

    public class HearingSchedule
    {
        public HearingSchedule()
        {
            HearingId = string.Empty;
            HearingType = string.Empty;
            CourtRoom = string.Empty;
            JudgeName = string.Empty;
            HearingStatus = string.Empty;
            NoticeRequirements = string.Empty;
        }

        public string HearingId { get; set; }
        public string HearingType { get; set; }
        public DateTime ScheduledDate { get; set; }
        public string CourtRoom { get; set; }
        public string JudgeName { get; set; }
        public List<string> RequiredParties { get; set; } = new();
        public string HearingStatus { get; set; }
        public string NoticeRequirements { get; set; }
    }

    public class CourtPolicyMetadata
    {
        public CourtPolicyMetadata()
        {
            PolicyId = string.Empty;
            PolicyName = string.Empty;
            PolicyVersion = string.Empty;
        }

        public string PolicyId { get; set; }
        public string PolicyName { get; set; }
        public string PolicyVersion { get; set; }
        public Dictionary<string, object> PolicyRules { get; set; } = new();
        public List<string> ApplicableDocumentTypes { get; set; } = new();
        public DateTime EffectiveDate { get; set; }
    }
}