using System.ComponentModel.DataAnnotations.Schema;

namespace SplitWiseRepository.Models;

public class Friend : AuditFields
{
    public int Id { get; set; }
    public int Friend1 { get; set; }
    public int Friend2 { get; set; }

    [ForeignKey("Friend1")]
    public virtual User Friend1UserNavigation { get; set; }

     [ForeignKey("Friend2")]
    public virtual User Friend2UserNavigation { get; set; }
}
