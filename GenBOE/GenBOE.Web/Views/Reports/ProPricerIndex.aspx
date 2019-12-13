<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Title" ContentPlaceHolderID="TitleContent" runat="server">
	Export to ProPricer - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Main" ContentPlaceHolderID="MainContent" runat="server">

<%: Scripts.Render("~/bundles/discrepancy") %>
<%: Scripts.Render("~/bundles/ProPricer") %>

<script type="text/javascript">
    $(function () {
        $('.main').addClass('propricer');        
    });
</script>

<% Html.RenderAction(WebConstants.ACTION_DISPLAY_EXPORT_TO_PROPRICER, 
                     WebConstants.CONTROLLER_REPORTS,
                     new { workspace = SiteMasterUtilities.GetCurrentWorkspace()  }); %>

</asp:Content>