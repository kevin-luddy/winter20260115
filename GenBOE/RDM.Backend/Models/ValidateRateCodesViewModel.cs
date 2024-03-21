namespace RDM.Backend.Models
{
	using System.Collections.Generic;

	public class ValidateRateCodesViewModel
	{
		public ICollection<RateCodeValidationViewModel> InsertRateCodes { get; set; }

		public ICollection<RateCodeValidationViewModel> UpdateRateCodes { get; set; }
	}
}
