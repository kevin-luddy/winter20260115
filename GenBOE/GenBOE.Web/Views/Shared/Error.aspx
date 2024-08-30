<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<System.Web.Mvc.HandleErrorInfo>" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Error
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
    
    <script type="text/javascript">
        $(function () {
            $(".main").addClass("error");
        });
    </script>
    
    <h2>
       Error
    </h2>
    
  
     <div class="error" style="word-spacing:normal">

        An error has occurred. Please try your request again. If the problem persists, 
        please create a ticket with Service Central. Sorry for the inconvenience. 
       <p> Link: <%= Utilities.ServiceCentralLink() %></p>
    <br />
    <br />
    <br />
    <%if (!Utilities.IsProduction())
        { %>
         <p>Controller Name: <%: Model.ControllerName %></p>
         <p>Action Name: <%: Model.ActionName %></p>
         <p>Message: <%: Model.Exception != null ? Model.Exception.Message : "[NULL]"%></p>
         <p>Stack Trace: <%: Model.Exception != null ? Model.Exception.StackTrace : "[NULL]"%></p>
    <% } %>
 </div>
</asp:Content>