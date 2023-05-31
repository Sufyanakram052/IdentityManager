using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
	public class ForgetPasswordViewModel
	{
		[Required]
		[EmailAddress]
		public string? Email { get; set; }
	}
}
