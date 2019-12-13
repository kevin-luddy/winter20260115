angular.module('gen.directives', [])

/** 
* Displays a validation box including the given error array, and hides the box when no errors exist.
*
* @example <gen-validation data-errors="errors"></gen-validation>
* 
* @param {array} errors - Array of errors objects display.  Each error must contain a message property.
*/
.directive('genValidation', ['$sce', function ($sce) {
    return {
        restrict: 'A, E',
        replace: true,
        transclude: false,
        scope: { errors: '=' },
        template:
            '<ul class="validation-box validation-directive" data-ng-show="errors.length">' +
                '<li data-ng-repeat="error in errors">' +
                    '<div data-ng-bind-html="displayError(error.ValidationIssue)"></div>' +
                '</li>' +
            '</ul>',
        link: function (scope, element, attrs) {

            scope.displayError = function (val) {
                return $sce.trustAsHtml(val);
            };

            scope.$on('$destroy', function () { });
        }
    };
}])

/**
* Directive for opening the bootstrap datepicker to select a date in MM/DD/YY format.
* From example: ui-grid-edit-datepicker with Angular UI Bootstrap 1.2.4 without jQuery - http://embed.plnkr.co/KHGb6BU0KAHEVfPlxnzU/
*/
.directive('uiGridEditDatepicker', ['$timeout', '$document', 'uiGridConstants', 'uiGridEditConstants', function($timeout, $document, uiGridConstants, uiGridEditConstants) {
    return {
        template: function(element, attrs) {	
            var html = '<div class="datepicker-wrapper" ><input type="text" uib-datepicker-popup datepicker-append-to-body="true" datepicker-options="datepickerOptions" is-open="isOpen" ng-model="datePickerValue" ng-change="changeDate($event)" popup-placement="auto top"/></div>';
            return html;
        },
        require: ['?^uiGrid', '?^uiGridRenderContainer'],
        scope: true,
        compile: function() {
            return {
                pre: function($scope, $elm, $attrs) {
                    if ($attrs.datepickerOptions){
                        if ($scope.col.grid.appScope[$attrs.datepickerOptions]){
                            $scope.datepickerOptions = $scope.col.grid.appScope[$attrs.datepickerOptions];
                        }
                    }
                },
                post: function($scope, $elm, $attrs, controllers) {
                    $scope.datePickerValue = new Date($scope.row.entity[$scope.col.field]);
                    $scope.isOpen = true;
                    var uiGridCtrl = controllers[0];
                    var renderContainerCtrl = controllers[1];

                    var onWindowClick = function (evt) {
                        var classNamed = angular.element(evt.target).attr('class');
                        if (classNamed) {
                            var inDatepicker = (classNamed.indexOf('datepicker-calendar') > -1);
                            if (!inDatepicker && evt.target.nodeName !== "INPUT") {
                                $scope.stopEdit(evt);
                            }
                        }
                        else {
                            $scope.stopEdit(evt);
                        }
                    };

                    var onCellClick = function (evt) {
                        angular.element(document.querySelectorAll('.ui-grid-cell-contents')).off('click', onCellClick);
                        $scope.stopEdit(evt);
                    };

                    $scope.changeDate = function (evt) {
                        $scope.row.entity[$scope.col.field] = $scope.datePickerValue;
                        $scope.stopEdit(evt);
                    };

                    $scope.$on(uiGridEditConstants.events.BEGIN_CELL_EDIT, function () {
                        if (uiGridCtrl.grid.api.cellNav) {
                            uiGridCtrl.grid.api.cellNav.on.navigate($scope, function (newRowCol, oldRowCol) {
                                $scope.stopEdit();
                            });
                        } else {
                            angular.element(document.querySelectorAll('.ui-grid-cell-contents')).on('click', onCellClick);
                        }
                        angular.element(window).on('click', onWindowClick);
                    });

                    $scope.$on('$destroy', function () {
                        angular.element(window).off('click', onWindowClick);
                        $('body > .dropdown-menu, body > div > .dropdown-menu').remove();
                    });

                    $scope.stopEdit = function(evt) {
                        $scope.$emit(uiGridEditConstants.events.END_CELL_EDIT);
                    };

                    $elm.on('keydown', function(evt) {
                        switch (evt.keyCode) {
                            case uiGridConstants.keymap.ESC:
                                evt.stopPropagation();
                                $scope.$emit(uiGridEditConstants.events.CANCEL_CELL_EDIT);
                                break;
                        }
                        if (uiGridCtrl && uiGridCtrl.grid.api.cellNav) {
                            evt.uiGridTargetRenderContainerId = renderContainerCtrl.containerId;
                            if (uiGridCtrl.cellNav.handleKeyDown(evt) !== null) {
                                $scope.stopEdit(evt);
                            }
                        } else {
                            switch (evt.keyCode) {
                                case uiGridConstants.keymap.ENTER:
                                case uiGridConstants.keymap.TAB:
                                    evt.stopPropagation();
                                    evt.preventDefault();
                                    $scope.stopEdit(evt);
                                    break;
                            }
                        }
                        return true;
                    });
                }
            };
        }
    };
}])


/**
* Directive to make the associated element scrollable. The element will be resized according to the window size.
* Attribute: data-trailing-elements-height="nn" specifies the height of the footer and any other trailing elements on the page.
* Notes: This code assumes there is only 1 scrollable element on a page.
*/
.directive('scrollableElement', ['$window', '$timeout', function ($window, $timeout) {
    return {
        restrict: 'A',
        link: function (scope, element, attr) {
            var $el = $(element[0]);
            var trailingElementsHeight = attr.trailingElementsHeight;
            var w = angular.element($window);
            scope.getWindowDimensions = function () {
                return {
                    'h': w.height(),
                    'w': w.width()
                };
            };

            /* Refresh the element size. */
            scope.refreshScrollableElementSize = function () {
                var elementTop = $el.offset().top;  // note: 0 if element is hidden or not found
                var newHeight = elementTop === 0 ? 0 : $window.innerHeight - elementTop - trailingElementsHeight;
                element.css({
                    'height': newHeight + 'px',
                    'margin-top': '5px',
                    'margin-bottom': '5px',
                    'overflow': 'auto'
                });
            };

            /* Refresh the element size after a brief delay, i.e. to allow the digest cycle to complete. */
            scope.refreshScrollableElementSizeWithDelay = function () {
                $timeout(function () {
                    scope.refreshScrollableElementSize();
                }, 100);
            };

            scope.$watch(scope.getWindowDimensions, function (newValue, oldValue) {
                scope.refreshScrollableElementSize();
            }, true);

            w.bind('resize', function () {
                scope.$apply();
            });
        }
    }
}]);

