<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%  string gridClass = ViewBag.AllowGridEdit ? "projectMapGrid" : "pagedProjectMapGrid";
    %>
    <script type="text/javascript">
        angular.module('genboe').value('isReadOnly', '<%: ViewData["READONLY"] %>'.isTrue());
        angular.module('genboe').value('allowGridEdit', '<%: ViewBag.AllowGridEdit %>'.isTrue());
        angular.module('genboe').value('isBucketized', '<%: ViewBag.IsBucketized.ToString() %>'.isTrue());
        angular.module('genboe').value('resources', <%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.Resources) %>);
        angular.module('genboe').value('performingOrgs', <%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.PerformingOrgs) %>);
        angular.module('genboe').value('legacyResources', <%= Newtonsoft.Json.JsonConvert.SerializeObject(ViewBag.LegacyResources) %>);
        angular.module('genboe').value('pageSize', <%= ConfigurationUtilities.GetAppSetting<int>("ProjectMapPageSize") %>);
    </script>
    <div data-ng-app="genboe" data-ng-controller="projectMapController">
        <div class="projectmap-warning-box" data-ng-class="validationClass" data-ng-show="showValidationMessage">
            <div class="projectmap-warning-message">
                <b>{{validationTitle}}</b>
                <div data-ng-repeat="message in validationMessages">
                    <ul>
                        <li>{{message}}</li>
                    </ul>
                </div>
            </div>
            <div class="small-close-button" data-ng-click="hideValidationMessages()"></div>
        </div>
        
        <div class="projectMapButtonRow">
            <button class="ies-action" id="AddButton-ProjectMap" type="button" data-ng-show="!isReadOnly && isGridEditAllowed()" data-ng-disabled="isBusy()" data-ng-click="addRow()">+ Add</button>
            <button class="ies-action" id="Export-ProjectMap" type="button" data-ng-disabled="isBusy() || PageIsDirty" data-ng-click="exportGrid('false')"><div data-ng-hide="isExporting">Export</div><div data-ng-show="isExporting" class="loader button-loader"></div></button>
            <button class="ies-action" id="Import-ProjectMap" data-ng-hide="isReadOnly" type="button" data-ng-disabled="isBusy() || PageIsDirty" data-ng-click="showImportDialog()">Import</button>
            <% if(ViewBag.AllowGridEdit) { Html.RenderAction(WebConstants.ACTION_DISPLAY_PROJECTMAP_BOE_SEARCH, WebConstants.CONTROLLER_BOE);} %>
            <button id="Export-ProjectMap-Offloaded" type="button" data-ng-disabled="isBusy() || PageIsDirty" data-ng-click="exportGrid('true')" 
                    style="float:right; background:linear-gradient(to bottom, #5a86d5, #b1caf6 1px, #6495ed); margin-right:0;" class="ies-action"><div data-ng-hide="isOffloadExporting">Export Offloaded</div><div data-ng-show="isOffloadExporting" class="loader button-loader"></div></button>
        </div>
        <div class="projectMapHeaderTotal">Total Hours:  {{ totalHours | number:<%:(int)ViewBag.HoursPrecision%>; }}</div>
        <div class="projectMapHeaderTotal">Total Dollars:  ${{ totalDollars | number:<%:(int)ViewBag.DollarsPrecision%> }}</div>
        <div class="projectMapHeaderTotal">Total Rows:  {{ totalRows }}</div>
        <div class="<%: gridClass%>" data-ng-hide="loading" ui-if="gridData.data.length>0" ui-grid="gridOptions" ui-grid-pinning ui-grid-edit ui-grid-cellNav<% if(!ViewBag.AllowGridEdit) { %> ui-grid-pagination<%}%> ui-grid-resize-columns ></div>
        <div data-ng-show="loading"><br /><div class="loader"></div></div>
        <% if (ViewBag.AllowGridEdit)
            { %>
        <div class="projectMapButtonRow">
            <button class="ies-action" id="SaveButton-ProjectMap" type="button" data-ng-show="!isReadOnly && isGridEditAllowed()" data-ng-disabled="isBusy() || !PageIsDirty" data-ng-click="saveProjectMapGrid()">Save</button>
            <button class="ies" id="CancelButton-ProjectMap" type="button" data-ng-show="!isReadOnly && isGridEditAllowed()" data-ng-disabled="isBusy() || !PageIsDirty" data-ng-click="cancel()">Cancel</button>
        </div>
        <% } %>
    </div>
</asp:Content>