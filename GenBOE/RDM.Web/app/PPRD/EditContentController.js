// The controller for the Edit Content modal.
angular.module('RDM').controller('EditContentController', ['$scope', '$uibModalInstance', '$rootScope', 'utilityService', 'model', 'PPRDModel', 'newNode', function ($scope, $uibModalInstance, $rootScope, utilityService, model, PPRDModel, newNode) {
    $scope.model = model;
    $scope.PPRDModel = PPRDModel;
    $scope.newNode = newNode;
    $rootScope.modalErrors = [];
    
    loadTinyMceOptions = function () {
        var options = utilityService.getTinyMceOptions();
        options.toolbar = "table | bold italic underline forecolor backcolor | alignleft aligncenter alignright alignjustify | bullist numlist outdent indent | undo redo | removeformat | paste fullscreen | code | addPageBreak removePageBreaks";
        
        return options;
    }
    
    $scope.tinyMceOptions = loadTinyMceOptions();

}]);