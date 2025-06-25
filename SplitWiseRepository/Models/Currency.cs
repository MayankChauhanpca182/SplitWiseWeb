using System.ComponentModel.DataAnnotations;

namespace SplitWiseRepository.Models;

public class Currency
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [MaxLength(5)]
    public string Code { get; set; }

    [MaxLength(10)]
    public string Symbole { get; set; }
}
