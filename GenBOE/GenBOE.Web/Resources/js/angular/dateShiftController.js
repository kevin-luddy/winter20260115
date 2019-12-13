angular.module('genboe').controller('DateShiftController', ['$scope', '$http', '$timeout', 'DateShiftModel', '$uibModal', function ($scope, $http, $timeout, DateShiftModel, $uibModal) {

    $scope.isLoading = true;

    $scope.constants = {
        UNSELECTED: -1,
        SPREADS_UNSELECTED: 0,
        NEW_CURVE_SPREAD: 1,
        NEW_CURVE_UNSELECTED: '-1',
        NO_CHANGE: 0,
        FLOWDOWN: 1,
        TO_POP: 2,
        TO_START: 3,
        TO_END: 4,
        SHIFT_ONLY: 1,
        DURATION_ONLY: 2,
        SHIFT_AND_DURATION: 3,
        LEFT: 'Left',
        RIGHT: 'Right',
        SHIFT: 'Shift',
        DURATION: 'Duration',
        NO_CHANGE_HELP_TEXT: 'No date changes to children. Author is expected to manually change child dates.',
        TRIPS_SHIFTED_LEFT: ' Travel trips will be shifted to the LEFT and may require manual update by the author for dates and amounts.',
        TRIPS_SHIFTED_RIGHT: ' Travel trips will be shifted to the RIGHT and may require manual update by the author for dates and amounts.',
        TRIPS_REMAIN_INPLACE: ' Travel trips will remain in place and may require manual update by the author for dates and amounts',
        ALIGN_ALL_CHILDREN: 'Align all children to the parent dates.',
        ALIGN_CHILDREN_END: 'Align all children to the parent END date.',
        ALIGN_CHILDREN_START: 'Align all children to the parent START date.',
        ALIGN_CHILDREN_CONTRACTED: 'Align all children to the CONTRACTED parent end date.',
        ALIGN_CHILDREN_EXPANDED: 'Align all children to the EXPANDED parent end date.',
        NO_EMAILS: 0,
    };

    $scope.model = DateShiftModel;
    $scope.startDate = $scope.model.startDate.toDate();
    $scope.endDate = $scope.model.endDate.toDate();

/*
 * ***************** NOTE ******************
 * Functions below relate to the Help Images
 * *****************************************
 */

    $scope.hierarchyImg = DateShiftModel.isRMS ? 'hierarchy_rms.PNG' : 'hierarchy.PNG';
    $scope.note = DateShiftModel.isRMS ? 'Resources Types, or Travel Tasks' : 'or Resource Types';

    $scope.help = [
        {
            type: $scope.constants.SHIFT,
            dir: $scope.constants.LEFT,
            caption: 'Shift Left',
            images: [
                {
                    src: 'ShiftLeftNoChange',
                    mod: $scope.constants.NO_CHANGE,
                    caption: 'No Change for Children',
                    help: $scope.constants.NO_CHANGE_HELP_TEXT,
                },
                {
                    src: 'ShiftLeftFlowdown',
                    mod: $scope.constants.FLOWDOWN,
                    caption: 'Flowdown for Children',
                    help: 'Shift all child dates the same number of months to the LEFT as the parent. Resource utilizing spread curves will be automatically updated.',
                },
                {
                    src: 'ShiftLeftToPOP',
                    mod: $scope.constants.TO_POP,
                    caption: 'Children to POP',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_ALL_CHILDREN + $scope.constants.TRIPS_SHIFTED_LEFT : $scope.constants.ALIGN_ALL_CHILDREN,
                },
                {
                    src: 'ShiftLeftToEnd',
                    mod: $scope.constants.TO_END,
                    caption: 'Children to End',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_CHILDREN_END + $scope.constants.TRIPS_SHIFTED_LEFT : $scope.constants.ALIGN_CHILDREN_END,
                }
            ]
        },
        {
            type: $scope.constants.SHIFT,
            dir: $scope.constants.RIGHT,
            caption: 'Shift Right',
            images: [
                {
                    src: 'ShiftRightNoChange',
                    mod: $scope.constants.NO_CHANGE,
                    caption: 'No Change for Children',
                    help: $scope.constants.NO_CHANGE_HELP_TEXT,
                },
                {
                    src: 'ShiftRightFlowdown',
                    mod: $scope.constants.FLOWDOWN,
                    caption: 'Flowdown for Children',
                    help: 'Shift all children dates the same number of months to the RIGHT as the parent. Resource utilizing spread curves will be automatically updated.',
                },
                {
                    src: 'ShiftRightToPOP',
                    mod: $scope.constants.TO_POP,
                    caption: 'Children to POP',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_ALL_CHILDREN + $scope.constants.TRIPS_SHIFTED_RIGHT : $scope.constants.ALIGN_ALL_CHILDREN,
                },
                {
                    src: 'ShiftRightToStart',
                    mod: $scope.constants.TO_START,
                    caption: 'Children to Start',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_CHILDREN_START + $scope.constants.TRIPS_SHIFTED_RIGHT : $scope.constants.ALIGN_CHILDREN_START,
                }
            ]
        },
        {
            type: $scope.constants.DURATION,
            dir: $scope.constants.LEFT,
            caption: 'Duration Shrink',
            images: [
                {
                    src: 'DurationContractNoChange',
                    mod: $scope.constants.NO_CHANGE,
                    caption: 'No Change for Children',
                    help: $scope.constants.NO_CHANGE_HELP_TEXT,
                },
                {
                    src: 'DurationContractFlowdown',
                    mod: $scope.constants.FLOWDOWN,
                    caption: 'Flowdown for Children',
                    help: 'Shrink all child dates the same number of months as the parent. Resource utilizing spread curves will be automatically updated.',
                },
                {
                    src: 'DurationContractToPOP',
                    mod: $scope.constants.TO_POP,
                    caption: 'Children to POP',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_ALL_CHILDREN + $scope.constants.TRIPS_REMAIN_INPLACE : $scope.constants.ALIGN_ALL_CHILDREN,
                },
                {
                    src: 'DurationContractToEnd',
                    mod: $scope.constants.TO_END,
                    caption: 'Children to End',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_CHILDREN_CONTRACTED + $scope.constants.TRIPS_REMAIN_INPLACE : $scope.constants.ALIGN_CHILDREN_CONTRACTED,
                }
            ]
        },
        {
            type: $scope.constants.DURATION,
            dir: $scope.constants.RIGHT,
            caption: 'Duration Expand',
            images: [
                {
                    src: 'DurationExpandNoChange',
                    mod: $scope.constants.NO_CHANGE,
                    caption: 'No Change for Children',
                    help: $scope.constants.NO_CHANGE_HELP_TEXT,
                },
                {
                    src: 'DurationExpandFlowdown',
                    mod: $scope.constants.FLOWDOWN,
                    caption: 'Flowdown for Children',
                    help: 'Expand all child dates the same number of months as the parent. Resource utilizing spread curves will be automatically updated.',
                },
                {
                    src: 'DurationExpandToPOP',
                    mod: $scope.constants.TO_POP,
                    caption: 'Children to POP',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_ALL_CHILDREN + $scope.constants.TRIPS_REMAIN_INPLACE : $scope.constants.ALIGN_ALL_CHILDREN,
                },
                {
                    src: 'DurationExpandToEnd',
                    mod: $scope.constants.TO_END,
                    caption: 'Children to End',
                    help: DateShiftModel.isRMS ? $scope.constants.ALIGN_CHILDREN_EXPANDED + $scope.constants.TRIPS_REMAIN_INPLACE : $scope.constants.ALIGN_CHILDREN_EXPANDED,
                }
            ]
        }
    ];

    $scope.isImageSelected = function (type, img) {
        if (type === $scope.constants.SHIFT) {
            return $scope.shift.childModification === img.mod;
        } else {
            // duration change
            return $scope.durationChange.childModification === img.mod; 
        }
    };

    $scope.showImage = function (type, img) {
        if (type === $scope.constants.SHIFT) {
            return $scope.shift.childModification === $scope.constants.UNSELECTED || $scope.shift.childModification === img.mod;
        } else {
            // duration change
            return $scope.durationChange.childModification === $scope.constants.UNSELECTED || $scope.durationChange.childModification === img.mod;
        }
    };

    $scope.showImages = function (modification) {
        if (modification.type === $scope.constants.SHIFT) {
            if (modification.dir === $scope.constants.LEFT) {
                return $scope.showShift && $scope.shift.offset <= 0;
            } else {
                // shift right
                return $scope.showShift && $scope.shift.offset >= 0;
            }
        } else {
            // duration change
            if (modification.dir === $scope.constants.LEFT) {
                return $scope.showDuration && $scope.durationChange.offset <= 0
            } else {
                // duration change right
                return $scope.showDuration && $scope.durationChange.offset >= 0;
            }
        }

        // should never get here
        return false;
    };

    $scope.selectChildHandling = function (type, modification) {
        if ($scope.step == 2) {
            if (type === $scope.constants.SHIFT) {
                $scope.shift.childModification = modification;
            } else {
                // duration change
                $scope.durationChange.childModification = modification;
            }
            
        }
    }

    /*
     * ***************** NOTE ******************
     * Functions below relate to the Lightbox Modals
     * https://www.w3schools.com/howto/howto_js_lightbox.asp
     * *****************************************
     */

    $scope.selectedHelp = $scope.help[0];
    $scope.helpSlideIndex = 0;
    $scope.showHelpModal = false;
    // Open the Modal
    $scope.openHelpModal = function (sectionIndex) {
        $scope.selectedHelp = $scope.help[sectionIndex];
        $scope.showHelpModal = true;
    };

    // Close the Modal
    $scope.closeHelpModal = function () {
        $scope.showHelpModal = false;
    };

    // Next/previous controls
    $scope.plusHelpSlides = function (n) {
        showHelpSlides($scope.helpSlideIndex + n);
    };

    // Thumbnail image controls
    $scope.currentHelpSlide = function (n) {
        showHelpSlides($scope.helpSlideIndex = n);
    };

    var showHelpSlides = function(n) {
        if (n >= $scope.selectedHelp.images.length) {
            $scope.helpSlideIndex = 0;
        } else if (n < 0) {
            $scope.helpSlideIndex = $scope.selectedHelp.images.length - 1;
        } else {
            $scope.helpSlideIndex = n
        }
    }

    showHelpSlides($scope.helpSlideIndex);

    /*
     * ***************** NOTE ******************
     * Functions below relate to the Wizard Steps
     * 1 - choose shift / duration / both
     *   put hierarchy image either below or to right with the note
     * 2 - Do shift / duration - next / validate
     * 3 - Discrete - Validate
     * 4 - (validation good) Email Results - click finish
     * 5 - done show results click return)
     * *****************************************
     */

    $scope.backButtonDisabled = function () {
        if ($scope.step === 1 || $scope.step === 5) {
            return true;
        } else {
            return false;
        }
    };

    $scope.nextButtonDisabled = function () {
        if ($scope.isLoading) {
            return true;
        } else if ($scope.step === 1 && $scope.model.dateShiftChosen === $scope.constants.UNSELECTED) {
            return true;
        } else if ($scope.step === 2 && $scope.showDiscreteStep() && (
            // shift is not filled out
            ($scope.showShift && ($scope.shift.childModification === $scope.constants.UNSELECTED || $scope.shift.offset === 0))
            ||
            // duration is not filled out
            ($scope.showDuration && ($scope.durationChange.childModification === $scope.constants.UNSELECTED || $scope.durationChange.offset === 0)))) {
            return true;
        } else {
            return false;
        }
    };

    $scope.validateButtonDisabled = function () {
        if ($scope.step === 2 &&
            // shift is not filled out
            ($scope.showShift && ($scope.shift.childModification === $scope.constants.UNSELECTED || $scope.shift.offset === 0))
            ||
            // duration is not filled out
            ($scope.showDuration && ($scope.durationChange.childModification === $scope.constants.UNSELECTED || $scope.durationChange.offset === 0))) {
            return true;
        }
        else if ($scope.step === 3 && $scope.discrete.spreadHandling === $scope.constants.SPREADS_UNSELECTED ||
            ($scope.discrete.spreadHandling === $scope.constants.NEW_CURVE_SPREAD && $scope.discrete.newCurve === $scope.constants.NEW_CURVE_UNSELECTED)) {
            // no spread handling selected or new curve selected for handling but the New Curve itself was unselected
            return true;
        } else {
            return false;
        }
    };

    $scope.showCancelButton = function () {
        return $scope.step < 5;
    };

    $scope.showBackButton = function () {
        return $scope.step > 1 && $scope.step < 5;
    };

    $scope.showNextButton = function () {
        return $scope.step === 1 || ($scope.step === 2 && $scope.showDiscreteStep());
    }

    $scope.showDiscreteStep = function () {
        // only show the discrete step if there are discrete spreads AND one of the following
        //   The discrete spreads are outside POP (which can cause truncation)
        //   There is a duration shrink
        return $scope.model.containsDiscrete &&
            ($scope.model.discreteOutsidePop || ($scope.showDuration && $scope.durationChange.offset < 0));
    }

    $scope.showValidateButton = function () {
        return ($scope.step === 2 && !$scope.showDiscreteStep()) || $scope.step ===3;
    };

    $scope.showFinishButton = function () {
        return $scope.step === 4;
    };

    $scope.showReturnButton = function () {
        return $scope.step === 5;
    };

    $scope.next = function () {
        if (!$scope.nextButtonDisabled()) {
            switch ($scope.step) {
                case 1:
                    if ($scope.model.dateShiftChosen !== $scope.constants.UNSELECTED) {
                        if ($scope.model.dateShiftChosen === $scope.constants.SHIFT_ONLY) {
                            $scope.showShift = true;
                            $scope.showDuration = false;
                        } else if ($scope.model.dateShiftChosen === $scope.constants.DURATION_ONLY) {
                            $scope.showShift = false;
                            $scope.showDuration = true;
                        } else if ($scope.model.dateShiftChosen === $scope.constants.SHIFT_AND_DURATION) {
                            $scope.showShift = true;
                            $scope.showDuration = true;
                        }
                        $scope.step = 2;
                    }
                    break;
                case 2:
                    if ($scope.showDiscreteStep()) {
                        $scope.step = 3;
                        $timeout(function () {
                            DateShiftWidget.applyHelpPopouts();
                            $scope.isLoading = false;
                        }, 1500);
                    } else {
                        $scope.step = 4;
                    }
                    break;
                case 3:
                    $scope.step = 4;
                    break;
                case 4:
                    $scope.step = 5;
                    break;
            }
        }
    };

    $scope.back = function () {
        if (!$scope.backButtonDisabled()) {
            switch ($scope.step) {
                case 2:
                    $scope.reset();
                    $scope.step = 1;
                    break;
                case 3:
                    $scope.step = 2;
                    break;
                case 4:
                    $scope.errors = [];
                    if ($scope.showDiscreteStep()) {
                        $scope.step = 3;
                        $timeout(function () {
                            DateShiftWidget.applyHelpPopouts();
                            $scope.isLoading = false;
                        }, 1500);
                    } else {
                        $scope.step = 2;
                    }
                    break;
            }
        }
    };

    $scope.getShiftCaption = function () {
        if ($scope.showShift) {
            var shiftMod = $scope.help.find(function (mod) {
                return mod.type === $scope.constants.SHIFT && $scope.showImages(mod);
            });

            if (shiftMod !== undefined) {
                return shiftMod.caption;
            }
        }

        return '';
    };

    $scope.getShiftText = function () {
        if ($scope.showShift) {
            var shiftMod = $scope.help.find(function (mod) {
                return mod.type === $scope.constants.SHIFT && $scope.showImages(mod);
            });

            if (shiftMod !== undefined) {
                var image = shiftMod.images.find(function (img) {
                    return img.mod === $scope.shift.childModification;
                });

                if (image !== undefined) {
                    return image.caption;
                }
            }
        } 

        return '';
    };

    $scope.getDurationCaption = function () {
        if ($scope.showDuration) {
            var durationMod = $scope.help.find(function (mod) {
                return mod.type === $scope.constants.DURATION && $scope.showImages(mod);
            });

            if (durationMod !== undefined) {
                return durationMod.caption;
            }
        }

        return '';
    };

    $scope.getDurationText = function () {
        if ($scope.showDuration) {
            var durationMod = $scope.help.find(function (mod) {
                return mod.type === $scope.constants.DURATION && $scope.showImages(mod);
            });

            if (durationMod !== undefined) {
                var image = durationMod.images.find(function (img) {
                    return img.mod === $scope.durationChange.childModification;
                });

                if (image !== undefined) {
                    return image.caption;
                }
            }
        }

        return '';
    };

    $scope.getDiscreteText = function () {
        switch ($scope.discrete.spreadHandling) {
            case 4:
                return 'Discard truncated values (if any)';
            case 2:
                return 'Add truncated values (if any) to first month';
            case 3:
                return 'Add truncated values (if any) to last month';
            case 1:
                return 'New Curve - ' + $scope.getDiscreteCurveText();
            default:
                return '';
        }
    };

    $scope.getDiscreteCurveText = function () {
        if ($scope.discrete.spreadHandling === 1) {
            return $scope.model.spreadCurves.find(function (ele) {
                return ele.Value == $scope.discrete.newCurve;
            }).Text;
        } else {
            return '';
        }
    };

    $scope.getError2Text = function () {
        switch ($scope.errorHandling.error2) {
            case 0:
                return 'Child outside POP - No Change (Author to manually correct)';
            case 2:
                return 'Child outside POP - To POP';
            case 3:
                return 'Child outside POP - To Start';
            case 4:
                return 'Child outside POP - To End';
            default:
                return '';
        }
    };

    /*
     * ***************** NOTE ******************
     * Functions below relate to the Date Adjust buttons
     * *****************************************
     */


    var dateAdjust = function (validateOnly) {
        $scope.isLoading = true;
        $scope.errors = [];
        var data = {
            id: DateShiftModel.id,
            dateShiftLevel: DateShiftModel.level,
            validateOnly: validateOnly,
            dateShiftModel: {
                Details: [],
                Error1FixSingleMonth: $scope.errorHandling.error1,
                Error2Handling: $scope.errorHandling.error2,
                EmailOption: validateOnly ? $scope.constants.NO_EMAILS : $scope.model.emailOption
            }
        };

        if ($scope.showShift) {
            data.dateShiftModel.Details.push({
                Operation: 0, // shift
                ChildModificationType: $scope.shift.childModification,
                SpreadHandling: $scope.model.containsDiscrete ? $scope.showDiscreteStep() ? $scope.discrete.spreadHandling : 4 : undefined, // 4 is default for when hiding discrete step when there are discrete values but no truncation can occur
                MonthChange: $scope.shift.offset,
                NewCurve: $scope.model.containsDiscrete ? $scope.discrete.newCurve : undefined,
            });
        }

        if ($scope.showDuration) {
            data.dateShiftModel.Details.push({
                Operation: 1, // change duration
                ChildModificationType: $scope.durationChange.childModification,
                SpreadHandling: $scope.model.containsDiscrete ? $scope.showDiscreteStep() ? $scope.discrete.spreadHandling : 4 : undefined, // 4 is default for when hiding discrete step when there are discrete values but no truncation can occur
                MonthChange: $scope.durationChange.offset,
                NewCurve: $scope.model.containsDiscrete ? $scope.discrete.newCurve : undefined,
            });
        }
        $(document).trigger("SHOW_LOADING_BOX");

        // finish adjust date metadata
        $http({
            method: 'POST',
            url: CreatePostURL(DateShiftModel.workspace, DateShiftModel.controller, DateShiftModel.dateShiftAction, ''),
            data: data
        }).then(function successCallback(response) {
            $scope.isLoading = false;
            $scope.next();

            $scope.results.startDate = response.data.startDate;
            $scope.results.endDate = response.data.endDate;
            if ($scope.results.startDate != $scope.shift.startDate || $scope.results.endDate != $scope.durationChange.endDate) {
                // the original date adjust failed but continued because of error correction
                $scope.showActual = true;
            }

            $(document).trigger("HIDE_LOADING_BOX");
        }, function errorCallback(response) {
            $scope.showErrorHandling = true;
            $scope.valid = false;
            $scope.isLoading = false;
            if (response && response.data && response.data.MessageList) {
                $scope.errors = response.data.MessageList;
            }
                $(document).trigger("HIDE_LOADING_BOX");
        });

    };

    $scope.return = function () {
        // navigate back to the original page
        window.location = $scope.model.returnUrl;
    };

    $scope.shiftDateChange = function () {
        // update offset based on date change
        if ($scope.shift.startDate.isDate()) {
            var newStart = $scope.shift.startDate.toDate();
            $scope.shift.offset = $scope.startDate.getMonthsBetween(newStart);
            updateDurationTime();
        }
    };

    $scope.shiftOffsetChange = function () {
        var offset = parseInt($scope.shift.offset);
        if (!isNaN(offset)) {
            $scope.shift.offset = offset;
            var newDate = new Date($scope.startDate);
            newDate.addMonths(offset);
            $scope.shift.startDate = newDate.toFormattedString(true);
            updateDurationTime();
        }
    };

    $scope.durationDateChange = function () {
        $scope.valid = false;
        // update offset based on date change
        if ($scope.durationChange.endDate.isDate()) {
            var newEnd = $scope.durationChange.endDate.toDate();
            $scope.durationChange.offset = $scope.shift.endDate.getMonthsBetween(newEnd);
            $scope.errors = [];
        }
    };

    $scope.durationOffsetChange = function () {
        var offset = parseInt($scope.durationChange.offset);
        if (!isNaN(offset)) {
            $scope.valid = false;
            $scope.durationChange.offset = offset;
            var newDate = new Date($scope.shift.endDate);
            newDate.addMonths(offset);
            $scope.durationChange.endDate = newDate.toFormattedString(true);
            $scope.errors = [];
        }
    };

    var updateDurationTime = function () {
        $scope.errors = [];
        var newDate = new Date($scope.endDate);
        newDate.addMonths($scope.shift.offset);
        $scope.shift.endDate = newDate;
        $scope.durationOffsetChange();
    };

    $scope.validate = function () {
        if (!$scope.isLoading && !$scope.validateButtonDisabled()) {
            dateAdjust(true);
        }
    };

    $scope.finish = function () {
        if (!$scope.isLoading) {
            dateAdjust(false);
        }
    };

    $scope.clearErrors = function () {
        $scope.errors = [];
    };

    $scope.showCurves = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'curveTemplate.html',
            controller: 'DateShiftPopupController',
            windowTopClass: 'bootstrap select-curve-modal', // 
            size: 'lg',
            windowClass: 'bootstrap',
            backdropClass: 'bootstrap'
        });

        modalInstance.result.then(
            function (selectedCurve) {
                if (angular.isDefined(selectedCurve)) {
                    $scope.discrete.newCurve = selectedCurve;
                }
            },
            function () { }
        );
    };

    $scope.reset = function () {
        $scope.model.dateShiftChosen = $scope.constants.UNSELECTED;
        $scope.step = 1;
        $scope.errors = [];
        $scope.showActual = false;
        $scope.valid = false;
        $scope.showErrorHandling = false;
        $scope.isLoading = false;
        $scope.showShift = true;
        $scope.showDuration = true;
        $scope.model.emailOption = $scope.constants.NO_EMAILS;

        $scope.results = {
            startDate: '',
            endDate: ''
        };

        $scope.shift = {
            startDate: $scope.model.startDate,
            endDate: $scope.model.endDate.toDate(),
            offset: 0,
            childModification: $scope.constants.UNSELECTED
        };

        $scope.durationChange = {
            endDate: $scope.model.endDate,
            offset: 0,
            childModification: $scope.constants.UNSELECTED
        };

        $scope.discrete = {
            spreadHandling: 0,
            newCurve: '-1',
            templateUrl: 'curveTemplate.html',
            appendToBody: true,
        };

        $scope.errorHandling = {
            error1: false,
            error2: undefined
        };
    };

    $scope.reset();
    $timeout(function () {
        HelpImagesWidget.applyHelpPopouts();
        $scope.isLoading = false;
    }, 1000);

}]);
angular.module('genboe').controller('DateShiftPopupController', ['$uibModalInstance', '$scope', function ($uibModalInstance, $scope) {
    $scope.selectCurve = function (curveId) {
        $uibModalInstance.close(curveId);
    };
}]);