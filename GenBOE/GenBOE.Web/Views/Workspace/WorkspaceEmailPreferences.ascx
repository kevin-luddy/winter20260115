<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<object>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%: Scripts.Render("~/bundles/identification") %>
<script type="text/javascript">
    var widgetConfig = {};

    widgetConfig.ContextID = "WorkspaceEmailPreferences";
    widgetConfig.isReadOnly = false;
    widgetConfig.IsModule = true;
    
    WorkspaceEmailPreferencesWidget = new GenWidget(widgetConfig);

   $(function () {
        refreshModule($('.workspace-email-preferences.module'));
    });
</script>
<div id="WorkspaceEmailPreferences" class="workspace-email-preferences module " >
    <div class="module-header-data">
        Workspace Email Preferences
    </div>
    <div class="module-content-data">
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
        <div data-ng-app="genboe" data-ng-controller="workspaceEmailController">
            <fieldset data-ng-disabled="isReadOnly" style="border:0; padding: 0; display: inherit;">
            <form name="workspaceEmailForm" id="workspaceEmailForm">
                <div data-ng-cloak>
                    <gen-validation data-errors="errors"></gen-validation>
                    <button class="ies-action" data-ng-disabled="isReadOnly" data-ng-click="toggleSelectAll()" id="toggleButton">{{toggleAllText}}</button>
                    <br />
                    <br />
                    <div data-ng-repeat="emailGroup in emailGroups">
                        <h2>{{::emailGroup[0].Category}}</h2>
                        <table class="email-preference">
                            <thead>
                                <tr>
                                    <th class="recipient">Recipient</th>
                                    <th>Trigger</th>
                                    <th class="forced">System Default
                                    </th>
                                    <th class="forced">System Default Forced</th>
                                    <th class="emailOverride"><span class="center">Workspace Overrides</span>
                                        <br />
                                        <input type="checkbox" data-ng-disabled="isReadOnly" data-ng-click="selectCategoryAll(emailGroup[0].Category, $event.target.checked)" /> All
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr data-ng-repeat="emailPreference in emailGroup">
                                    <td>{{::emailPreference.Recipient}}</td>
                                    <td>{{::emailPreference.Trigger}}</td>
                                    <td class="center">{{::emailPreference.SystemDefaultOn?'On':'Off'}}</td>
                                    <td class="center">{{::emailPreference.SystemForced?'Yes':'No'}}</td>
                                    <td>
                                        <div data-ng-hide="emailPreference.SystemForced">
                                            <input type="radio" data-ng-value="true" id="defaultOn" data-ng-model="emailPreference.WorkspaceOverrideOn" /><label for="defaultOn"> On</label>&nbsp;&nbsp;&nbsp;
                                            <input type="radio" data-ng-value="false" id="defaultOff" data-ng-model="emailPreference.WorkspaceOverrideOn" /><label for="defaultOff"> Off</label>&nbsp;&nbsp;&nbsp;
                                            <input type="radio" data-ng-value="null" id="defaultSystem" data-ng-model="emailPreference.WorkspaceOverrideOn" /><label for="defaultSystem"> Use System Default</label>
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>

                    <div class="form-element">
                        <div class="buttons inline">
                            <button class="ies ies-action" data-ng-click="save()" data-ng-disabled="isDataLoading || !workspaceEmailForm.$dirty || !workspaceEmailForm.$valid || isReadOnly" type="button">Save</button>
                            <div class="loader display-none"></div>
                            <button class="ies" data-ng-click="confirmCancel()" data-ng-disabled="isDataLoading || isReadOnly" type="button">Cancel</button>
                        </div>
                    </div>
                </div>
            </form>
        </fieldset>
        </div>  
    </div>
</div>
