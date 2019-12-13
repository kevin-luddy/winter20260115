<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<script type="text/javascript">
    var BOEOffload = new Widget("BOEOffload");

    $(function () {
        $('#BOEOffload-OffloadButton').click(BOEOffload.OffloadClicked);

        if(<%=ViewData["HideOffloadBOE"]%>)
        {
            $('#BOEOffload-OffloadButton').hide();
        }

    });

    BOEOffload.OffloadClicked = function()
    {
                var url = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_BOE%>',
                '<%: WebConstants.ACTION_DISPLAY_BOE_OFFLOAD_RESULTS%>',
                'boe/<%: ViewData["BOEID"] %>');
            window.open(url);
    };

</script>
<button class="ies" type="button" id="BOEOffload-OffloadButton">Preview Offloading</button>
