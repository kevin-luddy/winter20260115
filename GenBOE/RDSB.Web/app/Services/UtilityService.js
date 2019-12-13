angular.module("rdsb").factory("utilityService", [function () {
    var utilityService = {
        /* 
           Makes a uibModal dialog draggable and resizable 
           Parameters:
               selector - JQuery selector used to find the dialog element in the DOM.
                           Note: uibModal wraps this element with an outer <div class="modal"> element. 
               minHeight - Minimum height when resizing the dialog.
               minWidth - Minimum width when resizing the dialog.
       */
        makeModalDraggableAndResizable: function (selector, minHeight, minWidth) {
            var $modal = $(selector).closest('.modal');
            $modal.find('.modal-content').resizable({
                minHeight: minHeight,
                minWidth: minWidth
            });
            $modal.find('.modal-dialog').draggable();
        }
    };

    return utilityService;
}]);