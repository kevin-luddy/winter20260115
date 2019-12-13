// ---------------------------------------------------------------------------------------------
// Name:           User Lookup jQuery plugin
//
// Description:    Supports user lookup.
//
// Usage:          Widget should be created against the Display Name input element.
//
// Events:
//                 onDestroy - Fired when the widget is destroyed
//                 onChange - Fired when the selected user(s) is/are changed
//                 checkNameCallbackHandler - Fired after Check User is clicked, to report whether user is valid or not
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
// 07/23/2012  sjrosent    Initial development
// 05/22/2013  sjrosent    Add readOnly option
// 05/23/2013  sjrosent    Merge latest "master copy" changes from genCAT
// 10/25/2016  twilson3    Added Work Phone
// 11/29/2017  pattoncr    Removing domain.
// ---------------------------------------------------------------------------------------------

(function ($, undefined) {
    $.widget("ui.lookupUser", {
        options: {
            onDestroy: undefined,                    // Event handler called when widget is destroyed
            onChange: undefined,                     // Event handler called when selected user(s) is/are changed
            accountNameElementId: undefined,         // Id/name used for the User Account Name input element
            accountWorkPhoneElementId: undefined,             // Id/name used for the User Work Phone input element
            accountNameInitial: '',                  // Initial value of the User Account Name
            accountDisplayNameInitial: '',           // Initial value of the User Display Name
            accountWorkPhoneInitial: '',             // Initial value of the User Work Phone
            enabled: true,                           // Enabled/disabled flag
            readOnly: false,                         // Read-only display flag
            allowGroups: true,                       // Whether to support selection of group accounts
            fieldDisplayName: '',                    // Name to use to identify the entry field in error messages
            checkNameCallbackHandler: undefined      // Callback handler for the checkName operation
        },

        checkNameAnchor: undefined,                  // local variable to reference the Check Name anchor object
        lookupUserAnchor: undefined,                 // local variable to reference the Lookup anchor object
        displayNameElementId: undefined,             // Id/name used for the Display Name input element

        _create: function () {
            this.element.attr({ role: "lookupUser" });

            var self = this;
            var obj = this.element;

            this.displayNameElementId = obj.attr('id');

            var divContainer = $('<div style="display: inline-block; margin-left: 5px;" />');

            this.checkNameAnchor = $('<a href="#" title="Clicking on Check Name will verify the NTID you entered" name="CheckNameLink">Check Name</a>');
            this.lookupUserAnchor = $('<a href="#" title="Lookup">Lookup ...</a>');

            divContainer.append(this.checkNameAnchor);
            divContainer.append('&nbsp;&nbsp;|&nbsp;&nbsp;');
            divContainer.append(this.lookupUserAnchor);

            obj.after(divContainer);

            // If the input boxes don't already exist, we want to create them. Otherwise, leave them be
            var otherInputsHtml = '<div id="ReadOnly' + self.displayNameElementId + '" style="display: none">' + this.options.accountDisplayNameInitial + '</div>';
            if ($('#' + this.options.accountNameElementId).attr('name') == undefined) {
                otherInputsHtml +=
                    '<input id="' + this.options.accountNameElementId + '" name="' + this.options.accountNameElementId + '" value="' + this.options.accountNameInitial + '" type="hidden" placeholder="NTID" class="half" />'
            }

            if (this.options.accountDisplayNameInitial != '') {
                $('#' + self.displayNameElementId).val(this.options.accountDisplayNameInitial);
            }

            obj.after(otherInputsHtml);

            this.checkNameAnchor.click(function () {
                self.checkName();
                return false;
            });

            this.lookupUserAnchor.click(function () {
                self._lookupUser(
                    $('#' + self.displayNameElementId),
                    $('#' + self.options.accountNameElementId),
                    $('#' + self.options.accountWorkPhoneElementId));

                return false;
            });

            $('#' + self.displayNameElementId).change(function () {
                var newValue = $(this).val();

                $('#' + self.options.accountNameElementId).val(newValue);
                $('#' + self.options.accountWorkPhoneElementId).val('');

                $(this).data('isDirty', true);
            });

            this.setEnabled(this.options.enabled);

            if (this.options.readOnly) {
                this.hide();  // hide the links

                // show the display name
                $('#ReadOnly' + self.displayNameElementId).css('display', 'inline-block');
            }
        },

        _init: function () {
            return;
        },

        checkName: function () {
            var theCheckNameAnchor = this.checkNameAnchor;
            var theCheckNameCallbackHandler = this.options.checkNameCallbackHandler;
            var theOnChangeHandler = this.options.onChange;
            var beforeAccountName = $('#' + this.options.accountNameElementId).val();

            var theAccountNameElementId = this.options.accountNameElementId;
            var theDisplayNameElementId = this.displayNameElementId;
            var theWorkPhoneElementId = this.options.accountWorkPhoneElementId;

            var isDirty = $('#' + theDisplayNameElementId).data('isDirty');

            this._checkUserName(
                $('#' + theDisplayNameElementId),
                $('#' + theAccountNameElementId),
                $('#' + theWorkPhoneElementId),
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
                            $('#' + theWorkPhoneElementId).val('')
                        }
                    }

                    // invoke the user-defined checkName callback
                    if (theCheckNameCallbackHandler != undefined) {
                        theCheckNameCallbackHandler(isValidUser);
                    }

                    // a selection was made - does this need to consider valid vs. invalid - only if original value is replaced
                    if (theOnChangeHandler != undefined) {
                        theOnChangeHandler();
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
                case "onChange":
                    this.options.onChange = value;
                    break;
                case "accountNameElementId":
                    this.options.accountNameElementId = value;
                    break;
                case "accountWorkPhoneElementId":
                    this.options.accountWorkPhoneElementId = value;
                    break;
                case "accountNameInitial":
                    this.options.accountNameInitial = value;
                    break;
                case "accountDisplayNameInitial":
                    this.options.accountDisplayNameInitial = value;
                    break;
                case "accountWorkPhoneInitial":
                    this.options.accountWorkPhoneInitial = value;
                    break;
                case "enabled":
                    this.setEnabled(value);
                    break;
                case "readOnly":
                    this.options.readOnly = value;
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
                this.checkNameAnchor.prop('disabled', false);
                this.lookupUserAnchor.prop('disabled', false);
            } else {
                this.checkNameAnchor.prop('disabled', true);
                this.lookupUserAnchor.prop('disabled', true);
            }
        },

        isEnabled: function () {
            return this.options.enabled;
        },
        isReadOnly: function () {
            return this.options.readOnly;
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

        _lookupUser: function (displayNameElement, accountNameElement, accountWorkPhoneElement) {
            var theOnChangeHandler = this.options.onChange;

            ActiveDirectorySearchDialog(function (results) {
                var selectedAccountName = results.AccountName;
                var selectedDisplayName = results.DisplayName;
                var selectedWorkPhone = results.WorkPhone;
                if (selectedAccountName != '') {
                    accountNameElement.val(selectedAccountName).hide();
                    if (accountWorkPhoneElement != undefined) {
                        accountWorkPhoneElement.val(selectedWorkPhone);
                    }
                    displayNameElement.val(selectedDisplayName).show();
                    displayNameElement.attr('title', selectedAccountName);

                    displayNameElement.data('isDirty', false);

                    // a selection was made
                    if (theOnChangeHandler != undefined) {
                        theOnChangeHandler();
                    }
                }
            });
        },

        _checkUserName: function (displayNameElement, accountNameElement, accountWorkPhoneElement, allowGroupAccounts, fieldName, callbackHandler) {
            var userid = displayNameElement.val();  // i.e. user has replaced display name with userid

            if (userid) {
                var dataToSend = '{ "userAccount" : "' + $.trim(userid) + '" }';
                var searchUrl = '/' + 'default/Home/SearchUserName';

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
                            if (accountWorkPhoneElement != undefined) {
                                accountWorkPhoneElement.val(response.workPhone);
                            }
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