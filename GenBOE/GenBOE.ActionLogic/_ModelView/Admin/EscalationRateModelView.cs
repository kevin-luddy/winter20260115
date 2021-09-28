// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System;
    using GenBOE.ActionLogic.ModelView;
    using IES.Common.classes;
    using GenBOE.Dtos;

    public class EscalationRateModelView : PersistedDataModelView
    {
        public EscalationRateModelView()
        {
            this.EscalationRateID = -1;
            this.Year = 0;
            this.DevEscalation = 0;
            this.LMSIEscalation = 0;
            this.MiscRate = 0;
            this.Deleted = false;
            this.inUse = false;
        }

        public EscalationRateModelView(EscalationRatesDTO inMiscTravelRateDTO)
        {
            if (inMiscTravelRateDTO == null)
            {
                throw new ArgumentNullException(nameof(inMiscTravelRateDTO));
            }

            this.EscalationRateID = inMiscTravelRateDTO.EscalationRateID;
            this.Year = inMiscTravelRateDTO.Year;
            this.DevEscalation = (inMiscTravelRateDTO.DevEscalation * 100);
            this.LMSIEscalation = (inMiscTravelRateDTO.LMSIEscalation * 100);
            this.MiscRate = (inMiscTravelRateDTO.MiscRate * 100);
            this.UpdateDate = inMiscTravelRateDTO.UpdateDate;

        }

        public int EscalationRateID { get; set; }

        public int Year { get; set; }

        public decimal DevEscalation { get; set; }

        public decimal LMSIEscalation { get; set; }

        public decimal MiscRate { get; set; }

        public bool Deleted { get; set; }

        public bool inUse { get; set; }

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
        /// Whether to show the LMSI Escalation Rate.
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