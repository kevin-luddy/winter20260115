function ITCOConfirmationDialog(callbackFunction, callbackArgs) {
    var dialogText =
        '<div class="dialog-text">' +
            '<div class="form-row">' +
                '<div class="form-element">' +
                    '<input type="radio" class="radio ignore-dirty itco-radio-btn" name="ITCOSelection" id="ITCOVerifyTrue"/>' +
                    '<label for="ITCOVerifyTrue" class="itco-radio-btn-label">I verify that I have contacted my non-US International Trade Compliance Office (ITCO), and received approval to add this proposal record.</label>' +
                '</div>' +
            '</div> ' +
            '<div class="form-row">' +
                '<div class="form-element">' +
                    '<input type="radio" class="radio ignore-dirty itco-radio-btn" name="ITCOSelection" id="ITCOVerifyFalse"/>' +
                    '<label for="ITCOVerifyFalse" class="itco-radio-btn-label">I have not received approval from my non-US ITCO to add this proposal record.</label>' +
                '</div>' +
            '</div>' +
            '<div class="form-row itco-btn-row">' +
                '<div class="form-element">' +
                    '<button id="ITCOOkButton" name="ok-button" class="ies itco-ok-btn" type="button">OK</button>' +
                    '<button id="ITCOCancelButton" name="cancel-button" class="ies itco-cancel-btn" type="button">Cancel</button>' +
                '</div>' +
            '</div>' +
'</div>';

    $('body').append("<div id=\"ITCOConfirmationDialog\" class=\"ui-dialog-content ui-widget-content\">" + dialogText + "</div>");

    $('#ITCOConfirmationDialog').dialog({
            width: 425,
            height: 180,
            modal: true,
            title: "Approval Verification",
            closeOnEscape: false,
            open: function (even, ui)
            {
                $(".ui-dialog-titlebar-close").hide();

                // all previous radio button selections should be cleared
                $('#ITCOConfirmationDialog :input[type=radio]').each(function () {
                    this.checked = false;
                });

                // Disable the OK button
                $('#ITCOOkButton').addClass('disabled');
            }
    });

    $('#ITCOVerifyTrue').click(function () {
        EnableItcoOkButton(true);
    });

    $('#ITCOVerifyFalse').click(function () {
        EnableItcoOkButton(false);
    });

    $('#ITCOOkButton').click(function () {
        if (!$(this).hasClass('disabled')) {
            $('#ITCOConfirmationDialog').dialog("destroy").remove();
            callbackFunction(true);
        }
    });

    $('#ITCOCancelButton').click(function () {
        $('#ITCOConfirmationDialog').dialog("destroy").remove();
        callbackFunction(false);
    });
    
    $('#ITCOConfirmationDialog').dialog("open");
}

/*
* Enable/Disable Ok button based on ITCO Dialog.
*/
function EnableItcoOkButton(isEnabled) {
    var okButton = $('#ITCOOkButton');
    if (isEnabled) {
        okButton.removeClass('disabled');
    } else {
        okButton.addClass('disabled');
    }
}

