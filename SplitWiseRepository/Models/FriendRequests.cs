using System.ComponentModel.DataAnnotations.Schema;
using SplitWiseRepository.Constants;

namespace SplitWiseRepository.Models;

public class FriendRequests : AuditFields
{
    public int Id { get; set; }
    public int RequesterId { get; set; }
    public int? ReceiverId { get; set; }
    public int? ReferralId { get; set; }
    public FeriendRequestStatus Status { get; set; }

    [ForeignKey("RequesterId")]
    public virtual User RequesterUserNavigation { get; set; }

    [ForeignKey("ReceiverId")]
    public virtual User ReceiverUserNavigation { get; set; }
    
    [ForeignKey("ReferralId")]
    public virtual UserReferral UserReferral { get; set; }
}
