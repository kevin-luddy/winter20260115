// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2019 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ModelView.Admin
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using IES.Common;
    using GenBOE.Dtos;

    public class NonzoneFeesAndCostsModelView
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NonzoneFeesAndCostsModelView()
        {
            this.Mode = MSTTravelMode.None;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dto">DTO containing data for MV</param>
        public NonzoneFeesAndCostsModelView(MSTTravelNonzoneFeesAndCostsDTO dto)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }

            this.Mode = Enum.IsDefined(typeof(MSTTravelMode), dto.ModeID) ? (MSTTravelMode)dto.ModeID : MSTTravelMode.None;
            this.TravelAgencyFee = dto.TravelAgencyFee;
            this.MiscOther = dto.MiscOther;
        }

        /// <summary>
        /// Gets the Fees And Costs DTO based on the current MV
        /// </summary>
        /// <returns>DTO based on the current MV</returns>
        public MSTTravelNonzoneFeesAndCostsDTO GetFeesAndCostsDTO()
        {
            MSTTravelNonzoneFeesAndCostsDTO toReturn = new MSTTravelNonzoneFeesAndCostsDTO();

            toReturn.ModeID = (int)this.Mode;
            toReturn.TravelAgencyFee = this.TravelAgencyFee;
            toReturn.MiscOther = this.MiscOther;

            return toReturn;
        }
        
        /// <summary>
        /// MST Travel Mode
        /// </summary>
        public MSTTravelMode Mode { get; set; }

        /// <summary>
        /// Gets int value of the MSTTravelMode enum
        /// Needed for use with HiddenFor in ManageNonzoneFeesAndCosts.ascx
        /// </summary>
        public int ModeID
        {
            get
            {
                return (int) this.Mode;
            }
        }
        
        /// <summary>
        /// Travel Agency Fee
        /// </summary>
        [Required, Range(0.0, 200)]
        public decimal TravelAgencyFee { get; set; }

        /// <summary>
        /// Miscellaneous/Other Costs
        /// </summary>
        [Required, Range(0.0, 200)]
        public decimal MiscOther { get; set; }
    }
}
