function ChecklistTypeChangeConfirmationDialog(callbackFunction, callbackArgs) {
    var dialogText =
        '<div class="dialog-text">' +
            '<div class="form-row">' +
                '<div class="form-element">' +
                    '<span>The selection you have made will alter the type of checklist that you are currently working with.  If you continue and save this update, your checklist will be replaced with the designated checklist and all responses will be removed.</span>' + 
                '</div>' +
            '</div> ' +
            '<div class="form-row">' +
                '<div class="form-element">' +
                    '<span>If your proposal is currently &#34Completed&#34 it will be set back to &#34In Progress.&#34</span>' +
                '</div>' +
            '</div>' +
            '<div class="form-row">' +
                '<div class="form-element">' +
                    '<span> Would you like to continue with this update?</span>' +
                '</div>' +
            '</div>' +
            '<div class="form-row checklist-type-change-btn-row">' +
                '<div class="form-element">' +
                    '<button id="ChecklistTypeChangeYesButton" name="yes-button" class="ies checklist-type-change-yes-btn" type="button">Yes</button>' +
                    '<button id="ChecklistTypeChangeNoButton" name="no-button" class="ies checklist-type-change-no-btn" type="button">No</button>' +
                '</div>' +
            '</div>' +
        '</div>';

    $('body').append("<div id=\"ChecklistTypeChangeConfirmationDialog\" class=\"ui-dialog-content ui-widget-content\">" + dialogText + "</div>");

    $('#ChecklistTypeChangeConfirmationDialog').dialog({
        width: 450,
        height: 225,
        modal: true,
        title: "Proposal Checklist Type Update Verification",
        closeOnEscape: false,
        open: function (even, ui) {
            $(".ui-dialog-titlebar-close").hide();

            $(".ui-dialog-titlebar-close").blur();
        }
    });

    $('#ChecklistTypeChangeNoButton').click(function () {
        if (!$(this).hasClass('disabled')) {
            $('#ChecklistTypeChangeConfirmationDialog').dialog("destroy").remove();
            callbackFunction(true);
        }
    });

    $('#ChecklistTypeChangeYesButton').click(function () {
        $('#ChecklistTypeChangeConfirmationDialog').dialog("destroy").remove();
        callbackFunction(false);
    });

    $('#ChecklistTypeChangeConfirmationDialog').dialog("open");
}

