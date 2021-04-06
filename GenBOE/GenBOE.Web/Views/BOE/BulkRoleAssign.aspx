<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Bulk Role Assign - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/bulkRoleAssign") %>
<script type="text/javascript">
<%--    app.value('BulkRoleAssignModel', {
        workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        controller: '<%:WebConstants.CONTROLLER_BULK_ROLE_ASSIGN %>',
        action: '<%:WebConstants.GET_BULK_ROLE_ASSIGN_MODEL %>',
        saveAction: '<%:WebConstants.ACTION_SAVE_BULK_ROLE_ASSIGN %>'
    });--%>

    $(function () {
        $(".main").addClass("boe");
    });
</script>
    <%-- TODO DELETE --%>
<div data-ng-controller="BulkRoleAssignController" data-ng-cloak="">
    <div id="BulkRoleAssign" class="manage-boe module">
        <div class="module-header-data">Bulk Role Assign</div>
        <div class="module-content-data">
            <div class="form-row">Bulk assign roles for BOEs.</div>
            <div class="form-row css3pie-position-fix">
                <ul class="validation-box" style="display: none;"></ul>
                <gen-validation data-errors="errors"></gen-validation>
            </div>
            
        </div>
    </div>
</div>

<script type="text/javascript">
    angular.element(document).ready(function () {
        angular.bootstrap(document, ['genboe']);
    });
</script> 
</asp:Content>
