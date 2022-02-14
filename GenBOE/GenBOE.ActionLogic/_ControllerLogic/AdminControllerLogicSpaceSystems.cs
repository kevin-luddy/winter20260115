// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;
    using ModelView.Admin;

    public class AdminControllerLogicSpaceSystems : AdminControllerLogic
    {
        // Create static Regex object for RateDecimal.
        private static readonly Regex regexRateDecimal = new Regex(ValidationConstants.RATE_DECIMAL);

        /// <summary>
        /// Gets the labor rate example file location for SSC
        /// </summary>
        public override string LABOR_RATES_EXAMPLE_LOCATION
        {
            get { return WebConstants.SSC_LABOR_RATES_EXAMPLE_LOCATION; }
        }

        /// <summary>
        /// Gets the labor rate example file name for SSC
        /// </summary>
        public override string LABOR_RATES_EXAMPLE_NAME
        {
            get { return WebConstants.SSC_LABOR_RATES_EXAMPLE_NAME; }
        }

        #region Protected Properties and Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public AdminControllerLogicSpaceSystems(ICommonDataMapper inCommonDataMapper,
                                                IResourceListDTODataLoader inIResourceListDTODataLoader,
                                                IPermissionsDTODataLoader permissionsDTODataLoader)
            : base(inCommonDataMapper, inIResourceListDTODataLoader, permissionsDTODataLoader)
        {
            // Nothing to do here
        }

        #endregion

        #region Get Actions

        /// <summary>
        /// Gets the training course groups.
        /// </summary>
        /// <returns>Training course groups</returns>
        public override ICollection<TrainingCourseGroupModelView> GetTrainingCourseGroups()
        {
            ICollection<TrainingCourseGroupModelView> groups = base.GetTrainingCourseGroups();
            DateTime twoYearsAgo = DateTime.Now.AddYears(-2).Date;

            // Subcontract Admin
            groups.Add(new TrainingCourseGroupModelView
            {
                CourseGroupName = "Subcontract Administrator Training",
                Courses = new List<TrainingCourseModelView>
                {
                    new TrainingCourseModelView
                    {
                        CourseID = Constants.SUBCONTRACT_TRAINING_COURSEID
                    }
                },
                PermissionRequired = Role.SubcontractAdmin
            });

            // BOE Writing course
            groups.Add(new TrainingCourseGroupModelView
            {
                CourseGroupName = "BOE Writing Course",
                Courses = new List<TrainingCourseModelView>
                {
                    new TrainingCourseModelView
                    {
                        CourseID = Constants.SHARED_BOE_WRITING_COURSE,
                        LastCompletedByDate = twoYearsAgo
                    },
                    new TrainingCourseModelView
                    {
                        CourseID = Constants.SSC_OLD_BOE_WRITING_COURSE2,
                        ValidUntil = new DateTime(2019, 7, 29, 23, 59, 59),
                        LastCompletedByDate = twoYearsAgo
                    },
                    new TrainingCourseModelView
                    {
                        CourseID = Constants.SSC_OLD_BOE_WRITING_COURSE,
                        ValidUntil = new DateTime(2019, 7, 29, 23, 59, 59),
                        LastCompletedByDate = twoYearsAgo
                    }
                }
            });

            return groups;
        }

        #endregion Get Actions

        #region Extra validation

        /// <summary>
        /// Validates the escalation rate.
        /// </summary>
        /// <param name="rate">The escalation rate.</param>
        /// <param name="allRates">All current system rates.</param>
        /// <param name="validationErrors">A list of validation errors</param>
        public override void ValidateEscalationRate(EscalationRateModelView rate, ICollection<EscalationRatesDTO> allRates, ICollection<ValidationMessage> validationErrors)
        {
            if (validationErrors is null)
            {
                throw new ArgumentNullException(nameof(validationErrors));
            }

            if (rate is null)
            {
                throw new ArgumentNullException(nameof(rate));
            }

            base.ValidateEscalationRate(rate, allRates, validationErrors);

            // validate the LMSI Escalation Rate
            if (rate.LMSIEscalation == 0m)
            {
                validationErrors.Add(new ValidationMessage("LMSIEscalation", "LMSI Escalation is required."));
            }

            if (!regexRateDecimal.IsMatch(rate.LMSIEscalation.ToString()))
            {
                validationErrors.Add(new ValidationMessage("LMSIEscalation", "LMSI Escalation must be in the form of nnnn.nnn where n is a numeric number."));
            }

            if (rate.DevEscalation == 0m)
            {
                validationErrors.Add(new ValidationMessage("DevEscalation", "Dev Escalation is required."));
            }

            if (!regexRateDecimal.IsMatch(rate.DevEscalation.ToString()))
            {
                validationErrors.Add(new ValidationMessage("DevEscalation", "Dev Escalation must be in the form of nnnn.nnn where n is a numeric number."));
            }

        }

        #endregion
    }
}
