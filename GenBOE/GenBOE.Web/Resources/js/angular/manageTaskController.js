angular.module('genboe').controller('ManageTaskController', ['$scope', '$http', '$timeout', '$window', 'ManageTaskModel', 'utilityService', function ($scope, $http, $timeout, $window, ManageTaskModel, utilityService) {
    $scope.ManageTaskModel = ManageTaskModel;
    $scope.TaskCustomFields = [];
    $scope.LaborCustomFields = [];
    $scope.errors = [];
    $scope.laborTypeErrors = [];
    $scope.laborSpreadErrors = [];
    $scope.laborSpreadPasteErrors = [];
    $scope.modalErrors = [];
    $scope.model = {};
    $scope.tableData = []; // cloned copy of model (data) that is connected to labor table, useful for determining deltas
    $scope.deltaHours = '0';
    $scope.deltaSkillMixHours = '0';
    $scope.totalHours = '0';
    $scope.totalSkillMixHours = '0';
    $scope.totalCost = '0';
    $scope.totalUcotHours = '0';
    $scope.grandTotalHours = '0';
    $scope.totalSpreadCost = '0';
    $scope.totalSpreadHours = '0';
    $scope.totalUcotSpreadHours = '0';
    $scope.isSaving = false;
    $scope.invalidSpreads = false;
    $scope.showDropdowns = false;
    $scope.perfOrgTypeaheadLength = ManageTaskModel.IsSpace ? 0 : 2;
    $scope.offloadOptions = [
        { name: 'True', value: true },
        { name: 'False', value: false }
    ];
    $scope.model.AdjacentItems = {};
    $scope.model.AdjacentItems.PreviousId = undefined;
    $scope.model.AdjacentItems.NextId = undefined;
    $scope.SelectedMoqTypes = [];
    $scope.IsDraftOrDraftLocked = false;
    $scope.IsBRCEnabled = ManageTaskModel.IsBRCEnabled; // For CDSM table only show if this BRC Enabaled = true 
    $scope.IsSkillMixEnabled = ManageTaskModel.IsSkillMixEnabled;
    $scope.skillMixRationale = [];
    $scope.skillMixRationaleLaborTypeSelections = [];
    $scope.IsUsingTMRatesInTask = false;
    $scope.loadedMoqData = false;
    $scope.dataLoaded = false;
    $scope.showUCOT = false;
	$scope.skillMixHelperText = "";

    // Sets the Selected MOQ Types from the Selected MOQ Types from MoqEuationController.js.
    $scope.$on('MOQ_TYPE_SELECTION_CHANGED', function (event, selectedMoqTypes) {
        $scope.SelectedMoqTypes = selectedMoqTypes;
        $scope.loadedMoqData = true;
        $scope.updateShowUcot();

		$scope.refreshSkillMixTables();
		$scope.updateSkillMixHelperText();
    });

    $scope.$on('SAP_HOURS_CHANGED', function (e) {
        $scope.refreshSkillMixTables();
    });

	$scope.updateShowUcot = function () {
		let originalShowUCOT = $scope.showUCOT;

		$scope.showUCOT = $scope.ManageTaskModel.IsUcotEnabled &&
			$scope.IsBRCEnabled &&
			$scope.SelectedMoqTypes &&
			$scope.SelectedMoqTypes.length === 1 &&
			$scope.SelectedMoqTypes.every(x => x.SelectedMOQType == '5001' || x.SelectedMOQType == '5002' || x.SelectedMOQType == '5005');

		if (originalShowUCOT != $scope.showUCOT) {
			recalculateAllSpreadsAndTotals();
		}
    };
        
    $scope.addSkillMixRow = function (currentIndex) {
        var newRow = {
            ResourceOld: $scope.skillMixRationale.data.SkillMixRows[currentIndex].ResourceOld,
            ResourceNew: '',
            HistoricalHours: 0,
            LaborSkillMix: 0,
            Included: false,
            BOESkillMix: 0,
            ProposedHours: 0,
            Rationale: ''
        };
        $scope.skillMixRationale.data.SkillMixRows.splice(currentIndex + 1, 0, newRow);
        $scope.refreshSkillMixTables();
    };

    $scope.deleteSkillMixRow = function (index) {
        $scope.skillMixRationale.data.SkillMixRows.splice(index, 1);
        $scope.refreshSkillMixTables();
    };

    $scope.showSkillMixAddButton = function (resourceID) {
        // Hide add button for Space
        return !$scope.ManageTaskModel.IsSpace && !$scope.checkEmptyString(resourceID);
    }

    $scope.showSkillMixDeleteButton = function (resourceOld) {
        // Hide delete button for Space
        return !$scope.ManageTaskModel.IsSpace && $scope.skillMixRationale.data.SkillMixRows.filter(function (row) {
            return row.ResourceOld === resourceOld;
        }).length > 1;
    };

    $scope.openHelp = function (tableName) {
        var url = "";
        switch (tableName) {
            case "RMSCurrentSkillMixTable":
                url = $scope.ManageTaskModel.SkillMixTableHelpUrls.RMSCurrentSkillMixTableHelpUrl
                break;
            case "RMSLMEnterpriseSkillMixTable":
                url = $scope.ManageTaskModel.SkillMixTableHelpUrls.RMSLMEnterpriseSkillMixTableHelpUrl
                break;
            case "SpaceLegacySkillMixTable":
                url = $scope.ManageTaskModel.SkillMixTableHelpUrls.SpaceLegacySkillMixTableHelpUrl
                break;
            case "SpaceLMEnterpriseSkillMixTable":
                url = $scope.ManageTaskModel.SkillMixTableHelpUrls.SpaceLMEnterpriseSkillMixTableHelpUrl
                break;
        }
        if (url != "") {
            $window.open(url, '_blank');
        }
    };

    $scope.checkEmptyString = function (value) {
        return value === undefined || value === '';
    };

    $scope.filterResourceSelections = function () {
        $scope.skillMixRationaleLaborTypeSelections = [...$scope.model.LaborTypesData];
        
        // Filter out the labor types where their '.RateType' is not equal to 'Hours' (Hours equal to 1 as defined from the Enum.cs RateType)
        $scope.skillMixRationaleLaborTypeSelections = $scope.skillMixRationaleLaborTypeSelections.filter(option => option.RateType === 1);
        
        // Filter out the proper Labor Type selections by Resource Names.
        $scope.skillMixRationaleLaborTypeSelections.forEach(function (option, index) {
            $scope.skillMixRationaleLaborTypeSelections[index] = option.ResourceName || '';
        });

        $scope.skillMixRationaleLaborTypeSelections = $scope.skillMixRationaleLaborTypeSelections.filter((option, index, self) =>
            index === self.findLastIndex((t) => (t === option))
        );
    };

    $scope.refreshSkillMixTables = function (setDirty = true) {
        // Check if we are showing Skill Mix (checks for Skill Mix Enabled and if there are no T&M rates)
        if ($scope.showSkillMix() && $scope.dataLoaded && $scope.loadedMoqData) {
            $(document).trigger("SHOW_LOADING_BOX");

            if (setDirty) {
                $scope.setDirty();
            }

            let laborTypesData = $scope.model.LaborTypesData;
            if (laborTypesData) {
                laborTypesData = laborTypesData.filter((item) => !item.Deleted);
            }

            var data = {
                boeId: ManageTaskModel.boeId,
                selectedMoqTypes: $scope.SelectedMoqTypes,
                laborTypes: laborTypesData,
                currentSkillMixData: $scope.model.SkillMixData,
                currentCommonDisclosureData: $scope.model.CommonDisclosureSkillMixData,
                isManual: $scope.isSkillMixManual()
            };

            return $http({
                method: 'POST',
                data: data,
                url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.RefreshSkillMixTableAction, '')
            }).then(function (response) {
                $scope.skillMixRationale = response.data;
                $scope.model.SkillMixData = $scope.skillMixRationale.data.SkillMixRows;
                $scope.model.CommonDisclosureSkillMixData = $scope.skillMixRationale.data.CommonDisclosureRows;

                // Set ManuallySetIncluded (flag to show dropdown) to true for rows where Included is false and ResourceNew is false.
                let sumHours = 0;
                $scope.skillMixRationale.data.SkillMixRows.forEach(function (row) {
                    if (row.Included === false || row.ResourceNew === '') {
                        row.metadata = {
                            ManuallySetIncluded: true
                        };
                    } else {
                        row.metadata = {
                            ManuallySetIncluded: false
                        };
                        sumHours += row.ProposedHours;
                    }
                });

                $scope.skillMixRationale.data.CommonDisclosureRows.forEach(function (row) {
                    if (row.Included === false || row.BusinessResourceID === '' || row.BusinessResourceID === null) {
                        row.metadata = {
                            ManuallySetIncluded: true
                        };
                    } else {
                        row.metadata = {
                            ManuallySetIncluded: false
                        };
                        sumHours += row.ProposedHours;
                    }
                });

                $scope.totalSkillMixHours = sumHours;
                $scope.deltaSkillMixHours = $scope.getMOQTotal().minus($scope.totalSkillMixHours).toString();
                $scope.updateDropdowns();

                $scope.isLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                if (response.data && response.data.MessageList) {
                    $scope.errors = response.data.MessageList;
                }
                $scope.isLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            });
        } else if (!$scope.showSkillMix()) {
            $scope.skillMixRationale = [];
            $scope.model.SkillMixData = [];
            $scope.model.CommonDisclosureSkillMixData = [];
            $scope.totalSkillMixHours = 0;
            $scope.deltaSkillMixHours = 0;
        }
	}

	$scope.updateSkillMixHelperText = function () {
		$scope.skillMixHelperText = "";

		if (!$scope.ManageTaskModel.IsSkillMixEnabled) {
			$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because Workspace Creation Date is before Skill Mix Go Live Date.";
		} else if (!$scope.ManageTaskModel.UsingTemplateBOE) {
			$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because Workspace is not setup to use MOQ Templates.";
		} else {

			let items = $scope.ManageTaskModel.MOQTypeSelections;

			if ($scope.ManageTaskModel.IsSpace && !$scope.ManageTaskModel.SapConnectionEnabled) {
				$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because SAP connection must be set to Yes in Workspace Identification.";
			} else if ($scope.SelectedMoqTypes.length == 0) {
				$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because there needs to be 1 MOQ Type Selection for the Workspace.";
			} else if ($scope.SelectedMoqTypes.length > 1) {
				$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because there can only be 1 MOQ Type Selection for the Workspace.";
			} else {
				var isSapWebi = false;
				var needsActualsCalculated = false;

				// Doing this IF check here reduce timing
				if ($scope.skillMixHelperText.length == 0) {
					if ($scope.SelectedMoqTypes.some(moqType => moqType.TableData.length == 0)) {
						$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because MOQType needs to have Table Data populated.";
					} else if ($scope.ManageTaskModel.IsSpace && $scope.SelectedMoqTypes.some(moqType => moqType.SelectedMOQType != 5001 && moqType.SelectedMOQType != 5002 && moqType.SelectedMOQType != 5005)) {
						$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because MOQType Selection can only be of Type \"Historical\", \"Comparative\", or \"Analogous\".";
					} else if (!$scope.ManageTaskModel.IsSpace && $scope.SelectedMoqTypes.some(moqType => moqType.SelectedMOQType != 5001 && moqType.SelectedMOQType != 5002)) {
						$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because MOQType Selection can only be of Type \"Historical\" or \"Comparative\".";
					} else {
						$scope.SelectedMoqTypes.forEach(function (moqType) {

							// Doing IF Check here to reduce complexity
							if ($scope.skillMixHelperText.length == 0) {
								moqType.TableData.forEach(function (table) {
									if (table.ResourceHours !== undefined) {
										table.ResourceHours.forEach(function (hours) {
											if (hours.TotalHours.length == 0) {
												needsActualsCalculated = true;
											}
										});
									}

									if (table.RepositoryName == $scope.ManageTaskModel.SapWebiRepository) {
										isSapWebi = true;
									}
								});
							}
						});
					}
				}

				if ($scope.skillMixHelperText.length == 0) {
					if ($scope.ManageTaskModel.IsSpace && !isSapWebi) {
						$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because at least one MOQType Table needs to have SAP/Webi enabled.";
					}else if (needsActualsCalculated) {
						$scope.skillMixHelperText = "Skill Mix Section/Tables not showing because Actuals need to be recalculated for the MOQTypes.";
					}
				} 
			}
		}
	}

    // The Date split is because from the config the OneLMXCutOffDate comes with Timestamp
    // that the JS .toDate() method cannot handle and defaults the date to Dec 31, 1969
    $scope.oneLmxCutOff = ManageTaskModel.OneLMXCutOffDate.split(' ')[0].toDate();

    $scope.isPreviousTaskDisabled = function () {
        return $scope.model.AdjacentItems.PreviousId === undefined || $scope.model.AdjacentItems.PreviousId === null;
    };

    $scope.isNextTaskDisabled = function () {
        return $scope.model.AdjacentItems.NextId === undefined || $scope.model.AdjacentItems.NextId === null;
    };

    $scope.navigateToPrevious = function () {
        if (!$scope.isPreviousTaskDisabled()) {
            // reload the LM Labor grid
            var hash = '#LMLabor/task/' + $scope.model.AdjacentItems.PreviousId.toString();
            if (window.location.hash === hash) {
                hash = hash + "?t=2";
            }

            $scope.navigateToUrl(hash);
        }
    };

    $scope.navigateToNext = function () {
        if (!$scope.isNextTaskDisabled()) {
            // reload the LM Labor grid
            var hash = '#LMLabor/task/' + $scope.model.AdjacentItems.NextId.toString();
            if (window.location.hash === hash) {
                hash = hash + "?t=2";
            }

            $scope.navigateToUrl(hash);
        }
    };

    $scope.navigateToUrl = function (hash) {
        if (TaskElementDetailsWidget.isAnyDirty() || $scope.isDirty()) {
            Session.confirmDialog("Navigate to Next/Previous Task", "You have unsaved work that will not be saved upon navigation.  Would you like to change Tasks anyway?", function () {
                $scope.$apply(function () {
                    window.location.hash = hash;
                });
            }, null);
        } else {
            window.location.hash = hash;
        }
    };

    $scope.newId = -1;

    /** Resources and Performing Orgs **/
    $scope.ResourceModels = BOEDetails.WSResources;
    $scope.TMResourceIds = BOEDetails.WSResourcesTM;
    $scope.BusinessResourceCodeModels = BOEDetails.WSBusinessResourceCodes;
    $scope.PerfOrgModels = angular.copy(BOEDetails.WSPerfOrgs);

    $scope.isExporting = false;
    $scope.isLoading = true;
    $scope.taskElementId = -1; // default is 0 for new task elements (per original page)
    $scope.getTinyMceOptionsDescription = function (name) {
        return utilityService.getTinyMceOptions(name, { maxlen: ManageTaskModel.RteFieldSize, enableCharCounting: true }, TaskElementDetailsWidget);
    };

    $scope.init = function (taskElementId) {
        // this function is a private constructor for ManageTaskController
        $scope.showDropdowns = false;
        $scope.taskElementId = taskElementId;
        $scope.cleanDirty();

        angular.forEach($scope.PerfOrgModels, function (item, key) {
            item.Label = item.PerformingOrgName + '-' + item.PerformingOrgDesc;
        });

        // load the main data
        loadData(function () {
            // this is needed because ui-tinymce does things on its own schedule
            $scope.cleanDirty();
        });
    };

    /*
     * ******************* NOTE ********************
     * Functions below relate to the Read Only Text Logic
     * *********************************************
     */

    $scope.getElementOfCostText = function (elementOfCostId) {
        var text = '';

        if (elementOfCostId) {
            ManageTaskModel.ElementsOfCost.some(function (item) {
                if (item.ElementOfCostId === elementOfCostId) {
                    text = item.ElementOfCostName;
                    return true;
                }
            });
        }

        return text;
    };

    $scope.getWbsText = function (wbsId) {
        var text = '';

        if (wbsId) {
            ManageTaskModel.WBSElements.some(function (item) {
                if (item.Value === wbsId) {
                    text = item.Text;
                    return true;
                }
            });
        }

        return text;
    };

    $scope.getClinText = function (clinId) {
        var text = '';

        if (clinId) {
            ManageTaskModel.CLINElements.some(function (item) {
                if (item.Value === clinId) {
                    text = item.Text;
                    return true;
                }
            });
        }

        return text;
    };

    $scope.getLaborCustomFieldText = function (item, customField) {
        var text = '';
        item.CustomFieldValues.some(function (cfv) {
            if (cfv.CustomFieldID === customField.CustomFieldMetaData.CustomFieldID) {
                if (cfv.isOpenEnded) {
                    text = cfv.OpenEndedValue;
                } else {
                    // need to find the matching value in the options of the customfield
                    customField.CustomFieldOptions.some(function (cfo) {
                        if (cfo.CustomFieldOptionID === cfv.CustomFieldValueID) {
                            text = cfo.ID + "-" + cfo.Description;
                            return true;
                        }
                    });
                }

                return true;
            }
        });

        return text;
    };

    $scope.getSpreadCurveText = function (item) {
        var text = '';

        if (item.SpreadCurveID !== undefined) {
            var spreadCurves = item.RateType === ManageTaskModel.RateTypeCost ? ManageTaskModel.SpreadCurvesCost : ManageTaskModel.SpreadCurvesHours;

            // spread curve values are string
            spreadCurves.some(function (curve) {
                if (curve.Value === item.SpreadCurveID) {
                    text = curve.Text;
                    return true;
                }
            });
        }

        return text;
    };

    $scope.getPercentSpreadText = function (percentSpread) {
        var text = '0';

        if (percentSpread && percentSpread !== 0) {
            text = new BigNumber(percentSpread).toFixed(3);
        }

        return text;
    };

    $scope.getHoursSpreadText = function (hours) {
        var text = '0';

        if (hours && hours !== 0) {
            text = new BigNumber(hours).toString();
        }

        return text;
    };

    $scope.getCostSpreadText = function (cost) {
        var text = '0';

        if (cost && cost !== 0) {
            text = new BigNumber(cost).toString();
        }

        return text;
    };

    $scope.getPrecision = function (item) {
        if (item.RateType === ManageTaskModel.RateTypeCost) {
            return ManageTaskModel.CostDecimalPrecision;
        }

        return ManageTaskModel.DecimalPrecision;
    };

    $scope.getDollarSign = function (item) {
        var text = '';

        if (item.RateType === ManageTaskModel.RateTypeCost) {
            text = '$';
        }

        return text;
    };

    $scope.getUcotSpreadMonthText = function (item, month) {
        var text = '';

        if (item.UcotSpreads) {
            var spread = item.UcotSpreads.find(function (spreadItem) {
                return spreadItem.LaborSpreadDate === month;
            });

            var spreadValue = 0;

            if (spread !== undefined && spread.LaborSpreadValue !== undefined) {
                spreadValue = spread.LaborSpreadValue;
            }

            text = spreadValue.toString();
        }

        return text;
    };

    $scope.getSpreadMonthText = function (item, month) {
        var text = '';

        var spread = item.Spreads.find(function (spreadItem) {
            return spreadItem.LaborSpreadDate === month;
        });

        var spreadValue = 0;

        if (spread !== undefined && spread.LaborSpreadValue !== undefined) {
            spreadValue = spread.LaborSpreadValue;
        }

        text = spreadValue.toString();

        return text;
    };

    /*
     * ******************* NOTE ********************
     * Functions below relate to click events
     * *********************************************
     */
    $scope.Import = function () {
        ImportLaborType.showImportDialog();
    };

    $scope.tryExport = function () {
        if (TaskElementDetailsWidget.isAnyDirty()) {
            Session.confirmDialog("Export Resource Types", "You have unsaved work that will not be included in this export.  Would you like to export anyway?", function () {
                $scope.$apply(function () {
                    $scope.export(false);
                });
            }, null);
        } else {
            $scope.export(false);
        }
    };

    $scope.export = function (isTemplate) {
        if (!$scope.isLoading && !$scope.isExporting) {
            var timeoutTime = 2000;
            $scope.isExporting = true;

            var action = ManageTaskModel.exportAction;

            var urlPart = 'boe/' + ManageTaskModel.boeId + '/taskelement/' + $scope.taskElementId + '?isTemplate=' + isTemplate.toString();

            var exportUrl = CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, action, urlPart);
            GenWidget.prototype.performExport(exportUrl);

            // export is done via attaching an iframe, wait an arbitrary # of seconds (2-3) until showing the export button again to stop double-click
            $timeout(function () {
                $scope.isExporting = false;
            }, timeoutTime);

        }
        // this is needed so IE does not stop animating gif(s) on the page
        return false;
    };

    $scope.exportOffload = function () {
        if (!$scope.isLoading && !$scope.isExporting) {
            var timeoutTime = 2000;
            $scope.isExporting = true;

            var action = ManageTaskModel.exportOffloadAction;

            var urlPart = 'boe/' + ManageTaskModel.boeId;

            var exportUrl = CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, action, urlPart);
            GenWidget.prototype.performExport(exportUrl);

            // export is done via attaching an iframe, wait an arbitrary # of seconds (2-3) until showing the export button again to stop double-click
            $timeout(function () {
                $scope.isExporting = false;
            }, timeoutTime);

        }
        // this is needed so IE does not stop animating gif(s) on the page
        return false;
    };

    $scope.reorder = function () {
        if (!$scope.dirty) {
            TaskElementDetailsWidget.DisplayReOrderLaborTypesDialog();
        }
    };

    $scope.duplicate = function () {
        TaskElementDetailsWidget.DisplayDuplicateLaborTypesDialog();
    };

    $scope.updateDuplicates = function (evt) {
        // only allow digits
        var event = evt || window.event;
        var key = event.keyCode || event.which;
        key = String.fromCharCode(key);
        var regex = /[0-9]|\./;
        if (!regex.test(key)) {
            event.returnValue = false;
        } else {
            TaskElementDetailsWidget.EnableDuplicateSave();
        }
    };

    $scope.saveDuplicateResourceTypes = function () {
        TaskElementDetailsWidget.ShowDuplicateLoader();

        angular.forEach($scope.tableData, function (value) {
            if (value.NumberOfDuplicates > 0) {
                for (var i = 0; i < value.NumberOfDuplicates; i++) {
                    // create duplicate rows 
                    $scope.addDuplicateRow(value);
                }
            }
        });

        // update deltas and totals
        recalculateAllSpreadsAndTotals();
        $scope.refreshSkillMixTables();

        // close dialog
        $scope.clearDuplicates();
        TaskElementDetailsWidget.CancelDuplicateLaborTypes();
    };

    $scope.clearDuplicates = function () {
        angular.forEach($scope.tableData, function (value) {
            value.NumberOfDuplicates = 0;
        });
    };

    $scope.findTaskCustomField = function (customField) {
        var selectedItem = {
            selectedID: '-1',
            selectedOptionID: '-1',
            updateDateLong: '0',
            openEndedId: '-1',
            openEndedValue: '',
            selectedOptionText: ''
        };

        if ($scope.model && $scope.model.TaskElementData && $scope.model.TaskElementData.CustomFieldValues) {
            // Do a search through the saved custom fields to find the first match 
            customField.CustomFieldOptions.some(function (option) {

                var cfv = $scope.model.TaskElementData.CustomFieldValues.find(function (item) {
                    return item.CustomFieldValueID === option.CustomFieldOptionID;
                });
                var found = false;

                if (cfv !== undefined) {

                    $scope.model.TaskElementData.CustomFieldValues.some(function (cfv) {
                        if (cfv.CustomFieldValueID === option.CustomFieldOptionID) {
                            selectedItem.selectedID = cfv.SelectionID;
                            selectedItem.selectedOptionID = option.CustomFieldOptionID;
                            selectedItem.updateDateLong = cfv.UpdateDateLong;
                            selectedItem.openEndedId = cfv.CustomFieldValueID;
                            selectedItem.openEndedValue = cfv.OpenEndedValue;
                            if (customField.CustomFieldMetaData.isOpenEnded) {
                                selectedItem.selectedOptionText = cfv.OpenEndedValue;
                            } else {
                                selectedItem.selectedOptionText = option.ID.toString() + "-" + option.Description;
                            }

                            found = true;
                            return found;
                        }
                    });


                }

                return found;
            });
        }

        return selectedItem;
    };

    $scope.findLaborCustomField = function (customField, item) {
        var selectedItem = {
            selectedID: '-1',
            selectedOptionID: '-1',
            updateDateLong: '0',
            openEndedId: '-1',
            openEndedValue: '',
            selectedOptionText: ''
        };

        if (item && item.CustomFieldValues) {
            // Do a search through the saved custom fields to find the first match 
            customField.CustomFieldOptions.some(function (option) {

                var cfv = item.CustomFieldValues.find(function (item) {
                    return item.CustomFieldValueID === option.CustomFieldOptionID;
                });
                var found = false;

                if (cfv !== undefined) {

                    item.CustomFieldValues.some(function (cfv) {
                        if (cfv.CustomFieldValueID === option.CustomFieldOptionID) {
                            selectedItem.selectedID = cfv.SelectionID;
                            selectedItem.selectedOptionID = option.CustomFieldOptionID;
                            selectedItem.updateDateLong = cfv.UpdateDateLong;
                            selectedItem.openEndedId = cfv.CustomFieldValueID;
                            selectedItem.openEndedValue = cfv.OpenEndedValue;
                            if (customField.CustomFieldMetaData.isOpenEnded) {
                                selectedItem.selectedOptionText = cfv.OpenEndedValue;
                            } else {
                                selectedItem.selectedOptionText = option.ID.toString() + "-" + option.Description;
                            }

                            found = true;
                            return found;
                        }
                    });


                }

                return found;
            });
        }

        return selectedItem;
    };

    $scope.findMoqTableCustomFieldValue = function (customField, item) {
        var selectedItem = {
            selectedID: '-1',
            selectedOptionID: '-1',
            updateDateLong: '0',
            openEndedId: '-1',
            openEndedValue: '',
            selectedOptionText: ''
        };

        if (item && item.CustomFieldValueContainers) {
            customField.CustomFieldOptions.some(function (option) {
                var cfv = item.CustomFieldValueContainers.find(function (item) {
                    return item.CustomFieldValueID == option.CustomFieldOptionID;
                });
                var found = false;

                if (cfv !== undefined) {
                    item.CustomFieldValueContainers.some(function (cfv) {
                        if (cfv.CustomFieldValueID == option.CustomFieldOptionID) {
                            selectedItem.selectedID = cfv.SelectionID;
                            selectedItem.selectedOptionID = option.CustomFieldOptionID;
                            selectedItem.updateDateLong = cfv.UpdateDateLong;
                            selectedItem.openEndedId = cfv.CustomFieldValueID;
                            selectedItem.openEndedValue = cfv.OpenEndedValue;
                            selectedItem.selectedOptionText = cfv.OpenEndedValue;

                            found = true;
                            return found;
                        }
                    });
                }

                return found;
            });
        }

        return selectedItem;
    };

    $scope.recalculateTotals = function () {
        var cost = new BigNumber(0.0);
        var hours = new BigNumber(0.0);
        var ucotHours = new BigNumber(0.0);
        var grandTotalHours = new BigNumber(0.0);

        angular.forEach($scope.tableData, function (item, key) {
            if (!item.Deleted) {
                if (item.RateType === ManageTaskModel.RateTypeCost) {
                    cost = cost.plus(item.CostSpread);
                } else {
                    hours = hours.plus(item.HourSpread);
                    ucotHours = ucotHours.plus(item.UcotHours);
                }
            }
        });

        $scope.totalCost = cost.toString();
        $scope.totalHours = hours.toString();
        $scope.totalUcotHours = ucotHours.toString();
        
        cost = new BigNumber(0.0);
        hours = new BigNumber(0.0);
        ucotHours = new BigNumber(0.0);

        angular.forEach($scope.tableData, function (item, key) {
            if (!item.Deleted) {
                if (item.Spreads) {
                    angular.forEach(item.Spreads, function (spread, spreadKey) {
                        if (item.RateType === ManageTaskModel.RateTypeCost) {
                            cost = cost.plus(spread.LaborSpreadValue);
                        } else {
                            hours = hours.plus(spread.LaborSpreadValue);
                        }
                    });
                }
                if (item.UcotSpreads) {
                    angular.forEach(item.UcotSpreads, function (spread, spreadKey) {
                        if (item.RateType !== ManageTaskModel.RateTypeCost) {
                            ucotHours = ucotHours.plus(spread.LaborSpreadValue);
                        }
                    });
                }
            }
        });

        $scope.totalSpreadCost = cost.toString();
        $scope.totalSpreadHours = hours.toString();
        $scope.totalUcotSpreadHours = ucotHours.toString();
        grandTotalHours = ucotHours.plus(hours);
        $scope.grandTotalHours = grandTotalHours.toString();

        $scope.deltaHours = $scope.getMOQTotal().minus($scope.totalSpreadHours).toString();
        $scope.validateTotals();

        $scope.refreshSkillMixTables();
    };

    $scope.getMOQTotal = function () {
        var moqEquationTotal = $('#equals').text().replace(/[\$,]/gi, '');

        var parsedMoq = new BigNumber(moqEquationTotal);
        parsedMoq = $scope.checkNaN(parsedMoq);

        return parsedMoq;
    };

    $scope.moqUpdated = function (initialLoad) {
        var moqTotal = $scope.getMOQTotal();

        if (!initialLoad) {
            recalculateAllSpreadsAndTotals();
            $scope.setDirty();
        } else {
            $scope.recalculateTotals();
        }
    };

    $scope.checkNaN = function (num) {
        if (num.isNaN()) {
            num = new BigNumber(0);
        }

        return num;
    };

    /* Calculates the percent spread based on hours / MOQ */
    $scope.calculatePercentSpread = function (item, moqTotal) {

        if (item.RateType !== ManageTaskModel.RateTypeCost) {
            // assuming item.HourSpreadLocked or using discrete spread
            var percentSpread = new BigNumber(0);
            if (!moqTotal.isZero()) {
                percentSpread = new BigNumber(item.HourSpread);
                percentSpread = percentSpread.div(moqTotal).times(100);
            }

            percentSpread = $scope.checkNaN(percentSpread);
            // percent spread is always allowed up to 3 fractional digits
            item.PercentSpread = percentSpread.toFixed(3);
        } else {
            // set the percent and hours to 0 since this is cost
            item.PercentSpread = '0';
            item.HourSpread = '0';
        }
    };

    $scope.validateTotals = function () {
        // $scope.totalCost and $scope.totalHours are pulled from the labor types
        // validate that they are equal to the spreads

        $scope.laborTypeErrors = [];

        if ($scope.totalCost !== $scope.totalSpreadCost) {
            var error = "Resource Types cost total does not equal the Resource Spreads cost total.";
            $scope.laborTypeErrors.push({ ValidationIssue: error });
        }

        if ($scope.totalHours !== $scope.totalSpreadHours) {
            var hrError = "Resource Types hours total does not equal the Resource Spreads hours total.";
            $scope.laborTypeErrors.push({ ValidationIssue: hrError });
        }

        if ($scope.showUCOT && $scope.totalUcotHours !== $scope.totalUcotSpreadHours) {
            var hrError = "Resource Types UCOT hours total does not equal the Resource Spreads UCOT hours total.";
            $scope.laborTypeErrors.push({ ValidationIssue: hrError });
        }
    };    

    $scope.getUcotHoursTotals = function (month) {
        var hours = new BigNumber(0.0);

        angular.forEach($scope.tableData, function (item) {
            if (!item.Deleted && item.UcotSpreads) {
                if (item.RateType !== ManageTaskModel.RateTypeCost) {
                    var spread = item.UcotSpreads.find(function (spreadItem) {
                        return spreadItem.LaborSpreadDate === month;
                    });

                    if (spread) {
                        hours = hours.plus(spread.LaborSpreadValue);
                    }
                }
            }
        });

        return hours.toString();
    };

    $scope.getHoursTotals = function (month) {
        var hours = new BigNumber(0.0);

        angular.forEach($scope.tableData, function (item) {
            if (!item.Deleted) {
                if (item.RateType !== ManageTaskModel.RateTypeCost) {
                    var spread = item.Spreads.find(function (spreadItem) {
                        return spreadItem.LaborSpreadDate === month;
                    });

                    if (spread) {
                        hours = hours.plus(spread.LaborSpreadValue);
                    }
                }
            }
        });

        return hours.toString();
    };

    $scope.getCostTotals = function (month) {
        var cost = new BigNumber(0.0);

        angular.forEach($scope.tableData, function (item) {
            if (!item.Deleted) {
                if (item.RateType === ManageTaskModel.RateTypeCost) {
                    var spread = item.Spreads.find(function (spreadItem) {
                        return spreadItem.LaborSpreadDate === month;
                    });

                    if (spread) {
                        cost = cost.plus(spread.LaborSpreadValue);
                    }
                }
            }
        });

        return cost.toString();
    };

    $scope.getSpreadDisabled = function (item) {
        var disabled = false;
        if (item.SpreadCurveID !== ManageTaskModel.SpreadCurvesDiscreteHours && item.SpreadCurveID !== ManageTaskModel.SpreadCurvesDiscreteCost) {
            disabled = true;
        }

        return disabled;
    };

    $scope.spreadInputChanged = function (dt, index, item) {
        $scope.setDirty();
        $scope.laborSpreadErrors = [];
        // update spread value inside $scope.tableData
        var spread = item.Spreads.find(function (spreadItem) {
            return spreadItem.LaborSpreadDate === dt;
        });

        var spreadValueString = item.SpreadData[index];

        var spreadValue = new BigNumber(spreadValueString);

        if (spreadValue.isNaN()) {
            item.SpreadDataInvalid[index] = true;
            $scope.invalidSpreads = true;
            var error = "The value '" + spreadValueString + "' is not valid for Labor Spread Value";
            $scope.laborSpreadErrors.push({ ValidationIssue: error });
        } else {
            var previousInvalid = item.SpreadDataInvalid[index];

            // check precision
            var precision = $scope.getPrecision(item);
            var precisionValueString = spreadValue.toFixed(precision);
            if (!spreadValue.isEqualTo(precisionValueString)) {
                item.SpreadDataInvalid[index] = true;
                $scope.invalidSpreads = true;
                var error = "The value '" + spreadValueString + "' is not the correct Precision for Labor Spread Value";
                $scope.laborSpreadErrors.push({ ValidationIssue: error });
            } else {
                item.SpreadDataInvalid[index] = false;
                // search to see if there are any invalid spreads anymore
                if (previousInvalid) {
                    $scope.invalidSpreads = $scope.tableData.some(function (item) {
                        if (!item.Deleted) {
                            return item.SpreadDataInvalid.some(function (invalidItem) {
                                return invalidItem;
                            });
                        }
                    });
                }
            }

            var delta = new BigNumber(0.0);
            // set the spread value in original array from the copy array
            if (spread !== undefined && spread.LaborSpreadValue !== undefined) {
                delta = spreadValue.minus(spread.LaborSpreadValue);
                spread.LaborSpreadValue = spreadValue;
            } else {
                // this is a new value for the Spreads table
                // Assuming that we do not need these in order
                spread = { LaborSpreadDate: dt, LaborSpreadValue: spreadValue };
                item.Spreads.push(spread);
                delta = spreadValue;
            }

            // add delta to labor type object
            if (item.RateType === ManageTaskModel.RateTypeCost) {
                var costSpread = delta.plus(item.CostSpread);
                item.CostSpread = costSpread.toString();
            } else {
                var hourSpread = delta.plus(item.HourSpread);
                item.HourSpread = hourSpread.toString();
                $scope.calculatePercentSpread(item, $scope.getMOQTotal());
            }

			if ($scope.IsUcot(item) && dt.toDate() >= $scope.oneLmxCutOff) {
                var ucotSpread = item.UcotSpreads.find(function (spreadItem) {
                    return spreadItem.LaborSpreadDate === dt;
                });

				var precision = $scope.getPrecision(item);
				var ucotSpreadValue = spreadValue.multipliedBy($scope.ManageTaskModel.UcotFactor).decimalPlaces(precision);

                // set the Ucot spread value in original array from the copy array
				if (ucotSpread !== undefined && ucotSpread.LaborSpreadValue !== undefined) {
					delta = ucotSpreadValue.minus(ucotSpread.LaborSpreadValue);
                    ucotSpread.LaborSpreadValue = ucotSpreadValue;
                } else {
                    // this is a new value for the Ucot Spreads table
                    // Assuming that we do not need these in order
                    ucotSpread = { LaborSpreadDate: dt, LaborSpreadValue: ucotSpreadValue };
                    item.UcotSpreads.push(ucotSpread);
                    delta = ucotSpreadValue;
                }

                // add delta to labor type object Ucot Hours
				var ucotHourSpread = delta.plus(item.UcotHours);
				item.UcotHours = ucotHourSpread.toString();
            }

            // re-calculate totals
			$scope.recalculateTotals();
        }
    };

    $scope.deleteLaborType = function (item) {
        if (!item.NewLaborType) {
            item.Deleted = true;
            $scope.setDirty();
            $scope.recalculateTotals();
        }
    };

    $scope.deleteAllRecords = function () {
        angular.forEach($scope.tableData, function (item, key) {
            item.Deleted = true;
        });

        angular.forEach($scope.model, function (item, key) {
            if (!item.Deleted) {
                item.Deleted = true;
            }
        });

        $scope.setDirty();
        $scope.recalculateTotals();
    };

    $scope.isMonthInRange = function (dt, start, end) {
        var inRange = false;
        if (start && end && dt >= start && dt <= end) {
            inRange = true;
        }

        return inRange;
    };

    /* Useful for initial data load or when start/end for spreads changes 
       The SpreadSpreadData Table houses the spread values for all months (from spread dates full), not just for the allowed months (month in labor type date range)
    */
    $scope.generateSpreadTable = function () {
        // reset spread errors to false
        $scope.invalidSpreads = false;

        // loop through all of the labor types and reset SpreadData to nothing
        angular.forEach($scope.tableData, function (item, key) {
            if (!item.Deleted) {
                item.SpreadData = [];
                item.SpreadDataInvalid = [];
                item.UcotSpreadData = [];
            }
        });

        if ($scope.model.SpreadDatesFull && $scope.model.SpreadDatesFull.length > 0) {

            // loop through all of the spread dates
            angular.forEach($scope.model.SpreadDatesFull, function (dt) {
                // loop through all of the labor types
                angular.forEach($scope.tableData, function (item) {
                    if (!item.Deleted) {
                        // find the value (if any) for this and push it into the table for the item
                        var spreadValue;
                        var ucotSpreadValue;
                        var spreadText = $scope.getSpreadMonthText(item, dt);
                        var ucotSpreadText = $scope.getUcotSpreadMonthText(item, dt);
                        if (spreadText !== '') {
                            spreadValue = spreadText;
                        }
                        if (ucotSpreadText !== '') {
                            ucotSpreadValue = ucotSpreadText;
                        }

                        item.SpreadData.push(spreadValue);
                        item.UcotSpreadData.push(ucotSpreadValue);

                            

                        var invalid = false;

                        // check precision
                        var precision = $scope.getPrecision(item);
                        var spreadValueNumber = new BigNumber(spreadValue);
                        var precisionValueString = spreadValueNumber.toFixed(precision);
                        if (!spreadValueNumber.isEqualTo(precisionValueString)) {
                            invalid = true;
                            $scope.invalidSpreads = true;
                            var error = "The value '" + spreadValue + "' is not the correct Precision for Labor Spread Value";
                            $scope.laborSpreadErrors.push({ ValidationIssue: error });
                        }

                        item.SpreadDataInvalid.push(invalid);
                    }
                });
            });
        }
    };

    var recalculateAllSpreadsAndTotals = function () {
        var items = [];
        var dataArray = [];

		angular.forEach($scope.tableData, function (item, key) {
            if ($scope.validateDates(item, false) && item.SpreadCurveID !== "-1" && (item.ResourceID !== undefined || item.BusinessResourceCodeID !== undefined) && !item.Deleted) {
                // now check hours or cost depending on resource type
                var spreadValueString;
                if (item.RateType === ManageTaskModel.RateTypeCost) {
                    // cost
                    spreadValueString = item.CostSpread;
                } else {
                    // hours
                    spreadValueString = item.HourSpread;
                }

                var spreadValue = Number(spreadValueString);

                if (!isNaN(spreadValue)) {
                    // everything checks out, now 2 paths for discrete vs non-discrete
                    if ($scope.getSpreadDisabled(item)) {
                        // non-discrete, hours only
                        if (item.RateType === ManageTaskModel.RateTypeHours) {
                            var data = { value: spreadValue, rateType: item.RateType, elementOfCost: item.ElementOfCost, percentSpread: item.PercentSpread, percentLocked: item.PercentSpreadLocked, start: item.StartDate, end: item.EndDate, curve: item.SpreadCurveID };
                            items.push(item);
                            dataArray.push(data);
                        }
                    } else {
                        // discrete
                        $scope.fixDiscreteSpread(item, true);
                    }
                }
            }
		});

        if (items.length === 0) {
            // remake the spread array
            $scope.generateSpreadTable();
            $scope.recalculateTotals();
        } else {

            $(document).trigger("SHOW_LOADING_BOX");

            var postedData = {
                items: dataArray,
                moqTotalHours: $scope.getMOQTotal().toString(),
                calculateUCOT: $scope.showUCOT.toString()
            };

            $http({
                method: 'POST',
                data: postedData,
                url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.calculateSpreadAction, '')
            }).then(function (response) {
                // loop through and put the spreads to the correct item
                angular.forEach(items, function (item, key) {
                    var output = response.data[key];
                    item.Spreads = output.spreads;
                    item.UcotSpreads = output.ucotSpreads;
                    item.UcotHours = output.ucotHours;
                    if (item.RateType !== ManageTaskModel.RateTypeCost) {
                        // reset hours at top level in case delta smoothing occurred
                        item.HourSpread = output.value;
                        if (item.HourSpreadLocked) {
                            // reset the percentage in case it changed
                            item.PercentSpread = output.percentSpread;
                        }
                    }
                });

                // remake the spread array
                $scope.generateSpreadTable();
                $scope.recalculateTotals();

                $(document).trigger("HIDE_LOADING_BOX");
            }, function errorCallback(response) {
                if (response.data && response.data.MessageList) {
                    $scope.errors = response.data.MessageList;
                }

                $(document).trigger("HIDE_LOADING_BOX");
            });
        }
    };

    /* Calculate the spread for one row */
    var calculateSpread = function (item, value) {
        $(document).trigger("SHOW_LOADING_BOX");
        var precision = ManageTaskModel.DecimalPrecision;
        if (item.RateType === ManageTaskModel.RateTypeCost) {
            precision = ManageTaskModel.CostDecimalPrecision;
        }
        var data = { value: value, start: item.StartDate, end: item.EndDate, curve: item.SpreadCurveID, rateType: item.RateType, elementOfCost: item.ElementOfCost, percentLocked: item.PercentSpreadLocked, percentSpread: item.PercentSpread };
        var dataArray = [];
        dataArray.push(data);
        var postedData = {
            items: dataArray,
            moqTotalHours: $scope.getMOQTotal().toString(),
            calculateUCOT: $scope.showUCOT.toString()
        };

        $http({
            method: 'POST',
            data: postedData,
            url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.calculateSpreadAction, '')
        }).then(function (response) {
            var output = response.data[0];
            item.Spreads = output.spreads;
            item.UcotSpreads = output.ucotSpreads;
            item.UcotHours = output.ucotHours;
            if (item.RateType !== ManageTaskModel.RateTypeCost) {
                // reset hours at top level in case delta smoothing occurred
                item.HourSpread = output.value;
            }

            // remake the spread array
            $scope.generateSpreadTable();
            $scope.recalculateTotals();

            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            if (response.data && response.data.MessageList) {
                $scope.errors = response.data.MessageList;
            }

            $(document).trigger("HIDE_LOADING_BOX");
        });
    };
    var loadData = function (callback) {
		$(document).trigger("SHOW_LOADING_BOX");
        $scope.isLoading = true;
        $scope.dataLoaded = false;
        $scope.TaskCustomFields = [];
        $scope.MoqTableCustomFields = [];
        $scope.LaborCustomFields = [];
        $scope.model = {};
        $scope.model.AdjacentItems = {};
        $scope.model.AdjacentItems.PreviousId = undefined;
        $scope.model.AdjacentItems.NextId = undefined;
        $scope.tableData = [];
        $scope.laborTypeErrors = [];
        $scope.laborSpreadErrors = [];
        $scope.laborSpreadPasteErrors = [];
        $scope.errors = [];
        $scope.SelectedMoqTypes = [];
        var data = { boeId: ManageTaskModel.boeId, taskElementId: $scope.taskElementId };

        return $http({
            method: 'POST',
            data: data,
            url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.action, '')
        }).then(function (response) {
            $scope.model = response.data;
            $scope.SelectedMoqTypes = response.data.MOQTypes;
            if (!$scope.model.LaborTypesData) {
                $scope.model.LaborTypesData = [];
            }

            $scope.updateShowUcot();
            $scope.TaskCustomFields = $scope.model.TaskCustomFields;
            $scope.MoqTableCustomFields = $scope.model.MOQTypeTableCustomFields;
            $scope.LaborCustomFields = $scope.model.LaborCustomFields;
            $scope.IsUsingTMRatesInTask = $scope.model.IsUsingTMRatesInTask;

            delete $scope.model.TaskCustomFields;
            delete $scope.model.LaborCustomFields;
            delete $scope.model.MOQTypeTableCustomFields;

            var index = 0;
            if ($scope.model.TaskElementData.DescriptionTemplateAnswers) {
                angular.forEach($scope.model.TaskElementData.DescriptionTemplateAnswers, function (item, key) {
                    delete item.UpdateDate;
                    item.name = 'TaskDescription_' + index++;
                });
            }

            if ($scope.model.TaskElementData.MOQTemplateAnswers) {
                index = 0;
                angular.forEach($scope.model.TaskElementData.MOQTemplateAnswers, function (item, key) {
                    delete item.UpdateDate;
                    item.name = 'MOQDescription_' + index++;
                });
            }

            // generate SpreadDatesFull
            $scope.generateSpreadDatesFull();

            // loop through all of the labor types and convert integers to strings
            angular.forEach($scope.model.LaborTypesData, function (item, key) {
                if (item.WBSID) {
                    item.WBSID = item.WBSID.toString();
                } else {
                    item.WBSID = "-1"; // default
                }

                if (item.CLINID) {
                    item.CLINID = item.CLINID.toString();
                } else {
                    item.CLINID = "-1"; // default
                }

                if (item.SpreadCurveID !== undefined && item.SpreadCurveID !== null) {
                    item.SpreadCurveID = item.SpreadCurveID.toString();
                } else {
                    item.SpreadCurveID = "-1"; // default
                }

                item.HourSpread = item.HourSpread.toString();
                item.CostSpread = item.CostSpread.toString();
                item.PercentSpread = item.PercentSpread.toString();
            });

            $scope.updateDropdowns();

            // Add blank row for new labor type when not in read-only
            if (!TaskElementDetailsWidget.isReadOnly()) {
                $scope.addRow();
            }

            $scope.tableData = $scope.model.LaborTypesData;
            $scope.filterResourceSelections();

            angular.forEach($scope.tableData, function (value) {
                value.NumberOfDuplicates = 0;
            });

            $scope.generateSpreadTable();
            $scope.recalculateTotals();

            if (response.data.ValidationErrors) {
                $scope.errors = response.data.ValidationErrors;
            }

            // display the button only if there are errors, and the page is not in read only mode..
            if ($scope.errors && $scope.errors.length > 0 && !TaskElementDetailsWidget.isReadOnly()) {
                $('#recalculateTaskElementButton').show();
            } else {
                $('#recalculateTaskElementButton').hide();
            }

            $scope.IsDraftOrDraftLocked = TaskElementDetailsWidget.BoeStateDraftOrDraftLocked;

            AfterDomLoadTaskElementDetailsWidget(TaskElementDetailsWidget, $scope.taskElementId);
            if ($scope.taskElementId === "-1") {
                $scope.setDirty();
            }

            $scope.dataLoaded = true;
            // Refreshing the tables to calculate the totals rows for the UI, do not set dirty because there "should" be no changes from rows in DB
            $scope.refreshSkillMixTables(false);

            $scope.isLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");

            if (callback && typeof callback === 'function') {
                callback();
            }
        }, function errorCallback(response) {
            if (response.data && response.data.MessageList) {
                $scope.errors = response.data.MessageList;
            }
            $scope.isLoading = false;
            $(document).trigger("HIDE_LOADING_BOX");
        });
    };

    $scope.encode = function (text, elementId) {
        $('#' + elementId).html(text);
    };

    $scope.$on('$destroy', function () {
        $scope.data = [];
        $scope.tableData = [];
        $scope.spreadTableData = [];
        $scope.errors = [];
        $scope.laborTypeErrors = [];
        $scope.laborSpreadErrors = [];
    });

    /*
     * ******************* NOTE ********************
     * Functions below relate to the Edit Logic
     * *********************************************
     */

    $scope.saveAndContinue = function () {
        save(function () {
            $scope.refresh();
        });
    };

    $scope.refresh = function () {
        var data = {};
        data.taskElementID = $scope.taskElementId;
        $(document).trigger('DISPLAY_TASK_ELEMENT_DETAILS', data);  // this will destroy the angular scope and refresh task composites
    };

    $scope.saveAndClose = function (wsLocked) {
        if (wsLocked === undefined) {
            wsLocked = false;
        }

        save(function () {
            // reload the LM Labor grid
            window.location.hash = 'LMLabor';
            $(document).trigger('BOESUMMARYGRID_RELOAD');
            $scope.cleanDirty();
            // this is needed because ui-tinymce does things on its own schedule
            $timeout(function () {
                $scope.cleanDirty();
            }, 300);
        }, wsLocked);
    };

    $scope.saveAndAddAnother = function () {
        save(function () {
            $scope.init(-1);
            $(document).trigger('DISPLAY_TASK_ELEMENT_DETAILS', { taskElementId: -1 });
            $(document).trigger('BOESUMMARYGRID_RELOAD');
            $scope.setDirty();
        });
    };

    var save = function (callback, wsLocked) {
        if (wsLocked === undefined) {
            wsLocked = false;
        }

        if (TaskElementDetailsWidget.isAnyDirty() && !TaskElementDetailsWidget.waitingBeforeSubmit) {
            var invalidResources = $("#LaborTypesFixed td.resources.inputError").length > 0;
            var invalidBusinessResourceCodes = ManageTaskModel.IsBRCEnabled && $("#LaborTypesFixed td.business-resource-codes.inputError").length > 0;
            var invalidPerfOrgs = $("#LaborTypesFixed td.performing-org.inputError").length > 0;

            if ($scope.invalidSpreads || invalidResources || invalidBusinessResourceCodes || invalidPerfOrgs) {
                var invalidArray = [];
                if (invalidResources) {
                    invalidArray.push("Resources");
                }
                if (invalidBusinessResourceCodes) {
                    invalidArray.push("Business Resource Codes");
                }
                if (invalidPerfOrgs) {
                    invalidArray.push("Performing Orgs");
                }
                if ($scope.invalidSpreads) {
                    invalidArray.push("Discrete Spreads");
                }

                Session.alertDialog("Invalid Spreads", "Please fix all invalid " + invalidArray.join(", ") + " before saving your work.", null, null);
            } else {
                $(document).trigger("SHOW_LOADING_BOX");
                $scope.isSaving = true;

                var postedData = angular.copy($scope.model);
                // Copy MOQ data over
                postedData.TaskElementData.MOQHoursEquation = $('#MOQEquation').val();
                // set MOQ equation to "0" if it is empty
                if (postedData.TaskElementData.MOQHoursEquation === undefined || postedData.TaskElementData.MOQHoursEquation === '') {
                    postedData.TaskElementData.MOQHoursEquation = '0';
                }

                // New MOQ Type Data, MOQEquationFieldModel is declared in MOQEquationField.ascx and then used in the page / AngularJS
                if (MOQEquationFieldModel && MOQEquationFieldModel.SelectedMoqTypes) {

                    // Collect & set RTE Data
                    for (item in MOQEquationFieldModel.SelectedMoqTypes) {
                        var moqType = MOQEquationFieldModel.SelectedMoqTypes[item];
                        var moqTypeId = moqType.SelectedMOQType;

                        moqType.DescriptionHoursRequired = $('textarea[name="DescriptionHoursRequired_' + moqTypeId + '"]').val();
                        moqType.SmeReason = $('textarea[name="SmeReason_' + moqTypeId + '"]').val();
                        moqType.SmeHoursLogic = $('textarea[name="SmeHoursLogic_' + moqTypeId + '"]').val();
                        moqType.SmeDurationLogic = $('textarea[name="SmeDurationLogic_' + moqTypeId + '"]').val();
                        moqType.SmeTaskEstimates = $('textarea[name="SmeTaskEstimates_' + moqTypeId + '"]').val();
                        moqType.Rationale = $('textarea[name="Rationale_' + moqTypeId + '"]').val();
                        moqType.SkillMixRationale = $('textarea[name="SkillMixRationale_' + moqTypeId + '"]').val();
                        moqType.HistoricalReferenceExplanation = $('textarea[name="HistoricalReferenceExplanation_' + moqTypeId + '"]').val();
                    }

                    postedData.MOQTypes = angular.copy(MOQEquationFieldModel.SelectedMoqTypes);

                    // MOQ Table Custom Fields
                    var moqTablecustomFieldDictionary = [];

                    angular.forEach(angular.element("#moqTypes table[pkid]"), function (value) {
                        var currentTable = angular.element(value);
                        var pkid = currentTable.attr('pkid');

                        var currentMoqTableDictionaryEntry = { pkid: pkid };
                        var currentMoqTableItems = [];

                        angular.forEach(angular.element('td.custom-field input', currentTable), function (currInput) {
                            var currentInput = angular.element(currInput);
                            var value = currentInput.val();
                            var selectionid = currentInput.attr("selectionid");
                            var customfieldid = currentInput.attr("customfieldid");
                            var updateDateLong = currentInput.attr("updatedatelong");
                            var customfieldvalueid = currentInput.attr("customfieldvalueid");

                            currentMoqTableItems.push({
                                SelectionID: selectionid,
                                CustomFieldID: customfieldid,
                                CustomFieldValueID: customfieldvalueid,
                                UpdateDateLong: updateDateLong,
                                IsOpenEnded: true,
                                OpenEndedValue: value
                            });
                        });

                        currentMoqTableDictionaryEntry.CustomFieldValues = currentMoqTableItems;
                        moqTablecustomFieldDictionary[pkid] = currentMoqTableDictionaryEntry;
                    });

                    for (var mt in postedData.MOQTypes) {
                        var postedMoqType = postedData.MOQTypes[mt];

                        for (var table in postedMoqType.TableData) {
                            var postedTable = postedMoqType.TableData[table];
                            delete postedTable.UpdateDate;

                            if (Array.isArray(postedTable.AdditionalQueryFilters)) {
                                postedTable.AdditionalQueryFilters = postedTable.AdditionalQueryFilters.join("\n");
                            }

                            // Update table source if SAP Connection is disabled
                            if (!$scope.ManageTaskModel.SapConnectionEnabled) {
                                if ($scope.ManageTaskModel.IsSpace) {
                                    if (postedTable.RepositoryName == $scope.ManageTaskModel.SapWebiRepository) {
                                        postedTable.RepositoryName = $scope.ManageTaskModel.SscSapDisabledSource;
                                    }
                                } else {
                                    postedTable.RepositoryName = $scope.ManageTaskModel.RmsSapDisabledSource;
                                }
                            }

                            if (moqTablecustomFieldDictionary[postedTable.Id] !== undefined) {
                                postedTable.CustomFieldValueContainers = moqTablecustomFieldDictionary[postedTable.Id].CustomFieldValues;
                            } else {
                                postedTable.CustomFieldValueContainers = [];
                            }
                        }
                    }
                }

                postedData.TaskElementData.MOQType = $('#MOQType').val();
                postedData.TaskElementData.TaskOrdinaryVariables = [];
                postedData.TaskElementData.WorkspaceVariableIDs = [];

                for (widgetIndex in TaskElementDetailsWidget.ChildWidgets) {
                    if (TaskElementDetailsWidget.ChildWidgets[widgetIndex].HarvestOrdinaryVariablesForSave && typeof TaskElementDetailsWidget.ChildWidgets[widgetIndex].HarvestOrdinaryVariablesForSave === 'function') {

                        var ordinaryVariables = TaskElementDetailsWidget.ChildWidgets[widgetIndex].HarvestOrdinaryVariablesForSave();
                        postedData.TaskElementData.TaskOrdinaryVariables = postedData.TaskElementData.TaskOrdinaryVariables.concat(ordinaryVariables);

                        var workspaceVariables = TaskElementDetailsWidget.ChildWidgets[widgetIndex].HarvestWorkspaceVariablesForSave();
                        postedData.TaskElementData.WorkspaceVariableIDs = postedData.TaskElementData.WorkspaceVariableIDs.concat(workspaceVariables);
                    }
                }

                postedData.TaskElementData.LaborTypeWarning = $("#LaborTypeWarningFlag").is(":visible");

                // Labor Type Custom Fields
                var customFieldDictionary = [];

                angular.forEach(angular.element("#LaborTypes tr[pkid]"), function (value) {
                    var currentResourceTypeRow = angular.element(value);
                    var pkid = currentResourceTypeRow.attr('pkid');

                    var currentResourceDictionaryEntry = { pkid: pkid };
                    var currentResourceItems = [];

                    // Pull out all the selects
                    angular.forEach(angular.element('td.custom-field select', currentResourceTypeRow), function (currSelect) {
                        var currentSelect = angular.element(currSelect);

                        var value = $scope.convertToString(currentSelect.val());
                        var selectionid = currentSelect.attr("selectionid");
                        var customfieldid = currentSelect.attr("customfieldid");
                        var updateDateLong = currentSelect.attr("updatedatelong");

                        if (value === '') {
                            value = '-1';  // model uses a non-nullable int, so this MUST have a value
                        }

                        currentResourceItems.push({
                            SelectionID: selectionid,
                            CustomFieldID: customfieldid,
                            CustomFieldValueID: value,
                            UpdateDateLong: updateDateLong
                        });
                    });

                    // pull out all the inputs
                    angular.forEach(angular.element('td.custom-field input', currentResourceTypeRow), function (currInput) {
                        var currentInput = angular.element(currInput);

                        var value = currentInput.val();
                        var selectionid = currentInput.attr("selectionid");
                        var customfieldid = currentInput.attr("customfieldid");
                        var updateDateLong = currentInput.attr("updatedatelong");
                        var customfieldvalueid = currentInput.attr("customfieldvalueid");

                        currentResourceItems.push({
                            SelectionID: selectionid,
                            CustomFieldID: customfieldid,
                            CustomFieldValueID: customfieldvalueid,
                            UpdateDateLong: updateDateLong,
                            IsOpenEnded: true,
                            OpenEndedValue: value
                        });
                    });

                    currentResourceDictionaryEntry.CustomFieldValues = currentResourceItems;

                    customFieldDictionary[pkid] = currentResourceDictionaryEntry;
                });

                // Set the custom field values for each labor type
                for (var idx in postedData.LaborTypesData) {
                    var resourceTypeEntry = postedData.LaborTypesData[idx];
                    // remove the UpdateDate 
                    delete resourceTypeEntry.UpdateDate;

                    // remove the UCOT Spreads
                    delete resourceTypeEntry.UcotSpreads;

                    if (customFieldDictionary[resourceTypeEntry.BOELaborTypeID] !== undefined) {
                        resourceTypeEntry.CustomFieldValues = customFieldDictionary[resourceTypeEntry.BOELaborTypeID].CustomFieldValues;
                    } else {
                        // clear out anything already there
                        resourceTypeEntry.CustomFieldValues = [];
                    }

                    angular.forEach(resourceTypeEntry.Spreads, function (spread) {
                        // remove the UpdateDate 
                        delete spread.UpdateDate;
                    });

                    resourceTypeEntry.SelectedMOQType = resourceTypeEntry.SelectedMOQType;
                }

                // remove the UpdateDate 
                delete postedData.TaskElementData.UpdateDate;

                // Task Custom Fields 
                postedData.TaskElementData.CustomFieldValues = [];

                angular.forEach(angular.element("#TaskElementDetailsForm input.customField,#TaskElementDetailsForm select.customField"), function (value, key) {
                    var ele = angular.element(value);

                    var eleValue = $scope.convertToString(ele.val());

                    if (ele.attr("openended") === "true") {
                        postedData.TaskElementData.CustomFieldValues.push({
                            IsOpenEnded: true,
                            SelectionID: ele.attr("selectionid"),
                            CustomFieldValueID: ele.attr("customfieldvalueid"),
                            UpdateDateLong: ele.attr("UpdateDateLong"),
                            OpenEndedValue: eleValue,
                            CustomFieldID: ele.attr("customfieldid")
                        });
                    } else {
                        if (eleValue === "" && ele.attr("selectionid") !== "-1") {
                            //if the field value was deleted
                            postedData.TaskElementData.CustomFieldValues.push({
                                SelectionID: ele.attr("selectionid"),
                                UpdateDateLong: ele.attr("UpdateDateLong")
                            });
                        } else if (eleValue !== "" && ele.attr("selectionid") !== "-1") {
                            //if the field value was updated
                            postedData.TaskElementData.CustomFieldValues.push({
                                SelectionID: ele.attr("selectionid"),
                                CustomFieldValueID: eleValue,
                                UpdateDateLong: ele.attr("UpdateDateLong")
                            });
                        } else if (eleValue !== "" && ele.attr("selectionid") === "-1") {
                            //new field value
                            postedData.TaskElementData.CustomFieldValues.push({ CustomFieldValueID: eleValue });
                        } //else is no change
                    }
                });

                // there seems to be an issue with the jquery serializer when dealing with rich text pasted from excel so we need to get these field contents again
                postedData.TaskElementData.RteTemplateAnswers = [];
                GetRteTemplateJson(TaskElementDetailsWidget.TaskDescription, 'TaskDescription', postedData.TaskElementData);

                GetRteTemplateJson(TaskElementDetailsWidget.MOQText, 'MOQText', postedData.TaskElementData);

                // Filter out the blank row before save
                postedData.LaborTypesData = postedData.LaborTypesData.filter(function (d) { return d.NewLaborType === false });

                var data = { modelView: postedData };

                var saveAction = wsLocked ? "SaveLockedTaskDataModel" : ManageTaskModel.saveAction;

                return $http({
                    method: 'POST',
                    data: data,
                    url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, saveAction, '')
                }).then(function (response) {

                    // reset the task element id in case this was saving a new task element
                    $scope.taskElementId = response.data;

                    $timeout(function () {
                        $scope.cleanDirty();
                    }, 100);
                    $scope.errors = [];
                    $scope.laborTypeErrors = [];
                    $scope.laborSpreadErrors = [];
                    $scope.isSaving = false;
                    $(document).trigger("HIDE_LOADING_BOX");

                    if (callback && typeof callback === 'function') {
                        callback();
                    }

                }, function errorCallback(response) {
                    if (response.data && response.data.MessageList) {
                        // put the errors into the appropriate error boxes
                        var typeErrors = [];
                        var spreadErrors = [];
                        angular.forEach(response.data.MessageList, function (value) {
                            if (value.FormIDToTarget === 'LaborSpreadForm') {
                                spreadErrors.push(value);
                            } else if (value.FormIDToTarget === 'LaborTypesForm') {
                                typeErrors.push(value);
                            }
                        });
                        $scope.errors = response.data.MessageList;
                        $scope.laborTypeErrors = typeErrors;
                        $scope.laborSpreadErrors = spreadErrors;

                    }
                    $scope.isSaving = false;
                    $(document).trigger("HIDE_LOADING_BOX");

                    // scroll to top of the task to display the error
                    // timeout needed for first save attempt to allow mainErrorBox to render
                    $timeout(function () {
                        $([document.documentElement, document.body]).animate({
                            scrollTop: $("#mainErrorBox").offset().top
                        }, 1000);
                    }, 1);
                });
            }
        }
    };

    $scope.isDirty = function () {
        return (TaskElementDetailsWidget && TaskElementDetailsWidget.isDirty())
            || (MOQEquationFieldWidget && MOQEquationFieldWidget.isDirty());
    };

    $scope.cleanDirty = function () {
        TaskElementDetailsWidget.cleanDirty();
        TaskElementDetailsWidget.cleanAllDirty();
    };

    $scope.setDirty = function (formId) {
        TaskElementDetailsWidget.setDirty();
    };

    $scope.generateSpreadDatesFull = function () {
        $scope.model.SpreadDatesFull = [];
        $scope.model.SpreadDatesFullAsDate = [];

        var start = $scope.model.TaskElementData.StartDate.toDate();
        var end = $scope.model.TaskElementData.EndDate.toDate();
        var epoch = new Date(0);

        if (start !== epoch && end !== epoch) {
            if (start <= end) {
                for (var i = start; i <= end; i.addMonths(1)) {
                    $scope.model.SpreadDatesFull.push(i.toFormattedString(true));
                    $scope.model.SpreadDatesFullAsDate.push(new Date(i));
                }
            }
        }
    };

    /*
     * ******************* NOTE ********************
     * Functions below relate to Labor Type row update Logic
     * *********************************************
     */

    $scope.resourceUpdated = function (item) {
        // this takes care of deselections
        if ((item.ResourceInput === undefined || item.ResourceInput === '') && item.ResourceDescription !== undefined) {
            $scope.setDirty();
            item.ResourceDescription = undefined;
            item.ResourceName = undefined;
            item.ResourceType = undefined;
            item.ResourceID = undefined;
            $scope.filterResourceSelections();
            $scope.refreshSkillMixTables();
        }
        $scope.checkIfNewRowNeeded(item);
    };

    $scope.businessResourceCodeUpdated = function (item) {
        // this takes care of deselections
        if ((item.BusinessResourceCodeInput === undefined || item.BusinessResourceCodeInput === '') && item.BusinessResourceCodeDescription !== undefined) {
            $scope.setDirty();
            item.BusinessResourceCodeDescription = undefined;
            item.BusinessResourceCodeName = undefined;
            item.BusinessResourceCodeType = undefined;
            item.BusinessResourceCodeID = undefined;
            $scope.filterResourceSelections();
            $scope.refreshSkillMixTables();
        }
        $scope.checkIfNewRowNeeded(item);
    };

    $scope.getAndSetIsResourceValid = function (item, models, callBusinessResourceCode) {
        item.IsResourceValid = true;

        var startDate = item.StartDate.toDate();
        var endDate = item.EndDate.toDate();
        var input = item.ResourceInput;

        if (!$scope.IsBRCEnabled || $scope.TMResourceIds.includes(item.ResourceID)) {
            if (!item.NewLaborType) {
                if (input === undefined || (typeof input === 'string' && (input.length === 0
                    || models.filter(function (r) { return r.ResourceDesc.toUpperCase() === input.toUpperCase() }).length < 1))) {
                    item.IsResourceValid = false;
                }
            }
        }
        else {
            if (!item.NewLaborType) {
                if (endDate < $scope.oneLmxCutOff) {
                    if (input === undefined || (typeof input === 'string' && (input.length === 0
                        || models.filter(function (r) { return r.ResourceDesc.toUpperCase() === input.toUpperCase() }).length < 1))) {
                        item.IsResourceValid = false;
                    }
                }

                if (startDate < $scope.oneLmxCutOff && endDate >= $scope.oneLmxCutOff) {
                    if (input === undefined || (typeof input === 'string' && (input.length === 0
                        || models.filter(function (r) { return r.ResourceDesc.toUpperCase() === input.toUpperCase() }).length < 1))) {
                        item.IsResourceValid = false;
                    }

                    if (callBusinessResourceCode) {
                        item.IsBusinessResourceCodeValid = $scope.getAndSetIsBusinessResourceCodeValid(item, $scope.BusinessResourceCodeModels, false);
                    }
                }
            }
        }

        return item.IsResourceValid;
    }

    $scope.getAndSetIsBusinessResourceCodeValid = function (item, models, callResource) {
        item.IsBusinessResourceCodeValid = true;

        // The Date split is because from the config the OneLMXCutOffDate comes with Timestamp
        // that the JS .toDate() method cannot handle and defaults the date to Dec 31, 1969
        var startDate = item.StartDate.toDate();
        var endDate = item.EndDate.toDate();
        var input = item.BusinessResourceCodeInput;

        if (!item.NewLaborType) {
            if (!$scope.TMResourceIds.includes(item.ResourceID)) {
                if (startDate >= $scope.oneLmxCutOff) {
                    if (input === undefined || (typeof input === 'string' && (input.length === 0
                        || models.filter(function (r) { return r.ResourceDesc.toUpperCase() === input.toUpperCase() }).length < 1))) {
                        item.IsBusinessResourceCodeValid = false;
                    }
                }

                if (startDate < $scope.oneLmxCutOff && endDate >= $scope.oneLmxCutOff) {
                    if (input === undefined || (typeof input === 'string' && (input.length === 0
                        || models.filter(function (r) { return r.ResourceDesc.toUpperCase() === input.toUpperCase() }).length < 1))) {
                        item.IsBusinessResourceCodeValid = false;
                    }

                    if (callResource) {
                        item.IsResourceValid = $scope.getAndSetIsResourceValid(item, $scope.ResourceModels, false);
                    }
                }
            }
        }

        return item.IsBusinessResourceCodeValid;
    }

    $scope.isPerfOrgValid = function (input, models) {
        result = true;

        if (input === undefined || (typeof input === 'string' && (input.length === 0
            || models.filter(function (r) { return r.PerformingOrgName.toUpperCase() === input.toUpperCase() }).length < 1))) {
            result = false;
        }

        return result;
    }

    $scope.resourceSelected = function (item, model) {
        $scope.setDirty();

        if (item && item.ResourceDesc && item.ResourceDesc !== '') {
            // resource was selected
            model.ResourceDescription = item.ResourceDesc;
            model.ResourceName = item.ResourceName;
            model.ResourceType = item.ResourceTypeCategory;
            // if rate type changes, reset both to zero
            if (model.RateType !== item.RateType) {
                model.HourSpread = '0';
                model.CostSpread = '0';
                model.PercentSpread = '0';
                model.UcotHours = '0';

                // If the spread curve was discrete, the ID needs to be fixed
                if (item.SpreadCurveID === undefined && model.SpreadCurveID < 2) {
                    model.SpreadCurveID = "-1";
                }
            }
            model.RateType = item.RateType;
            model.ResourceID = item.Id;
            $scope.recalculateSpreads(model);
        }

        $scope.checkIfNewRowNeeded(model);
        $scope.filterResourceSelections();

        if (ManageTaskModel.IsSpace) {
            $scope.checkTMRates();
        }
    };

    $scope.businessResourceCodeSelected = function (item, model) {
        $scope.setDirty();

        if (item && item.ResourceDesc && item.ResourceDesc !== '') {
            // Business Resource Code was selected
            model.BusinessResourceCodeDescription = item.ResourceDesc;
            model.BusinessResourceCodeName = item.ResourceName;
            model.BusinessResourceCodeType = item.ResourceTypeCategory;

            // Rate Type change
            if (model.RateType !== item.RateType) {
                model.HourSpread = '0';
                model.CostSpread = '0';
                model.PercentSpread = '0';
                model.UcotHours = '0';

                // If the spread curve was discrete, the ID needs to be fixed
                if (item.SpreadCurveID === undefined && model.SpreadCurveID < 2) {
                    model.SpreadCurveID = "-1";
                }
            }

            model.RateType = item.RateType;
            model.BusinessResourceCodeID = item.Id;
            $scope.recalculateSpreads(model);
        }

        $scope.checkIfNewRowNeeded(model);
        $scope.filterResourceSelections();

        if (ManageTaskModel.IsSpace) {
            $scope.checkTMRates();
        }
    }

    $scope.checkTMRates = function () {
        var data = {
            boeId: ManageTaskModel.boeId,
            laborTypes: $scope.model.LaborTypesData,
        };
        return $http({
            method: 'POST',
            data: data,
            url: CreatePostURL(ManageTaskModel.workspace, ManageTaskModel.controller, ManageTaskModel.CheckTMRatesAction, '')
		}).then(function (response) {
            $scope.IsUsingTMRatesInTask = response.data.data;
        }, function errorCallback(response) {
            if (response.data && response.data.MessageList) {
                $scope.errors = response.data.MessageList;
                $scope.isLoading = false;
                $(document).trigger("HIDE_LOADING_BOX");
            }
        });
    };

    $scope.rationaleUpdated = function () {
        // Rationale updated, so dirty the form
        $scope.setDirty();
    };

    $scope.showSkillMix = function () {
        if ($scope.IsSkillMixEnabled && $scope.IsUsingTMRatesInTask === false) {
            // Check if there is exactly one MOQ Type selected
            if ($scope.SelectedMoqTypes.length === 1) {
                // Space
                if ($scope.ManageTaskModel.IsSpace) {
                    // Check if the single MOQ type is one of the big three (Comparative, Historical, Analagous)
                    let hasBigThreeMoqType = [5001, 5002, 5005, '5001', '5002', '5005'].includes($scope.SelectedMoqTypes[0].SelectedMOQType);

                    // Check if the single MOQ table has a SAP/WEBI repository
                    let hasSapWebiRepository = $scope.SelectedMoqTypes[0].TableData !== undefined && $scope.SelectedMoqTypes[0].TableData.some(function (table) {
                        return table.RepositoryName === $scope.ManageTaskModel.SapWebiRepository;
                    });

                    return hasBigThreeMoqType && hasSapWebiRepository;
                    // RMS
                } else {
                    let hasProperMoqTypes = [5001, 5002, '5001', '5002'].includes($scope.SelectedMoqTypes[0].SelectedMOQType);
                    return $scope.IsSkillMixEnabled && hasProperMoqTypes;
                }
            } else {
                // If there is not exactly one MOQ Type selected, hide Skill Mix
                return false;
            }
        }
    };

    $scope.isSkillMixManual = function () {
        // Set Skill Mix to be manual if SAP Connection is disabled or if there is any MOQ Type that is not Historical (5001) or Comparative (5002) MOQ Type or if there are no selected moqtypes or if using TM Rates in Task
        // Space is never Manual
        return !$scope.ManageTaskModel.IsSpace && $scope.isSkillMixManualPerMOQ();
    };

    $scope.isSkillMixManualPerMOQ = function () {
        let isSkillMixManual = ($scope.SelectedMoqTypes === undefined || $scope.SelectedMoqTypes.length === 0 || !$scope.ManageTaskModel.SapConnectionEnabled || !$scope.SelectedMoqTypes.every(x => x.SelectedMOQType == '5001' || x.SelectedMOQType == '5002'));
        if (!isSkillMixManual && $scope.ManageTaskModel.IsSpace) {
            angular.forEach($scope.SelectedMoqTypes, function (item, key) {
                if (item.TableData === undefined || item.TableData.length === 0 || !item.TableData.every(y => y.RepositoryName == $scope.ManageTaskModel.SapWebiRepository)) {
                    isSkillMixManual = true;
                }
            });
        }
        return isSkillMixManual;
    };

    $scope.isSkillMixDisabled = function () {
        // Check if Skill Mix is Automatic
        let isAutomatic = !$scope.isSkillMixManual();

        // Check if there are no MOQ Tables
        let noMoqTables = $scope.SelectedMoqTypes === undefined || $scope.SelectedMoqTypes.length === 0;

		// Check if any MOQ Table is missing SAP Resource Hours
		let hasMissingSAPResourceHours = !$scope.SelectedMoqTypes.every(x =>
			x.TableData !== undefined &&
			x.TableData.length > 0 &&
			x.TableData.some(y =>
				// Space requires at least 1 table using SAP/WEBI Repository
				y.RepositoryName === $scope.ManageTaskModel.SapWebiRepository || !$scope.ManageTaskModel.IsSpace
			) &&
			x.TableData.every(y =>
				(y.ResourceHours !== undefined && y.ResourceHours.length > 0)
			)
		);

        let disabledSkillMix = isAutomatic && (noMoqTables || hasMissingSAPResourceHours);

        // Return true if Skill Mix is Automatic and either of the conditions occur
        return disabledSkillMix;
    };

    $scope.perfOrgSelected = function (item, model) {
        $scope.setDirty();

        if (item && item.PerformingOrgID && item.PerformingOrgID !== '') {
            // perf org was selected

            model.PerformingOrgID = item.PerformingOrgID;
            model.PerformingOrgName = item.PerformingOrgName;
        }

        $scope.checkIfNewRowNeeded(model);
    };

    $scope.perfOrgUpdated = function (item) {
        // this takes care of deselections
        if ((item.PerfOrgInput === undefined || item.PerfOrgInput === '') && item.PerformingOrgID !== undefined) {
            $scope.setDirty();
            item.PerformingOrgID = undefined;
            item.PerformingOrgName = undefined;
        }
        $scope.checkIfNewRowNeeded(item);
    };

    $scope.runAllTaskDateValidation = function () {
        $scope.laborTypeErrors = [];

        angular.forEach($scope.tableData, function (item, key) {
            $scope.validateDates(item, true);
        });
    };

    $scope.validateDates = function (item, displayErrors) {
        var valid = false;

        if (item.StartDate !== undefined && item.EndDate !== undefined && item.StartDate.isDate() && item.EndDate.isDate()) {
            var start = item.StartDate.toDate();
            var end = item.EndDate.toDate();
            var epoch = new Date(0);
            if (start !== epoch && end !== epoch) {
                if (start > end) {
                    if (displayErrors === true) {
                        $scope.laborTypeErrors.push({ ValidationIssue: "Resource Type Start date must be before End date." });
                    }
                } else {
                    var taskStartDate = $scope.model.TaskElementData.StartDate.toDate();
                    var taskEndDate = $scope.model.TaskElementData.EndDate.toDate();
                    if (start < taskStartDate) {
                        if (displayErrors === true) {
                            $scope.laborTypeErrors.push({ ValidationIssue: "Resource Types Start Date cannot be before Task Element Start Date." });
                        }
                    } else if (end > taskEndDate) {
                        if (displayErrors === true) {
                            $scope.laborTypeErrors.push({ ValidationIssue: "Resource Types End Date cannot be after the Task Element End Date." });
                        }
                    } else {
                        valid = true;
                    }
                }
            }
        }

        return valid;
    };

    $scope.validateStartDate = function (item, taskStart) {
        var valid = true;

        if (item.StartDate !== undefined && item.StartDate.isDate()) {
            var epoch = new Date(0);
            var start = item.StartDate.toDate();

            // verify start date against task
            var taskStartDate = taskStart.toDate();
            if (start < taskStartDate) {
                valid = false;
            }

            // verify start date against end date
            if (item.EndDate !== undefined && item.EndDate.isDate()) {
                var end = item.EndDate.toDate();

                if (start !== epoch && end !== epoch && start > end) {
                    valid = false;
                }
            }
        } else { valid = false; }

        return valid;
    };

    $scope.validateEndDate = function (item, taskEnd) {
        var valid = true;

        if (item.EndDate !== undefined && item.EndDate.isDate()) {
            var epoch = new Date(0);
            var end = item.EndDate.toDate();

            // verify end date against task
            var taskEndDate = taskEnd.toDate();
            if (end > taskEndDate) {
                valid = false;
            }

            // verify start date against end date
            if (item.StartDate !== undefined && item.StartDate.isDate()) {
                var start = item.StartDate.toDate();

                if (start !== epoch && end !== epoch && start > end) {
                    valid = false;
                }
            }
        } else { valid = false; }

        return valid;
    };

    $scope.recalculateSpreads = function (item) {
        // validate everything is kosher for the row
        if ($scope.validateDates(item, false) && item.SpreadCurveID !== "-1" && (item.ResourceID !== undefined || item.BusinessResourceCodeID !== undefined)) {
            // now check hours or cost depending on resource type
            var spreadValueString;
            if (item.RateType === ManageTaskModel.RateTypeCost) {
                // cost
                spreadValueString = item.CostSpread;
            } else {
                // hours
                spreadValueString = item.HourSpread;
            }

            var spreadValue = Number(spreadValueString);

            if (!isNaN(spreadValue)) {
                // everything checks out, now 2 paths for discrete vs non-discrete
                if ($scope.getSpreadDisabled(item)) {
                    // non-discrete
                    calculateSpread(item, spreadValue);
                } else {
                    // discrete
                    $scope.fixDiscreteSpread(item, false);
                }
            }
        } else if (item.SpreadCurveID === "-1") {
            // clear spreads
            item.Spreads = [];
            item.UcotSpreads = [];
            item.SpreadData = [];
            item.UcotSpreadData = [];
            item.SpreadDataInvalid = [];

            // loop through all of the spread dates and set spreads to 0
            angular.forEach($scope.model.SpreadDatesFull, function (dt) {
                item.Spreads.push({ LaborSpreadDate: dt, LaborSpreadValue: 0 });
                item.SpreadData.push('0');
                item.SpreadDataInvalid.push(false);
            });
        }

        // Finally, recalculate Skill Mix tables
        $scope.refreshSkillMixTables();
    };

    $scope.fixDiscreteSpread = function (item, skipRecalc) {
        // Need to fix the discrete spread by either lopping off old months or adding on new months
        var newSpreads = [];
        var start = item.StartDate.toDate();
        var end = item.EndDate.toDate();
        var total = new BigNumber(0);
        angular.forEach(item.Spreads, function (spread) {
            var month = spread.LaborSpreadDate.toDate();
            if (month >= start && month <= end) {
                newSpreads.push(spread);
                total = total.plus(spread.LaborSpreadValue);
            }
        });

        if (item.RateType === ManageTaskModel.RateTypeCost) {
            item.CostSpread = total.toString();
        } else {
            item.HourSpread = total.toString();
            $scope.calculatePercentSpread(item, $scope.getMOQTotal());
        }

        item.Spreads = newSpreads;

        if ($scope.IsUcot(item)) {
            var newUcotSpreads = [];
            var ucotTotal = new BigNumber(0);
            angular.forEach(item.UcotSpreads, function (spread) {
                var month = spread.LaborSpreadDate.toDate();
                if (month >= start && month <= end) {
                    newUcotSpreads.push(spread);
                    ucotTotal = ucotTotal.plus(spread.LaborSpreadValue);
                }
			});

            item.UcotHours = ucotTotal;
            item.UcotSpreads = newUcotSpreads;
        }

        if (!skipRecalc) {
            $scope.generateSpreadTable();
            $scope.recalculateTotals();
        }
    };

    $scope.IsUcot = function (item) {
		// ElementOfCost Enum value 1 is LM Labor
        return $scope.showUCOT && item.ElementOfCost === 1 && item.BusinessResourceCodeName && item.RateType === ManageTaskModel.RateTypeHours;
    };

    $scope.startDateUpdated = function (item) {
        $scope.setDirty();
        //$scope.setColumnDisabled(item); Can be set in future

        // update spreads (this validates the row)
        $scope.recalculateSpreads(item);
        $scope.runAllTaskDateValidation();
    };

    $scope.endDateUpdated = function (item) {
        $scope.setDirty();
        //$scope.setColumnDisabled(item); can be set in future

        // update spreads (this validates the row)
        $scope.recalculateSpreads(item);
        $scope.runAllTaskDateValidation();
    };

    $scope.setColumnDisabled = function (item) {
        var itemStartDate = item.StartDate.toDate();
        var itemEndDate = item.EndDate.toDate();

        if ($scope.TMResourceIds.includes(item.ResourceID)) {
            item.disableResource = false;
            item.disableBRC = true;
        } else {

            if (itemEndDate < $scope.oneLmxCutOff) {
                item.disableResource = false;
                item.disableBRC = true;
            }

            if (itemStartDate < $scope.oneLmxCutOff && itemEndDate >= $scope.oneLmxCutOff) {
                item.disableBRC = false;
                item.disableResource = false;
            }

            if (itemStartDate >= $scope.oneLmxCutOff) {
                item.disableResource = true;
                item.disableBRC = false;
            }
        }
    }

    $scope.percentSpreadUpdated = function (item) {
        $scope.setDirty();

        // recalculate all since we may need to smoothe deltas across labor types
        recalculateAllSpreadsAndTotals();
    };

    $scope.hourSpreadUpdated = function (item) {
        $scope.setDirty();
        $scope.calculatePercentSpread(item, $scope.getMOQTotal());
        $scope.recalculateSpreads(item);
    };

    $scope.costSpreadUpdated = function (item) {
        $scope.setDirty();
        $scope.recalculateSpreads(item);
    };

    $scope.spreadCurveUpdated = function (item) {
        $scope.setDirty();
        $scope.recalculateSpreads(item);

        if (item.SpreadCurveID === ManageTaskModel.SpreadCurvesDiscreteCost || item.SpreadCurveID === ManageTaskModel.SpreadCurvesDiscreteHours) {
            $scope.expandSpreads();
        }

        $scope.checkIfNewRowNeeded(item);
    };

    $scope.elementOfCostUpdated = function (item) {
        // reset selected Resource to undefined
        item.ResourceInput = '';
        // update Resource which sets Dirty
        $scope.resourceUpdated(item);

        if ($scope.IsBRCEnabled) {
            item.BusinessResourceCodeInput = '';
            $scope.businessResourceCodeUpdated(item);
        }

        $scope.checkIfNewRowNeeded(item);
    };

    $scope.addRow = function () {
        $scope.setDirty();
        var row = {
            BOELaborTypeID: $scope.newId--,
            BOETaskElementID: $scope.taskElementId,
            CLINID: '-1',
            CanOffload: false,
            CostSpread: '0',
            CustomFieldValues: [],
            Deleted: false,
            ElementOfCost: 1, // default is LM Labor
            EndDate: $scope.model.TaskElementData.EndDate,
            HourSpread: '0',
            UcotHours: '0',
            HourSpreadLocked: false,
            IsCostDisabled: true,
            IsHoursDisabled: false,
            PercentSpread: '0',
            PercentSpreadLocked: false,
            PerfOrgInput: undefined,
            PerformingOrgID: undefined,
            PerformingOrgName: undefined,
            RateType: ManageTaskModel.RateTypeHours,
            ResourceDescription: undefined,
            ResourceID: undefined,
            ResourceInput: undefined,
            ResourceName: undefined,
            ResourceType: undefined,
            BusinessResourceCodeID: undefined,
            BusinessResourceCodeInput: undefined,
            BusinessResourceCodeName: undefined,
            BusinessResourceCodeType: undefined,
            BusinessResourceCodeDescription: undefined,
            SelectedMOQType: undefined,
            SpreadCurveID: '-1',
            SpreadData: [],
            UcotSpreadData: [],
            SpreadDataInvalid: [],
            Spreads: [],
            UcotSpreads: [],
            StartDate: $scope.model.TaskElementData.StartDate,
            TieredPercentage: undefined,
            UpdateDateLong: '0',
            WBSID: '-1',
            NewLaborType: true,
            LaborTypeOrder: 2000 // New resource types should be put at bottom of order
        };

        if ($scope.model.SpreadDatesFull && $scope.model.SpreadDatesFull.length > 0) {
            // loop through all of the spread dates
            angular.forEach($scope.model.SpreadDatesFull, function (dt) {
                row.Spreads.push({ LaborSpreadDate: dt, LaborSpreadValue: 0 });
                row.SpreadData.push('');
                row.UcotSpreads.push({ LaborSpreadDate: dt, LaborSpreadValue: 0 });
                row.UcotSpreadData.push('');
                row.SpreadDataInvalid.push(false);
            });
        }

        $scope.model.LaborTypesData.push(row);

        setTimeout(function () { validateCustomFields(); }, 1);
    };

    $scope.addDuplicateRow = function (laborType) {
        $scope.setDirty();

        var row = {
            BOELaborTypeID: $scope.newId--,
            BOETaskElementID: $scope.taskElementId,
            CLINID: laborType.CLINID,
            CanOffload: laborType.CanOffload,
            CostSpread: laborType.CostSpread,
            CustomFieldValues: [],
            Deleted: false,
            ElementOfCost: laborType.ElementOfCost,
            EndDate: laborType.EndDate,
            HourSpread: laborType.HourSpread,
            UcotHours: laborType.UcotHours,
            HourSpreadLocked: laborType.HoursSpreadLocked,
            IsCostDisabled: laborType.IsCostDisabled,
            IsHoursDisabled: laborType.IsHoursDisabled,
            PercentSpread: laborType.PercentSpread,
            PercentSpreadLocked: laborType.PercentSpreadLocked,
            PerfOrgInput: laborType.PerfOrgInput,
            PerformingOrgID: laborType.PerformingOrgID,
            PerformingOrgName: laborType.PerformingOrgName,
            RateType: laborType.RateType,
            ResourceDescription: laborType.ResourceDescription,
            ResourceID: laborType.ResourceID,
            ResourceInput: laborType.ResourceInput,
            ResourceName: laborType.ResourceName,
            ResourceType: laborType.ResourceType,
            BusinessResourceCodeDescription: laborType.BusinessResourceCodeDescription,
            BusinessResourceCodeID: laborType.BusinessResourceCodeID,
            BusinessResourceCodeInput: laborType.BusinessResourceCodeInput,
            BusinessResourceCodeName: laborType.BusinessResourceCodeName,
            BusinessResourceCodeType: laborType.BusinessResourceCodeType,
            SpreadCurveID: laborType.SpreadCurveID,
            SpreadData: angular.copy(laborType.SpreadData),
            UcotSpreadData: angular.copy(laborType.UcotSpreadData),
            SpreadDataInvalid: angular.copy(laborType.SpreadDataInvalid),
            Spreads: angular.copy(laborType.Spreads),
            UcotSpreads: angular.copy(laborType.UcotSpreads),
            StartDate: laborType.StartDate,
            TieredPercentage: laborType.TieredPercentage,
            UpdateDateLong: '0',
            WBSID: laborType.WBSID,
            NewLaborType: false,
            NumberOfDuplicates: 0,
            SelectedMOQType: laborType.SelectedMOQType,
            LaborTypeOrder: 2000 // New resource types should be put at bottom of order
        };

        // Add standard custom fields
        angular.forEach(laborType.CustomFieldValues, function (cfv) {
            if (!cfv.IsOpenEnded) {
                var cfValue = {};
                Object.assign(cfValue, cfv);

                // get the value from the element in case it was updated
                var cfElement = angular.element("#LT" + laborType.BOELaborTypeID + "CF" + cfv.CustomFieldID);
                var elementValue = parseInt(cfElement.val());
                if (!angular.isNumber(elementValue)) {
                    elementValue = -1;
                }
                cfValue.CustomFieldValueID = elementValue;

                cfValue.SelectionID = -1;
                cfValue.UpdateDate = '0';
                cfValue.UpdateDateLong = '0';

                row.CustomFieldValues.push(cfValue);
            }
        });

        // insert before the blank row
        $scope.model.LaborTypesData.splice($scope.model.LaborTypesData.length - 1, 0, row);

        // Add open ended custom fields after rows have been created
        angular.forEach(laborType.CustomFieldValues, function (cfv) {
            if (cfv.IsOpenEnded) {
                // get the value from the element in case it was updated
                var cfElement = angular.element("#LT" + laborType.BOELaborTypeID + "CF" + cfv.CustomFieldID);
                var elementValue = cfElement.val();

                var cfValue = {};
                Object.assign(cfValue, cfv);
                cfValue.openEndedValue = elementValue;
                cfValue.CustomFieldValueID = -1;
                cfValue.SelectionID = -1;
                cfValue.UpdateDate = '0';
                cfValue.UpdateDateLong = '0';

                // Add to the row and update the input value
                row.CustomFieldValues.push(cfValue);
                setTimeout(function () { TaskElementDetailsWidget.UpdateOpenEndedCustomFields("LT" + row.BOELaborTypeID + "CF" + cfv.CustomFieldID, elementValue); }, 1);
            }
        });

        setTimeout(function () { validateCustomFields(); }, 1);
    };

    $scope.checkIfNewRowNeeded = function (item) {
        if (item.NewLaborType) {
            item.NewLaborType = false;
            $scope.addRow();
        }
    };

    $scope.updatePercentLock = function (item) {
        if (item.RateType !== ManageTaskModel.RateTypeCost && item.SpreadCurveID !== ManageTaskModel.SpreadCurvesDiscreteHours) {
            $scope.setDirty();
            item.PercentSpreadLocked = true;
            item.HourSpreadLocked = false;
        }
    };

    $scope.updateHoursLock = function (item) {
        if (item.RateType !== ManageTaskModel.RateTypeCost && item.SpreadCurveID !== ManageTaskModel.SpreadCurvesDiscreteHours) {
            $scope.setDirty();
            item.PercentSpreadLocked = false;
            item.HourSpreadLocked = true;
        }
    }

    $scope.convertToString = function (value) {
        var val = value.toString();

        if (val.startsWith('? ') && val.endsWith(' ?')) {
            val = val.substring(2, val.length - 2);
        }

        if (val.startsWith('number:')) {
            val = val.substring(7);
        }

        if (val.startsWith('string:')) {
            val = val.substring(7);
        }

        return val;
    };

    $scope.updateDropdowns = function () {

        angular.forEach($scope.model.LaborTypesData, function (item) {

            if ($scope.showDropdowns) {
                if (item.ResourceDescription) {
                    item.ResourceInput = $scope.ResourceModels.find(function (res) {
                        return res.ResourceDesc == item.ResourceDescription;
                    });
                }

                if (item.BusinessResourceCodeDescription) {
                    item.BusinessResourceCodeInput = $scope.BusinessResourceCodeModels.find(function (res) {
                        return res.ResourceDesc == item.BusinessResourceCodeDescription
                    });
                }

                if (item.PerformingOrgName) {
                    item.PerfOrgInput = $scope.PerfOrgModels.find(function (perf) {
                        return perf.PerformingOrgName == item.PerformingOrgName;
                    });
                }
            } else {
                item.ResourceInput = item.ResourceDescription;
                item.BusinessResourceCodeInput = item.BusinessResourceCodeDescription;
                item.PerfOrgInput = item.PerformingOrgName;
            }
        });
    };

    $scope.expandSpreads = function () {
        // Change the Labor Spread Section to be expanded if collapsed
        $('#ManageLaborSpread.collapsed .collapsible-header').click();
    };

    $scope.spreadsPaste = function (e) {
        $scope.laborSpreadPasteErrors = [];
        e.preventDefault();
        var data;
        if (e.originalEvent.clipboardData && e.originalEvent.clipboardData.getData) {
            data = e.originalEvent.clipboardData.getData('text/plain');
        } else {
            data = window.clipboardData.getData('Text'); // IE
        }

        var rows = data.split("\n");
        var currentRow = $(e.target);
        var startingCellId = currentRow.attr('cellId');
        var rowId = currentRow.attr('rowId');
        var cellId = currentRow.attr('cellId');

        // remove last trailing row if nothing in it
        if (rows.length > 0 && rows[rows.length - 1] == '') {
            rows.pop();
        }

        for (var y in rows) {

            var cells = rows[y].split("\t");

            // if last row and nothing in rows[y] then this was a trailing linebreak
            if (rows[y] == '' && y == rows.length - 1) {
                break;
            }

            var realItems = $scope.tableData.filter(function (item) {
                return item.BOELaborTypeID == rowId;
            });

            if (!realItems || realItems.length === 0) {
                // no more rows, so we are done
                break;
            }

            var realItem = realItems[0];
            var spreads = realItem.SpreadData;
            var itemStartDate = realItem.StartDate.toDate();
            var itemEndDate = realItem.EndDate.toDate();
            // validation if row is not discrete
            if ($scope.getSpreadDisabled(realItem)) {
                addError($scope.laborSpreadPasteErrors, 'This row is not discrete, so it cannot be updated.');
            } else {

                // we are processing tabular data, so off we go
                for (var x in cells) {
                    cells[x] = new BigNumber(cells[x]);
                    cells[x] = $scope.checkNaN(cells[x]);

                    // validation if spread month is not inside the row's date range
                    if ($scope.model.SpreadDatesFullAsDate[cellId] < itemStartDate || $scope.model.SpreadDatesFullAsDate[cellId] > itemEndDate) {
                        addError($scope.laborSpreadPasteErrors, 'Unable to insert value into the spreads because it is outside of date range.');
                    } else if (spreads[cellId]) {
                        spreads[cellId] = cells[x];
                        $scope.spreadInputChanged($scope.model.SpreadDatesFull[cellId], cellId, realItem);
                    } else {
                        addError($scope.laborSpreadPasteErrors, 'Unable to insert value into the spreads because it is outside of date range.');
                    }

                    cellId++;
                }
            }

            // find next row if not last row
            if (y < (rows.length - 1)) {
                var nextTR = currentRow.closest('tr').next();
                if (nextTR.attr('id') !== 'LaborSpreadDataDividerRow') {
                    var currentRow = nextTR.find('.LSDateinput').first();
                    rowId = currentRow.attr('rowId');
                    cellId = startingCellId;
                } else {
                    // no more rows, so we are done
                    addError($scope.laborSpreadPasteErrors, 'Too many rows pasted.');
                    break;
                }
            }
        }

        recalculateAllSpreadsAndTotals();
    };

    addError = function (errorArray, error) {
        // Do not allow duplicates
        if (!errorArray.some(function (item) {
            if (item.ValidationIssue === error) {
                return true;
            }
        })) {
            errorArray.push({ ValidationIssue: error });
        }
    }
}]); 