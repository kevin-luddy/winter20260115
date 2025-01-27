// The controller for the Edit Section modal.
angular.module('RDM').controller('EditSectionController', ['$scope', '$uibModalInstance', '$rootScope', 'model', function ($scope, $uibModalInstance, $rootScope, model) {
    $scope.model = model;
	$rootScope.modalErrors = [];
	$scope.showWarnings = false;

	if (model.Id == -1) {
		model.SectionContainsCasbDisclosureCore = false;
		model.SectionContainsNonComplianceCore = false;
	}

    $scope.uncheckRequiredIfInternal = function () {
        if (model.IsInternalSection) {
            model.IsRdsbRequired = false;
        }
	};

	$scope.uncheckIsDisclosureStatementAdequateCore = function () {
		if (document.getElementById("SectionContainsCasbDisclosureCoreCheckbox").checked == false) {
			model.IsDisclosureStatementAdequate = false;
			document.getElementById("IsDisclosureStatementAdequateCoreCheckbox").checked = false;
		}
	};

	$scope.uncheckNonComplianceNotificationCore = function () {
		if (document.getElementById("SectionContainsNonComplianceCoreCheckbox").checked == false) {
			model.NonComplianceNotification = false;
			document.getElementById("NonComplianceNotificationCoreCheckbox").checked = false;
		}
	};

	$scope.uncheckIsDisclosureStatementAdequateService = function () {
		if (document.getElementById("SectionContainsCasbDisclosureServiceCheckbox").checked == false) {
			model.IsDisclosureStatementAdequate = false;
			document.getElementById("IsDisclosureStatementAdequateServiceCheckbox").checked = false;
		}
	};

	$scope.uncheckNonComplianceNotificationService = function () {
		if (document.getElementById("SectionContainsNonComplianceServiceCheckbox").checked == false) {
			model.NonComplianceNotification = false;
			document.getElementById("NonComplianceNotificationServiceCheckbox").checked = false;
		}
	};

	$scope.showPublishErrors = function () {
		$scope.showWarnings = true;
	}
}]);