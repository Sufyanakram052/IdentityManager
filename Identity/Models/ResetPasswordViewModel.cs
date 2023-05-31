using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Identity.Models
{
	public class ResetPasswordViewModel
	{
		[Required]
		[EmailAddress]
		[Display(Name = "Email")]
		public string? Email { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} character long", MinimumLength = 6)]
		[DataType(DataType.Password)]
		public string? Password { get; set; }

		[DataType(DataType.Password)]
		[Compare("Password", ErrorMessage = "The Password and Confirmation Password Don't Match")]
		[Display(Name = "Confirm Password")]
		public string? ConfirmPassword { get; set; }

		public string Code { get; set; }

	}
}
