<%@ Page Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" %>

<asp:Content ID="errorTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Error
</asp:Content>

<asp:Content ID="errorContent" ContentPlaceHolderID="MainContent" runat="server">
  <div class="securityerror">   
    <script type="text/javascript">
        $(function () {
            $(".main").addClass("error");
        });
    </script>
    
   
        <h2>
           Error
        </h2>
    
  
         <div class="error" style="word-spacing:normal">
            You do not have permission to this page.
            Please create a ticket with Service Central.
           <p> Link:  <%= Utilities.ServiceCentralLink() %></p>
      </div>
   </div>
   
</asp:Content>