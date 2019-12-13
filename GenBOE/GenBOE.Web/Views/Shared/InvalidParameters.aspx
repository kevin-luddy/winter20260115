<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	InvalidParameters
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">
        $(function () {
            $(".main").addClass("error");
        });
    </script>
    
    <h2>
       Error
    </h2>
    
  
     <div class="error" style="word-spacing:normal">

       The page cannot be found. Please check the address and try again. If the problem persists, please contact the GenBOE Helpdesk. Sorry for the inconvenience. 
       <p> Email: <%: Utilities.HelpdeskEmailAddress() %></p>
 </div>

</asp:Content>
