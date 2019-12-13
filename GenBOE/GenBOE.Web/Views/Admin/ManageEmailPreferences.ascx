<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>

<script type="text/javascript">
    var ManageEmailPreferencesWidget = new Widget('ManageEmailPreferences', false);
    
    $(function () {

        ManageEmailPreferencesWidget.registerForEvent('EMAIL_PREFERENCES_LOADED', function () {
             return true;
        });
    });
    
</script>

<div class="section">
    <div class="title">Manage Email Preferences</div>
    <div class="data">
        <div>
            Edit the default email preferences used by Workspaces. Updates to the default options will immediately be used
            for new Workspaces. Updates to the forced options will immediately be used for all Workspaces.
        </div>
        <br />
        <div>
            <% if (ConfigurationUtilities.GetAppSetting<bool>("DisableAllEmails")) {%>
            <div class="validation-box email-validation-box">
                The overall Email switch is currently turned off.
            </div>
            <% }
                if (ConfigurationUtilities.GetAppSetting<bool>("EmailsToCurrentlyLoggedInUser")) {%>
            <div class="validation-box email-validation-box">
                All Email is currently being redirected to the logged in user.
            </div>
            <% } %>
        </div>
        <div data-ng-app="genboe" data-ng-controller="systemEmailController">
            <form name="systemEmailForm" id="systemEmailForm">
                <div data-ng-cloak>
                    <gen-validation data-errors="errors"></gen-validation>
                    <button class="ies-action" data-ng-click="toggleSelectAll()" id="toggleButton">{{toggleAllText}}</button>
                    <br />
                    <br />
                    <div data-ng-repeat="emailGroup in emailGroups">
                        <h2>{{::emailGroup[0].Category}}</h2>
                        <table class="email-preference">
                            <thead>
                                <tr>
                                    <th>Recipient</th>
                                    <th class="trigger">Trigger</th>
                                    <th class="default">System Default
                                        <br />
                                        <input type="checkbox" data-ng-click="selectCategoryAll(emailGroup[0].Category, $event.target.checked)" /> All
                                    </th>
                                    <th class="forced">System Forced</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr data-ng-repeat="emailPreference in emailGroup">
                                    <td>{{::emailPreference.Recipient}}</td>
                                    <td>{{::emailPreference.Trigger}}</td>
                                    <td><input type="radio" data-ng-value="true" id="defaultOn" data-ng-model="emailPreference.DefaultOn" /><label for="defaultOn"> On</label>&nbsp;&nbsp;&nbsp;<input type="radio" data-ng-value="false" id="defaultOff" data-ng-model="emailPreference.DefaultOn" /><label for="defaultOff"> Off</label></td>
                                    <td class="center">
                                        <input type="checkbox" data-ng-value="true" data-ng-model="emailPreference.Forced" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <div class="form-element">
                        <div class="buttons inline">
                            <button class="ies ies-action" data-ng-click="save()" data-ng-disabled="isDataLoading || !systemEmailForm.$dirty || !systemEmailForm.$valid" type="button">Save</button>
                            <div class="loader display-none"></div>
                            <button class="ies" data-ng-click="confirmCancel()" data-ng-disabled="isDataLoading" type="button">Cancel</button>
                        </div>
                    </div>
                </div>
            </form>
        </div>   
    </div>
</div>
