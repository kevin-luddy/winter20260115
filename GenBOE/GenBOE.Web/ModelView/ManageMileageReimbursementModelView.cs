using System.ComponentModel.DataAnnotations;
namespace GenBOE.Web.ModelView
{
    public class ManageMileageReimbursementModelView
    {
        public ManageMileageReimbursementModelView()
        {
            MileageReimbursementRate = 0;
        }

        [Required(ErrorMessage = "Reimbursement rate is required.")]
        [RegularExpression("^[0-9]{1,1}(\\.[0-9]{1,5})?$", ErrorMessage = "Mileage Rate must be in the form of n.nnnnn")]
        public decimal MileageReimbursementRate { get; set; }
    }
}