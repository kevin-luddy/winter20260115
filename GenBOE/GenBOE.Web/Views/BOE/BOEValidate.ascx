<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">
    var BOEValidate = new Widget("BOEValidate");

    $(function () {
        $('#BOEValidate-ValidateButton').click(BOEValidate.ValidateClicked);

        if(<%=ViewData["ValidateBOE_ReadOnly"]%>)
        {
            $('#BOEValidate-ValidateButton').hide();
        }

    });

    BOEValidate.ValidateClicked = function()
    {
                var url = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE%>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_VALIDATE_RESULTS%>',
                'boe/<%: ViewData["BOEID"] %>');
            window.open(url);
    };

</script>
<button id="BOEValidate-ValidateButton" class="ies boe-validate-button" type="button">Validate</button>
