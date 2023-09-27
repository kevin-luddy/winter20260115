<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">
    var boeId = '<%=ViewData["BOEID"]%>';
    var boeController = '<%: WebConstants.CONTROLLER_BOE%>';
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var boeContainsSumBoesUrl = CreatePostURL(workspace, boeController, '<%: WebConstants.ACTION_BOE_CONTAINS_SUM_OF_BOES%>', 'boe/' + boeId);
    var boeSubmitForApprovalUrl = CreatePostURL(workspace, boeController, '<%: WebConstants.ACTION_SUBMIT_FOR_APPROVAL%>', 'boe/' + boeId);
    var displayBoeValidateResultsUrl = CreatePostURL(workspace, boeController, '<%: WebConstants.ACTION_DISPLAY_BOE_VALIDATE_RESULTS%>', 'boe/' + boeId);

    var BOESubmitForApproval = InitializeBOESubmitForApprovalWidget(boeContainsSumBoesUrl, boeSubmitForApprovalUrl, displayBoeValidateResultsUrl, workspace, boeId);

    $(function () {
        var submitForApprovalReadOnly = '<%=ViewData["SubmitForApproval_ReadOnly"]%>'.isTrue();
        $('#BOESubmitForApproval-Button').click(BOESubmitForApproval.SubmitForApprovalClicked);
        if (submitForApprovalReadOnly) {
            $('#BOESubmitForApproval-Button').hide();
        }
    });
</script>

<% if (!Html.GetViewDataValue<bool>("ReadOnlyMode", false))
    { %>
    <button id="BOESubmitForApproval-Button" class="ies-action submit-for-approval-button" type="button">Submit for Approval</button>
<% } %>
<div id="BOESubmitForApproval-Loader" class="loader display-none float-right"></div>
