(function () {
    'use strict';

    var rpmApp = angular.module('RPM', [
        // Custom modules 
        'gen.directives',
        // 3rd Party Modules
        'chart.js', 'angularMoment', 'gantt', 'gantt.table', 'ngCookies'
    ]);

    rpmApp.config(['$httpProvider', function ($httpProvider) {
        // Include this header with all ajax calls made by $http.  This allows "Request.IsAjaxRequest()" to work on the server.
        $httpProvider.defaults.headers.common['X-Requested-With'] = 'XMLHttpRequest';
    }]);

    rpmApp.filter('true_false', function() {
        return function(text, length, end) {
            if (text) {
                return 'Yes';
            }
            return 'No';
        }
    });

    rpmApp.directive('jqdatepicker', function() {
        return {
            restrict: 'A',
            require: 'ngModel',
            link: function(scope, element, attrs, ctrl) {
                $(element).datepicker({
                    changeMonth: true,
                    changeYear: true,
                    dateFormat: 'mm/dd/yy',
                    onSelect: function(date) {
                        ctrl.$setViewValue(date);
                        ctrl.$render();
                        scope.$apply();
                    }
                });
            }
        };
    });

    rpmApp.directive('multiselectpicker', function () {
        return {
            link: function (scope, element, attrs) {
                element.multiselect({
                    selectedList: 1,
                    header: true,
                    classes: 'multiselectpicker',
                    height: 300,
                    noneSelectedText: 'All',
                    onChange: function(optionElement, checked) {
                        if (optionElement != null) {
                            optionElement.removeAttr('selected');

                            if (checked) {
                                optionElement.prop('selected', 'selected');
                            }
                        }
                        element.change();
                    }
            });
 
        // Watch for any changes to the length of our select element
        scope.$watch(function () {
            return element[0].length;
        }, function () {
            scope.$applyAsync(element.multiselect('refresh'));
        });
 
        // Watch for any changes from outside the directive and refresh
        scope.$watch(attrs.ngModel, function () {
            element.multiselect('refresh');
        });
 
    }
 
    };
});

})();