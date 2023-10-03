<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">
    
    var boeId = <%=ViewData["BOEID"]%>;
    var boeSubmitForReviewUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            '<%: WebConstants.CONTROLLER_BOE%>',
            '<%: WebConstants.ACTION_BOE_SUBMIT_FOR_REVIEW%>',
            'boe/<%: ViewData["BOEID"] %>');
    
    var BOESubmitForReview = InitializeSubmitForReviewWidget(boeId, boeSubmitForReviewUrl);
    var True = true;

    $(function () {
        $('#BOESubmitForReview-Button').click(BOESubmitForReview.ValidateClicked);
        if(<%= (String)ViewData["SubmitForReview_ReadOnly"] == "true" %>)
        {
            $('#BOESubmitForReview-Button').hide();
        }
    });

</script>

<% if (ViewData["SubmitForReview_ReadOnly"].ToString().ToLower() != "true")
    { %>
<button id="BOESubmitForReview-Button" class="ies" type="button">Submit for Review</button>
<% } %>
<div id="BOESubmitForReview-Loader" class="loader display-none"></div>

