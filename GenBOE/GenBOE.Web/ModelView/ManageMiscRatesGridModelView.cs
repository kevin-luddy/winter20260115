using System.Collections.ObjectModel;

namespace GenBOE.Web.ModelView
{
    public class ManageMiscRatesGridModelView
    {
        public ManageMiscRatesGridModelView()
        {
            MiscRateModelView = new Collection<MiscRateModelView>();
        }

        public Collection<MiscRateModelView> MiscRateModelView { get; set; }
    }
}