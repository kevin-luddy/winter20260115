// The controller for the Edit Section modal.
angular.module('RDM').controller('EditSectionController', ['$scope', '$uibModalInstance', '$rootScope', 'model', function ($scope, $uibModalInstance, $rootScope, model) {
    $scope.model = model;
    $rootScope.modalErrors = [];

    $scope.uncheckRequiredIfInternal = function () {
        if (model.IsInternalSection) {
            model.IsRdsbRequired = false;
        }
	};

	$scope.uncheckIsDisclosureStatementAdequate = function () {
		if (document.getElementById("SectionContainsCasbDisclosureCheckbox").checked == false) {
			model.IsDisclosureStatementAdequate = false;
			document.getElementById("IsDisclosureStatementAdequateCheckbox").checked = false;
			
		}
	};

	$scope.uncheckNonComplianceNotification = function () {
		if (document.getElementById("SectionContainsNonComplianceCheckbox").checked == false) {
			model.NonComplianceNotification = false;
			document.getElementById("NonComplianceNotificationCheckbox").checked = false;
		}
	};
}]);