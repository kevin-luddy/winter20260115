// The controller for the Edit Content modal.
angular.module("RDM").controller("EditMappingsController", 
    ["$scope", "$uibModalInstance", "$rootScope", "model", "rateModel",
    function ($scope, $uibModalInstance, $rootScope, model, rateModel) {
        $scope.model = model;
        $scope.rateModel = rateModel;
        $scope.modalErrors = [];
        $scope.ResourceTypeLabor = rateModel.ResourceTypeLabor;
        $scope.canEdit = function () { return rateModel.lockInfo !== undefined && !rateModel.lockInfo.IsReadOnly; };
        // save starting Section
        $scope.originalSection = model.S;

        // Event handlers
        $scope.onResourceTypeChange = function () {
            if (model.RT !== $scope.ResourceTypeLabor) {
                model.Gen = false;
            }
        };

        // validate mappings data, show error messages if invalid or update mappings on main rate object if ok
        $scope.editMappings = function (content) {
            $scope.isDataValid = true;
            if (content !== undefined) {
                if (content.RT === $scope.ResourceTypeLabor && content.Gen) {
                    content.RD = "";
                    content.RCId = 0;
                } else {
                    content.RD1 = "";
                    content.RD2 = "";
                    content.RD3 = "";
                    content.RD4 = "";
                    content.RD5 = "";
                    content.RD6 = "";
                    content.RD7 = "";
                    content.RCId1 = 0;
                    content.RCId2 = 0;
                    content.RCId3 = 0;
                    content.RCId4 = 0;
                    content.RCId5 = 0;
                    content.RCId6 = 0;
                    content.RCId7 = 0;
                }

                // Warn user if ProPricer mappings are empty.
                var warnings = "";
                if (content.S === 0 && $scope.originalSection !== 0) {
                    warnings += "Linked Section was cleared.<br />";
                }
                if ($scope.isProPricerMappingCleared(content)) {
                    warnings += "No values entered for ProPricer Direct Rate Mappings.<br />";
                }
                if (warnings !== "") {
                    ConfirmDialog("Warning", "The following mapping(s) were cleared or are empty:<br />" + warnings + "Continue?", function () {
                        // Yes
                        $scope.updateMappings(content);
                    });
                } else {
                    $scope.editProPricerMappings(content);
                }
            };
        };

        // validate ProPricer mappings
        $scope.editProPricerMappings = function(content) {
            var errMsg = "";

            // require Commercial and Government burden pools to be both populated, or both empty
            if (!($scope.isGovernmentAndCommercialBurdenPoolPopulated(content) ||
                $scope.isGovernmentAndCommercialBurdenPoolEmpty(content))) {
                errMsg +=
                    "Commercial and Government Burden Pool selections must be both populated, or both empty.\n";
            }
            if (!content.Ra || content.Ra < 1) { // rate type selection required
                errMsg += "Rate Type Selection Required.\n";
            }
            if (!content.RT || content.RT < 1) { // Resource type selection required
                errMsg += "Resource Type Selection Required.\n";
            }
            if (!$scope.isOneOrMoreRateDescriptionsPopulated(content)) {
                errMsg += "A description is required.\n";
            }

            if (errMsg.length > 0) {
                var error = {};
                error.Title = "Error Saving ProPricer Direct Rate Mapping";
                error.Message = "\n" + errMsg;
                DisplaySimpleExceptionDialog(error);
            } else {
                // one last confirmation before saving the mappings
                if ($scope.isGovernmentAndCommercialBurdenPoolEmpty(content)) {
                    ConfirmDialog("Confirm No Burden Pool Mappings", "Are you sure you want to leave Government and Commercial Burden Pool fields blank? Burden pool fields may only be blank for the following Resources: ******NL, Mileage, NLBESCCH, Travel Escalation, Travel Factor, or TRAVLESC.<br />Press \"Yes\" to continue and leave the burden pools blank or \"No\" to return to the burden pool mapping dialog to add the appropriate burden pools.", function () {
                        // Yes
                        $scope.updateMappings(content);
                    });
                } else {
                    $scope.updateMappings(content);
                }
            }
        }

        // update rateModel with current mapping values
        $scope.updateMappings = function(content) {
            for (var i = 0; i < $scope.rateModel.data.length; i++) {
                if ($scope.rateModel.data[i].Id === content.Id) {
                    content.D = true;
                    $scope.rateModel.data[i] = content;
                    break;
                }
            }
            $uibModalInstance.close($scope.rateModel);
        }

        $scope.isGovernmentOrCommercialBurdenPoolPopulated = function(content) {
            return ((content.CBPId && content.CBPId > 0) ||
                (content.GBPId && content.GBPId > 0));
        };

        $scope.isGovernmentAndCommercialBurdenPoolPopulated = function (content) {
            return ((content.CBPId && content.CBPId > 0) &&
                (content.GBPId && content.GBPId > 0));
        }

        $scope.isGovernmentAndCommercialBurdenPoolEmpty = function(content) {
            return ((!content.CBPId || content.CBPId < 1) &&
                (!content.GBPId || content.GBPId < 1));
        };

        $scope.isOneOrMoreRateDescriptionsPopulated = function (content) {
            var result = content.RD ||
                content.RD1 ||
                content.RD2 ||
                content.RD3 ||
                content.RD4 ||
                content.RD5 ||
                content.RD6 ||
                content.RD7;
            return result;
        };

        $scope.isProPricerMappingCleared = function (content) {
            return (!content.RT || content.RT < 1) &&                       // Resource Type not selected
                (!content.Ra || content.Ra < 1) &&                          // Rate Type not selected                
                !$scope.isOneOrMoreRateDescriptionsPopulated(content) &&    // No descriptions entered
                $scope.isGovernmentAndCommercialBurdenPoolEmpty(content);   // No burden pools selected
        }
    }]);


