/* Create a POST url via parameters */
function createPostURL(controllerName, actionName, id, portionMarkingRequired) {
    var postURL = window.location.protocol + '//' + window.location.host + '/' +
        controllerName + '/';

    if (actionName != undefined) {
        postURL += actionName;

        if (id != undefined) {
            postURL += '/' + id;
        }

        if (portionMarkingRequired != undefined) {
            postURL += '/?portionMarkingRequired=' + portionMarkingRequired.toString();
        }

    }

    return postURL;
}

/**
    * Displays Exception Dialog.
    * 
    * @param error
    *            to display.
    */
DisplayExceptionDialog = function (error) {
    $("#ErrorText").html(error.Message.replace(/\n/g, '<br/>'));
    $("#ErrorDetails").html(error.Details.replace(/\n/g, '<br/>')).toggle(false); // hide by default
    $("#ExceptionDialog").dialog({
        width: 400,
        modal: true,
        resizable: true,
        draggable: true,
        title: error.Title,
        zIndex: 2000
    });
};

// no toggle for details
DisplaySimpleExceptionDialog = function (error) {
    $("#ErrorText").html(error.Message.replace(/\n/g, '<br/>'));
    $("#ToggleErrorDetails").hide();
    $("#ErrorDetails").hide();
    $("#ExceptionDialog").dialog({
        width: 400,
        modal: true,
        resizable: true,
        draggable: true,
        title: error.Title
    });
};

function CommonDialog(title, body, buttons, inWidth, closeBoxCallback, onCloseCallback) {

    $("#CommonDialog .buttons").text("");

    if (buttons != null) {
        for (x in buttons) {
            if (buttons.hasOwnProperty(x)) {
                if (typeof (buttons[x].ButtonText) === 'undefined') {
                    $("#CommonDialog .buttons").append('<div class="' + buttons[x].buttonClass + '"></div>');

                    // Close must occur before other callbackMethods.  If the order is reversed, you cannot trigger the display of a new dialog from within the
                    //  callback of a different dialog.
                    $("#CommonDialog .buttons" + " ." + buttons[x].buttonClass).click(CloseCommonDialog);
                    if (buttons[x].callbackMethod != null) {
                        $("#CommonDialog .buttons" + " ." + buttons[x].buttonClass).click(buttons[x].callbackMethod);
                    }
                } else {
                    var buttonName = buttons[x].ButtonName !== 'undefined' ? buttons[x].ButtonName : buttons[x].ButtonText.replace(/ /g, '');

                    $("#CommonDialog .buttons").append('<button class="' + buttons[x].buttonClass + '" name="' + buttonName + '" type="button">' + buttons[x].ButtonText + '</button>');

                    // Close must occur before other callbackMethods.  If the order is reversed, you cannot trigger the display of a new dialog from within the
                    //  callback of a different dialog.
                    $("#CommonDialog .buttons" + " button[name='" + buttonName + "']").click(CloseCommonDialog);
                    if (buttons[x].callbackMethod != null) {
                        $("#CommonDialog .buttons" + " button[name='" + buttonName + "']").click(buttons[x].callbackMethod);
                    }
                }
            }
        }
    }

    $("#CommonDialogBody").html(body);
    $("#CommonDialog").dialog({
        width: inWidth || 450,
        minHeight: 50,
        modal: true,
        resizable: false,
        draggable: true,
        title: title,
        beforeClose: function (evt, ui) {
            // was the close triggered by the close box?
            var triggeredByCloseBox = $(evt.currentTarget).hasClass('ui-dialog-titlebar-close');
            if (triggeredByCloseBox) {
                if (closeBoxCallback) {
                    closeBoxCallback();
                }
            }
        },
        close: function (event, ui) {
            if (onCloseCallback) {
                onCloseCallback();
            }
        }
    });
};

/**
   * close common dialog.
   */
function CloseCommonDialog() {
    $('#CommonDialog').dialog('close');
};

/**
    * a common confirm dialog.
    * 
    * @param title
    *            title of the dialog
    * @param body
    *            the message for the body
    * @param acceptCallback
    *            optional method for the yes button.
    * @param cancelCallback
    *            optional method for the no button.
    * @param closeBoxCallback
    *            optional callback method for when the close-box is clicked
    * @param {function}
    *            onCloseCallback optional callback function to be invoked after the dialog has closed
    */
function ConfirmDialog(title, body, acceptCallback, cancelCallback, closeBoxCallback, onCloseCallback) {
    CommonDialog(title, body, [{
        buttonClass: "ies",
        ButtonText: 'Yes',
        ButtonName: 'yes-button',
        callbackMethod: acceptCallback
    }, {
        buttonClass: "ies",
        ButtonText: 'No',
        ButtonName: 'no-button',
        callbackMethod: cancelCallback
    }], 450, closeBoxCallback, onCloseCallback);
};

/**
* an alert dialog.
* 
* @param title
*            title of the dialog
* @param alert
*            the message for the body
* @param okCallback
*            optional method for the ok button.
* @param closeBoxCallback
*            optional callback method for when the close-box is clicked
* @param {function}
*            onCloseCallback optional callback function to be invoked after the dialog has closed
*/
function AlertDialog(title, alert, okCallback, closeBoxCallback, onCloseCallback) {
    CommonDialog(title, alert, [{
        buttonClass: 'ies',
        ButtonText: 'OK',
        ButtonName: "ok-button",
        callbackMethod: okCallback
    }], 450, closeBoxCallback, onCloseCallback);
};

DisplayUnderConstruction = function () {
    if (window.location.hostname.toString() != "localhost") {
        $("#UnderConstruction").dialog({
            width: 400,
            height: 110,
            modal: true,
            resizable: true,
            draggable: true
        });
    }
}

/*
    * Download a file from the specified URL.
    * 
    * @param targetIFrameName
    *            The name to use in the ID of the IFrame so that we can keep the DOM clean and not add duplicate IFrames.
    * @param controller
    *            The controller for the URL.
    * @param action
    *            (Optional) The action for the URL.
    * @param id
    *            (Optional) The id for the URL.
    * @param portionMarkingRequired
    *            (Optional) The boolean option for portion marking for the URL.
*/
function DownloadFile(targetIFrameName, controller, action, id, portionMarkingRequired) {
    // If an IFrame already exists, remove it so we don't clog-up the DOM by creating duplicates.
    if ($("iframe#DownloadFileTarget-" + targetIFrameName).length > 0) {
        $("iframe#DownloadFileTarget-" + targetIFrameName).remove();
    }

    // Create a new hidden IFrame.  Set its ID to the provided targetIFrameName and its source to the file's URL.
    var targetIFrame = $('<iframe />', {
        'id': 'DownloadFileTarget-' + targetIFrameName,
        'class': 'display-none',
        'src': createPostURL(controller, action, id, portionMarkingRequired)
    });

    // Append the IFrame to the body, causing the file download to occur within the IFrame.
    targetIFrame.appendTo('body');
}

/**
    * Closes exception dialog.
    */
this.CloseExceptionDialog = function () {
    $('#ExceptionDialog').dialog('close');
};

$(document).bind('SHOW_LOADING_BOX', function () {
    $('#PageLoading').removeClass('display-none');
});

$(document).bind('HIDE_LOADING_BOX', function () {
    $('#PageLoading').addClass('display-none');
});

//https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/String/endsWith
if (!String.prototype.endsWith) {
    String.prototype.endsWith = function (search, this_len) {
        if (this_len === undefined || this_len > this.length) {
            this_len = this.length;
        }
        return this.substring(this_len - search.length, this_len) === search;
    };
}

window.addEventListener('message', function (e) {
    var iframe = $(".bannerframe");
    var eventName = e.data[0];
    var data = e.data[1];
    switch (eventName) {
        case 'setHeight':
            iframe.height(data);
            break;
        case 'appOffline':
            if (data === true) {
                iframe = $(".appOfflineFrame");
                iframe.height("100%");
                iframe.css("position", "fixed");
                iframe.css("z-index", "99999");
            }
            break;
    }
}, false);