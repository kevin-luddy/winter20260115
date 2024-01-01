// -----------------------------------------------------------------------
// <copyright company="Lockheed Martin Corporation">
//     Copyright (c) 2011 - 2021 Lockheed Martin Corporation
// </copyright>
// -----------------------------------------------------------------------

namespace IES.ActionLogic.ControllerLogic
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;
	using System.Transactions;
	using IES.DataBridge.Loaders;
	using IES.DataBridge.ModelViews;
	using IES.Standard;
	using IES.Standard.Exceptions;
	using IES.Standard.PickList;
	using ModelView;
	using Validation;

	/// <summary>
	/// Controller Logic for IES Portal Admin area
	/// </summary>
	public class IESPortalAdminControllerLogic
    {
        /// <summary>
        /// PTM Pick List Mapper
        /// </summary>
        private IPickListMapper ptmPickListMapper;

        /// <summary>
        /// BOE Pick List Mapper
        /// </summary>
        private IPickListMapper boePickListMapper;

        /// <summary>
        /// Offline Application Loader
        /// </summary>
        private IOfflineApplicationLoader offlineApplicationLoader;

        /// <summary>
        /// Initializes a new instance of the <see cref="IESPortalAdminControllerLogic"/> class.
        /// </summary>
        /// <param name="ptmPickListMapper">The PTM pick list mapper.</param>
        /// <param name="boePickListMapper">The BOE pick list mapper.</param>
        /// <param name="offlineApplicationLoader">Offline Application Loader</param>
        public IESPortalAdminControllerLogic(IPickListMapper ptmPickListMapper, IPickListMapper boePickListMapper, IOfflineApplicationLoader offlineApplicationLoader)
        {
            this.boePickListMapper = boePickListMapper;
            this.ptmPickListMapper = ptmPickListMapper;
            this.offlineApplicationLoader = offlineApplicationLoader;
        }

        #region Pick Lists

        /// <summary>
        /// Gets the pick list items for the specified pick list type
        /// </summary>
        /// <param name="pickListType">Which List?</param>
        /// <param name="loadChildren">if set to <c>true</c> [load children].</param>
        /// <returns>All of the items</returns>
        public PickListGridMV GetPickListItems(PickListEnum pickListType, bool loadChildren = false)
        {            
            PickListGridMV ptmData = this.ptmPickListMapper.GetPickListValues(pickListType, loadChildren);
            PickListGridMV boeData = this.boePickListMapper.GetPickListValues(pickListType, loadChildren);

            if (ptmData != null && boeData != null)
            {
                this.ValidatePickLists(ptmData, boeData);
            }

            PickListGridMV result = ptmData ?? boeData;
            this.ConfigurePickListIds(result, ptmData, boeData);
            
            return result;
        }

        /// <summary>
        /// Configures the PTM/BOE ids on the models.
        /// </summary>
        /// <param name="pickListGrid">The pick list grid.</param>
        /// <param name="ptmGrid">The PTM grid.</param>
        /// <param name="boeGrid">The BOE grid.</param>
        private void ConfigurePickListIds(PickListGridMV pickListGrid, PickListGridMV ptmGrid, PickListGridMV boeGrid)
        {
            List<PickListDto> models = new List<PickListDto>();

            foreach (PickListDto dto in pickListGrid.PickLists)
            {
                PickListModelView model = new PickListModelView(dto);
                models.Add(model);

                if (ptmGrid != null)
                {
                    model.PtmId = model.Id;
                }

                if (boeGrid != null)
                {
                    // TODO Needs updates if we are allowing both BOE/PTM to have parents
                    PickListDto boe = boeGrid.PickLists.FirstOrDefault(p => p.Text == model.Text);
                    if (boe != null)
                    {
                        model.BoeId = boe.Id;
                        model.IsReadOnly = boe.IsReadOnly || model.IsReadOnly;
                        model.InUse = boe.InUse || model.InUse;
                    }
                }
            }

            pickListGrid.PickLists = models;
        }

        /// <summary>
        /// Validates the pick lists have the same data.
        /// </summary>
        /// <param name="ptmData">The PTM data.</param>
        /// <param name="boeData">The BOE data.</param>
        public void ValidatePickLists(PickListGridMV ptmData, PickListGridMV boeData)
        {
            if (ptmData == null)
            {
                throw new ArgumentNullException(nameof(ptmData));
            }

            if (boeData == null)
            {
                throw new ArgumentNullException(nameof(boeData));
            }

            List<ValidationMessage> messages = new List<ValidationMessage>();

            if (ptmData.ContainsParent && boeData.ContainsParent)
            {
                // This is a serious error since we currently do not reconfigure parentIds for BOE vs PTM
                throw new GenValidationException(Constants.PickListValidation.CONTAINS_PARENTS);
            }

            // this currently will never hit until we allow both picklists to contain parents
            if (ptmData.ContainsParent && boeData.ContainsParent && ptmData.AllowsMultipleParents != boeData.AllowsMultipleParents)
            {
                // This is a serious error, throw exception
                throw new GenValidationException(Constants.PickListValidation.MULTIPLE_PARENTS);
            }

            if (ptmData.PickListName != boeData.PickListName)
            {
                // This is a serious error, throw exception
                throw new GenValidationException(Constants.PickListValidation.NAME);
            }

            // remove Read-Only values when comparing lists
            ICollection<PickListDto> ptmValues = ptmData.PickLists.Where(p => !p.IsReadOnly).ToList();
            ICollection<PickListDto> boeValues = boeData.PickLists.Where(p => !p.IsReadOnly).ToList();
            if (ptmValues.Count != boeValues.Count)
            {
                messages.Add(new ValidationMessage(Constants.PickListValidation.NUMBER_ITEMS));
            }

            foreach (PickListDto ptm in ptmValues)
            {
                // TODO Needs updates if we are allowing both BOE/PTM to have parents
                PickListDto boe = boeValues.FirstOrDefault(b => b.Text == ptm.Text);
                if (boe != null)
                {
                    if (ptm.IsActive != boe.IsActive)
                    {
                        messages.Add(new ValidationMessage(string.Format(Constants.PickListValidation.ACTIVE, ptm.Text)));
                    }
                }
                else
                {
                    messages.Add(new ValidationMessage(string.Format(Constants.PickListValidation.MISSING, ptm.Text, "BOE")));
                }
            }

            foreach (PickListDto boe in boeValues)
            {
                // TODO Needs updates if we are allowing both BOE/PTM to have parents
                PickListDto ptm = ptmValues.FirstOrDefault(b => b.Text == boe.Text);
                if (ptm == null)
                {
                    messages.Add(new ValidationMessage(string.Format(Constants.PickListValidation.MISSING, boe.Text, "PTM")));
                }
            }

            if (messages.Any())
            {
                ptmData.Messages = messages;
            }
        }

        /// <summary>
        /// Validates the pick list items for the specified pick list type
        /// </summary>
        /// <param name="pickListType">Which List?</param>
        /// <param name="dataToSave">Data to validate</param>
        /// <returns>The list of Validation Messages to throw back to the user.</returns>
        public ICollection<ValidationMessage> ValidatePickListItems(PickListEnum pickListType, ICollection<PickListModelView> dataToSave)
        {
            List<ValidationMessage> validationErrors = new List<ValidationMessage>();

            // Validate updated items have unique Text values.
            List<PickListModelView> updatedItems = dataToSave.Where(x => x.Updateable == UpdateType.Upsert).ToList();
            PickListGridMV gridModelView = this.GetPickListItems(pickListType, true);

            if (updatedItems.Any())
            {
                List<PickListDto> existingItems = gridModelView.PickLists.ToList();
                foreach (PickListDto updatedItem in updatedItems)
                {
                    // Validate Pick List Parents
                    if (gridModelView.ContainsParent)
                    {
                        if (gridModelView.AllowsMultipleParents)
                        {
                            // require at least 1 parent
                            if (updatedItem.ParentIds == null || updatedItem.ParentIds.Count == 0)
                            {
                                validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.PICKLIST_MISSING_PARENT, updatedItem.Text)));
                            }

                            if (updatedItem.ParentIds.Any(i => i <= 0))
                            {
                                validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.PICKLIST_MISSING_PARENT, updatedItem.Text)));
                            }
                        }
                        else
                        {
                            // require exactly 1 parent
                            if (updatedItem.ParentIds == null || updatedItem.ParentIds.Count != 1)
                            {
                                validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.PICKLIST_MISSING_PARENT, updatedItem.Text)));

                                // skip past Text uniqueness check because the Parent may be missing
                                continue;
                            }

                            if (updatedItem.ParentIds.Any(i => i <= 0))
                            {
                                validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.PICKLIST_MISSING_PARENT, updatedItem.Text)));
                            }

                            // only validating Text uniqueness against other Pick Lists in the same Parent
                            existingItems = gridModelView.PickLists.Where(p => p.ParentIds.Contains(updatedItem.ParentIds.First())).ToList();
                        }
                    }

                    if (existingItems.Any(x => x.Id != updatedItem.Id && x.Text.Equals(updatedItem.Text, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.PICKLIST_VALUE_MUST_BE_UNIQUE, updatedItem.Text)));
                    }
                }
            }

            // Don't allow Read-Only items to be deleted or updated.
            if (dataToSave.Any(x => (x.Updateable == UpdateType.Deleted ||
                                     x.Updateable == UpdateType.Upsert) &&
                                     x.IsReadOnly))
            {
                validationErrors.Add(new ValidationMessage(AdminValidationConstants.READ_ONLY_PICKLIST_ITEMS_MAY_NOT_BE_MODIFIED));
            }

            // Validate deleted items don't have children.
            List<PickListModelView> deletedItems = dataToSave.Where(x => x.Updateable == UpdateType.Deleted).ToList();
            if (deletedItems.Any() && gridModelView.ContainsChildren)
            {
                foreach (PickListModelView deletedItem in deletedItems)
                {
                    if (gridModelView.Children.Any(c => c.ParentIds.Contains(deletedItem.Id)))
                    {
                        // found a child for the item, cannot delete it
                        validationErrors.Add(new ValidationMessage(string.Format(AdminValidationConstants.PICKLIST_ITEM_MAY_NOT_BE_DELETED, deletedItem.Text, string.Join(", ", gridModelView.Children.Where(c => c.ParentIds.Contains(deletedItem.Id)).Select(d => d.Text)))));
                    }
                }
            }

            return validationErrors;
        }

        /// <summary>
        /// Fixes the pick list errors.
        /// </summary>
        /// <param name="pickListType">Type of the pick list.</param>
        public void FixPickListErrors(PickListEnum pickListType)
        {
            // Not including Read-Only DTOs
            ICollection<PickListDto> ptmData = this.ptmPickListMapper.GetPickListValues(pickListType).PickLists.Where(p => !p.IsReadOnly).ToList();
            ICollection<PickListDto> boeData = this.boePickListMapper.GetPickListValues(pickListType).PickLists.Where(p => !p.IsReadOnly).ToList();

            ICollection<PickListDto> updatedPTM = new List<PickListDto>();
            ICollection<PickListDto> updatedBOE = new List<PickListDto>();

            foreach (PickListDto ptm in ptmData)
            {
                // TODO Needs updates if we are allowing both BOE/PTM to have parents
                PickListDto boe = boeData.FirstOrDefault(p => p.Text == ptm.Text);
                // Check for missing and for differences
                if (boe == null)
                {
                    PickListModelView boeModel = new PickListModelView(ptm);
                    boeModel.Id = -1;
                    boeModel.Updateable = UpdateType.Upsert;
                    updatedBOE.Add(boeModel);
                }
                else if (ptm.IsActive != boe.IsActive)
                {
                    boe.IsActive = ptm.IsActive;
                    boe.Updateable = UpdateType.Upsert;
                    updatedBOE.Add(boe);
                }
            }

            foreach (PickListDto boe in boeData)
            {
                // TODO Needs updates if we are allowing both BOE/PTM to have parents
                PickListDto ptm = ptmData.FirstOrDefault(p => p.Text == boe.Text);
                
                // Only need to check for missing
                if (ptm == null)
                {
                    PickListModelView ptmModel = new PickListModelView(boe);
                    ptmModel.Id = -1;
                    ptmModel.Updateable = UpdateType.Upsert;
                    updatedPTM.Add(ptmModel);
                }
            }

            if (updatedPTM.Any())
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(ConfigurationUtilities.GetAppSetting("TransactionTimeout"))) }))
                {
                    this.ptmPickListMapper.SavePickList(pickListType, updatedPTM);
                    scope.Complete();
                }
            }

            if (updatedBOE.Any())
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Snapshot, Timeout = new TimeSpan(0, 0, Convert.ToInt32(ConfigurationUtilities.GetAppSetting("TransactionTimeout"))) }))
                {
                    this.boePickListMapper.SavePickList(pickListType, updatedBOE);
                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Saves the pick list items for the specified pick list type
        /// </summary>
        /// <param name="pickListType">Which List?</param>
        /// <param name="dataToSave">Data to save</param>
        public void SavePickListItems(PickListEnum pickListType, ICollection<PickListModelView> dataToSave)
        {
            if (dataToSave == null)
            {
                throw new ArgumentNullException(nameof(dataToSave));
            }
            
            ICollection<PickListDto> ptmDtos = new List<PickListDto>();
            ICollection<PickListDto> boeDtos = new List<PickListDto>();
            // Save with ptm ids
            foreach (PickListModelView model in dataToSave)
            {
                model.Id = model.PtmId;
                ptmDtos.Add(model);
            }

            // Save with boe ids
            foreach (PickListModelView model in dataToSave)
            {
                PickListModelView boeModel = new PickListModelView(model);
                boeModel.Id = boeModel.BoeId;
                boeDtos.Add(boeModel);
            }

            using (TransactionScope scope1 = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable, Timeout = new TimeSpan(0, 0, Convert.ToInt32(ConfigurationUtilities.GetAppSetting("TransactionTimeout"))) }))
            {
                this.ptmPickListMapper.SavePickList(pickListType, ptmDtos);

                // Now save to BOE inside a nested transaction
                using (TransactionScope scope2 = new TransactionScope(TransactionScopeOption.RequiresNew, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable, Timeout = new TimeSpan(0, 0, Convert.ToInt32(ConfigurationUtilities.GetAppSetting("TransactionTimeout"))) }))
                {
                    this.boePickListMapper.SavePickList(pickListType, boeDtos);

                    scope2.Complete();
                }

                // Complete the PTM transaction after the BOE transaction completes
                scope1.Complete();
            }
        }

        #endregion

        /// <summary>
        /// Get the offline application data
        /// </summary>
        /// <returns>offline application data</returns>
        public ICollection<OfflineApplicationModelView> GetOfflineApplicationData()
        {
            return this.offlineApplicationLoader.GetAll();
        }

        /// <summary>
        /// Save Offline Applications data
        /// </summary>
        /// <param name="applications">applications modelviews</param>
        public void SaveOfflineApplicationData(ICollection<OfflineApplicationModelView> applications)
        {
            if(applications == null)
            {
                throw new ArgumentNullException(nameof(applications));
            }

            // determine which applications changed status
            ICollection<OfflineApplicationModelView> originalValues = this.GetOfflineApplicationData();
            ICollection<OfflineApplicationModelView> toUpdate = new Collection<OfflineApplicationModelView>();

            foreach(OfflineApplicationModelView app in applications)
            {
                OfflineApplicationModelView original = originalValues.FirstOrDefault(x => x.ApplicationName == app.ApplicationName);
                if (original != null && app.IsOffline != original.IsOffline)
                {
                    toUpdate.Add(app);
                }
            }

            // Only update those that changed
            foreach(OfflineApplicationModelView dto in toUpdate)
            {
                this.offlineApplicationLoader.Update(dto);
            }
        }
    }
}
