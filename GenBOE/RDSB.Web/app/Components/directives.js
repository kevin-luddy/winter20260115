angular.module('gen.directives', [])

/** 
* Displays a validation box including the given error array, and hides the box when no errors exist.
*
* @example <gen-validation data-errors="errors"></gen-validation>
* 
* @param {array} errors - Array of errors objects display.  Each error must contain a message property.
*/
.directive('genValidation', ['$sce', '$rootScope', function ($sce, $rootScope) {
    return {
        restrict: 'A, E',
        replace: true,
        transclude: false,
        scope: { errors: '=' },
        template:
            '<ul class="validation-box validation-directive" data-ng-show="$root.errors.length">' +
                '<li data-ng-repeat="error in $root.errors">' +
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
 * Directive for updating the group column header widths for the grid.  The result is that each group column width
 * will match the cumulative width of all columns that belong to its group.
 * 
 * Note:  This is a modified version of the example found here:  http://embed.plnkr.co/fsJdENoN1ll4FUGsPzts/ (see file super-col-with-update.js).
 * 
 * @example: <div group-column-width-update></div>
*/
.directive('groupColumnWidthUpdate', ['$timeout', '$rootScope', function ($timeout, $rootscope) {
    return {
        'restrict': 'A',
        'link': function (scope, element) {
            var groupColumnName = scope.col.colDef.groupColumn,
                columnHeader = jQuery(element);
            columnHeader.on('resize', function () {
                updateGroupColumnWidth();
            });

            // This stores an array of the objects that contain a mapping between a groupColumnName and an initial groupColumnWidth on the rootscope
            //  as this information needs to persist through all columns to appropriately calculate the total groupColumnWidth.
            var getInitialGroupColumnWidth = function (columnName, columnWidth) {
                // Try to find the initialWidth for the specified columnName.
                if ($rootscope.rdsbInitialGroupColumnWidths) {
                    for (i = 0; i < $rootscope.rdsbInitialGroupColumnWidths.length; i++) {
                        if ($rootscope.rdsbInitialGroupColumnWidths[i].groupName == columnName) {
                            return $rootscope.rdsbInitialGroupColumnWidths[i].initialWidth;
                        }
                    }
                } else {
                    // Create the array if it doesn't exist.
                    $rootscope.rdsbInitialGroupColumnWidths = [];
                }

                // We have a new column configuration to store.  Store the groupName and the initialWidth (which will always be the initial width since
                //  subsequent calls to this function will find the existing configuration and will not reach this section of code).
                var columnConfig = { groupName: columnName, initialWidth: columnWidth };
                $rootscope.rdsbInitialGroupColumnWidths.push(columnConfig);
                return columnWidth;
            };
            
            var updateGroupColumnWidth = function () {
                $timeout(function () {
                    var groupColumnHeader = jQuery('.ui-grid-header-cell[col-name="' + groupColumnName + '"]');
                    var groupColumnWidth = groupColumnHeader.outerWidth(),
                        columnWidth = columnHeader.outerWidth();

                    var initialGroupColumnWidth = getInitialGroupColumnWidth(groupColumnName, groupColumnWidth);

                    // If the groupColumnWidth is larger than where it started, add the new column's width.  Otherwise, set it to the column's width.  We could
                    //  not just compare to 0 because some columns have a non-zero initial width due to thicker borders.
                    if (groupColumnWidth > initialGroupColumnWidth) {
                        groupColumnWidth += columnWidth;
                    } else {
                        groupColumnWidth = columnWidth;
                    }
                  
                    // Set the width and show the group column header now that it has been appropriately resized.
                    groupColumnHeader.css({
                        'min-width': groupColumnWidth + 'px',
                        'max-width': groupColumnWidth + 'px',
                        'text-align': "center",
                        'display': ""
                    });
                }, 0);
            };
            updateGroupColumnWidth();
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
        template: '<button type="button" class="{{ css }}" data-ng-transclude data-ng-click="load()">',
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