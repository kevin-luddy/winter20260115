// ---------------------------------------------------------------------------------------------
// Name:           User Lookup jQuery plugin
//
// Description:    Supports user lookup.
//
// Usage:          Widget should be created against the Display Name input element.
//
// Events:
//                 onDestroy - Fired when the widget is destroyed
//
// Dependencies:
//                 GenSession
//                 ActiveDirectorySearch.js
//
// ---------------------------------------------------------------------------------------------
//                                          Change Log
// ---------------------------------------------------------------------------------------------
// DATE        AUTHOR      DESCRIPTION
// ----------  ----------  ------------------------------------------------------------------
// 03/29/2013  goodwik1    Initial development (copied from genCAT)
// ---------------------------------------------------------------------------------------------

(function ($, undefined) {
    $.widget("ui.lookupUser", {
        options: {
            onDestroy: undefined,                    // Event handler called when widget is destroyed
            accountNameElementId: undefined,         // Id/name used for the User Name input element
            accountNameInitial: '',                  // Initial value of the User Name
            accountDisplayNameInitial: '',           // Initial value of the User Display Name
            enabled: true,                           // Enabled/disabled flag
            allowGroups: true,                       // Whether to support selection of group accounts
            fieldDisplayName: '',                    // Name to use to identify the entry field in error messages
            checkNameCallbackHandler: undefined      // Callback handler for the checkName operation
        },

        checkNameAnchor: undefined,                  // local variable to reference the Check Name anchor object
        lookupUserAnchor: undefined,                 // local variable to reference the Lookup anchor object
        displayNameElementId: undefined,             // Id/name used for the Display Name input element
        changeTriggeredBySearchDialog: false,       // flag indicating that the Lookup User search dialog is triggering the change event

        _create: function () {
            this.element.attr({ role: "lookupUser" });

            var self = this;
            var obj = this.element;

            this.displayNameElementId = obj.attr('id');

            var divContainer = $('<div style="display: inline-block; margin-left: 5px;" class="userlinks" />');

            this.checkNameAnchor = $('<a title="Clicking on Check Name will verify the NTID you entered" name="CheckNameLink">Check Name</a>');
            this.lookupUserAnchor = $('<a title="Lookup">Lookup ...</a>');

            divContainer.append(this.checkNameAnchor);
            divContainer.append('&nbsp;&nbsp;|&nbsp;&nbsp;');
            divContainer.append(this.lookupUserAnchor);

            obj.after(divContainer);

            // If the input boxes don't already exist, we want to create them. Otherwise, leave them be
            var otherInputsHtml = '';
            if ($('#' + this.options.accountNameElementId).attr('name') == undefined) {
                otherInputsHtml = '<input id="' + this.options.accountNameElementId + '" name="' + this.options.accountNameElementId + '" value="' + this.options.accountNameInitial + '" type="hidden" placeholder="NTID" class="half" />';
            }

            if (this.options.accountDisplayNameInitial != '') {
                $('#' + self.displayNameElementId).val(this.options.accountDisplayNameInitial);
            }

            obj.after(otherInputsHtml);

            this.checkNameAnchor.click(function () {
                self.checkName();
            });

            this.lookupUserAnchor.click(function () {
                self._lookupUser(
                    $('#' + self.displayNameElementId),
                    $('#' + self.options.accountNameElementId));

                return false;
            });

            $('#' + self.displayNameElementId).change(function () {
                if (!self.changeTriggeredBySearchDialog) {
                    var newValue = $(this).val();

                    $('#' + self.options.accountNameElementId).val(newValue);

                    if (newValue == '') {
                        $(this).removeAttr('title');
                    }

                    $(this).data('isDirty', true);
                }
                self.changeTriggeredBySearchDialog = false;
            });

            this.setEnabled(this.options.enabled);
        },

        _init: function () {
            return;
        },

        checkName: function () {
            var theCheckNameAnchor = this.checkNameAnchor;
            var theCheckNameCallbackHandler = this.options.checkNameCallbackHandler;
            var beforeAccountName = $('#' + this.options.accountNameElementId).val();

            var theAccountNameElementId = this.options.accountNameElementId;
            var theDisplayNameElementId = this.displayNameElementId;

            var isDirty = $('#' + theDisplayNameElementId).data('isDirty');

            this._checkUserName(
                $('#' + theDisplayNameElementId),
                $('#' + theAccountNameElementId),
                this.options.allowGroups,
                this.options.fieldDisplayName,
                // callback handler
                function (isValidUser, newDirtyBitValue) {
                    if (newDirtyBitValue != undefined) {
                        $('#' + theDisplayNameElementId).data('isDirty', newDirtyBitValue);
                        isDirty = newDirtyBitValue;
                    }

                    if (!isValidUser) {
                        // set the NTID to the DisplayName (if and only) if it is not already set
                        if (beforeAccountName == '' || (isDirty == true)) {
                            $('#' + theAccountNameElementId).val($('#' + theDisplayNameElementId).val());
                            $('#' + theDisplayNameElementId).removeAttr('title');
                        }
                    }

                    // invoke the user-defined checkName callback
                    if (theCheckNameCallbackHandler != undefined) {
                        theCheckNameCallbackHandler(isValidUser);
                    }
                });

            return false;
        },

        destroy: function () {
            var obj = this.element;

            // unbind click event handlers
            this.checkNameAnchor.unbind('click');
            this.lookupUserAnchor.unbind('click');

            // remove the wrapper div - root container will take children with it
            var divContainer = this.checkNameAnchor.parent('div');
            divContainer.remove();

            $.Widget.prototype.destroy.apply(this, arguments);

            // call the destroy handler (for custom post-processing of the object)
            if (this.options.onDestroy != undefined) {
                this.options.onDestroy(obj);
            }
        },

        _setOption: function (key, value) {
            switch (key) {
                case "onDestroy":
                    this.options.onDestroy = value;
                    break;
                case "accountNameElementId":
                    this.options.accountNameElementId = value;
                    break;
                case "accountNameInitial":
                    this.options.accountNameInitial = value;
                    break;
                case "accountDisplayNameInitial":
                    this.options.accountDisplayNameInitial = value;
                    break;
                case "enabled":
                    this.setEnabled(value);
                    break;
                case "allowGroups":
                    this.options.allowGroups = value;
                    break;
                case "fieldDisplayName":
                    this.options.fieldDisplayName = value;
                    break;
                case "checkNameCallbackHandler":
                    this.options.checkNameCallbackHandler = value;
                    break;
            }

            $.Widget.prototype._setOption.apply(this, arguments);
        },

        setEnabled: function (value) {
            this.options.enabled = value;

            if (value) {
                this.checkNameAnchor.removeAttr('disabled');
                this.lookupUserAnchor.removeAttr('disabled');
            } else {
                this.checkNameAnchor.attr('disabled', 'disabled');
                this.lookupUserAnchor.attr('disabled', 'disabled');
            }
        },

        isEnabled: function () {
            return this.options.enabled;
        },

        toggle: function () {
            this.setEnabled(!this.options.enabled);
        },

        hide: function () {
            var divContainer = this.checkNameAnchor.parent('div');
            divContainer.hide();
            return;
        },

        show: function () {
            var divContainer = this.checkNameAnchor.parent('div');
            divContainer.show();
            return;
        },

        _lookupUser: function (displayNameElement, accountNameElement) {
            this.changeTriggeredBySearchDialog = true;
            ActiveDirectorySearchDialog(function (results) {
                var selectedAccountName = results.AccountName;
                var selectedDisplayName = results.DisplayName;
                if (selectedAccountName != '') {
                    accountNameElement.val(selectedAccountName).hide();
                    displayNameElement.val(selectedDisplayName).show();
                    displayNameElement.attr('title', selectedAccountName);
                    displayNameElement.change();

                    displayNameElement.data('isDirty', false);
                }
            });
        },

        _checkUserName: function (displayNameElement, accountNameElement, allowGroupAccounts, fieldName, callbackHandler) {
            var userid = displayNameElement.val();  // i.e. user has replaced display name with userid

            if (userid) {
                var dataToSend = '{ "userAccount" : "' + $.trim(userid) + '" }';
                var searchUrl = '/' + GenSession.getToolPrefix() + 'Home/SearchUserName';

                $.ajax({
                    type: 'POST',
                    url: searchUrl,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'json',
                    data: dataToSend,
                    success: function (data, textStatus, jqXHR) {
                        var response = data;

                        var isValidAccount = false;
                        var errorMessage = null;

                        if (response.success) {
                            if (!allowGroupAccounts && response.isGroup) {
                                isValidAccount = false;
                                errorMessage = fieldName + ' must be a valid individual and not a group.';

                            } else {
                                isValidAccount = true;
                            }
                        } else {
                            isValidAccount = false;
                            errorMessage = response.error;
                        }

                        if (isValidAccount) {
                            displayNameElement.val(response.userFullName);
                            accountNameElement.val(response.userAccount);
                            displayNameElement.attr('title', response.userAccount);

                            accountNameElement.hide();
                            displayNameElement.show();

                            callbackHandler(true, false);

                        } else {
                            callbackHandler(false);

                            alert(errorMessage);
                        }
                    },
                    error: function (jqXHR, textStatus, errorThrown) {
                        var response = jqXHR.responseText;

                        callbackHandler(false);

                        alert("There was an error on the name lookup.");
                    }
                });
            }
        }

    });  // end $.widget

    $.extend($.ui.lookupUser, {
        version: "1.8.18"
    });

})(jQuery);