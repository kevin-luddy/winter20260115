namespace IESPortal.Web
{
    using System;
    using System.Web.Mvc;

    /// <summary>
    /// 
    /// </summary>
    public static class FilterConfig
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Visual Studio required this even though the argument is validated.")]
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            if (filters == null)
            {
                throw new ArgumentNullException(nameof(filters));
            }

            filters.Add(new HandleErrorAttribute());
        }
    }
}
