using System.ComponentModel.DataAnnotations;

namespace MyBank.Models
{
    public class TransferViewModel
    {
        [Required]
        [Range(0.0, 1000000.0)]
        [Display(Name = "Betrag")]
        public double Amount { get; set; }

        [Required]
        [Display(Name = "Empfänger")]
        public int To { get; set; }

        [Required]
        [Display(Name = "Absender")]
        public int From { get; set; }
    }
}