// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace GenBOE.ActionLogic.ControllerLogic
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Transactions;
    using GenBOE.ActionLogic.Common;
    using GenBOE.ActionLogic.ModelView.Admin;
    using GenBOE.ActionLogic.Validation;
    using GenBOE.DataBridge.Common;
    using GenBOE.DataBridge.DTO;
    using GenBOE.Dtos;
    using IES.Common;
    using IES.Common.Exceptions;

    public class AdminControllerLogicMST : AdminControllerLogic
    {
        private readonly IMSTZoneTravelOriginDTODataLoader mstZoneTravelOriginDTODataLoader;
        private readonly IMSTZoneTravelDestinationDTODataLoader mstZoneTravelDestinationDTODataLoader;
        private readonly IMSTTravelNonzoneFeesAndCostsDTODataLoader mstTravelNonzoneFeesAndCostsDTODataLoader;

        // Create static Regex object for RateDecimal.
        private static readonly Regex regexRateDecimal = new Regex(ValidationConstants.RATE_DECIMAL);

        /// <summary>
        /// Gets the labor rate example file location for MST
        /// </summary>
        public override string LABOR_RATES_EXAMPLE_LOCATION
        {
            get { return WebConstants.MST_LABOR_RATES_EXAMPLE_LOCATION; }
        }

        /// <summary>
        /// Gets the labor rate example file name for MST
        /// </summary>
        public override string LABOR_RATES_EXAMPLE_NAME
        {
            get { return WebConstants.MST_LABOR_RATES_EXAMPLE_NAME; }
        }

        /// <summary>
        /// Gets the grid data for the Manage Zone Travel Origins Page
        /// </summary>
        /// <returns>Grid data</returns>
        public override ManageMSTZoneTravelOriginsGridModelView GetZoneTravelOriginGridData()
        {
            ManageMSTZoneTravelOriginsGridModelView theModelView = new ManageMSTZoneTravelOriginsGridModelView();

            ICollection<MSTZoneTravelOriginDTO> allOriginDTOs = this.mstZoneTravelOriginDTODataLoader.GetAllOrigins();

            foreach (MSTZoneTravelOriginDTO dto in allOriginDTOs)
            {
                theModelView.PagedIndexes.Add(dto.OriginID);
                Collection<MSTZoneTravelResourceDTO> resources = this.mstZoneTravelOriginDTODataLoader.GetOriginResourcesByOriginID(dto.OriginID).ToCollection();
                MSTZoneTravelOriginModelView result = new MSTZoneTravelOriginModelView(dto, resources);
                theModelView.MSTZoneTravelOriginsCollection.Add(result);
            }

            return theModelView;
        }

        /// <summary>
        /// Gets the zone travel origins.
        /// </summary>
        /// <returns>A collection of Zone Travel Origin data.</returns>
        public override ICollection<MSTZoneTravelOriginModelView> GetZoneTravelOrigins()
        {
            return this.GetZoneTravelOriginGridData().MSTZoneTravelOriginsCollection;
        }

        /// <summary>
        /// Gets the grid data for the Manage Zone Travel Destinations Page
        /// </summary>
        /// <returns>grid data</returns>
        public override ManageMSTZoneTravelDestinationsGridModelView GetZoneTravelDestinationsGridData()
        {
            ManageMSTZoneTravelDestinationsGridModelView theModelView = new ManageMSTZoneTravelDestinationsGridModelView();

            ICollection<MSTZoneTravelDestinationDTO> allDestinationDTOs = this.mstZoneTravelDestinationDTODataLoader.GetAllDestinations();

            foreach (MSTZoneTravelDestinationDTO dto in allDestinationDTOs)
            {
                theModelView.PagedIndexes.Add(dto.DestinationID);
                MSTZoneTravelDestinationModelView result = new MSTZoneTravelDestinationModelView(dto.DestinationID, dto.Destination, dto.Abbreviation, dto.Zone);
                theModelView.MSTZoneTravelDestinationsCollection.Add(result);
            }

            return theModelView;
        }

        /// <summary>
        /// Gets the grid data for the Manage Fees and Costs for Nonzone Travel page
        /// </summary>
        /// <returns>grid data</returns>
        public override NonzoneFeesAndCostsGridModelView GetNonzoneFeesAndCostsGridData()
        {
            NonzoneFeesAndCostsGridModelView theModelView = new NonzoneFeesAndCostsGridModelView();

            theModelView.NonzoneFeesAndCostsCollection = this.mstTravelNonzoneFeesAndCostsDTODataLoader.getAllFeesAndCosts().Select(x => new NonzoneFeesAndCostsModelView(x)).ToCollection();
            
            return theModelView;
        }

        /// <summary>
        /// Saves Origin and Resource Data for MST Zone Travel - Upsert or Delete
        /// </summary>
        /// <param name="data">Origin MV with data to be saved</param>
        public override void SaveOriginData(ICollection<MSTZoneTravelOriginModelView> data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }
            if (!data.Any())
            {
                throw new ArgumentException("Nothing to save");
            }

            #region Validation and preprocessing
            //If this is a deletion, this will contain the dto(s) to delete
            ICollection<MSTZoneTravelOriginDTO> originsToDelete = new Collection<MSTZoneTravelOriginDTO>();

            //If this is an insertion, this DTO will need to be processed and saved
            ICollection<MSTZoneTravelOriginDTO> originsToSave = new Collection<MSTZoneTravelOriginDTO>();
            Dictionary<int, ICollection<MSTZoneTravelResourceDTO>> resourceDTOs = new Dictionary<int, ICollection<MSTZoneTravelResourceDTO>>();

            this.ValidateAndPreProcessOriginDataForSaving(data, originsToDelete, originsToSave, resourceDTOs);

            #endregion

            //Save
            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                //Deletes
                foreach(MSTZoneTravelOriginDTO originToDelete in originsToDelete)
                {
                    try
                    {
                        this.mstZoneTravelOriginDTODataLoader.SaveOrigin(originToDelete, null);
                    }
                    catch (Exception ex)
                    {
                        string message = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                        throw new ValidationException(message);
                    }
                }

                //Upserts
                foreach (MSTZoneTravelOriginDTO originToSave in originsToSave)
                {
                    this.mstZoneTravelOriginDTODataLoader.SaveOrigin(originToSave, resourceDTOs[originToSave.OriginID]);
                }

                scope.Complete();
            }
        }

        /// <summary>
        /// Saves Destination Data for MST Zone Travel - updates to assigned zone
        /// </summary>
        /// <param name="data">Destination MV with data to be saved</param>
        public override void SaveDestinationData(MSTZoneTravelDestinationModelView data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.mstZoneTravelDestinationDTODataLoader.SaveDestination(data.GetDestinationDTO());
                scope.Complete();
            }
        }

        /// <summary>
        /// Saves Fees and Costs data for Nonzone Travel
        /// </summary>
        /// <param name="data">NonzoneFeesAndCosts MV with data to be saved</param>
        public override void SaveFeesAndCostsData(NonzoneFeesAndCostsModelView data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, ConfigurationUtilities.GetAppSetting<int>("TransactionTimeout", Constants.DB_COPY_WORKSPACE_TRANSACTION_SCOPE_TIMEOUT_SECONDS_DEFAULT)) }))
            {
                this.mstTravelNonzoneFeesAndCostsDTODataLoader.saveFeesAndCosts(data.GetFeesAndCostsDTO());
                scope.Complete();
            }
        }

		/// <summary>
		/// Get System Settings from the database
		/// </summary>
		/// <returns>System settings</returns>
		public override ICollection<SystemSettingDTO> GetSystemSettings()
		{
			return systemSettingDTODataLoader.GetRmsSystemSettings();
		}

		#region Private Methods

		/// <summary>
		/// Validates and Pre-processes Origin data for saving
		/// </summary>
		/// <param name="inOrigins">Origin being saved</param>
		/// <param name="originsToDelete">DTO for deleted origin</param>
		/// <param name="originsToSave">DTO for upserted origin</param>
		/// <param name="resourceDTOsDictionary">DTOs for upserted origin's resources</param>
		private void ValidateAndPreProcessOriginDataForSaving(ICollection<MSTZoneTravelOriginModelView> inOrigins, ICollection<MSTZoneTravelOriginDTO> originsToDelete, ICollection<MSTZoneTravelOriginDTO> originsToSave, Dictionary<int, ICollection<MSTZoneTravelResourceDTO>> resourceDTOsDictionary)
        {
            foreach (MSTZoneTravelOriginModelView inOrigin in inOrigins)
            {
                if (inOrigin.Deleted)
                {
                    MSTZoneTravelOriginDTO originToDelete = this.mstZoneTravelOriginDTODataLoader.GetOriginByOriginID(inOrigin.OriginID);
                    originToDelete.Updateable = UpdateType.Deleted;
                    originsToDelete.Add(originToDelete);
                }
                else
                {
                    MSTZoneTravelOriginDTO originDTO = inOrigin.GetOriginDTO();
                    ICollection<MSTZoneTravelResourceDTO> resourceDTOs = new Collection<MSTZoneTravelResourceDTO>();
                    if (inOrigin.OriginID < 0)
                    {
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourcePRZ1));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourcePRZ2));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourcePRZ3));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourcePRZ4));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourcePRZ5));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourcePRZ6));

                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceTRZ1));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceTRZ2));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceTRZ3));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceTRZ4));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceTRZ5));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceTRZ6));
                    }
                    else
                    {
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDPRZ1, inOrigin.ResourcePRZ1));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDPRZ2, inOrigin.ResourcePRZ2));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDPRZ3, inOrigin.ResourcePRZ3));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDPRZ4, inOrigin.ResourcePRZ4));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDPRZ5, inOrigin.ResourcePRZ5));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDPRZ6, inOrigin.ResourcePRZ6));

                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDTRZ1, inOrigin.ResourceTRZ1));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDTRZ2, inOrigin.ResourceTRZ2));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDTRZ3, inOrigin.ResourceTRZ3));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDTRZ4, inOrigin.ResourceTRZ4));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDTRZ5, inOrigin.ResourceTRZ5));
                        resourceDTOs.Add(new MSTZoneTravelResourceDTO(inOrigin.ResourceIDTRZ6, inOrigin.ResourceTRZ6));
                    }

                    // Verify Origin is unique
                    if (this.mstZoneTravelOriginDTODataLoader.originExists(originDTO))
                    {
                        throw new GenValidationException("The Origin you have entered matches an existing Origin. Origins must be unique. Please enter a unique Origin.");
                    }

                    // This has to be done because we cannot use objects w/ ref in queries and other places in this method
                    originsToSave.Add(originDTO);
                    resourceDTOsDictionary[originDTO.OriginID] = resourceDTOs;
                }
            }
        }

        #endregion

        #region Protected Properties and Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public AdminControllerLogicMST(ICommonDataMapper inCommonDataMapper,
                                       IResourceListDTODataLoader inIResourceListDTODataLoader,
                                       IMSTZoneTravelOriginDTODataLoader inIMSTZoneTravelOriginDTODataLoader,
                                       IMSTZoneTravelDestinationDTODataLoader inIMSTZoneTravelDestinationDTODataLoader,
                                       IMSTTravelNonzoneFeesAndCostsDTODataLoader inIMSTTravelNonzoneFeesAndCostsDTODataLoader,
                                       IPermissionsDTODataLoader permissionsDTODataLoader,
									   ISystemSettingDTODataLoader systemSettingDTODataLoader)
            : base(inCommonDataMapper, inIResourceListDTODataLoader, permissionsDTODataLoader, systemSettingDTODataLoader)
        {
            this.mstZoneTravelOriginDTODataLoader = inIMSTZoneTravelOriginDTODataLoader;
            this.mstZoneTravelDestinationDTODataLoader = inIMSTZoneTravelDestinationDTODataLoader;
            this.mstTravelNonzoneFeesAndCostsDTODataLoader = inIMSTTravelNonzoneFeesAndCostsDTODataLoader;
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

            if (rate.DevEscalation == 0m)
            {
                validationErrors.Add(new ValidationMessage("DevEscalation", "Airfare Escalation is required."));
            }
            // trim off any trailing zeroes which may cause regex to not match.  These can be caused by converting between DTO and ModelView (multiplying and divinding by 100)
            else if (!regexRateDecimal.IsMatch(rate.DevEscalation.ToString().TrimEnd('0', '.')))
            {
                validationErrors.Add(new ValidationMessage("DevEscalation", "Airfare Escalation must be in the form of nnnn.nnn where n is a numeric number."));
            }

            if (rate.LMSIEscalation == 0m)
            {
                validationErrors.Add(new ValidationMessage("LMSIEscalation", "Per Diem Escalation is required."));
            }
            // trim off any trailing zeroes which may cause regex to not match.  These can be caused by converting between DTO and ModelView (multiplying and divinding by 100)
            else if (!regexRateDecimal.IsMatch(rate.LMSIEscalation.ToString().TrimEnd('0', '.')))
            {
                validationErrors.Add(new ValidationMessage("LMSIEscalation", "Per Diem Escalation must be in the form of nnnn.nnn where n is a numeric number."));
            }

            if (rate.MiscRate == 0m)
            {
                validationErrors.Add(new ValidationMessage("MiscRate", "Misc/Car Escalation is required."));
            }
            // trim off any trailing zeroes which may cause regex to not match.  These can be caused by converting between DTO and ModelView (multiplying and divinding by 100)
            else if (!regexRateDecimal.IsMatch(rate.MiscRate.ToString().TrimEnd('0', '.')))
            {
                validationErrors.Add(new ValidationMessage("MiscRate", "Misc/Car Escalation must be in the form of nnnn.nnn where n is a numeric number."));
            }
        }

        #endregion
    }
}
