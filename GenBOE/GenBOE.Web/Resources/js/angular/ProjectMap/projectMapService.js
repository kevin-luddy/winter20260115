(function () {
    'use strict';
    var app = angular.module('genboe');
    
    app.service('projectMapService', ['$http', function ($http) {
        var service = this;
        var workspaceName = window.location.pathname;

        service.loadProjectMapPagedData = function (pageNumber) {
            // adding time to string so $http.get ajax request is not cached by IE
            var time = new Date().getTime().toString();

            return $http({
                method: 'GET',
                url: workspaceName + '/Workspace/GetProjectMapData?page=' + pageNumber + '&t=' + time
            });
        }

        service.loadProjectMapData = function () {
            // adding time to string so $http.get ajax request is not cached by IE
            var time = new Date().getTime().toString();

            return $http({
                method: 'GET',
                url: workspaceName + '/Workspace/GetProjectMapData?t=' + time
            });
        }

        service.saveProjectMapGrid = function (projectMapData) {
            // adding time to string so $http.get ajax request is not cached by IE
            var time = new Date().getTime().toString();

            return $http({
                method: 'POST',
                url: workspaceName + '/Workspace/SaveProjectMapData?t=' + time,
                data: projectMapData
            });
        }
    }]);
})();