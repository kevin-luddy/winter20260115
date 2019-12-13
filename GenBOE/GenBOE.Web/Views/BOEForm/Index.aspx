<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Manage INL Forms - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

<script type="text/javascript">
    $(function () {
        $(".main").addClass("boe");
    });
</script>

<% Html.RenderAction(WebConstants.ACTION_DISPLAY_MANAGE_BOE_FORMS, 
    WebConstants.CONTROLLER_BOE_FORMS,
    new { workspace = SiteMasterUtilities.GetCurrentWorkspace()  }); %>
    
</asp:Content>
