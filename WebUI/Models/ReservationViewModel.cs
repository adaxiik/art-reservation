using System.ComponentModel.DataAnnotations;
using DataLayer.Models;

namespace WebUI.Models;


public class ReservationViewModel
{
    [Required]
    [Display(Name = "Date from")]
    [DataType(DataType.Date)]
	public DateTime DateFrom { get; set; } = DateTime.Now;
    [Required]
    [Display(Name = "Date to")]
    [DataType(DataType.Date)]
    public DateTime DateTo { get; set; } = DateTime.Now;

    [Required]
    [Display(Name = "Card number")]
    // [CreditCard]
    public string CardNumber { get; set; } = String.Empty;


}
