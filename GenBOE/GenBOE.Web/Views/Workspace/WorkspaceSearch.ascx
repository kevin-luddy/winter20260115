<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Workspace.WorkspaceSearchModelView>" %>

<script type="text/javascript">

    var WorkspaceSearchWidget = new Widget("WorkspaceSearch");

    WorkspaceSearchWidget.BindEvents = function () {
        // BIND Element events

        // On focus for divs containing default text elements, hide the default text and set the input active
        $('#WorkspaceSearch .proposal-start-date, #WorkspaceSearch .proposal-end-date, #WorkspaceSearch .cost-volume-lead').focusin(function () {
            $(this).children('.default-text').addClass('display-none');
            $(this).children('input').addClass('active');
        });

        // Focus input fields with default text on click
        $('#WorkspaceSearch .proposal-start-date, #WorkspaceSearch .proposal-end-date, #WorkspaceSearch .cost-volume-lead').click(function () {
            $(this).children('input').focus();
        });

        // On focusout of fields with default-text, if the field value is blank, unhide the default text
        $('#WorkspaceSearch .proposal-start-date, #WorkspaceSearch .proposal-end-date, #WorkspaceSearch .cost-volume-lead').children('input').focusout(function () {
            if (!$.trim($(this).val()).length) {
                $(this).removeClass('active');
                $(this).val('').siblings('.default-text').removeClass('display-none');
            }
        });

        $('#WorkspaceSearch-CancelButton').click(function () { $('#WorkspaceSearch').dialog('close'); });
        $('#WorkspaceSearch-SearchButton').click(WorkspaceSearchWidget.SearchButtonClick);

        $('#WorkspaceSearchResults-SearchAgainButton').click(function () {
            $('#WorkspaceSearchResults').dialog('close');
            $('#WorkspaceSearch').dialog('open');
        });

        $('#WorkspaceSearchResults-CloseButton').click(function () {
            $('#WorkspaceSearchResults').dialog('close');
        });

        $('#WorkspaceSearchForm input, #WorkspaceSearchForm select').change(function () {
            return true;
        });
    };

    WorkspaceSearchWidget.RegisterForTriggeredEvents = function () {
        WorkspaceSearchWidget.registerForEvent('WorkspaceSearch', function () {
            $('#WorkspaceSearch').dialog('open');
        });

        WorkspaceSearchWidget.registerForEvent('CloseWorkspaceSearch', function () {
            $('#WorkspaceSearch').dialog('close');
            $('#WorkspaceSearchResults').dialog('close');
        });

        WorkspaceSearchWidget.registerForEvent("PageWorkspaceSearch", function (event, data) {
            var dataToSend = JSON.stringify(data);

            $.ajax({
                type: 'POST',
                url: CreateSystemAdminPostURL(
                    '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%: WebConstants.ACTION_PAGE_WORKSPACE_SEARCH_RESULTS %>'),
                contentType: 'application/json; charset=utf-8',
                dataType: 'html',
                data: dataToSend,
                success: function (response) {
                    var SearchResults = $('#WorkspaceSearchResults-Results');
                    SearchResults.html(response);
                },
                error: function (response) {

                }
            }, $('#WorkspaceSearch-SearchButton'));
        });
    };



    WorkspaceSearchWidget.SearchButtonClick = function () {

        $('#WorkspaceSearch-SearchButton').addClass('display-none');
        $('#WorkspaceSearch-Loader').removeClass('display-none');

        var dataToSend = {};

        dataToSend.WorkspaceProposalName = $('#WorkspaceSearchForm :input[name=WorkspaceProposalName]').val();
        dataToSend.WorkspaceDescription = $('#WorkspaceSearchForm :input[name=WorkspaceDescription]').val();
        dataToSend.ProposalStartDate = $('#WorkspaceSearchForm :input[name=ProposalStartDate]').val();
        dataToSend.ProposalEndDate = $('#WorkspaceSearchForm :input[name=ProposalEndDate]').val();
        dataToSend.CostLead = $('#WorkspaceSearchForm :input[name=CostLead]').val();
        dataToSend.ProjectMapType = $('#WorkspaceSearchForm :input[name=ProjectMapType]').val();
        dataToSend = JSON.stringify(dataToSend);

        validationUrl = CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_WORKSPACE%>', '<%:WebConstants.ACTION_VALIDATE_WORKSPACE_SEARCH%>');

        WorkspaceSearchWidget.ajaxRequest({
            url: validationUrl,
            data: dataToSend,
            success: function (results) {
                if (results.Status)
                    WorkspaceSearchWidget.PerformSearch(dataToSend);
            },
            error: function () {
                $('#WorkspaceSearch-SearchButton').removeClass('display-none');
                $('#WorkspaceSearch-Loader').addClass('display-none');
            }
        });
    }


    WorkspaceSearchWidget.PerformSearch = function (data) {

        $('#WorkspaceSearch-SearchButton').addClass('display-none');
        $('#WorkspaceSearch-Loader').removeClass('display-none');

        $.ajax({
            type: 'POST',
            url: CreateSystemAdminPostURL('<%:WebConstants.CONTROLLER_WORKSPACE%>', '<%:WebConstants.ACTION_PERFORM_WORKSPACE_SEARCH%>'),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: data,
            success: function (response) {
                var SearchResults = $('#WorkspaceSearchResults-Results');
                SearchResults.html(response);
                $('#WorkspaceSearch').dialog('close');
                $('#WorkspaceSearchResults').dialog('open');
                $('#WorkspaceSearch-Loader').addClass('display-none');
                $('#WorkspaceSearch-SearchButton').removeClass('display-none');
            },
            error: function (response) {
                $('#WorkspaceSearch-Loader').addClass('display-none');
                $('#WorkspaceSearch-SearchButton').removeClass('display-none');
            }
        });

    }

    $(function () {
        WorkspaceSearchWidget.BindEvents();
        WorkspaceSearchWidget.RegisterForTriggeredEvents();
        $('#WorkspaceSearch').dialog({ autoOpen: false, title: "Search for Workspace to Copy", modal: true, resizable: false, width: 640 }).removeClass('display-none');
        $('#WorkspaceSearchResults').dialog({ autoOpen: false, title: "Workspace Search Results", modal: true, resizable: false, width: 840 }).removeClass('display-none');
    });
</script>


<div id="WorkspaceSearch" class="workspace-advanced-search display-none">
    <div class="container">
        <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "WorkspaceSearchForm", name = "WorkspaceSearchForm" }))
            { %>
                <ul class="validation-box"></ul>
                <div class="form-row">
                    <div class="form-label"><strong>Please Note:</strong>  If you don't find the results you are expecting, please verify the Workspace does <strong>NOT</strong> contain OCI information and that <q>Share & Allow Search</q> is set to <strong>YES</strong></div>
                    <br />
                    <br />
                </div>
                <div class="form-row">
                    <div class="form-label">Workspace/Proposal Name</div>
                    <div class="form-element">
                        <%: Html.TextBox("WorkspaceProposalName", "", new { @class = "half oneRequired", @maxlength = "100"}) %>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label">Workspace Description</div>
                    <div class="form-element">
                        <%: Html.TextBox("WorkspaceDescription", "", new { @class = "half oneRequired", @maxlength = "100" })%>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label">Proposal Submittal Date Range</div>
                    <div class="form-element proposal-start-date">
                        <div class="default-text">mm/dd/yyyy</div>
                        <%: Html.TextBox("ProposalStartDate", "", new { @class = "normal-date oneRequired" })%>
                    </div>
                    <div class="form-label proposal-date-range-separator">to</div>
                    <div class="form-element proposal-end-date">
                        <div class="default-text">mm/dd/yyyy</div>
                        <%: Html.TextBox("ProposalEndDate", "", new { @class = "normal-date oneRequired" })%>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-label">Estimating Lead/Pricer</div>
                    <div class="form-element cost-volume-lead">
                        <div class="default-text" style="margin-right: -118px; *margin-right: -121px;">Lastname, Firstname</div>
                        <%: Html.TextBox("CostLead", "", new { @class = "half oneRequired", @maxlength = "40" })%>
                    </div>
                </div>
                <% if (IES.Common.classes.SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST
                        && SiteMasterUtilities.IsProjectMapEnabled) { %>
                <div class="form-row">
                    <div class="form-label">Workspace Type</div>
                    <div class="form-element cost-volume-lead">
                        <%: Html.DropDownList("ProjectMapType", (IEnumerable<SelectListItem>)ViewData["ProjectMapTypes"], new { @class = "half oneRequired" })%>
                    </div>
                </div>
                <% } %>
                <div class="form-row">
                    <div class="form-label"></div>
                    <div class="form-element">
                        <div class="buttons" style="width: 200px;">
                            <button id="WorkspaceSearch-SearchButton" class="ies-action" type="button">Search</button>
                            <div id="WorkspaceSearch-Loader" class="loader display-none"></div>
                            <button id="WorkspaceSearch-CancelButton" class="ies" name="cancel-button" type="button">Cancel</button>
                        </div>                
                    </div>
                </div>
        <% } %>
    </div>
</div>

<div id="WorkspaceSearchResults">
    Select a Workspace to copy. Workspace Administrator permission is required to copy
    a Workspace. If you do not have permission, you can request it.
    <br />
    <br />
    <strong>Please Note:</strong>  If you don't find the results you are expecting, please verify the Worksapce does <strong>NOT</strong> contain OCI information and that<br />
    <q>Share & Allow Search</q> is set to <strong>YES</strong>
    <div id="WorkspaceSearchResults-Results" style="margin: 20px 0px; max-height: 340px; overflow: auto;">
    </div>
    <div class="buttons" style="text-align: center;">
        <button id="WorkspaceSearchResults-SearchAgainButton" class="ies" type="button">Search again</button>
        <div id="WorkspaceSearchResults-Loader" class="loader display-none"></div>
        <button id="WorkspaceSearchResults-CloseButton" class="ies" name="close-button" type="button">Close</button>
    </div>
</div>
