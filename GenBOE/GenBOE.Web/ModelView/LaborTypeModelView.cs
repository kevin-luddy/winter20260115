namespace GenBOE.Web.ModelView
{

    public class LaborTypeModelView
    {
        public LaborTypeModelView()
        {
            ResourceID = 0;
            LaborTypeName = string.Empty;
        }

        public int ResourceID { get; set; }
        public string LaborTypeName { get; set; }
    }
}
