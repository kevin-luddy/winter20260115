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
            '<ul class="validation-box validation-directive" ng-show="errors.length">' +
                '<li ng-repeat="error in errors">' +
                    '<div ng-bind-html="displayError(error.ValidationIssue)"></div>' +
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
* Displays a user lookup text box and lookup link
*
* @example <gen-user-lookup data-ntids="ntids" data-readonly="false"></gen-user-lookup>
* 
* @param {string} ntids - ntids from the AD lookup
* @param {bool} readonly - true to put the control in readonly mode.
*/
.directive('genUserLookup', function () {
    return {
        restrict: 'A, E',
        replace: true,
        transclude: false,
        scope: {
            ntids: '=',
            readonly: '='
        },
        template: '<div><input class="ad-lookup-box" ng-model="ntids" ng-disabled="!readonly" />' +
                   '<a ng-click="lookup()" ng-hide="!readonly" title="Lookup">Lookup...</a></div>',
        link: function (scope, element, attrs) {
            scope.lookup = function () {
                ActiveDirectorySearchDialog(function (results) {
                    scope.$apply(function () {
                        var selectedAccountName = results.AccountName;
                        if (selectedAccountName != '') {

                            if ($.trim(scope.ntids).length == 0) {
                                scope.ntids = selectedAccountName;
                            } else {
                                // If there isn't already a semicolon on the end of the user/group string, we'll add one
                                if (!$.trim(scope.ntids).endsWith(';')) {
                                    scope.ntids += ';';
                                }
                                scope.ntids += selectedAccountName;
                            }
                        }
                    });
                });
            };

            scope.$on('$destroy', function () { });
        }
    };
})

/**
* Wraps the jQuery UI dialog in a directive.
* 
* @example <gen-dialog><dialog content></gen-dialog>
*
* @param {bool} [autoOpen=false] - Auto open on declaration.
* @param {string} title
* @param [width=600] 
* @param [height=auto]
* @param {string} [show] - Animation when the dialog is opened.
* @param {string} [hide] - Animation when the dialog is closed.
* @param {bool} [draggable=true]
* @param {bool} [resizable=false]
* @param {bool} [closeOnEscape=false]
* @param {bool} open - bi-directional boolean to open and close the dialog.
* @param {bool} [focusOnOpen=true] - Focus into the first visible and enabled input.
* @param {function} [onOpen] - Method to call when the dialog opens.
* @param {function} [onClose] - Method to call when the dialog closes.
* @param {function} [onEnter] - Method to call when the user hits the enter key.
*/
.directive('genDialog', ['$timeout', function ($timeout) {
    return {
        restrict: 'A, E',
        replace: false,
        transclude: true,
        scope: {
            autoOpen: '@',
            title: '@',
            width: '@',
            height: '@',
            show: '@',
            hide: '@',
            draggable: '@',
            resizable: '@',
            closeOnEscape: '@',
            open: '=',
            focusOnOpen: '@',
            onOpen: '&',
            onClose: '&',
            onEnter: '&'
        },
        template: '<div ng-transclude></div>',
        link: function (scope, element, attrs) {
            var exists = false;
            attrs.focusOnOpen = attrs.focusOnOpen ? attrs.focusOnOpen : true;

            // by default we want to hide the dialog window. this line will set the display attribute to none
            // upon each creation of this dialog directive
            element.css('display', 'none');

            // set parent's $.open to false when the dialog is closed.
            var onClose = function () {
                if (scope.open === true) {
                    scope.$apply(function () {
                        scope.open = false;
                    });
                    // prevent the onClose method from firing twice by preventing this close 
                    // and relying on the scope.open observer to perform the close.
                    return false;
                }
            };

            var dialogOptions = {
                autoOpen: attrs.autoOpen ? attrs.autoOpen : false,
                title: attrs.title,
                width: attrs.width || 600,
                height: attrs.height || 'auto',
                modal: attrs.modal ? attrs.modal : true,
                show: attrs.show || null,
                hide: attrs.hide || null,
                draggable: attrs.draggable ? attrs.draggable : true,
                resizable: attrs.resizable ? attrs.resizable : false,
                closeOnEscape: attrs.closeOnEscape ? attrs.closeOnEscape : false,
                beforeClose: onClose,
                close: scope.onClose || null,
                open: scope.onOpen || null
            };

            // Initialize the element as a dialog
            // Timeout is required to support multiple dialogs
            $timeout(function () {
                element.dialog(dialogOptions);
                exists = true;
            }, 0);

            // watch the parent scope's open property to open and close the dialog.
            scope.$watch('open', function (val) {
                if (exists) {
                    if (val === true) {
                        element.dialog('open');

                        // focus on open
                        var inputs = element.find(':input:visible:enabled');
                        if (attrs.focusOnOpen === true && inputs.length > 0) {
                            inputs[0].focus();
                        }
                    }
                    else {
                        element.dialog('close');
                    }
                }
            });

            // This allows title to bind and change.
            attrs.$observe('title', function (val) {
                if (exists) {
                    element.dialog('option', 'title', val);
                }
            });

            // Setup the onEnter function.
            if (scope.onEnter) {
                element.on('keydown', function (e) {
                    var keyCode = (e.keyCode ? e.keyCode : (e.which ? e.which : e.charcode));
                    if (keyCode == 13) {
                        scope.onEnter();
                    }
                });
            }

            // Destory the dialog on directive destruction.
            scope.$on('$destroy', function () {
                element.dialog('destroy');
            });
        }
    };
}])

/**
* Directive for the common error dialog.
* 
* @example <gen-error-dialog></gen-error-dialog>
*
* @param {string} text
* @param {string} details - Animation when the dialog is opened.
* @param {bool} open - bi-directional boolean to open and close the dialog.
*/
.directive('genErrorDialog', ['$sce', function ($sce) {
    return {
        restrict: 'A, E',
        replace: true,
        scope: {
            open: '=',
            text: '@',
            details: '@'
        },
        template:
            '<div>' + // required since this and gen-dialog both need isolate scope, using <div gen-dialg> instead of <gen-dialog> because IE8 breaks.
                '<div gen-dialog style="display: none; text-align: center;" data-width="300", data-title="Application Error" data-open="open" data-focus-on-open="false">' +
                    '<div class="dialog-text" ng-bind-html="displayText()"></div>' +
                    '<div ng-init="animate.visible=false">' + 
                        '<span class="underline pointer" ng-click="animate.visible=!animate.visible">Error Details</span>' +
                        '<div class="vertical-slide error-details" ng-bind-html="displayDetails()" ng-hide="!animate.visible" style="margin-bottom: 0px"></div>' +
                    '</div>' +
                    '<div class="error-buttons" style="margin-top: 10px">' +
                        '<button type="button" ng-click="closeDialog()">OK</button>' +
                    '</div>' +
                '</div>' +
            '</div>',
        link: function (scope, element, attrs) {
            scope.closeDialog = function () {
                scope.open = false;
            };

            scope.displayText = function () {
                return $sce.trustAsHtml(scope.text);
            };

            scope.displayDetails = function () {
                return $sce.trustAsHtml(scope.details);
            };
        }
    };
}])

/**
* Directive for a submit button (any button that requires a loading image).
* 
* @example <gen-submit-button></gen-submit-button>
*
* @param {css} any css classes to add to the button
* @param {function} click - the function to call on click
*/
.directive('genSubmitButton', ['$q', function ($q) {
    return {
        restrict: 'A, E',
        replace: true,
        transclude: true,
        scope: {
            css: '@',
            click: '&'
        },
        template: '<button type="button" class="{{ css }}" ng-transclude ng-click="load()">',
        link: function (scope, element, attrs) {
            scope.load = function () {
                element.addClass('loader');
                element.prev('css3pie').addClass('display-none');
                var defer = $q.defer();

                // When the promise is resolved, this code runs to remove the loader and display the background in ie.
                defer.promise.then(function () {
                    element.removeClass('loader');
                    element.prev('css3pie').removeClass('display-none');
                });

                // pass the promise to the click
                scope.click({ result: defer });
            };
        }
    };
}])

/**
* Wraps the jQuery UI datepicker in a directive.
* 
* @example <gen-datepicker></gen-datepicker>
*/
.directive('genDatepicker', function () {
})


/**
* Directive for paging
* 
* @example <div paging></div>
*
* @param {int} numPages - total number or pages to be displayed
* @param {int} currentPage - currently displayed page of results
*/
.directive('genpaging', function ($sce) {
    return {
        restrict: "A",
        replace: true,
        scope: {
            numPages: '@',
            currentPage: '='
        },
        template: '<div class="paging">' +
                    '<a ng-class="{\'current-page\': currentPage==0, page:currentPage!=0}" ng-click="pageNumClicked(0)")>1</a>' +
                    '<span class="elipsis" ng-show="showFirstElipsis()">...</span>' +
                    '<a ng-class="{\'current-page\': currentPage==page, page:currentPage!=page}" ng-repeat="page in getPageNumbers(numPages)" ng-click="pageNumClicked(page)">{{ page + 1 }}</a>' +
                    '<span class="elipsis" ng-show="showSecondElipsis()">...</span>' +
                    '<a ng-class="{\'current-page\': currentPage==numPages-1, page:currentPage!=numPages-1}" ng-show="showLastPageNum()" ng-click="clickLastPage()">{{numPages}}</a>' +
                  '</div>',
        link: function (scope, element, attrs) {
            scope.firstElipsis = false;
            scope.secondElipsis = false;

            scope.showLastPageNum = function () {
                return (scope.numPages > 1 && scope.currentPage != scope.numPages-1);
            };
            scope.showFirstElipsis = function () {
                return scope.firstElipsis;
            };
            scope.showSecondElipsis = function () {
                return scope.secondElipsis;
            };

            scope.pageNumClicked = function (page) {
                scope.currentPage = page;
            };

            scope.clickLastPage = function () {
                scope.currentPage = scope.numPages - 1;
            };

            scope.getPageNumbers = function (n) {
                scope.firstElipsis = false;
                scope.secondElipsis = false;
                var pagenumbers = [];
                var currentPage = scope.currentPage;
                var numPages = scope.numPages;
                if (currentPage < 7) {
                    for (var i = 1; i < currentPage; i++) {
                        pagenumbers.push(i);
                    }
                }
                else {
                    scope.firstElipsis = true;
                    for (var i = currentPage - 4; i < currentPage; i++) {
                        pagenumbers.push(i);
                    }
                }
                if (currentPage > 0 && currentPage < numPages) {
                    pagenumbers.push(currentPage);
                }

                if (numPages < currentPage + 6) {
                    for (var i = currentPage + 1; i < numPages-1; i++) {
                        pagenumbers.push(i);
                    }
                }
                else {
                    for (var i = currentPage + 1; i < currentPage + 4; i++) {
                        pagenumbers.push(i);
                    }
                    scope.secondElipsis = true;
                }
                return pagenumbers;
            };
        }
    };
})

/**
* Directive for creating a Pie Chart for metrics
* 
* @example <metrics-pie-chart model="model" type="Org"/>
*
*/
.directive('metricsPieChart', function () {
    return {
        templateUrl: '/app/views/MetricsPieChart.html',
        controller: 'MetricsPieChartController',
        scope: {
            model: '=',
            type: '@'
        }
    };
})

/**
* Directive for creating a stacked Bar Chart for metrics
* 
* @example <metrics-bar-chart model="model"/>
*
*/
.directive('metricsBarChart', function () {
    return {
        templateUrl: '/app/views/MetricsBarChart.html',
        controller: 'MetricsBarChartController',
        scope: {
            model: '='
        }
    };
})

/**
* Directive for creating a ROS Bar Chart for metrics
* 
* @example <metrics-ros-bar-chart model="model"/>
*
*/
.directive('metricsRosBarChart', function () {
    return {
        templateUrl: '/app/views/MetricsROSBarChart.html',
        controller: 'MetricsROSBarChartController',
        scope: {
            model: '='
        }
    };
})