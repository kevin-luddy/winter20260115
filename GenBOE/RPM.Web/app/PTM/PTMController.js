angular.module('RPM').controller('PTMController', ['$scope', '$http', '$filter', '$cookies', '$window', function ($scope, $http, $filter, $cookies, $window) {

    $scope.isPTMError = false;
    $scope.firstLoad = true;
    $scope.isPTMLoading = true;
    $scope.selectedPTM = null;
    $scope.hidePTMDetails = true;
    $scope.filteredPTMCount = 0;
    $scope.filteredPTMValue = 0.0;
    $scope.allPTMData = [];
    $scope.ganttPTMData = [];
    $scope.predicatePTM = 'Title';
    $scope.reversePTM = false;
    $scope.pageSizePTM = 16;
    $scope.currentPTMPage = 0;

    $scope.boolToYesOrNo = function (val) {
        if (val == true) return 'Yes';
        if (val == false) return 'No';
        return val;
    }

    // Filter arrays for PTM filter dropdowns    
    $scope.filterPTM = {
        year: {}, status: {}, proposalType: {}, pricingTool: {}, referenceNumber: {}, customer: {}, boeTool: {},
        manager: {}, pricer: {}, coverSheet: {}, indpReviewer: {}, pricingVerifier: {}, primeSub: {}, materialLead: {}, subcontractLead: {},
        pgm: {}, contractType: {}, programArea: {}, lob: [],
        costLead: {}
    };

    // Selected values for PTM Filters
    $scope.selectedPTMFilters = {
        year: 0, status: blank, proposalType: blank, pricingTool: blank,
        referenceNumber: blank, customer: blank, boeTool: blank, manager: blank, primeSub: blank,
        pricer: [], coverSheet: [], indpReviewer: [], pricingVerifier: [], pgm: blank, contractType: blank, materialLead: [], subcontractLead: [],
        lob: [],
        programArea: blank,
        nameContains: blank,
        includeSupport: false,
        iwta: blank,
        costLead: blank,
        startDate: '', endDate: '', startDateAsDate: undefined, endDateAsDate: undefined,
        minValue: '', maxValue: '',
        startDuration: '', endDuration: '',
        startDurationAsInt: undefined, endDurationAsInt: undefined
    };

    /* Options for the PTM Gantt Chart */
    $scope.optionsPTM = {
        scale: 'week',
        sortMode: undefined,
        sideMode: 'Table',
        columns: ['model.Title', 'model.Lead', 'model.PA', 'model.EVal | currency : \'\' : 1'],
        columnsHeaders: { 'model.Title': 'Proposal', 'model.Lead': 'Lead Estimator', 'model.PA': 'PA', 'model.EVal | currency : \'\' : 1': 'Value ($M)' },
        columnsClasses: { 'model.Title': 'gantt-column-name', 'model.Lead': 'gantt-column-pricer', 'model.PA': 'gantt-column-pa', 'model.EVal | currency : \'\' : 1': 'gantt-column-value' },
        columnsHeaderContents: {
            'model.Title': '<span class="gantt-column-sortable-header" ng-click="toggleSort(\'Title\')">{{getHeader()}}</span>',
            'model.Lead': '<span class="gantt-column-sortable-header" ng-click="toggleSort(\'Lead\')">{{getHeader()}}</span>',
            'model.PA': '<span class="gantt-column-sortable-header" ng-click="toggleSort(\'PA\')">{{getHeader()}}</span>',
            'model.EVal | currency : \'\' : 1': '<span class="gantt-column-sortable-header" ng-click="toggleSort(\'EVal\')">{{getHeader()}}</span>'
        },
        fromDate: null,
        toDate: null,
        taskContent: '',
        allowSideResizing: true,
        api: function (api) {
            // Important: Define function in gantt scope
            api.gantt.$scope.toggleSort = function (column) {

                if ($scope.predicatePTM == column) {
                    $scope.reversePTM = !$scope.reversePTM;
                } else {
                    $scope.reversePTM = false;
                }

                $scope.predicatePTM = column;
                $scope.refreshPTMData(true);
            }

            // Set api object on $scope so that we can refresh data later
            $scope.api = api;

            api.core.on.ready($scope, function () {

                // Load the proposals then add the events (it has to be this order for the directives to bind correctly)
                loadPTMData().then(function () {

                    //Add some DOM events
                    api.directives.on.new($scope, function (directiveName, directiveScope, element) {
                        if (directiveName === 'ganttTask') {
                            element.bind('click', function (event) {
                                event.stopPropagation();
                                $scope.$apply($scope.selectedPTM = directiveScope.task.row);
                                $scope.$apply($scope.hidePTMDetails = false);
                            });
                        } else if (directiveName === 'ganttRow') {
                            element.bind('click', function (event) {
                                event.stopPropagation();
                                $scope.$apply($scope.selectedPTM = directiveScope.row);
                                $scope.$apply($scope.hidePTMDetails = false);
                            });
                        } else if (directiveName === 'ganttRowLabel') {
                            element.bind('click', function () {
                                $scope.$apply($scope.selectedPTM = directiveScope.row);
                                $scope.$apply($scope.hidePTMDetails = false);
                            });
                        }
                    })
                });
            })
        }
    };

    /* Toggle the sorting of the gantt chart by the column (a-z, then z-a) */
    $scope.toggleSort = function (column) {

        if ($scope.predicatePTM == column) {
            $scope.reversePTM = !$scope.reversePTM;
        } else {
            $scope.reversePTM = false;
        }

        $scope.predicatePTM = column;
        $scope.refreshPTMData(true);
    }

    /* Refresh the PTM gantt data */
    $scope.refreshPTMData = function (resetCurrentPage) {
        if (resetCurrentPage) {
            $scope.currentPTMPage = 0;
        }

        $scope.filterPtmGanttRows();
        if (angular.isDefined($scope.api)) {
            $scope.api.rows.refresh();
        }
    }

    /* Push data through filters before adding into the PTM Gantt chart */
    $scope.filterPtmGanttRows = function () {
        $scope.filteredPTMCount = 0;
        $scope.filteredPTMValue = 0.0;
        var data = [];

        // parse duration
        $scope.selectedPTMFilters.startDurationAsInt = undefined;
        $scope.selectedPTMFilters.endDurationAsInt = undefined;

        if ($scope.selectedPTMFilters.startDuration !== undefined && $scope.selectedPTMFilters.startDuration != '') {
            $scope.selectedPTMFilters.startDurationAsInt = parseInt($scope.selectedPTMFilters.startDuration);
            if (isNaN($scope.selectedPTMFilters.startDurationAsInt)) {
                $scope.selectedPTMFilters.startDurationAsInt = undefined;
            }
        }

        if ($scope.selectedPTMFilters.endDuration !== undefined && $scope.selectedPTMFilters.endDuration != '') {
            $scope.selectedPTMFilters.endDurationAsInt = parseInt($scope.selectedPTMFilters.endDuration);
            if (isNaN($scope.selectedPTMFilters.endDurationAsInt)) {
                $scope.selectedPTMFilters.endDurationAsInt = undefined;
            }
        }

        data = $scope.allPTMData.filter(function (row) {
            return $scope.filterPTMProposals(row);
        });

        data = $filter('orderBy')(data, $scope.predicatePTM, $scope.reversePTM);

        $scope.filteredPTMCount = data.length;
        data.forEach(function (row) {
            $scope.filteredPTMValue += row.EVal;
        });

        // paging
        data = $filter('limitTo')(data, $scope.pageSizePTM, $scope.currentPTMPage * $scope.pageSizePTM);


        // find the min and max dates
        var minDate = null;
        var maxDate = null;
        if (data.length > 0) {
            data.forEach(function (row) {
                if (row.StDate !== undefined && (minDate == null || row.StDate < minDate)) {
                    minDate = new Date(row.StDate);
                }

                if (row.EndDate !== undefined && (maxDate == null || row.EndDate > maxDate)) {
                    maxDate = new Date(row.EndDate);
                }
            });
        }


        // always add 2 months on the end so that annotation will display without being truncated
        if (maxDate != null) {
            maxDate.setMonth(maxDate.getMonth() + 2);

            if (minDate != null) {
                // check that we have at least 7 months showing
                var sevenMonths = new Date();
                sevenMonths.setFullYear(minDate.getFullYear());
                // add 25 weeks
                sevenMonths.setMonth(minDate.getMonth());
                sevenMonths.setDate(minDate.getDate() + 7 * 25);

                if (maxDate < sevenMonths) {
                    maxDate = sevenMonths;
                }
            }
        }

        if (minDate == null || maxDate == null) {
            $scope.optionsPTM.fromDate = new Date();
            $scope.optionsPTM.toDate = new Date().setMonth($scope.optionsPTM.fromDate.getMonth() + 6);
        } else {
            $scope.optionsPTM.fromDate = minDate;
            $scope.optionsPTM.toDate = maxDate;
        }

        // Here we are going to use a copy/clone of the real data because subsequent sets will cause problems as angular-gantt messes with the data
        if (data.length > 0) {
            data = JSON.parse(JSON.stringify(data));
        }

        $scope.ganttPTMData = data;

        $scope.api.scroll.to(0);
        var todaysDate = new Date();
        if (todaysDate >= $scope.optionsPTM.fromDate && todaysDate <= $scope.optionsPTM.toDate) {
            $window.setTimeout(function () {
                $scope.api.scroll.toDate(todaysDate);
            }, 100);
        }
    }

    /* Update whether to show/hide the Support Annotation in the PTM Gantt grid */
    $scope.annotationPTMUpdate = function () {

        if ($scope.selectedPTMFilters.includeSupport) {
            $scope.optionsPTM.taskContent = '<span class="annotation">{{task.row.model.annotation}}</span>';
        } else {
            $scope.optionsPTM.taskContent = '';
        }
    }

    /* Loads the PTM filters from the cookie */
    $scope.loadFiltersFromCookiePTM = function () {
        var cookies = $cookies.getAll();
        $scope.selectedPTMFilters.startDuration = $scope.LoadJSON($scope.selectedPTMFilters.startDuration, cookies['startDurationPTM']);
        $scope.selectedPTMFilters.endDuration = $scope.LoadJSON($scope.selectedPTMFilters.endDuration, cookies['endDurationPTM']);
        $scope.selectedPTMFilters.status = $scope.LoadJSON($scope.selectedPTMFilters.status, cookies['statusPTM'], 'StatusFilter');
        $scope.selectedPTMFilters.proposalType = $scope.LoadJSON($scope.selectedPTMFilters.proposalType, cookies['proposalTypePTM'], 'PropTypeFilter');
        $scope.selectedPTMFilters.pricingTool = $scope.LoadJSON($scope.selectedPTMFilters.pricingTool, cookies['pricingToolPTM'], 'PricingToolFilter');
        $scope.selectedPTMFilters.referenceNumber = $scope.LoadJSON($scope.selectedPTMFilters.referenceNumber, cookies['referenceNumberPTM'], 'REFFilter');
        $scope.selectedPTMFilters.customer = $scope.LoadJSON($scope.selectedPTMFilters.customer, cookies['customerPTM'], 'CustomerFilter');
        $scope.selectedPTMFilters.boeTool = $scope.LoadJSON($scope.selectedPTMFilters.boeTool, cookies['boeToolPTM'], 'BOEFilter');
        $scope.selectedPTMFilters.manager = $scope.LoadJSON($scope.selectedPTMFilters.manager, cookies['managerPTM'], 'ManagerFilter');
        $scope.selectedPTMFilters.primeSub = $scope.LoadJSON($scope.selectedPTMFilters.primeSub, cookies['primeSubPTM'], 'PrimeFilter');
        $scope.selectedPTMFilters.pricer = $scope.LoadJSON($scope.selectedPTMFilters.pricer, cookies['pricerPTM'], 'PricerFilter');
        $scope.selectedPTMFilters.coverSheet = $scope.LoadJSON($scope.selectedPTMFilters.coverSheet, cookies['coverSheetPTM'], 'CoverSheetFilter');
        $scope.selectedPTMFilters.indpReviewer = $scope.LoadJSON($scope.selectedPTMFilters.indpReviewer, cookies['indpReviewerPTM'], 'IndpReviewerFilter');
        $scope.selectedPTMFilters.pricingVerifier = $scope.LoadJSON($scope.selectedPTMFilters.pricingVerifier, cookies['pricingVerifierPTM'], 'PricingVerifierFilter');
        $scope.selectedPTMFilters.materialLead = $scope.LoadJSON($scope.selectedPTMFilters.materialLead, cookies['materialLeadPTM'], 'MaterialLeadFilter');
        $scope.selectedPTMFilters.subcontractLead = $scope.LoadJSON($scope.selectedPTMFilters.subcontractLead, cookies['subcontractLeadPTM'], 'SubcontractLeadFilter');
        $scope.selectedPTMFilters.pgm = $scope.LoadJSON($scope.selectedPTMFilters.pgm, cookies['pgmPTM'], 'PGMFilter');
        $scope.selectedPTMFilters.contractType = $scope.LoadJSON($scope.selectedPTMFilters.contractType, cookies['contractTypePTM'], 'ContFilter');
        $scope.selectedPTMFilters.lob = $scope.LoadJSON($scope.selectedPTMFilters.lob, cookies['lobPTM'], 'LOBFilter');
        $scope.selectedPTMFilters.programArea = $scope.LoadJSON($scope.selectedPTMFilters.programArea, cookies['programAreaPTM'], 'PAFilter');
        $scope.selectedPTMFilters.nameContains = $scope.LoadJSON($scope.selectedPTMFilters.nameContains, cookies['nameContainsPTM']);
        $scope.selectedPTMFilters.includeSupport = $scope.LoadJSON($scope.selectedPTMFilters.includeSupport, cookies['includeSupportPTM']);
        $scope.selectedPTMFilters.iwta = $scope.LoadJSON($scope.selectedPTMFilters.iwta, cookies['iwtaPTM'], 'IwtaFilter');
        $scope.selectedPTMFilters.costLead = $scope.LoadJSON($scope.selectedPTMFilters.costLead, cookies['costLeadPTM'], 'CostLeadFilter');
        $scope.selectedPTMFilters.year = $scope.LoadJSON($scope.selectedPTMFilters.year, cookies['yearPTM'], 'YearFilter');
        $scope.selectedPTMFilters.startDate = $scope.LoadJSON($scope.selectedPTMFilters.startDate, cookies['startDatePTM']);
        $scope.selectedPTMFilters.endDate = $scope.LoadJSON($scope.selectedPTMFilters.endDate, cookies['endDatePTM']);
        $scope.selectedPTMFilters.minValue = $scope.LoadJSON($scope.selectedPTMFilters.minValue, cookies['minValuePTM']);
        $scope.selectedPTMFilters.maxValue = $scope.LoadJSON($scope.selectedPTMFilters.maxValue, cookies['maxValuePTM']);

        $scope.updateDates();

        // now show/hide the clear filter icons
        filterObjectRangeChange($scope.selectedPTMFilters.endDate, $scope.selectedPTMFilters.startDate, 'DateFilterClear');
        filterObjectRangeChange($scope.selectedPTMFilters.startDuration, $scope.selectedPTMFilters.endDuration, 'DurationFilterClear');
        filterObjectRangeChange($scope.selectedPTMFilters.minValue, $scope.selectedPTMFilters.maxValue, 'ValueFilterClear');

        filterObjectChange($scope.selectedPTMFilters.nameContains, 'NameFilterClear');
        $scope.annotationPTMUpdate();
    }

    /* Load the JSON object out of the string*/
    $scope.LoadJSON = function (originalValue, jsonString, filterName) {

        if (jsonString === undefined || jsonString === null || jsonString === "null" || jsonString == "\"\"") {
            return originalValue;
        }
        var returnValue = JSON.parse(jsonString);
        if (typeof (originalValue) === "object") {
            // if returnValue is a json object, and not a array of json objects,
            // convert it to the array.
            if (typeof (returnValue) === "object" && !Array.isArray(returnValue)) {
                returnValue = returnValue.value !== "(blanks)" ? new Array(returnValue) : new Array();
            }

            filterObjectChange(returnValue.length > 0 ? returnValue[0] : -1, filterName + 'Clear');
        }

        return returnValue;
    }

    /* Loads the PTM filters from the cookie */
    $scope.saveFiltersIntoCookiePTM = function () {
        var now = new $window.Date(),
        // this will set the expiration to 6 months
        exp = new $window.Date(now.getFullYear(), now.getMonth() + 6, now.getDate());
        // if user has made filtering selection, write cookie, otherwise remove cookie (user may have cleared a filter)
        if ($scope.selectedPTMFilters.year.length > 0 && $scope.selectedPTMFilters.year != undefined)
            $cookies.put('yearPTM', angular.toJson($scope.selectedPTMFilters.year), { expires: exp });
        else
            $cookies.remove('yearPTM');
        if ($scope.selectedPTMFilters.startDate != '' && $scope.selectedPTMFilters.startDate != undefined)
            $cookies.put('startDatePTM', angular.toJson($scope.selectedPTMFilters.startDate), { expires: exp });
        else
            $cookies.remove('startDatePTM');
        if ($scope.selectedPTMFilters.endDate != '' && $scope.selectedPTMFilters.endDate != undefined)
            $cookies.put('endDatePTM', angular.toJson($scope.selectedPTMFilters.endDate), { expires: exp });
        else
            $cookies.remove('endDatePTM');
        if ($scope.selectedPTMFilters.minValue != '' && $scope.selectedPTMFilters.minValue != undefined)
            $cookies.put('minValuePTM', angular.toJson($scope.selectedPTMFilters.minValue), { expires: exp });
        else
            $cookies.remove('minValuePTM');
        if ($scope.selectedPTMFilters.maxValue != '' && $scope.selectedPTMFilters.maxValue != undefined)
            $cookies.put('maxValuePTM', angular.toJson($scope.selectedPTMFilters.maxValue), { expires: exp });
        else
            $cookies.remove('maxValuePTM');
        if ($scope.selectedPTMFilters.nameContains != '' && $scope.selectedPTMFilters.nameContains != undefined)
            $cookies.put('nameContainsPTM', angular.toJson($scope.selectedPTMFilters.nameContains), { expires: exp });
        else
            $cookies.remove('nameContainsPTM');
        if ($scope.selectedPTMFilters.startDuration != '' && $scope.selectedPTMFilters.startDuration != undefined)
            $cookies.put('startDurationPTM', angular.toJson($scope.selectedPTMFilters.startDuration), { expires: exp });
        else
            $cookies.remove('startDurationPTM');
        if ($scope.selectedPTMFilters.endDuration != '' && $scope.selectedPTMFilters.endDuration != undefined)
            $cookies.put('endDurationPTM', angular.toJson($scope.selectedPTMFilters.endDuration), { expires: exp });
        else
            $cookies.remove('endDurationPTM');
        if ($scope.selectedPTMFilters.lob.length > 0)
            $cookies.put('lobPTM', angular.toJson($scope.selectedPTMFilters.lob), { expires: exp });
        else
            $cookies.remove('lobPTM');
        if ($scope.selectedPTMFilters.status.length > 0)
            $cookies.put('statusPTM', angular.toJson($scope.selectedPTMFilters.status), { expires: exp });
        else
            $cookies.remove('statusPTM');
        if ($scope.selectedPTMFilters.proposalType.length > 0)
            $cookies.put('proposalTypePTM', angular.toJson($scope.selectedPTMFilters.proposalType), { expires: exp });
        else
            $cookies.remove('proposalTypePTM');
        if ($scope.selectedPTMFilters.pricingTool.length > 0)
            $cookies.put('pricingToolPTM', angular.toJson($scope.selectedPTMFilters.pricingTool), { expires: exp });
        else
            $cookies.remove('pricingToolPTM');
        if ($scope.selectedPTMFilters.referenceNumber.length > 0)
            $cookies.put('referenceNumberPTM', angular.toJson($scope.selectedPTMFilters.referenceNumber), { expires: exp });
        else
            $cookies.remove('referenceNumberPTM');
        if ($scope.selectedPTMFilters.customer.length > 0)
            $cookies.put('customerPTM', angular.toJson($scope.selectedPTMFilters.customer), { expires: exp });
        else
            $cookies.remove('customerPTM');
        if ($scope.selectedPTMFilters.boeTool.length > 0)
            $cookies.put('boeToolPTM', angular.toJson($scope.selectedPTMFilters.boeTool), { expires: exp });
        else
            $cookies.remove('boeToolPTM');
        if ($scope.selectedPTMFilters.manager.length > 0)
            $cookies.put('managerPTM', angular.toJson($scope.selectedPTMFilters.manager), { expires: exp });
        else
            $cookies.remove('managerPTM');
        if ($scope.selectedPTMFilters.primeSub.length > 0)
            $cookies.put('primeSubPTM', angular.toJson($scope.selectedPTMFilters.primeSub), { expires: exp });
        else
            $cookies.remove('primeSubPTM');
        if ($scope.selectedPTMFilters.pricer.length > 0)
            $cookies.put('pricerPTM', angular.toJson($scope.selectedPTMFilters.pricer), { expires: exp });
        else
            $cookies.remove('pricerPTM');
        if ($scope.selectedPTMFilters.coverSheet.length > 0)
            $cookies.put('coverSheetPTM', angular.toJson($scope.selectedPTMFilters.coverSheet), { expires: exp });
        else
            $cookies.remove('coverSheetPTM');
        if ($scope.selectedPTMFilters.indpReviewer.length > 0)
            $cookies.put('indpReviewerPTM', angular.toJson($scope.selectedPTMFilters.indpReviewer), { expires: exp });
        else
            $cookies.remove('indpReviewerPTM');
        if ($scope.selectedPTMFilters.pricingVerifier.length > 0)
            $cookies.put('pricingVerifierPTM', angular.toJson($scope.selectedPTMFilters.pricingVerifier), { expires: exp });
        else
            $cookies.remove('pricingVerifierPTM');
        if ($scope.selectedPTMFilters.materialLead.length > 0)
            $cookies.put('materialLeadPTM', angular.toJson($scope.selectedPTMFilters.materialLead), { expires: exp });
        else
            $cookies.remove('materialLeadPTM');
        if ($scope.selectedPTMFilters.subcontractLead.length > 0)
            $cookies.put('subcontractLeadPTM', angular.toJson($scope.selectedPTMFilters.subcontractLead), { expires: exp });
        else
            $cookies.remove('subcontractLeadPTM');
        if ($scope.selectedPTMFilters.pgm.length > 0)
            $cookies.put('pgmPTM', angular.toJson($scope.selectedPTMFilters.pgm), { expires: exp });
        else
            $cookies.remove('pgmPTM');
        if ($scope.selectedPTMFilters.contractType.length > 0)
            $cookies.put('contractTypePTM', angular.toJson($scope.selectedPTMFilters.contractType), { expires: exp });
        else
            $cookies.remove('contractTypePTM');
        if ($scope.selectedPTMFilters.programArea.length > 0)
            $cookies.put('programAreaPTM', angular.toJson($scope.selectedPTMFilters.programArea), { expires: exp });
        else
            $cookies.remove('programAreaPTM');
        if ($scope.selectedPTMFilters.iwta.length > 0)
            $cookies.put('iwtaPTM', angular.toJson($scope.selectedPTMFilters.iwta), { expires: exp });
        else
            $cookies.remove('iwtaPTM');
        if ($scope.selectedPTMFilters.costLead.length > 0)
            $cookies.put('costLeadPTM', angular.toJson($scope.selectedPTMFilters.costLead), { expires: exp });
        else
            $cookies.remove('costLeadPTM');
        $cookies.put('includeSupportPTM', angular.toJson($scope.selectedPTMFilters.includeSupport), { expires: exp });
    }

    /* reset all PTM filters to be empty and unselected */
    $scope.clearAllPTMFilters = function () {
        $scope.selectedPTMFilters.startDate = '';
        $scope.selectedPTMFilters.endDate = '';
        $scope.selectedPTMFilters.startDateAsDate = undefined;
        $scope.selectedPTMFilters.endDateAsDate = undefined;
        $scope.selectedPTMFilters.minValue = '';
        $scope.selectedPTMFilters.maxValue = '';
        $scope.selectedPTMFilters.startDuration = '';
        $scope.selectedPTMFilters.endDuration = '';
        $scope.selectedPTMFilters.year = [];
        $scope.selectedPTMFilters.status = [];
        $scope.selectedPTMFilters.proposalType = [];
        $scope.selectedPTMFilters.pricingTool = [];
        $scope.selectedPTMFilters.referenceNumber = [];
        $scope.selectedPTMFilters.primeSub = [];
        $scope.selectedPTMFilters.customer = [];
        $scope.selectedPTMFilters.boeTool = [];
        $scope.selectedPTMFilters.manager = [];
        $scope.selectedPTMFilters.pricer = [];
        $scope.selectedPTMFilters.coverSheet = [];
        $scope.selectedPTMFilters.indpReviewer = [];
        $scope.selectedPTMFilters.pricingVerifier = [];
        $scope.selectedPTMFilters.materialLead = [];
        $scope.selectedPTMFilters.subcontractLead = [];
        $scope.selectedPTMFilters.pgm = [];
        $scope.selectedPTMFilters.contractType = [];
        $scope.selectedPTMFilters.nameContains = '';
        $scope.selectedPTMFilters.lob = [];
        $scope.selectedPTMFilters.programArea = [];
        $scope.selectedPTMFilters.costLead = [];
        $scope.selectedPTMFilters.iwta = [];

        // refresh filter clear glyphs since we're clearing FilterClear images when indiv. filter not engaged
        angular.element(document.querySelector('#YearFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#StatusFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PricingToolFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PropTypeFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CustomerFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#LOBFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#BOEFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PAFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#ManagerFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#DateFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PGMFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#DurationFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#REFFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PrimeFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PricerFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CoverSheetFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#IndpReviewerFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#PricingVerifierFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#MaterialLeadFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#SubcontractLeadFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#ValueFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#CostLeadFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#NameFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#ContFilterClear')).attr("Hidden", "Hidden");
        angular.element(document.querySelector('#IwtaFilterClear')).attr("Hidden", "Hidden");
        if (!$scope.isPTMLoading) {
            $scope.refreshPTMData(true);
        }
    };

    /* Update the real Date objects from the strings */
    $scope.updateDates = function () {
        if ($scope.selectedPTMFilters.startDate !== undefined && $scope.selectedPTMFilters.startDate != '') {
            $scope.selectedPTMFilters.startDateAsDate = new Date($scope.selectedPTMFilters.startDate);
            $scope.selectedPTMFilters.startDateAsDate.setDate($scope.selectedPTMFilters.startDateAsDate.getDate() - 1);
        } else {
            $scope.selectedPTMFilters.startDateAsDate = undefined;
        }

        if ($scope.selectedPTMFilters.endDate !== undefined && $scope.selectedPTMFilters.endDate != '') {
            $scope.selectedPTMFilters.endDateAsDate = new Date($scope.selectedPTMFilters.endDate);
            $scope.selectedPTMFilters.endDateAsDate.setDate($scope.selectedPTMFilters.endDateAsDate.getDate() + 1);
        } else {
            $scope.selectedPTMFilters.endDateAsDate = undefined;
        }
    }

    /* Called when there has been an update to the date range for PTM Filters */
    $scope.daterangePTMUpdate = function () {
        $scope.updateDates();
        $scope.refreshPTMData(true);
    }

    /* helper method used by filterPTMProposals; checks whether item matches filter or not; returns: false if row dos not match, true if it does; */
    $scope.checkMultiSelectFilter = function (filterValue, propertyValue) {
        if (filterValue != undefined && filterValue.length > 0) {
            return filterValue.some(function (selectedItem) { // return true if match, false otherwise
                if (selectedItem.value == null && selectedItem.value == propertyValue) return true; // if 'No Value' selected
                if (selectedItem.display == propertyValue) return true; // there was a match
                return false;
            });
        }
    };

    $scope.checkMultiSelectFilterContains = function (filterValue, propertyValue) {
        if (filterValue != undefined && filterValue.length > 0) {
            return filterValue.some(function (selectedItem) { // return true if match, false otherwise
                if (selectedItem.value == null && selectedItem.value == propertyValue) return true; // if 'No Value' selected
                if (propertyValue.indexOf(selectedItem.display) >= 0) return true; // there was a match
                return false;
            });
        }
    };

    /* Method used to filter the PTM Proposals */
    $scope.filterPTMProposals = function (value) {
        // if any of these rows dont match then return false!
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.year, value.EndDate.getFullYear()) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.status, value.Status) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.proposalType, value.PropType) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.pricingTool, value.PricingTool) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.referenceNumber, value.Ref) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.primeSub, value.PrmSub) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.customer, value.Cust) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.boeTool, value.BoeTool) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.manager, value.Mgr) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.pricer, value.Pricers) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.coverSheet, value.CSA) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.pricingVerifier, value.PV) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.materialLead, value.ML) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.subcontractLead, value.SL) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.indpReviewer, value.IndpRev) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.pgm, value.PrgName) == false) return false;
        if ($scope.checkMultiSelectFilterContains($scope.selectedPTMFilters.contractType, value.ContType) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.costLead, value.Lead) == false) return false;

        // filter based on search term against proposal name
        if ($scope.selectedPTMFilters.nameContains !== undefined && $scope.selectedPTMFilters.nameContains != '' && value.Title.toLowerCase().indexOf($scope.selectedPTMFilters.nameContains.toLowerCase()) == -1) {
            return false;
        }

        var lob = [];
        $scope.selectedPTMFilters.lob.forEach(function (el) {
            lob.push({ display: el, value: el });
        });
        if ($scope.checkMultiSelectFilter(lob, value.LOB) == false) return false;
        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.programArea, value.PA) == false) return false;

        var subDate = new Date(value.SubDate);
        if ($scope.selectedPTMFilters.startDateAsDate !== undefined && $scope.selectedPTMFilters.endDateAsDate !== undefined) {
            // start and end filters are valid dates, and they do not intersect this row's from date
            if ($scope.selectedPTMFilters.startDateAsDate <= $scope.selectedPTMFilters.endDateAsDate && (subDate > $scope.selectedPTMFilters.endDateAsDate || subDate < $scope.selectedPTMFilters.startDateAsDate)) {
                return false;
            }
        } else if ($scope.selectedPTMFilters.startDateAsDate !== undefined && subDate < $scope.selectedPTMFilters.startDateAsDate) {
            return false;
        } else if ($scope.selectedPTMFilters.endDateAsDate !== undefined && subDate > $scope.selectedPTMFilters.endDateAsDate) {
            return false;
        }

        // filter Min Value
        if ($scope.selectedPTMFilters.minValue != '' && !isNaN($scope.selectedPTMFilters.minValue)) {
            if ($scope.selectedPTMFilters.minValue > value.EVal) {
                return false;
            }
        }

        // filter Max Value
        if ($scope.selectedPTMFilters.maxValue != '' && !isNaN($scope.selectedPTMFilters.maxValue)) {
            if ($scope.selectedPTMFilters.maxValue < value.EVal) {
                return false;
            }
        }

        if ($scope.checkMultiSelectFilter($scope.selectedPTMFilters.iwta, value.IWTA) == false) return false;

        // filter Duration
        if ($scope.selectedPTMFilters.startDurationAsInt !== undefined && $scope.selectedPTMFilters.endDurationAsInt !== undefined &&
            $scope.selectedPTMFilters.startDurationAsInt <= $scope.selectedPTMFilters.endDurationAsInt) {
            if (value.Duration !== undefined) {
                if (value.Duration < $scope.selectedPTMFilters.startDurationAsInt || value.Duration > $scope.selectedPTMFilters.endDurationAsInt) {
                    return false;
                }
            } else {
                // filter out proposals that do not have creation and submittal dates
                return false;
            }
        }

        // none of the filters threw out the row, so it must match

        return true;
    };

    /* Refresh the PTM Proposals from the Backend Server */
    $scope.refreshPTMProposals = function () {
        loadPTMData();
    }

    /* Refresh the PTM gantt chart when the user clicks the paging controls */
    $scope.$watch(function (scope) { return scope.currentPTMPage },
        function (newValue, oldValue) {
            if (newValue !== oldValue) {
                $scope.refreshPTMData(false);
            }
        }
    );

    /* Find the number of pages based on the total number of Rows and pageSize */
    $scope.numberOfPages = function (numFilteredRows, pageSize) {
        return Math.ceil(numFilteredRows / pageSize);
    }

    /*
     * *********** NOTE ************
     * Private variables & functions 
     * *****************************
     */
    var blank = "(blanks)";
    var oneDay = 24 * 60 * 60 * 1000; // hours*minutes*seconds*milliseconds

    /* Converts a hash table to an array */
    var convertToArray = function (data, sortByDisplayName) {
        var newData = [];

        for (var key in data) {
            var item = data[key];

            // set the value to lowercase for strings
            if (angular.isDefined(item.value) && item.value !== null && angular.isString(item.value)) {
                item.value = item.value.toLowerCase();
            }

            newData.push(item);
        }

        if (sortByDisplayName) {
            newData.sort(function (a, b) { return a.display.localeCompare(b.display) });
        }

        return newData;
    };

    /* Loads the PTM Data */
    var loadPTMData = function () {
        $scope.isPTMLoading = true;
        $scope.isPTMError = false;
        $scope.hidePTMDetails = true;
        $scope.ganttPTMData = [];
        $scope.filteredPTMCount = 0;
        $scope.filteredPTMValue = 0.0;
        $scope.allPTMData = [];

        return $http({
            method: 'POST',
            url: createPostURL('Home', 'RetrieveProposalData')
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available

            // Reset filters to empty arrays
            $scope.filterPTM.year = {};
            $scope.filterPTM.status = {};
            $scope.filterPTM.proposalType = {};
            $scope.filterPTM.pricingTool = {};
            $scope.filterPTM.referenceNumber = {};
            $scope.filterPTM.primeSub = {};
            $scope.filterPTM.customer = {};
            $scope.filterPTM.boeTool = {};
            $scope.filterPTM.manager = {};
            var filterPTMPricer = {};
            $scope.filterPTM.coverSheet = {};
            $scope.filterPTM.indpReviewer = {};
            $scope.filterPTM.pricingVerifier = {};
            $scope.filterPTM.materialLead = {};
            $scope.filterPTM.subcontractLead = {};
            $scope.filterPTM.pgm = {};
            $scope.filterPTM.contractType = {};
            $scope.filterPTM.programArea = {};
            $scope.filterPTM.costLead = {};
            $scope.filterPTM.iwta = {};
            $scope.filterPTM.lob = response.data.LOBs.sort().slice();

            var contractTypes = [];
            response.data.ContractTypes.sort().forEach(function (el) {
                contractTypes.push({ display: el, value: el });
            });
            $scope.filterPTM.contractType = contractTypes;

            // Setup other dropdowns based on existing values in data
            response.data.Proposals.forEach(function (item) {

                // build filter sets based on the data returned
                addOption($scope.filterPTM.status, item.Status);
                addOption($scope.filterPTM.proposalType, item.PropType);
                addOption($scope.filterPTM.pricingTool, item.PricingTool);
                addOption($scope.filterPTM.referenceNumber, item.Ref);
                addOption($scope.filterPTM.primeSub, item.PrmSub);
                addOption($scope.filterPTM.customer, item.Cust);
                addOption($scope.filterPTM.boeTool, item.BoeTool);
                addOption($scope.filterPTM.manager, item.Mgr);
                addOption($scope.filterPTM.pgm, item.PrgName);
                addOption($scope.filterPTM.costLead, item.Lead);
                addOption($scope.filterPTM.iwta, item.IWTA);
                addOption($scope.filterPTM.coverSheet, item.CSA);
                addOption($scope.filterPTM.indpReviewer, item.IndpRev);
                addOption($scope.filterPTM.pricingVerifier, item.PV);
                addOption($scope.filterPTM.materialLead, item.ML);
                addOption($scope.filterPTM.subcontractLead, item.SL);

                response.data.ProgramAreas.forEach(function (programArea) {
                    if (programArea.ProgramArea == item.PA) {
                        $scope.filterPTM.programArea[item.PA] = { display: item.PA, value: item.PA, lob: programArea.LineOfBusiness };
                    }
                });

                // build annotation content
                var content = '';
                item.Pricers = [];
                if (item.Lead !== undefined && item.Lead != null) {
                    item.Pricers.push(item.Lead);
                }

                if (item.Price1 !== undefined && item.Price1 != null) {
                    content += 'E1:' + item.Price1 + ' ';
                    item.Pricers.push(item.Price1);
                }

                if (item.Price2 !== undefined && item.Price2 != null) {
                    content += 'E2:' + item.Price2 + ' ';
                    item.Pricers.push(item.Price2);
                }

                item.Pricers.forEach(function (pricer) {
                    filterPTMPricer[pricer] = { display: pricer, value: pricer };
                });

                if (item.Cost !== undefined) {
                    content += 'CV:' + item.Cost;
                }

                item.annotation = content;

                // setup tasks in gantt chart
                if (item.StDate != null && item.EndDate != null && item.StDate !== undefined && item.EndDate !== undefined) {
                    // pull out the year for later filtering, date in format yyyy-mm-dd
                    item.tasks = [{ name: '', color: "#9FC5F8", from: item.StDate, to: item.EndDate }];
                }

                if (item.StDate != null) {
                    item.StDate = new Date(item.StDate);
                }

                if (item.EndDate != null) {
                    item.EndDate = new Date(item.EndDate);
                }

                // set Duration
                if (item.SubDate !== undefined && item.CrDate !== undefined) {
                    var submittal = new Date(item.SubDate);
                    var created = new Date(item.CrDate);
                    item.Duration = Math.ceil(Math.abs((submittal.getTime() - created.getTime()) / (oneDay)));
                }
            });

            // Add years for min to max year
            var d = new Date();
            var minYear = response.data.MinYear;
            var maxYear = d.getFullYear();

            for (i = minYear; i <= maxYear; i++) {
                var yearString = i.toString();
                addOption($scope.filterPTM.year, i);
            }

            // need to convert to arrays and sort the data
            $scope.filterPTM.year = convertToArray($scope.filterPTM.year, false);
            $scope.filterPTM.status = convertToArray($scope.filterPTM.status, true);
            $scope.filterPTM.proposalType = convertToArray($scope.filterPTM.proposalType, true);
            $scope.filterPTM.pricingTool = convertToArray($scope.filterPTM.pricingTool, true);
            $scope.filterPTM.referenceNumber = convertToArray($scope.filterPTM.referenceNumber, true);
            $scope.filterPTM.primeSub = convertToArray($scope.filterPTM.primeSub, true);
            $scope.filterPTM.customer = convertToArray($scope.filterPTM.customer, true);
            $scope.filterPTM.boeTool = convertToArray($scope.filterPTM.boeTool, true);
            $scope.filterPTM.manager = convertToArray($scope.filterPTM.manager, true);
            $scope.filterPTM.pgm = convertToArray($scope.filterPTM.pgm, true);
            $scope.filterPTM.pricer = convertToArray(filterPTMPricer, true);
            $scope.filterPTM.coverSheet = convertToArray($scope.filterPTM.coverSheet, true);
            $scope.filterPTM.indpReviewer = convertToArray($scope.filterPTM.indpReviewer, true);
            $scope.filterPTM.pricingVerifier = convertToArray($scope.filterPTM.pricingVerifier, true);
            $scope.filterPTM.materialLead = convertToArray($scope.filterPTM.materialLead, true);
            $scope.filterPTM.subcontractLead = convertToArray($scope.filterPTM.subcontractLead, true);
            $scope.filterPTM.programArea = convertToArray($scope.filterPTM.programArea, true);
            $scope.filterPTM.costLead = convertToArray($scope.filterPTM.costLead, true);
            $scope.filterPTM.iwta = convertToArray($scope.filterPTM.iwta, false);



            // Add blank value here because sorting puts blank value at end for integers
            $scope.filterPTM.status.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.proposalType.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.pricingTool.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.referenceNumber.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.primeSub.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.boeTool.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.manager.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.pgm.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.contractType.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.programArea.splice(0, 0, { display: 'No Value', value: null, lob: null });
            $scope.filterPTM.costLead.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.customer.splice(0, 0, { display: 'No Value', value: null });
            $scope.filterPTM.year.splice(0, 0, { display: 'No Value', value: 0 });
            $scope.filterPTM.iwta.splice(0, 0, { display: 'No Value', value: null });

            if ($scope.firstLoad) {
                $scope.firstLoad = false;
                $scope.clearAllPTMFilters();
                $scope.loadFiltersFromCookiePTM();
            }

            $scope.allPTMData = response.data.Proposals;
            $scope.filterPtmGanttRows();
            $scope.isPTMLoading = false;

        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            $scope.isPTMLoading = false;
            $scope.isPTMError = true;
        });
    }

    $scope.exportCSV = function() {
        // we need to filter the data here since the data pushed into gantt chart is paged
        var data = [];

        // parse duration
        $scope.selectedPTMFilters.startDurationAsInt = undefined;
        $scope.selectedPTMFilters.endDurationAsInt = undefined;

        if ($scope.selectedPTMFilters.startDuration !== undefined && $scope.selectedPTMFilters.startDuration != '') {
            $scope.selectedPTMFilters.startDurationAsInt = parseInt($scope.selectedPTMFilters.startDuration);
            if (isNaN($scope.selectedPTMFilters.startDurationAsInt)) {
                $scope.selectedPTMFilters.startDurationAsInt = undefined;
            }
        }

        if ($scope.selectedPTMFilters.endDuration !== undefined && $scope.selectedPTMFilters.endDuration != '') {
            $scope.selectedPTMFilters.endDurationAsInt = parseInt($scope.selectedPTMFilters.endDuration);
            if (isNaN($scope.selectedPTMFilters.endDurationAsInt)) {
                $scope.selectedPTMFilters.endDurationAsInt = undefined;
            }
        }

        data = $scope.allPTMData.filter(function (row) {
            return $scope.filterPTMProposals(row);
        });

        data = $filter('orderBy')(data, $scope.predicatePTM, $scope.reversePTM);
        var csvArray = [];
        var header = 'Proposal Title,Tracking #,Lead Estimator,LOB,Program Area,Status,IWTA,EOC,Prop Type,Capture Manager,Contract Type,Program,Customer Type,Prime/Sub,Customer,Contract Leader,Prop Start Date,Prop End Date,Create Date,Submit Date,Estimate $M,Submit $M,Cost Volume Lead,Subcontract Lead,Material Lead,Cover Sheet Approver,Pricing Verifier,Independent Reviewer,Additional Pricers,Pricer Type,Pricing Tool,BOE Tool';
        csvArray.push(header);
        data.forEach(function (item) {
            var itemArray = ['"' + item.Title, item.Ref, item.Lead, item.LOB, item.PA, item.Status, $scope.boolToYesOrNo(item.IWTA), item.EoC, item.PropType, item.Mgr, item.ContType, item.PrgName, item.CusType, item.PrmSub, item.Cust, item.ContLdr, item.StDate, item.EndDate, item.CrDate, item.SubDate, item.EVal, item.SVal, item.cost, item.SL, item.ML, item.CSA, item.PV, item.IndpRev, item.Price1 + item.Price2, item.PricerType, item.PricingTool, item.BoeTool + '"'];

            var itemString = itemArray.join('","');
            csvArray.push(itemString);
        });

        var csvString = csvArray.join('\n');

        var a = $('<a/>', {
            style: 'display:none',
            href: 'data:application/octet-stream;base64,' + btoa(unescape(encodeURIComponent(csvString))),
            download: 'PTM_Proposal_Data.csv'
        }).appendTo('body');
        a[0].click();
        a.remove();
    };
}]);