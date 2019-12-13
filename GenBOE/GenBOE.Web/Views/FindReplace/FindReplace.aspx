<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    FindReplace - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">
        $(function () {
            $(".main").addClass("find-replace");
        });
    </script>

    <% Html.RenderAction("DisplayFindReplace", 
        "FindReplace",
        new { workspace = SiteMasterUtilities.GetCurrentWorkspace()  }); %>

</asp:Content>
