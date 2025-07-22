
using SplitWiseRepository.Models;

namespace SplitWiseRepository.ViewModels;

public class ActivityListVM
{
    public List<Activity> Activities { get; set; } = new List<Activity>();
    public int? GroupId { get; set; }
}
