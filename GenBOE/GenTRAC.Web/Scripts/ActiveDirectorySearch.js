/**
* Launch the Active Directory user search dialog window.
* @param {function} - callbackFunction - Callback handler to receive the selected user's data
* @param {object} - callbackArgs - Optional set of arguments to be passed through to the callback function
* @returns {boolean} false
*/
function ActiveDirectorySearchDialog(callbackFunction, callbackArgs) {
    // dynamically generate and add a div so we can create the pop-up
    var dialogText =
        '<form id="ActiveDirectorySearchForm" action="#" onsubmit="return SubmitActiveDirectorySearch(this);">' +
        '<table class="SearchCriteria">' +
            '<tr><td colspan="2">Enter a name or account to search for:</td></tr>' +
            '<tr>' +
                    '<td>' +
                        '<input id="userSearchString" name="userSearchString" type="text" />' +
                        '<input id="selectedAccountName" name="selectedAccountName" type="hidden" />' +
                        '<input id="selectedEmail" name="selectedEmail" type="hidden" />' +
                        '<input id="selectedDisplayName" name="selectedDisplayName" type="hidden" />' +
                    '</td>' +
                    '<td><button class="ies-action" style="vertical-align: middle;" id="btnActiveDirectorySearch" type="button">Search</button></td>' +
            '</tr>' +
            '<tr>' +
                '<td colspan="2">' +
                    '<table>' +
                        '<tr>' +
                            '<td><strong>Search By:</strong></td>' +
                            '<td><strong>Search Type:</strong></td>' +
                        '</tr>' +
                        '<tr>' +
                            '<td>' +
                                // Note: "searchBy" values must match enumerated type values
                                '<input id="searchByLastName" name="searchBy" type="radio" value="LastName" checked="checked" />&nbsp;<label for="searchByLastName">Last name, first name MI</label><br />' +
                                '<input id="searchByAccount" name="searchBy" type="radio" value="Account" />&nbsp;<label for="searchByAccount">User account</label>' +
                            '</td>' +
                            '<td>' +
                                // Note: "matchBy" values must match enumerated type values
                                '<input id="matchByStartsWith" name="matchBy" type="radio" value="StartsWith" checked="checked" />&nbsp;<label for="matchByStartsWith">Begins with</label><br />' +
                                '<input id="matchByExact" name="matchBy" type="radio" value="Exact" />&nbsp;<label for="matchByExact">Exactly matches</label><br />' +
                                '<input id="matchByContains" name="matchBy" type="radio" value="Contains" />&nbsp;<label for="matchByContains">Contains</label>' +
                            '</td>' +
                        '</tr>' +
                    '</table>' +
                '</td>' +
            '</tr>' +
            '<tr><td colspan="2">&nbsp;</td></tr>' +
        '</table>' +
        '</form>' +
        '<div id="ActiveDirectorySearchResults" style="clear: both; overflow: auto;"></div>';

    $('body').append("<div id=\"ActiveDirectorySearchDialog\" style=\"display:none\" title=\"Search Users\">" + dialogText + "</div>");

    $('#btnActiveDirectorySearch').click(function () {
        SubmitActiveDirectorySearch($('#ActiveDirectorySearchForm'));
    });

    // define/configure the modal pop-up
    $("#ActiveDirectorySearchDialog").dialog({
        draggable: true,
        resizable: true,
        width: 700,
        modal: false,
        height: 500,
        open: function () {
            $("#ActiveDirectorySearchDialog ~ div.ui-dialog-buttonpane button:eq(0)").focus().addClass('ui-state-focus');
            $(this).css({ 'max-height': $(document).height() - 200, 'overflow-y': 'auto' });
            $('#userSearchString').focus();  // put focus on name field
        },
        buttons: {}
    }).bind('dialogbeforeclose', function (event, ui) {
        // Close (X) button was clicked; NOT the OK button
        if (callbackFunction != undefined) {
            var selectedAccountName = $('#selectedAccountName').val();
            var selectedEmail = $('#selectedEmail').val();
            var selectedDisplayName = $('#selectedDisplayName').val();

            var results = { "AccountName": selectedAccountName, "Email": selectedEmail, "DisplayName": selectedDisplayName };
            callbackFunction(results, callbackArgs);
        }
    }).bind('dialogclose', function () {
        $(this).dialog("destroy").remove();
    });

    // 'Contains' match by option is not available when searching by 'Account' as this would often lead to a timeout
    //  and is not recommended when searching Active Directory.
    $('#searchByAccount').click(function () {
        $('#matchByContains').prop("disabled", true);
        $("label[for='matchByContains']").addClass("disabled");

        // If 'Contains' is selected and the search by option is changed to 'Account', automatically change the
        //  match by option to 'StartsWith' since 'Contains' is now invalid.
        if ($('#matchByContains').is(':checked')) {
            $('#matchByStartsWith').prop("checked", true);
        }
    });

    // 'Contains' match by option is available when searching by 'Last Name'.
    $('#searchByLastName').click(function () {
        $('#matchByContains').prop("disabled", false);
        $("label[for='matchByContains']").removeClass("disabled");
    });

    $('#ActiveDirectorySearchDialog').dialog("open");

    return false;
}

/**
* Trigger processing of the selected user's data (copy data values into hidden variables on the ActiveDirectorySearch view.
* Note: This function is for private use only and is triggered from the user link on the Active Directory Search results view.
* @param {object} - results - The selected user's data values
* @returns {boolean} false
*/
function ProcessActiveDirectorySelection(results) {
    // see hidden variable declarations in ActiveDirectorySearchDialog function (above)
    $('#selectedAccountName').val(results.AccountName);
    $('#selectedEmail').val(results.Email);
    $('#selectedDisplayName').val(results.DisplayName);

    $("#ActiveDirectorySearchDialog").dialog('close');

    return false;  // prevent clicked anchor (selected user) from triggering navigation
}

/**
* Trigger the lookup and load the results to the ActiveDirectorySearch view.
* Note: This function is for private use only and is triggered from the Search button on the ActiveDirectorySearchDialog.
* @param {object} - btn - The Search button on the ActiveDirectorySearch view
* @returns {boolean} false
*/
function SubmitActiveDirectorySearch(form) {
    var prefix = GenSession.getToolPrefix();

    var irisurl = "/" + prefix + "Home/Search?" + $(form).serialize();
    $('div#ActiveDirectorySearchResults').html('<div class="loader"></div>');
    $('div#ActiveDirectorySearchResults').load(irisurl, function () {
        //HideSpinner();
    });

    return false;  // prevent form element from triggering action
}
