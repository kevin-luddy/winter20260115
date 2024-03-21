namespace RDM.Backend.Models
{
	using Newtonsoft.Json;
	using System.ComponentModel.DataAnnotations;

	public class RateCodeValidationViewModel
	{
		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		[Required]
		public string Description { get; set; }

		/// <summary>
		/// Gets or sets the rate code.
		/// </summary>
		[Required]
		public string RateCode { get; set; }
	}
}
