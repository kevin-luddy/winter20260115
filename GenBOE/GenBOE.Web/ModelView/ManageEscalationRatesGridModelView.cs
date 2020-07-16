// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2020 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.Web.ModelView
{
    using System.Collections.ObjectModel;
    using GenBOE.ActionLogic.ModelView.Admin;
    using IES.Common.classes;

    public class ManageEscalationRatesGridModelView
    {
        public ManageEscalationRatesGridModelView()
        {
            EscalationRateModelViews = new Collection<EscalationRateModelView>();
        }

        public Collection<EscalationRateModelView> EscalationRateModelViews { get; set; }

        /// <summary>
        /// The Dev Escalation Title.
        /// </summary>
        public virtual string DevEscalationTitle
        {
            get
            {
                if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
                {
                    return "Airfare Escalation";
                }
                else
                {
                    return "Dev Escalation";
                }
            }
        }

        /// <summary>
        /// The LMSI Escalation Title.
        /// </summary>
        public virtual string PerDiemOrLmsiEscalationTitle
        {
            get
            {
                if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
                {
                    return "Per Diem Escalation";
                }
                else
                {
                    return "LS Escalation";
                }
            }
        }

        /// <summary>
        /// Whether to show the Misc Escalation Rate.
        /// </summary>
        public virtual bool ShowMiscRate
        {
            get
            {
                if (SystemConfiguration.Instance().CompanyMode == IES.Common.CompanyConfiguration.MST)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}