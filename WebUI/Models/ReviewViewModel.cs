using System.ComponentModel.DataAnnotations;
using DataLayer.Models;

namespace WebUI.Models;


public class ReviewViewModel
{
    [Required]
    [Display(Name = "Rating")]
    [Range(1, 5)]
    public int? Rating { get; set; } = 1;

    [Display(Name = "Comment")]
    [DataType(DataType.MultilineText)]
    public string? Comment { get; set; } = String.Empty;

    public int? ArtworkId { get; set; }
    public int? ReviewId { get; set; }

}
