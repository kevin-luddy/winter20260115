<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="IES.Common.classes" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	<%if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.SpaceSystems){%>genBOE<%} else {%>generation - genBOE<%} %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <script type="text/javascript">
        $(function () {
            $('.header .title').addClass('genBOE');

            var chooseWorkspaceURL = CreateSystemAdminPostURL('<%: WebConstants.CONTROLLER_WORKSPACE %>', '<%: WebConstants.ACTION_DISPLAY_CHOOSE_WORKSPACE %>');

            $('#WorkspaceSelection').load(chooseWorkspaceURL);

            $(this).bind('WORKSPACE_STATUS_FILTER_CHANGED', function (e, params) {
                $('#ChooseWorkspace-WorkspaceDropDownList').hide();
                $('#workspaceSelectionLoader').show();
                // show a loader instead?

                var selectedFilterOption = $("#SelectWorkspaceFilter").val();

                var dataToSend = {};
                dataToSend.workspaceStatus = selectedFilterOption;
                dataToSend = JSON.stringify(dataToSend);

                $.ajax({
                    type: 'POST',
                    url: chooseWorkspaceURL,
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'html',
                    data: dataToSend,
                    success: function (response) {
                        $('#WorkspaceSelection').html(response);
                        $("#SelectWorkspaceFilter").val(selectedFilterOption);
                    }
                });
            });
        });
    </script>
    <div class="module">
        <div class="module-content-data">
            <div class="get-started section" style="width: 950px;">
        <div class="title">Get Started</div>
        <div class="workspace">
            <div class="name">My Workspaces</div>
            <div id="WorkspaceSelection">
                Loading Available Workspaces...
                <div class="loader"></div>
            </div>
        </div>
          <div class="divider"></div>
        <div class="workspace">
            <div class="name">New Workspace</div>
            <% if ((Boolean)ViewData["CanCreateWS"] == true)
               { %>
                    Create a new workspace for your project's Basis of Estimates
                    <a href="/default/Workspace/CreateWorkspace"><button class="ies create-button" type="button">Create</button></a>
                <% }%>
        </div>        
    </div>
        </div>
    </div>
</asp:Content>