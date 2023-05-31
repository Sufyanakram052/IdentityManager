using System.ComponentModel.DataAnnotations;

namespace Identity.Models
{
	public class ExternalLoginConfirmationViewModel
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
		public string Name { get; set; }
	}
}
