/// <reference path="directives.js" />
// The controller for the MOQ equation section.
moqEquationApp.controller('MoqEquationController', ['$scope', '$document', '$uibModal', '$window', 'ManageTaskModel', '$timeout', function ($scope, $document, $uibModal, $window, ManageTaskModel, $timeout) {
    $scope.init = function () {
        var MOQEquationFieldWidget = null;

        $scope.model = $window.MOQEquationFieldModel;
        $scope.model.IsCostEquation = ($scope.model.MoqEquationType == 'Cost');
        $scope.model.insertWorkspaceModalOpen = false;

        $scope.newTableId = -1;

        // This is needed to allow for some other processing to finish, otherwise we get errors from angular.js
        setTimeout(function () { 
            initializeWidget();

            angular.forEach($scope.model.SelectedMoqTypes.map(e => e.SelectedMOQType.toString()), function (id) {
                $scope.InitializeRteFields(id);
            });
        }, 10);        
    }

    // Called when the Insert Workspace Variable dropdown item is clicked.
    $scope.InsertWorkspaceVariableClicked = function () {
        // show the modal
        var modalInstance = $uibModal.open({
            templateUrl: $scope.model.BaseUrl + 'Resources/InsertWorkspaceVariable.html',
            controller: 'InsertWorkspaceVariableController',
            windowTopClass: 'bootstrap insert-ws-var-modal',
            windowClass: 'bootstrap',
            backdropClass: 'bootstrap',
            resolve: { model: [function () { return $scope.model.WorkspaceVariables; }] }
        });

        $scope.model.insertWorkspaceModalOpen = true;

        // handle the result
        modalInstance.result.then(function (selectedVariable) {
            $scope.model.insertWorkspaceModalOpen = false;

            if (angular.isDefined(selectedVariable)) {
                // add the variable to the MOQ equation field
                var moqEquationField = $('#MOQEquationField #MOQEquation');
                var previousValue = moqEquationField.val();
                moqEquationField.val(previousValue + selectedVariable.WorkspaceVariableName);

                // re-validate the equation
                $(document).trigger('ValidateMOQEquation');
            }
        });
    };

    // Called when the Search Estimating Catalog dropdown item is clicked.
    $scope.SearchEstimatingCatalogClicked = function () {
        $(document).trigger('SEARCH_METRICS');
    }

    // Called when the Copy MOQ from BOE dropdown item is clicked.
    $scope.CopyMoqFromBoeClicked = function () {
        $('#CopyMoqFromBoeLink').data('moq-task-id', $scope.model.TaskElementId);
        $(document).trigger("COPY_MOQ_SELECT");
    }

    // Adds MOQ Type to Selected MOQ Types (and removes it from the dropdown of available types)
    $scope.AddMoqType = function () {
        var selectedItem = $scope.model.selectedMOQType;
        selectedItem.Order = 2000;

        $scope.model.SelectedMoqTypes.push(selectedItem);
        $scope.InitializeRteFields(selectedItem.SelectedMOQType, true);

        // If both Comparative and Historical exist and one is being added here, 
        // Initialize RTE fields for the other to prevent an issue that occurs after
        // converting one and then adding the other
        if (selectedItem.SelectedMOQType == $scope.model.HistoricalMoqType && $scope.model.SelectedMoqTypes.find(x => x.SelectedMOQType == $scope.model.ComparativeMoqType)) {
            $scope.InitializeRteFields($scope.model.ComparativeMoqType, true);
        } else if (selectedItem.SelectedMOQType == $scope.model.ComparativeMoqType && $scope.model.SelectedMoqTypes.find(x => x.SelectedMOQType == $scope.model.HistoricalMoqType)) {
            $scope.InitializeRteFields($scope.model.HistoricalMoqType, true);
        }

        selectedItem.TableData = [];
        $scope.CreateNewTable(selectedItem.TableData);
        $scope.$emit('MOQ_TYPE_SELECTION_CHANGED', $scope.model.SelectedMoqTypes);
        MOQEquationFieldWidget.setDirty();
    }

    // Removes MOQ Type from Selected MOQ Types (and adds it into the dropdown)
    $scope.RemoveMoqType = function (item) {
        GenSession.confirmDialog('Delete MOQ Type?', 'Are you sure you want to delete the selected MOQ Type and all associated data?<br/> Once deleted, this can not be undone.', function () {
            $scope.$apply(function () {
                var index = $scope.model.SelectedMoqTypes.indexOf(item);
                $scope.model.SelectedMoqTypes.splice(index, 1);
                $scope.$emit('MOQ_TYPE_SELECTION_CHANGED', $scope.model.SelectedMoqTypes);
                MOQEquationFieldWidget.setDirty();
            });
        });
    }

    // Initializes RTE fields for the selected MOQ Type option
    $scope.InitializeRteFields = function (id, skipInitialClean) {
        // setTimeout is needed to allow for the objects to be added into the DOM, before we can transform them into RTE
        setTimeout(function () {
            if (!$scope.model.IsReadOnly || $scope.model.ShouldMoqReadOnlyBeReversed) {
                InitializeRTE('DescriptionHoursRequired_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
                InitializeRTE('SmeReason_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
                InitializeRTE('SmeHoursLogic_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
                InitializeRTE('SmeDurationLogic_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
                InitializeRTE('SmeTaskEstimates_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
                InitializeRTE('Rationale_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
                InitializeRTE('SkillMixRationale_' + id, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, skipInitialClean);
            }

            if($scope.model.IsReadOnly && $scope.model.ShouldMoqReadOnlyBeReversed) {
                $('.moqContainerClass .replacedWidgetText').remove();
            }
        }, 1);
    }

    // Actual Read Only, including reversal
    $scope.ActualReadOnly = function()
    {
        return $scope.model.IsReadOnly && !$scope.model.ShouldMoqReadOnlyBeReversed;
    }

    // Create New Table Data for the MOQ Type
    $scope.CreateNewTable = function (tableDataArray) {
        var newTable = {};
        newTable.Id = $scope.newTableId--;
        newTable.Order = 2000;

        tableDataArray.push(newTable);
        MOQEquationFieldWidget.setDirty();
    }

    // Remove existing Table Data
    $scope.RemoveTable = function (item, tableDataArray) {
        GenSession.confirmDialog('Delete Data Table?', 'Are you sure you want to delete the selected Data Table?<br/> Once deleted, this can not be undone.', function () {
            $scope.$apply(function () {
                var index = tableDataArray.indexOf(item);
                tableDataArray.splice(index, 1);
                MOQEquationFieldWidget.setDirty();
            });
        });
    }

    // Generates string for the description of what MOQ types are based on
    $scope.PortionOfTask = function () {
        if ($scope.model.SelectedMoqTypes.length > 1) {
            return 'This portion of the task is based on ';
        } else {
            return 'This task is based on ';
        }
    }

    // Generates placeholder text for MOQ Types
    $scope.MoqTypesPlaceholder = function (field, selectedMOQType) {
        /*
            Enum values:
            Historical = 5001,
            Comparative = 5002,
            CostEstimatingRelationships = 5003,
            ParametricEstimates = 5004,
            AnalogousRelationships = 5005,
            SOW = 5006,
            LOE = 5007,
            SME = 5008,
            NonLabor = 5009
        */
        switch (field) {
            case 'Rationale':
                switch (parseInt(selectedMOQType)) {
                    case 5001:
                        return 'Need to provide how the actual hours are relevant to the proposed effort.  Explain complexity factors.';
                    case 5002:
                        return 'Need to provide how the similar historical actuals are relevant to the proposed effort.  Explain all complexity factors and skill mix.';
                    case 5003:
                        return 'If CERs is not submitted in advance to USG, provide a complete explanation of how the model works, the historical or other data sources used, and how any model output was used to calculate proposed hours.  Proposal teams cannot claim the model is Lockheed Martin Proprietary Information and not provide.';
                    case 5004:
                        return 'If parametric estimating model is not submitted in advance to USG, provide a complete explanation of how the model works, the historical or other data sources used, and how any model output was used to calculate proposed hours.  Proposal teams cannot claim the model is Lockheed Martin Proprietary Information and not provide.';
                    case 5005:
                        return 'If AR is not submitted in advance to USG, provide a complete explanation of how the model works, the historical or other data sources used, and how any model output was used to calculate proposed hours.  Proposal teams cannot claim the model is Lockheed Martin Proprietary Information and not provide.';
                    case 5006:
                        return 'Document any business area hours per month used.';
                    case 5007:
                        return 'Document any business area hours per month used & why LOE selected.';
                    case 5009:
                        return 'Document any Non-Labor values used & why.';
                    case 5008:
                        return 'For each task please provide task name, a description of the task and the number of hours.  In the SME judgement estimate please break estimated tasks into small enough chunks that customer can fully understand what is being estimated.';
                }
                break;
            case 'Location':
                switch (parseInt(selectedMOQType)) {
                    case 5003:
                    case 5004:
                    case 5005:
                        return 'Reminder:  need to provide a copy of any model used/created that was used for estimating.';
                }
                break;
            case 'Skill Mix Rationale':
                return 'Need to provide a narrative which documents the source and rationale as to why the proposed skill mix was selected. Remember: If based on actuals and not the exact same labor mix, a summary table of the historical labor mix is required. Click the grey question mark for additional information and suggested format.';

        }
    }

    // Toggles an item between collapsed and expended
    $scope.toggle = function (item) {
        item.collapsed = !item.collapsed;
    }

    $scope.openHelp = function (suffix) {
        var url = $scope.model.MoqTypeHelpUrls.BaseUrl + suffix;
        $window.open(url, '_blank');
    };

    //#region MOQ Type Ordering

    // Returns an Ordered Selected Moq Types array
    $scope.OrderedSelectedMoqTypes = function () {
        return $scope.model.SelectedMoqTypes.sort((a, b) => (a.Order < b.Order) ? -1 : 1);
    }

    // Used to lock the page up while sorting. This is needed for TinyMCE to initialize correctly.
    $scope.sortingInProgress = false;

    // Move MOQ Type up
    $scope.MoveUpMoqType = function () {
        $scope.sortingInProgress = true;
        var newOrderArray = [];
        var pos = 0;
        $scope.OrderedSelectedMoqTypes().forEach(function (item) {
            if (item.SelectedMOQType === $scope.model.SortingMoqType.SelectedMOQType) {
                var previous = newOrderArray.pop();
                item.Order = pos - 1;
                newOrderArray.push(item);

                if (previous) {
                    previous.Order = pos++;
                    newOrderArray.push(previous);
                }
            }
            else {
                item.Order = pos++;
                newOrderArray.push(item);
            }
        });

        $scope.UpdateOrderingMoqTypes(newOrderArray);
    }

    // Move MOQ Type down
    $scope.MoveDownMoqType = function () {
        $scope.sortingInProgress = true;
        var newOrderArray = [];
        var pos = 0;
        var nextOffset = 0;
        $scope.OrderedSelectedMoqTypes().forEach(function (item) {
            if (item.SelectedMOQType === $scope.model.SortingMoqType.SelectedMOQType) {
                item.Order = 1 + pos++;

                nextOffset = -1;
            }
            else {
                item.Order = nextOffset + pos++;

                nextOffset = 0;
            }

            newOrderArray.push(item);
        });

        $scope.UpdateOrderingMoqTypes(newOrderArray);
    }

    // Applies the new order of MOQ Types to the underlying model.
    $scope.UpdateOrderingMoqTypes = function (newOrderArray) {
        $timeout(function () {
            $scope.$apply(function () {
                $scope.model.SelectedMoqTypes.forEach(function (item) {
                    var matchingItem = newOrderArray.find(({ SelectedMOQType }) => SelectedMOQType === item.SelectedMOQType);
                    item.Order = matchingItem.Order;

                    InitializeRTE('DescriptionHoursRequired_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                    InitializeRTE('SmeReason_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                    InitializeRTE('SmeHoursLogic_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                    InitializeRTE('SmeDurationLogic_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                    InitializeRTE('SmeTaskEstimates_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                    InitializeRTE('Rationale_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                    InitializeRTE('SkillMixRationale_' + item.SelectedMOQType, { maxlen: $scope.model.RteFieldSize, enableCharCounting: true }, MOQEquationFieldWidget, true);
                });
            });

            $timeout(function () { $scope.sortingInProgress = false; }, 0);
        }, 0);
    }

    // Returns an Ordered Selected Moq Types array
    $scope.OrderedSelectedMoqTables = function () {
        return $scope.model.SortingMoqTablesMoqTypeParent.TableData.sort((a, b) => (a.Order < b.Order) ? -1 : 1);
    }

    // Move MOQ Table up
    $scope.MoveUpMoqTable = function () {
        var newOrderArray = [];
        var pos = 0;
        $scope.OrderedSelectedMoqTables().forEach(function (item) {
            if (item.TableName === $scope.model.SortingMoqTable.TableName) {
                var previous = newOrderArray.pop();
                item.Order = pos - 1;
                newOrderArray.push(item);

                if (previous) {
                    previous.Order = pos++;
                    newOrderArray.push(previous);
                }
            }
            else {
                item.Order = pos++;
                newOrderArray.push(item);
            }
        });

        $scope.UpdateOrderingMoqTables(newOrderArray);
    }

    // Move MOQ Table down
    $scope.MoveDownMoqTable = function () {
        var newOrderArray = [];
        var pos = 0;
        var nextOffset = 0;
        $scope.OrderedSelectedMoqTables().forEach(function (item) {
            if (item.TableName === $scope.model.SortingMoqTable.TableName) {
                item.Order = 1 + pos++;
                nextOffset = -1;
            }
            else {
                item.Order = nextOffset + pos++;
                nextOffset = 0;
            }

            newOrderArray.push(item);
        });

        $scope.UpdateOrderingMoqTables(newOrderArray);
    }

    // Applies the new order of MOQ Tables to the underlying model.
    $scope.UpdateOrderingMoqTables = function (newOrderArray) {
        $timeout(function () {
            $scope.$apply(function () {
                $scope.model.SortingMoqTablesMoqTypeParent.TableData.forEach(function (item) {
                    var matchingItem = newOrderArray.find(({ TableName }) => TableName === item.TableName);
                    item.Order = matchingItem.Order;
                });
            });
        }, 0);
    }

    // Display Reorder MOQ Types Dialog
    $scope.displayReOrderMoqTypesDialog = function () {
        $scope.model.SortingMoqType = $scope.model.SelectedMoqTypes[0];
        MOQEquationFieldWidget.OpenDialogAfterInitialize(MOQEquationFieldWidget.ReOrderMoqTypesDialog);
    };

    // Close Reorder MOQ Types Dialog
    $scope.closeReOrderMoqTypes = function () {
        MOQEquationFieldWidget.CloseDialog(MOQEquationFieldWidget.ReOrderMoqTypesDialog);
        MOQEquationFieldWidget.setDirty();
    }

    // Display Reorder MOQ Tables Dialog
    $scope.displayReOrderMoqTablesDialog = function (moqType) {
        $scope.model.SortingMoqTablesMoqTypeParent = moqType;
        $scope.model.SortingMoqTable = moqType.TableData[0];

        MOQEquationFieldWidget.OpenDialogAfterInitialize(MOQEquationFieldWidget.ReOrderMoqTablesDialog);
    };

    // Close Reorder MOQ Tables Dialog
    $scope.closeReOrderMoqTables = function () {
        MOQEquationFieldWidget.CloseDialog(MOQEquationFieldWidget.ReOrderMoqTablesDialog);
        $scope.model.SortingMoqTables = [];
        MOQEquationFieldWidget.setDirty();
    }

    $scope.clearPoPDates = function (tableData) {
        tableData.PoPStart = undefined;
        tableData.PoPStartWeek = undefined;
        tableData.PoPStartYear = undefined;
        tableData.PoPEnd = undefined;
        tableData.PoPEndWeek = undefined;
        tableData.PoPEndYear = undefined;
    }

    $scope.disableHistoricalComparativeConvertButtons = function () {
        // Disable the buttons if task already has both Historical and Comparative MOQ Types
        return $scope.model.SelectedMoqTypes.some(function (moqType) {
            return moqType.SelectedMOQType == $scope.model.HistoricalMoqType;
        }) && $scope.model.SelectedMoqTypes.some(function (moqType) {
            return moqType.SelectedMOQType == $scope.model.ComparativeMoqType;
        });
    }

    $scope.convertMoqType = function (moqType, convertToType) {
        var convertToMoq = $scope.model.MOQTypes.find(x => x.SelectedMOQType == convertToType);

        moqType.SelectedMOQType = convertToMoq.SelectedMOQType;
        moqType.SelectedMOQTypeText = convertToMoq.SelectedMOQTypeText;

        MOQEquationFieldWidget.setDirty();
    }

    //#endregion
}]);

// initialize MOQ Equation Widget.. moved here so that way this much script is not in the ascx page
InitializeMOQEquationFieldWidget = function (MOQEquationFieldWidget_ReadOnly, workspaceVariables, ordinaryVariables, newOrdinaryVariableID, sumOfBOEs, discrete, validationUrl, shouldMoqReadOnlyBeReversed,
                                        calculateMOQResultUrl, openSumOfBoesByWbsUrl, openSumOfBoesByClinUrl, isNotSubContractor, sortBOEByWBS, sortBOEByClin) {
    // create base js object;
    MOQEquationFieldWidget = new Widget("MOQEquationField", MOQEquationFieldWidget_ReadOnly);

    MOQEquationFieldWidget.initialLoad = false;
    MOQEquationFieldWidget.MarkedEquation = "";
    MOQEquationFieldWidget.WorkspaceVariables = workspaceVariables;
    MOQEquationFieldWidget.OrdinaryVariables = ordinaryVariables;
    MOQEquationFieldWidget.MOQCalculating = false;
    MOQEquationFieldWidget.MOQValidating = false;
    MOQEquationFieldWidget.MOQValid = false;
    MOQEquationFieldWidget.SavePending = false;
    MOQEquationFieldWidget.NewOrdinaryVariableID = newOrdinaryVariableID;
    
    MOQEquationFieldWidget.HarvestWorkspaceVariablesForSave = function () {
        var toReturn = [];
        var workspaceVariableFields = MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

        for (var ndx = 0; ndx < MOQEquationFieldWidget.WorkspaceVariables.length; ndx++) {
            var variableID = MOQEquationFieldWidget.WorkspaceVariables[ndx].WorkspaceVariableID;
            var variableField = workspaceVariableFields.filter('[pkid=' + variableID + ']');

            if (variableField.length) {
                toReturn.push(variableID);
            }
        }

        return toReturn;
    };
    MOQEquationFieldWidget.HarvestOrdinaryVariablesForSave = function () {
        var toReturn = [];
        var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        for (var ndx = 0; ndx < MOQEquationFieldWidget.OrdinaryVariables.length; ndx++) {
            var currentTaskOrdinaryVariable = MOQEquationFieldWidget.OrdinaryVariables[ndx];
            var variableID = currentTaskOrdinaryVariable.OrdinaryVariableID;
            var variableField = currentVariableInputs.filter('[pkid=' + variableID + ']');
            var variableValue;

            if (variableField.length && (variableValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField)).length > 0) {
                currentTaskOrdinaryVariable.Deleted = false;
                currentTaskOrdinaryVariable.OrdinaryVariableValue = variableValue;
                currentTaskOrdinaryVariable.IsPercentage = MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(variableField);

                if (variableField.attr('BOEToSum') != undefined) {
                    currentTaskOrdinaryVariable.BOEToSum = JSON.parse(variableField.attr('BOEToSum'));
                }

                if (variableField.attr('WBSToSum') != undefined) {
                    currentTaskOrdinaryVariable.WBSToSum = JSON.parse(variableField.attr('WBSToSum'));
                }

                if (variableField.attr('CLINToSum') != undefined) {
                    currentTaskOrdinaryVariable.CLINToSum = JSON.parse(variableField.attr('CLINToSum'));
                }

                if (variableField.attr('ResourceTypes') != undefined) {
                    currentTaskOrdinaryVariable.ResourceTypes = JSON.parse(variableField.attr('ResourceTypes'));
                }

                if (variableField.attr('SortBOEBy') != undefined) {
                    currentTaskOrdinaryVariable.SortBOEBy = variableField.attr('SortBOEBy');
                }

                if (currentTaskOrdinaryVariable.BOEToSum.length || currentTaskOrdinaryVariable.WBSToSum.length || currentTaskOrdinaryVariable.CLINToSum.length) {
                    currentTaskOrdinaryVariable.OrdinaryVariableValueType = sumOfBOEs;
                }
                else {
                    currentTaskOrdinaryVariable.OrdinaryVariableValueType = discrete;
                }
            }
            else {
                currentTaskOrdinaryVariable.Deleted = true;
                currentTaskOrdinaryVariable.OrdinaryVariableValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(variableField);
                currentTaskOrdinaryVariable.OrdinaryVariableValueType = currentTaskOrdinaryVariable.OrdinaryVariableValueType.toString();
            }

            delete currentTaskOrdinaryVariable.UpdateDate;
            currentTaskOrdinaryVariable.UpdateDateLong = currentTaskOrdinaryVariable.UpdateDateLong.toString();
            toReturn.push(currentTaskOrdinaryVariable);
        }

        if (currentVariableInputs.length > 0) {
            for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++) {
                var variableAlreadyInReturnSet = false;
                var currentVariableInput = $(currentVariableInputs[inputNdx]);
                var currentVariableInputID = currentVariableInput.attr('pkid');
                var currentVariableInputValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInput);

                if (currentVariableInputValue.length > 0) {
                    for (var returnNdx = 0; returnNdx < toReturn.length; returnNdx++) {
                        if (currentVariableInputID == toReturn[returnNdx].OrdinaryVariableID) {
                            variableAlreadyInReturnSet = true;
                            break;
                        }
                    }

                    if (!variableAlreadyInReturnSet) {
                        var newOrdinaryVariable = {};
                        newOrdinaryVariable.OrdinaryVariableID = currentVariableInputID;
                        newOrdinaryVariable.OrdinaryVariableName = currentVariableInput.attr('name');;
                        newOrdinaryVariable.OrdinaryVariableValue = currentVariableInputValue;
                        newOrdinaryVariable.IsPercentage = MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage(currentVariableInput);
                        newOrdinaryVariable.DefaultSize = currentVariableInput.closest('tr').data('variable-default-size');

                        newOrdinaryVariable.BOEToSum = JSON.parse(currentVariableInput.attr('BOEToSum'));
                        newOrdinaryVariable.WBSToSum = JSON.parse(currentVariableInput.attr('WBSToSum'));
                        newOrdinaryVariable.CLINToSum = JSON.parse(currentVariableInput.attr('CLINToSum'));
                        newOrdinaryVariable.ResourceTypes = JSON.parse(currentVariableInput.attr('ResourceTypes'));
                        newOrdinaryVariable.SortBOEBy = currentVariableInput.attr('SortBOEBy');

                        if (newOrdinaryVariable.BOEToSum.length || newOrdinaryVariable.WBSToSum.length || newOrdinaryVariable.CLINToSum.length) {
                            newOrdinaryVariable.OrdinaryVariableValueType = sumOfBOEs;
                        }
                        else {
                            newOrdinaryVariable.OrdinaryVariableValueType = discrete;
                        }

                        toReturn.push(newOrdinaryVariable);
                    }
                }
            }
        }

        return toReturn;
    };
    MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen = function () {
        var scope = angular.element(document.getElementById('MOQEquationField')).scope();
        return scope.model.insertWorkspaceModalOpen;
    };
    MOQEquationFieldWidget.InsertMOQElementDialogClosing = function () {
        if (!MOQEquationFieldWidget.MOQValid && !MOQEquationFieldWidget.MOQValidating) {
            MOQEquationFieldWidget.ShowMOQValidationError();
        }
    };
    MOQEquationFieldWidget.GetAllVariableFields = function () {
        return $('#MOQEquationField #MOQEquation-Variables [wsVar], #MOQEquationField #MOQEquation-Variables [ordVar]');
    };
    MOQEquationFieldWidget.RefreshStyles = function () {
        MOQEquationFieldWidget.applyReadOnly();

        if (MOQEquationFieldWidget.isReadOnly()) {
            $("#MOQEquationField #InsertWorkspaceVariableLink").parent().prev().remove();
            $("#MOQEquationField #InsertWorkspaceVariableLink").parent().remove();
            $("#MOQEquationField #SearchEstimatingCatalogLink").parent().prev().remove();
            $("#MOQEquationField #SearchEstimatingCatalogLink").parent().remove();

            /*
                MOQ-Type (drop-down) and MOQ-Text EFFECTIVELY inherit THEIR editability from the Task Details widget.
 
                This partial view contains the MOQ Equation, MOQ Type and MOQ Text fields.  MOQ Equation is NOT editable in
                a Locked state, but the other two are.  The Widget logic automatically locks access to all three, so this
                logic is added to override that for the Type and Text fields.
 
                The REAL issue here (and in a broader sense, throughout the UI) is that the Widgets apply their
                read-only flag to EVERY child element as a whole -- But that CANNOT work for cases (like this one) where
                different elements can have different accessibilities.  The ideal approach is to have a SET of access flags
                for different (individual) fields and/or groups of fields.  This is what the security matrix is SUPPOSED to
                be used for, but our code does not always consult it (or even consult it correctly or completely in every case).
                In addition, we often have access-determination logic in the views themselves that should really be executed
                in the view models (or at least in the controllers) and then forwarded to the views as booleans.
            */
            if (!TaskElementDetailsWidget.isReadOnly()) {
                var moqSection = $('#MOQEquationField');

                var moqTypeElement = $("#MOQType", moqSection);
                $('.replacedWidgetText', moqTypeElement.parent()).addClass('display-none');
                moqTypeElement.removeClass('display-none');

                RemoveRTETemplateReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText', moqSection);
            }
        }

        refreshModule($('.task-element-details.module'));
    };
    MOQEquationFieldWidget.ValidateMOQEquation = function () {
        TaskElementDetailsWidget.waitingBeforeSubmit = true;

        GenSession.ShowLoadingBox();
        MOQEquationFieldWidget.MOQValidating = true;

        MOQEquationFieldWidget.HideError();

        var moqEquation = $.trim($("#MOQEquationField #MOQEquation").val());

        var dataToSend = { "moqEquation": moqEquation }
        dataToSend = JSON.stringify(dataToSend);

        $.ajax({
            type: 'POST',
            url: validationUrl,
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: dataToSend,
            success: function (response, textStatus) {
                MOQEquationFieldWidget.MOQValidated(response);

                if (MOQEquationFieldWidget.isReadOnly() && shouldMoqReadOnlyBeReversed) {
                    $('.magnifier-button').addClass('display-none');
                    $('#MoqType').children('.replacedWidgetText').remove();
                    $('.moqTypes').removeClass('display-none');

                    var moqSection = $('#MOQEquationField');
                    RemoveRTETemplateReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText', moqSection);
                }
            },
            error: function (response, textStatus) {
                MOQEquationFieldWidget.MOQValidationError(response);
            }
        });
    };
    MOQEquationFieldWidget.MOQValidationError = function (exceptionstring) {
        MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", "A general validation error occurred. Please check the MOQ equation for validity.");
        MOQEquationFieldWidget.ShowMOQValidationError();
        MOQEquationFieldWidget.MOQValidating = false;
        TaskElementDetailsWidget.waitingBeforeSubmit = false;
        GenSession.HideLoadingBox();
    };
    MOQEquationFieldWidget.SetMOQError = function (errorSubject, errorMessage) {
        if (errorSubject != undefined && errorMessage != undefined) {
            $("#MOQEquationField #MOQEquation-ErrorTitle").html(errorSubject);
            $("#MOQEquationField #MOQEquation-ErrorText").html(errorMessage);
        }
    };
    MOQEquationFieldWidget.ShowMOQValidationError = function () {
        $("#MOQEquationField #MOQEquation-Variables").hide();
        MOQEquationFieldWidget.ShowMOQError();
    };
    MOQEquationFieldWidget.ShowMOQError = function () {
        $("#MOQEquationField #MOQEquation-Error").show();
        MOQEquationFieldWidget.SetMOQResult("");
        MOQEquationFieldWidget.RefreshStyles();
    };
    MOQEquationFieldWidget.HideError = function () {
        $("#MOQEquationField #MOQEquation-Error").hide();
        MOQEquationFieldWidget.RefreshStyles();
    };
    MOQEquationFieldWidget.FocusError = function () {
        $("#MOQEquationField #MOQEquation-ErrorTitle").focus();
    }
    MOQEquationFieldWidget.MOQEquationPreparedForSubmit = function () {

        if (MOQEquationFieldWidget.MOQValidating || MOQEquationFieldWidget.MOQCalculating) {
            MOQEquationFieldWidget.SavePending = true;
        }

        return (!$("#MOQEquationField #MOQEquation-Error").is(":visible") &&
            !MOQEquationFieldWidget.MOQValidating &&
            !MOQEquationFieldWidget.MOQCalculating &&
            MOQEquationFieldWidget.ReadyForCalculate());
    };
    MOQEquationFieldWidget.MOQValidated = function (results) {
        TaskElementDetailsWidget.waitingBeforeSubmit = true;
        GenSession.ShowLoadingBox();
        // Set Widget valid flag
        MOQEquationFieldWidget.MOQValid = results.Status;

        if (results.Status) {
            var variables = results.Variables;

            MOQEquationFieldWidget.RemoveInvalidVariables(variables);

            for (var ndx = 0; ndx < variables.length; ndx++) {
                if (ndx == 0) {
                    MOQEquationFieldWidget.MarkedEquation = variables[ndx];
                }
                else {
                    var defaultSize = MOQEquationFieldWidget.GetVariableDefaultSize(variables[ndx]);
                    MOQEquationFieldWidget.AddVariableField(variables[ndx], defaultSize);
                }
            }
            // Reinitialize imported historical metric data.
            $('#MOQDefaultSizes').val("");
            $('#HistoricalMetricEquation').val("");

            var currentVariables = MOQEquationFieldWidget.GetAllVariableFields();

            if (currentVariables.length > 0) {
                $("#MOQEquationField #MOQEquation-Variables").show();
                $("#MOQEquationField .moqVariableNote").removeClass('display-none');
            }
            else {
                $("#MOQEquationField #MOQEquation-Variables").hide();
            }

            MOQEquationFieldWidget.RefreshStyles();

            MOQEquationFieldWidget.CalculateMOQResult();
        }
        else {
            // Set the validation error text
            MOQEquationFieldWidget.SetMOQError("Invalid MOQ Equation", results.Message);

            // If the user isn't inserting a variable, we'll show the error
            if (!MOQEquationFieldWidget.InsertWorkspaceVariableDialogIsOpen() &&
                (!TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogMST').isOpen() ||
                    !TaskElementDetailsWidget.getDialog('MOQEquation-SearchEstimatingCatalogDialogCommon').isOpen())) {
                MOQEquationFieldWidget.ShowMOQValidationError();
            }

            MOQEquationFieldWidget.SavePending = false;
        }

        MOQEquationFieldWidget.MOQValidating = false;
        GenSession.HideLoadingBox();
        TaskElementDetailsWidget.waitingBeforeSubmit = false;
    };
    /**
    * Gets a default size value for a variable that was created as a result of importing a historical
    * metric.  Returns "" for all other variable types.
    */
    MOQEquationFieldWidget.GetVariableDefaultSize = function (currentVariable) {
        var metricEquation = $('#HistoricalMetricEquation').val().toUpperCase();
        var defaultSize = "";
        if (metricEquation.length) {
            var newMetricDefaultSizes = $('#MOQDefaultSizes').val();
            if (newMetricDefaultSizes != "" && metricEquation.indexOf(currentVariable.toUpperCase()) != -1) {
                // Parse out the 1st default sizes for assignment to the current variable.
                var moqVariableDefaultSizes = [];
                moqVariableDefaultSizes = JSON.parse(newMetricDefaultSizes);
                if (moqVariableDefaultSizes.length) {
                    defaultSize = moqVariableDefaultSizes[0];
                    moqVariableDefaultSizes.splice(0, 1);
                    $('#MOQDefaultSizes').val(JSON.stringify(moqVariableDefaultSizes));
                }
            }
        }
        return defaultSize;
    };
    MOQEquationFieldWidget.RemoveInvalidVariables = function (currentVariables) {
        var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields();

        if (currentVariableInputs.length > 0) {
            for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++) {
                var currentVariableInput = $(currentVariableInputs[inputNdx]);
                var variableStillValid = false;

                for (var variableNdx = 0; variableNdx < currentVariables.length; variableNdx++) {
                    if (currentVariableInput.attr('name') == currentVariables[variableNdx]) {
                        variableStillValid = true;
                        break;
                    }
                }

                if (!variableStillValid) {
                    currentVariableInput.parents('tr').remove();
                }
            }
        }
    };
    MOQEquationFieldWidget.AddVariableField = function (variableName, defaultSize) {
        var workspaceVariable = undefined;
        var ordinaryVariable = undefined;
        var existingVariableField = MOQEquationFieldWidget.GetAllVariableFields().filter('[name="' + variableName + '"]');
        if (!existingVariableField.length) {
            var moqEquationVariablesElement = $('#MOQEquationField #MOQEquation-Variables');

            // Workspace Variable Field
            if ((workspaceVariable = MOQEquationFieldWidget.GetWorkspaceVariable(variableName)) != undefined) {
                // Summed BOE Workspace Variable
                if (workspaceVariable.WorkspaceVariableValueType == 2) {
                    moqEquationVariablesElement.append('<tr var="' + workspaceVariable.WorkspaceVariableID + '" data-variable-default-size="" ><td class="variableLabel">' + workspaceVariable.WorkspaceVariableName + ':</td><td><a wsVar="true" readOnlyVar="true" pkid="' + workspaceVariable.WorkspaceVariableID + '" name="' + workspaceVariable.WorkspaceVariableName.toUpperCase() + '" SortBOEBy="' + workspaceVariable.SortBOEBy + '">' + Helper.addCommas(workspaceVariable.WorkspaceVariableValue) + '</a></td></tr>');
                }
                // Discrete Workspace Variable
                else {
                    moqEquationVariablesElement.append('<tr var="' + workspaceVariable.WorkspaceVariableID + '" data-variable-default-size="" ><td class="variableLabel">' + workspaceVariable.WorkspaceVariableName + ':</td><td><div wsVar="true" pkid="' + workspaceVariable.WorkspaceVariableID + '" name="' + workspaceVariable.WorkspaceVariableName.toUpperCase() + '" SortBOEBy="' + workspaceVariable.SortBOEBy + '">' + Helper.addCommas(workspaceVariable.WorkspaceVariableValue) + '</div></td></tr>');
                }
            }
            // Existing (Saved) Task Ordinary Variable Field
            else if ((ordinaryVariable = MOQEquationFieldWidget.GetOrdinaryVariable(variableName)) != undefined) {
                var defaultSizeLabel = '';
                if (ordinaryVariable.DefaultSize != undefined && ordinaryVariable.DefaultSize != '') {
                    defaultSizeLabel = '(' + ordinaryVariable.DefaultSize + ')';
                }
                if (ordinaryVariable.OrdinaryVariableID < 0) {
                    // Actually a new variable that came from a refreshed patial view.
                    MOQEquationFieldWidget.NewOrdinaryVariableID--;
                }
                if (MOQEquationFieldWidget.isReadOnly()) {
                    // READ ONLY Summed BOE Existing Task Ordinary Variable code
                    if (ordinaryVariable.OrdinaryVariableValueType == sumOfBOEs) {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.Size + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName
                            + ':</td><td><a ordVar="true" readOnlyVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '">'
                            + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '</a></td></tr>');
                    }
                    // READ ONLY Discrete Existing Task Ordinary Variable code
                    else {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.Size + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName
                            + ':</td><td><input ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue)
                            + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/></td></tr>');
                    }
                }
                else {
                    var notSubText = '';
                    if (isNotSubContractor) {
                        notSubText = '<a class="sumVariable" var="' + ordinaryVariable.OrdinaryVariableID + '">Select BOEs to sum</a>';
                    }

                    // Summed BOE Existing Task Ordinary Variable code
                    if (ordinaryVariable.OrdinaryVariableValueType == sumOfBOEs) {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '"><td class="variableLabel">' + ordinaryVariable.OrdinaryVariableName + ':</td><td><input disabled="disabled" ordVar="true" pkid="'
                            + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue)
                            + '" BOEToSum="' + JSON.stringify(ordinaryVariable.BOEToSum) + '" WBSToSum="' + JSON.stringify(ordinaryVariable.WBSToSum) + '" CLINToSum="' + JSON.stringify(ordinaryVariable.CLINToSum) + '" ResourceTypes="'
                            + JSON.stringify(ordinaryVariable.ResourceTypes) + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/>'
                            + notSubText + '<span class="clearVariable" var="' + ordinaryVariable.OrdinaryVariableID + '"> | <a>Clear</a></span></td></tr>');
                    }
                    // Discrete Existing Task Ordinary Variable code
                    else {
                        moqEquationVariablesElement.append('<tr var="' + ordinaryVariable.OrdinaryVariableID + '" data-variable-default-size="' + ordinaryVariable.DefaultSize + '"><td class="variableLabel">' + defaultSizeLabel
                            + ordinaryVariable.OrdinaryVariableName + ':</td><td><input ordVar="true" pkid="' + ordinaryVariable.OrdinaryVariableID + '" name="' + ordinaryVariable.OrdinaryVariableName
                            + '" class="variableInput" value="' + Helper.addCommas(ordinaryVariable.OrdinaryVariableValue) + '" BOEToSum="' + JSON.stringify(ordinaryVariable.BOEToSum) + '" WBSToSum="'
                            + JSON.stringify(ordinaryVariable.WBSToSum) + '" CLINToSum="' + JSON.stringify(ordinaryVariable.CLINToSum) + '" ResourceTypes="' + JSON.stringify(ordinaryVariable.ResourceTypes)
                            + '" SortBOEBy="' + ordinaryVariable.SortBOEBy + '"/>' + notSubText + '</td></tr>');
                    }
                }
            }
            // New Task Ordinary Variable Field
            else { 
                var defaultSizeLabel = '';
                if (defaultSize != undefined && defaultSize != '') {
                    defaultSizeLabel = '(' + defaultSize + ')';
                }
                MOQEquationFieldWidget.NewOrdinaryVariableID--;
                var notSubText = '';
                if (isNotSubContractor) {
                    notSubText = '<a class="sumVariable" var="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '">Select BOEs to sum</a>';
                }

                moqEquationVariablesElement.append('<tr var="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '" data-variable-default-size="' + defaultSize + '"><td class="variableLabel">' + defaultSizeLabel
                    + variableName + ':</td><td><input ordVar="true" pkid="' + MOQEquationFieldWidget.NewOrdinaryVariableID + '" name="' + variableName + '" class="variableInput" value="' + defaultSize
                    + '" BOEToSum="[]" WBSToSum="[]" CLINToSum="[]" ResourceTypes="[]" SortBOEBy="' + sortBOEByWBS + '"/> '
                    + notSubText + '</td></tr>');
            }
        }
    };
    MOQEquationFieldWidget.GetWorkspaceVariable = function (variableName) {
        for (var variableNdx = 0; variableNdx < MOQEquationFieldWidget.WorkspaceVariables.length; variableNdx++) {
            if ($.trim(variableName.toUpperCase()) == $.trim(MOQEquationFieldWidget.WorkspaceVariables[variableNdx].WorkspaceVariableName.toUpperCase())) {
                return MOQEquationFieldWidget.WorkspaceVariables[variableNdx];
            }
        }

        return undefined;
    };
    MOQEquationFieldWidget.GetOrdinaryVariable = function (variableName) {
        for (var variableNdx = 0; variableNdx < MOQEquationFieldWidget.OrdinaryVariables.length; variableNdx++) {
            if ($.trim(variableName.toUpperCase()) == $.trim(MOQEquationFieldWidget.OrdinaryVariables[variableNdx].OrdinaryVariableName.toUpperCase())) {
                return MOQEquationFieldWidget.OrdinaryVariables[variableNdx];
            }
        }

        return undefined;
    };
    MOQEquationFieldWidget.CalculateMOQResult = function () {
        if (MOQEquationFieldWidget.ReadyForCalculate()) {
            GenSession.ShowLoadingBox();
            var moqEquation = MOQEquationFieldWidget.GetInputEquation();

            MOQEquationFieldWidget.MOQCalculating = true;

            var dataToSend = { "moqEquation": moqEquation }
            dataToSend = JSON.stringify(dataToSend);

            $.ajax({
                type: 'POST',
                url: calculateMOQResultUrl,
                contentType: 'application/json; charset=utf-8',
                dataType: 'json',
                data: dataToSend,
                success: MOQEquationFieldWidget.MOQCalculationSuccess,
                error: MOQEquationFieldWidget.MOQCalculationError
            });
        }
        else {
            MOQEquationFieldWidget.SetMOQResult("");

            MOQEquationFieldWidget.SavePending = false;
        }
    };
    MOQEquationFieldWidget.ReadyForCalculate = function () {
        MOQEquationFieldWidget.HideError();

        var currentVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        if (currentVariableInputs.length > 0) {
            for (var inputNdx = 0; inputNdx < currentVariableInputs.length; inputNdx++) {
                var currentValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(currentVariableInputs[inputNdx]);

                if (currentValue.length > 0) {
                    if (!currentValue.match(/^[\+\-]*[\d,]*\.?[\d,]+%?$/)) {
                        MOQEquationFieldWidget.SetMOQError("Invalid MOQ Variables", "All MOQ equation variables must be valid numerical values. Please check each variable field below.");
                        MOQEquationFieldWidget.ShowMOQError();
                        return false;
                    }
                    else {
                        $(currentVariableInputs[inputNdx]).val(Helper.addCommas(Helper.removeCommas(currentValue)));
                    }
                }
                else {
                    MOQEquationFieldWidget.SetMOQError("Incomplete MOQ Variables", "All MOQ equation variable fields must contain values. Please fill in all of the variable fields below.");
                    MOQEquationFieldWidget.ShowMOQError();
                    return false;
                }
            }
        }

        return true;
    };
    MOQEquationFieldWidget.GetInputEquation = function () {
        var markedEquation = $.trim(MOQEquationFieldWidget.MarkedEquation);
        var ordinaryVariableInputs = MOQEquationFieldWidget.GetAllVariableFields().filter('[ordVar]');

        if (ordinaryVariableInputs.length > 0) {
            for (var inputNdx = 0; inputNdx < ordinaryVariableInputs.length; inputNdx++) {
                var currentVariableInput = $(ordinaryVariableInputs[inputNdx]);
                var currentVariableInputName = currentVariableInput.attr('name');
                var currentVariableInputValue = MOQEquationFieldWidget.GetOrdinaryVariableFieldValue(ordinaryVariableInputs[inputNdx]);
                var result = currentVariableInputName.replace(/\(\d+.*\) /g, "");
                var currentVariableRegEx = new RegExp("<" + result + ">", "gi");
                markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
            }
        }

        workspaceVariableDivs = MOQEquationFieldWidget.GetAllVariableFields().filter('[wsVar]');

        if (workspaceVariableDivs.length > 0) {
            for (var inputNdx = 0; inputNdx < workspaceVariableDivs.length; inputNdx++) {
                var currentVariableInput = $(workspaceVariableDivs[inputNdx]);
                var currentVariableInputName = currentVariableInput.attr('name');
                var currentVariableInputValue = currentVariableInput.text();

                var currentVariableRegEx = new RegExp("<" + currentVariableInputName + ">", 'gi');
                markedEquation = markedEquation.replace(currentVariableRegEx, currentVariableInputValue);
            }
        }

        return markedEquation;
    };
    MOQEquationFieldWidget.GetOrdinaryVariableFieldValue = function (ordinaryVariableField) {
        ordinaryVariableField = $(ordinaryVariableField);

        var fieldValue = '';

        if (ordinaryVariableField.is('[readOnlyVar]')) {
            fieldValue = ordinaryVariableField.text();
        }
        else {
            fieldValue = ordinaryVariableField.val();
        }

        return fieldValue;
    };
    MOQEquationFieldWidget.OrdinaryVariableFieldIsPercentage = function (ordinaryVariableField) {
        ordinaryVariableField = $(ordinaryVariableField);

        var fieldValue = '';

        if (ordinaryVariableField.is('[readOnlyVar]')) {
            fieldValue = ordinaryVariableField.text();
        }
        else {
            fieldValue = ordinaryVariableField.val();
        }

        return fieldValue.match(/%/) != null;
    };
    MOQEquationFieldWidget.MOQCalculationSuccess = function (results) {
        GenSession.ShowLoadingBox();

        if (results.Status) {
            MOQEquationFieldWidget.SetMOQResult(results.Result);

            if (MOQEquationFieldWidget.SavePending) {
                MOQEquationFieldWidget.SavePending = false;
                MOQEquationFieldWidget.MOQCalculating = false;
                MOQEquationFieldWidget.CallSaveManually();
            }

            // MOQ equation has been changed, the change has been validated, and the new hours total (<result>) has been determined - OK to apply recalculations
            var scope = angular.element(document.querySelector("#TaskElementsComposite")).scope();
            scope.moqUpdated(MOQEquationFieldWidget.initialLoad);
            scope.$apply();
        }
        else {
            MOQEquationFieldWidget.MOQCalculationError(results.Message);

            MOQEquationFieldWidget.SavePending = false;
        }

        // if the moqEquation is empty, resource types/spreads will be calculated as if the equation  = 0, but we don't want to show the result as 0 to the user 
        var moqEquation = MOQEquationFieldWidget.GetInputEquation();
        if (moqEquation.length == 0) {
            MOQEquationFieldWidget.SetMOQResult("");
        }
        MOQEquationFieldWidget.MOQCalculating = false;
        MOQEquationFieldWidget.initialLoad = false;

        GenSession.HideLoadingBox();
    };
    MOQEquationFieldWidget.MOQCalculationError = function (exceptionstring) {
        MOQEquationFieldWidget.SetMOQError("MOQ Calculation Error", exceptionstring);
        MOQEquationFieldWidget.ShowMOQError();

        MOQEquationFieldWidget.SavePending = false;
        MOQEquationFieldWidget.MOQCalculating = false;
        MOQEquationFieldWidget.initialLoad = false;

        GenSession.HideLoadingBox();
    };
    MOQEquationFieldWidget.SetMOQResult = function (result) {
        $("#MOQEquationField #equals").text(Helper.addCommas(result));
        $(document).trigger('MOQEquationReady');
    };
    MOQEquationFieldWidget.CallSaveManually = function () {
        $(document).trigger('SaveBOEUpdatesAndClose');
    };
    MOQEquationFieldWidget.FilterFieldToIntegers = function (field) {
        field.value = field.value.replace(/[^+\-\,\.0-9]/g, '');
    };
    MOQEquationFieldWidget.OpenSumOfBOEsByWBS = function (event, eventData) {
        ShowLoadingBox();
        $.ajax({
            type: 'POST',
            url: openSumOfBoesByWbsUrl,
            success: function (response) {
                HideLoadingBox();
                $('#VariableSumOfBOEsByWBSDialogContainer').html(response);
                $(document).trigger(event, eventData);

            },
            error: function () {
                HideLoadingBox();
            }
        });
    };
    MOQEquationFieldWidget.OpenSumOfBOEsByCLIN = function (event, eventData) {
        ShowLoadingBox();
        $.ajax({
            type: 'POST',
            url: openSumOfBoesByClinUrl,
            success: function (response) {
                HideLoadingBox();
                $('#VariableSumOfBOEsByCLINDialogContainer').html(response);
                $(document).trigger(event, eventData);
            },
            error: function () {
                HideLoadingBox();
            }
        });
    };
    MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent = function (event, eventData) {
        var VariableBOESumByWBSDialog = $('#VariableBOESumByWBS');

        if (VariableBOESumByWBSDialog == undefined || VariableBOESumByWBSDialog.length == 0) {
            // if the WBS dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByWBS function)
            MOQEquationFieldWidget.OpenSumOfBOEsByWBS(event, eventData);
        }
    };
    MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent = function (event, eventData) {
        var VariableBOESumByCLINDialog = $('#VariableBOESumByCLIN');

        if (VariableBOESumByCLINDialog == undefined || VariableBOESumByCLINDialog.length == 0) {
            // if the CLIN dialog has not been created yet, then first create it (the event will be retriggered in the OpenSumOfBOEsByCLIN function)
            MOQEquationFieldWidget.OpenSumOfBOEsByCLIN(event, eventData);
        }
    };


    MOQEquationFieldWidget.AfterDomLoad = function (taskElementDetailsWidget, showMoqQuestions, numberOfMoqQuestions) {
        taskElementDetailsWidget.ChildWidgets.push(MOQEquationFieldWidget);
        taskElementDetailsWidget.registerForDelegateEvent('click', '.menu-icon', function (event) {
            if ($('#menu-options-box').hasClass("display-none")) {
                $('#menu-options-box').removeClass('display-none');
            }
            else {
                $('#menu-options-box').addClass('display-none');
            }
        });
        taskElementDetailsWidget.registerForDelegateEvent('click', '#menu-options-box', function (event) {
            $('#menu-options-box').addClass('display-none');
            event.stopPropagation();
        });

        taskElementDetailsWidget.registerForDelegateEvent('change', '.moqRteFieldContainer input, .tableData textarea', function (event) {
            MOQEquationFieldWidget.setDirty();
        });

        taskElementDetailsWidget.CheckToShowMetrics();
        taskElementDetailsWidget.MOQText = CreateRteTemplate(showMoqQuestions, numberOfMoqQuestions);

        if (!MOQEquationFieldWidget.isReadOnly() || shouldMoqReadOnlyBeReversed || !taskElementDetailsWidget.isReadOnly()) {
            InitializeRteTemplate(taskElementDetailsWidget.MOQText, 'MOQText', rteFieldSize);
        }
        else {
            setTimeout(function () {
                HandleRTETemplateDataForReadOnly(TaskElementDetailsWidget.MOQText, 'MOQText');
            }, 1);
        }

        MOQEquationFieldWidget.InitializeDialog(MOQEquationFieldWidget.ReOrderMoqTypesDialog);
        MOQEquationFieldWidget.InitializeDialog(MOQEquationFieldWidget.ReOrderMoqTablesDialog);

        $(document).trigger('MOQWidgetLoaded', "MOQEquationField");
        $(document).trigger('WidgetLoaded', "MOQEquationField");
    }

    /// Reordering MOQ Types
    MOQEquationFieldWidget.ReOrderMoqTypesDialog = {};
    MOQEquationFieldWidget.ReOrderMoqTypesDialog.Element = $("#ReOrderMoqTypesDialog");
    MOQEquationFieldWidget.ReOrderMoqTypesDialog.Params = { width: 600, height: 250, modal: true, resizable: false, draggable: true, title: 'Sort Moq Types' };
    /// END Reordering MOQ Types

    /// Reordering MOQ Tables
    MOQEquationFieldWidget.ReOrderMoqTablesDialog = {};
    MOQEquationFieldWidget.ReOrderMoqTablesDialog.Element = $("#ReOrderMoqTablesDialog");
    MOQEquationFieldWidget.ReOrderMoqTablesDialog.Params = { width: 600, height: 230, modal: true, resizable: false, draggable: true, title: 'Sort Moq Tables' };
    /// END Reordering MOQ Tables

    $("#MOQEquationField #MOQEquation").change(function () {
        $(document).trigger('ValidateMOQEquation');
    });
    $("#MOQEquationField #MOQEquation").change(MOQEquationFieldWidget.setDirty);

    // use change instead of focusout to catch all changes having to deal with the moq equation or variables. focusout was not catching changes on boe to sum variable in IE
    MOQEquationFieldWidget.registerForLiveEvent('change', "#MOQEquationField #MOQEquation-Variables input.variableInput", function () {
        MOQEquationFieldWidget.CalculateMOQResult();
    });
    MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables a[readOnlyVar]', function () {
        var eventData = {};
        var variable = undefined;

        if ($(this).is('[wsVar]')) {
            variable = MOQEquationFieldWidget.GetWorkspaceVariable($(this).attr('name'));

            if (variable == undefined) { return; }

            eventData.dialogTitle = 'Summed BOEs for ' + variable.WorkspaceVariableName;
        }
        else if ($(this).is('[ordVar]')) {
            variable = MOQEquationFieldWidget.GetOrdinaryVariable($(this).attr('name'));

            if (variable == undefined) { return; }

            eventData.dialogTitle = 'Summed BOEs for ' + variable.OrdinaryVariableName;
        }

        eventData.BOEToSum = variable.BOEToSum;
        eventData.WBSToSum = variable.WBSToSum;
        eventData.CLINToSum = variable.CLINToSum;
        eventData.ResourceTypes = variable.ResourceTypes;
        eventData.SortBOEBy = variable.SortBOEBy;
        eventData.ReadOnly = true;

        if (eventData.SortBOEBy == sortBOEByClin) {
            $(document).trigger('VariableBOESumByCLIN_OpenReadOnly', eventData);
        }
        else {
            $(document).trigger('VariableBOESumByWBS_OpenReadOnly', eventData);
        }
    });
    MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables a.sumVariable', function () {
        var pkid = $(this).attr('var');
        var selectedInput = $(this).siblings('input[ordVar][pkid=' + pkid + ']');

        var eventData = {};
        eventData.pkid = pkid;
        eventData.BOEToSum = JSON.parse(selectedInput.attr('BOEToSum'));
        eventData.WBSToSum = JSON.parse(selectedInput.attr('WBSToSum'));
        eventData.CLINToSum = JSON.parse(selectedInput.attr('CLINToSum'));
        eventData.ResourceTypes = JSON.parse(selectedInput.attr('ResourceTypes'));
        eventData.SortBOEBy = selectedInput.attr('SortBOEBy');
        eventData.ReadOnly = false;

        if (eventData.BOEToSum.length || eventData.WBSToSum.length || eventData.CLINToSum.length) {
            if (eventData.SortBOEBy == sortBOEByClin) {
                $(document).trigger('VariableBOESumByCLIN_Open', eventData);
            } else {
                $(document).trigger('VariableBOESumByWBS_Open', eventData);
            }
        }
        else {
            if (eventData.SortBOEBy == sortBOEByClin) {
                $(document).trigger('VariableBOESumByCLIN_OpenNew', eventData);
            } else {
                $(document).trigger('VariableBOESumByWBS_OpenNew', eventData);
            }
        }
    });
    MOQEquationFieldWidget.registerForLiveEvent('click', '#MOQEquationField #MOQEquation-Variables span.clearVariable a', function () {
        var pkid = $(this).parent().attr('var');

        $(this).parents('#MOQEquationField #MOQEquation-Variables')
            .find('input[pkid=' + pkid + ']')
            .attr('BOEToSum', '[]')
            .attr('WBSToSum', '[]')
            .attr('CLINToSum', '[]')
            .attr('ResourceTypes', '[]')
            .attr('SortBOEBy', sortBOEByWBS)
            .prop('disabled', false)
            .val('')
            .change()
            .siblings('span.clearVariable[var=' + pkid + ']')
            .remove();
    });

    MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Create', function (event, eventData) {
        MOQEquationFieldWidget.setDirty();

        var variableInput = $('#MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');

        variableInput
            .val(Helper.addCommas(eventData.Sum))
            .attr('BOEToSum', JSON.stringify(eventData.BOEToSum))
            .attr('WBSToSum', '[]')
            .attr('CLINToSum', JSON.stringify(eventData.CLINToSum))
            .attr('ResourceTypes', JSON.stringify(eventData.ResourceTypes))
            .attr('SortBOEBy', eventData.SortBOEBy)
            .prop('disabled', true)
            .change();

        if (!variableInput.siblings('span.clearVariable[var=' + eventData.pkid + ']').length) {
            variableInput
                .parent()
                .append(' <span class="clearVariable" var="' + eventData.pkid + '">|<a>Clear</a></span>');
        }

        $(document).trigger('ValidateMOQEquation');
    });
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Create', function (event, eventData) {
        MOQEquationFieldWidget.setDirty();

        var variableInput = $('#MOQEquationField #MOQEquation-Variables input[pkid=' + eventData.pkid + ']');

        variableInput
            .val(Helper.addCommas(eventData.Sum))
            .attr('BOEToSum', JSON.stringify(eventData.BOEToSum))
            .attr('WBSToSum', JSON.stringify(eventData.WBSToSum))
            .attr('CLINToSum', '[]')
            .attr('ResourceTypes', JSON.stringify(eventData.ResourceTypes))
            .attr('SortBOEBy', eventData.SortBOEBy)
            .prop('disabled', true)
            .change();

        if (!variableInput.siblings('span.clearVariable[var=' + eventData.pkid + ']').length) {
            variableInput
                .parent()
                .append(' <span class="clearVariable" var="' + eventData.pkid + '">| <a>Clear</a></span>');
        }

        $(document).trigger('ValidateMOQEquation');
    });
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_Open', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenNew', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByWBS_OpenReadOnly', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByWBSEvent);
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_Open', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenNew', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
    MOQEquationFieldWidget.registerForEvent('VariableBOESumByCLIN_OpenReadOnly', MOQEquationFieldWidget.InterceptVariableSumOfBOEsByCLINEvent);
    MOQEquationFieldWidget.registerForEvent('ValidateMOQEquation', function (event, options) {
        if (options != undefined && options.initialLoad != undefined && options.initialLoad != null) {
            MOQEquationFieldWidget.initialLoad = options.initialLoad;
        } else {
            MOQEquationFieldWidget.initialLoad = false;
        }

        MOQEquationFieldWidget.ValidateMOQEquation();
    });
    MOQEquationFieldWidget.registerForEvent('InsertMOQElementDialogClosing', MOQEquationFieldWidget.InsertMOQElementDialogClosing);

    if (MOQEquationFieldWidget_ReadOnly) {
        $('.magnifier-button').addClass('display-none');
    }

    return MOQEquationFieldWidget;
}
