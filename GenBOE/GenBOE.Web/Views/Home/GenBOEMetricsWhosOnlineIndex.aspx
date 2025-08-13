<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<GenBOE.ActionLogic._ModelView.WhosOnlineGridModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%
        var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    %>
<script type="text/javascript">
    var WhosOnlineWidget;

    $(function () { 

        //Paging Data Config.
        var pagingData = {};
        pagingData.ContentDiv = $('#WhosOnlineDivContent');
        pagingData.pagingUrl= GenSession.CreateUrl({
            controller: '<%:WebConstants.CONTROLLER_HOME %>',
            action: '<%:WebConstants.ACTION_HOME_PAGE_WHOSONLINE %>' });
        pagingData.type = "PageNumber";

        var WhosOnlineWidgetConfig = { ContextID: "WhosOnlineDivContainer", IsModule: false, isReadOnly: true };
        WhosOnlineWidgetConfig.PagingData = pagingData;
        WhosOnlineWidget = new GenListWidget(WhosOnlineWidgetConfig);
    });

</script>
<div class="module">
    <div class="module-header-data">Who's Online</div>
    <div class="module-content-data" style="padding-bottom:20px;">
        <% Html.RenderPartial(WebConstants.VIEW_HOME_WHOS_ONLINE, Model); %>
    </div>
</div>
</asp:Content>
