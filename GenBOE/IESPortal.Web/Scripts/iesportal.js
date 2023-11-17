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
};

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

function CommonDialog (title, body, buttons, inWidth, closeBoxCallback, onCloseCallback) {

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
function CloseCommonDialog () {
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
function ConfirmDialog (title, body, acceptCallback, cancelCallback, closeBoxCallback, onCloseCallback) {
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
function AlertDialog (title, alert, okCallback, closeBoxCallback, onCloseCallback) {
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

/**
    * Closes exception dialog.
    */
CloseExceptionDialog = function () {
    $('#ExceptionDialog').dialog('close');
};

$(document).bind('SHOW_LOADING_BOX', function () {
    $('#PageLoading').removeClass('display-none');
});

$(document).bind('HIDE_LOADING_BOX', function () {
    $('#PageLoading').addClass('display-none');
});

// Add Array.includes for Internet Explorer
// https://tc39.github.io/ecma262/#sec-array.prototype.includes
if (!Array.prototype.includes) {
    Object.defineProperty(Array.prototype, 'includes', {
        value: function (searchElement, fromIndex) {

            if (this == null) {
                throw new TypeError('"this" is null or not defined');
            }

            // 1. Let O be ? ToObject(this value).
            var o = Object(this);

            // 2. Let len be ? ToLength(? Get(O, "length")).
            var len = o.length >>> 0;

            // 3. If len is 0, return false.
            if (len === 0) {
                return false;
            }

            // 4. Let n be ? ToInteger(fromIndex).
            //    (If fromIndex is undefined, this step produces the value 0.)
            var n = fromIndex | 0;

            // 5. If n ≥ 0, then
            //  a. Let k be n.
            // 6. Else n < 0,
            //  a. Let k be len + n.
            //  b. If k < 0, let k be 0.
            var k = Math.max(n >= 0 ? n : len - Math.abs(n), 0);

            function sameValueZero(x, y) {
                return x === y || (typeof x === 'number' && typeof y === 'number' && isNaN(x) && isNaN(y));
            }

            // 7. Repeat, while k < len
            while (k < len) {
                // a. Let elementK be the result of ? Get(O, ! ToString(k)).
                // b. If SameValueZero(searchElement, elementK) is true, return true.
                if (sameValueZero(o[k], searchElement)) {
                    return true;
                }
                // c. Increase k by 1. 
                k++;
            }

            // 8. Return false
            return false;
        }
    });
}

// https://tc39.github.io/ecma262/#sec-array.prototype.findIndex
if (!Array.prototype.findIndex) {
    Object.defineProperty(Array.prototype, 'findIndex', {
        value: function (predicate) {
            // 1. Let O be ? ToObject(this value).
            if (this == null) {
                throw new TypeError('"this" is null or not defined');
            }

            var o = Object(this);

            // 2. Let len be ? ToLength(? Get(O, "length")).
            var len = o.length >>> 0;

            // 3. If IsCallable(predicate) is false, throw a TypeError exception.
            if (typeof predicate !== 'function') {
                throw new TypeError('predicate must be a function');
            }

            // 4. If thisArg was supplied, let T be thisArg; else let T be undefined.
            var thisArg = arguments[1];

            // 5. Let k be 0.
            var k = 0;

            // 6. Repeat, while k < len
            while (k < len) {
                // a. Let Pk be ! ToString(k).
                // b. Let kValue be ? Get(O, Pk).
                // c. Let testResult be ToBoolean(? Call(predicate, T, « kValue, k, O »)).
                // d. If testResult is true, return k.
                var kValue = o[k];
                if (predicate.call(thisArg, kValue, k, o)) {
                    return k;
                }
                // e. Increase k by 1.
                k++;
            }

            // 7. Return -1.
            return -1;
        }
    });
}

// https://tc39.github.io/ecma262/#sec-array.prototype.find
if (!Array.prototype.find) {
    Object.defineProperty(Array.prototype, 'find', {
        value: function (predicate) {
            // 1. Let O be ? ToObject(this value).
            if (this == null) {
                throw new TypeError('"this" is null or not defined');
            }

            var o = Object(this);

            // 2. Let len be ? ToLength(? Get(O, "length")).
            var len = o.length >>> 0;

            // 3. If IsCallable(predicate) is false, throw a TypeError exception.
            if (typeof predicate !== 'function') {
                throw new TypeError('predicate must be a function');
            }

            // 4. If thisArg was supplied, let T be thisArg; else let T be undefined.
            var thisArg = arguments[1];

            // 5. Let k be 0.
            var k = 0;

            // 6. Repeat, while k < len
            while (k < len) {
                // a. Let Pk be ! ToString(k).
                // b. Let kValue be ? Get(O, Pk).
                // c. Let testResult be ToBoolean(? Call(predicate, T, « kValue, k, O »)).
                // d. If testResult is true, return kValue.
                var kValue = o[k];
                if (predicate.call(thisArg, kValue, k, o)) {
                    return kValue;
                }
                // e. Increase k by 1.
                k++;
            }

            // 7. Return undefined.
            return undefined;
        }
    });
}