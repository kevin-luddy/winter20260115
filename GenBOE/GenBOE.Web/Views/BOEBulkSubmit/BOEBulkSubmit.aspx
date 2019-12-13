<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	genBOE - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>


<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/boebulksubmit") %>
    <script type="text/javascript">
        $(function () {
            // pulls out the error message from the query..
            var errorMessage = sessionStorage.errorMessage;
            if (errorMessage) {
                $('#errorMessage').addClass('validation-box').css('display', 'block').html(errorMessage);
            }
            sessionStorage.errorMessage = '';

            $(".main").addClass("workspace");
        });

    </script>

    <script type="text/javascript">
        app.value('BulkSubmitModel', {
            workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            controller: '<%:WebConstants.CONTROLLER_BOE_BULK_SUBMIT %>',
            draftState: '<%: (int)BOEState.Draft %>',
            awaitingState: '<%: (int)BOEState.AwaitingApproval %>',
            action: '<%:WebConstants.ACTION_GET_BOE_BULK_SUBMIT_MODEL %>',
            boeController: '<%:WebConstants.CONTROLLER_BOE %>',
            editAction: '<%: WebConstants.ACTION_BOE_BULK_SUBMIT %>',
            editBOEAction: '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>'
        });
    </script>

    <div id="workspace-home" data-ng-controller="BoeBulkSubmitController" data-ng-cloak="">
        <div class="module workspace-home" id="boesmodule">
            <div class="module-header-data">BOE Bulk Submit</div>
            <div class="module-content-data">
                <div class="form-row css3pie-position-fix">
                    <gen-validation data-errors="errors"></gen-validation>
                    <div class="buttons inline css3pie-position-fix" style="line-height: 28px;">
                        <button data-ng-click="saveChanges()" data-ng-disabled="isLoading || noDirty" type="button" class="ies-action css3pie-position-fix ng-scope">Save Changes</button>
                        <button data-ng-click="toggleAllDraft()" data-ng-disabled="isLoading" type="button" class="ies css3pie-position-fix ng-scope">Reset All to Draft</button>
                        <button data-ng-click="toggleAllAwaitingApproval()" data-ng-disabled="isLoading" type="button" class="ies css3pie-position-fix ng-scope">Submit All for Approval</button>
                    </div>
                </div>
                <div class="form-row">
                    <div class="boebulksubmit-grid container">
                        <table id="BoeBulkSubmitGrid" class="grid readonly">
                            <thead>
                                <tr>
                                    <th class="bootstrap">Work Breakdown Structure (WBS)</th>
                                    <th class="bootstrap">BOE Title</th>
                                    <th class="bootstrap">Contract Line Item Number (CLIN)</th>
                                    <th class="status-value bootstrap">Status</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr data-ng-show="isLoading"><td colspan="{{colSpan}}"><div class="loader"></div></td></tr>
                                <tr data-ng-show="!isLoading && (data.length === 0 || filteredResults.length === 0)"><td colspan="{{colSpan}}"><div class="empty-grid-text">There are no BOEs you are authoring for the Workspace.</div></td></tr>
                                <tr data-ng-repeat="boe in data">
                                    <td><a title="{{::boe.WBSText}}" href="{{::boe.url}}">{{::boe.WBSText}}</a></td>
                                    <td><a title="{{::boe.BOETitle}}" href="{{::boe.url}}">{{::boe.BOETitle}}</a></td>
                                    <td><a title="{{::boe.CLINText}}" href="{{::boe.url}}">{{::boe.CLINText}}</a></td>
                                    <td>
                                        <div data-ng-show="boe.Invalid">
                                            <a href="/<%: SiteMasterUtilities.GetCurrentWorkspace() %>/<%:WebConstants.CONTROLLER_BOE %>/<%:WebConstants.ACTION_DISPLAY_BOE_VALIDATE_RESULTS %>/boe/{{::boe.Id}}" target="_blank">Validation Errors</a>
                                        </div>
                                        <div data-ng-hide="boe.Invalid" class="boeStatusToggle">
                                            <input type="checkbox" data-ng-model="boe.isAwaitingApproval" data-ng-change="updateDirty()" name="boeStatusToggle" class="boeStatusToggle-checkbox" id="toggleswitch-{{::boe.Id}}">
                                            <label class="boeStatusToggle-label" for="toggleswitch-{{::boe.Id}}">
                                                <span class="boeStatusToggle-inner"></span>
                                                <span class="boeStatusToggle-switch"></span>
                                            </label>
                                        </div>
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="form-row">
                    <div class="full-width">
                        
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        angular.element(document).ready(function () {
            angular.bootstrap(document, ['genboe']);
        });
    </script>
</asp:Content>