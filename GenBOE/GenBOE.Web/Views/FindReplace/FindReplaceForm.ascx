<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl" %>

<script type="text/javascript">

    FindReplaceWidget = new Widget("FindReplaceForm", <%: ViewData["READONLY"] %>);

    //Initialize the dialog
    FindReplaceWidget.Initialize = function() {
        FindReplaceWidget.Module = $('#FindReplace');
        FindReplaceWidget.FindResultsDialog = {};
        FindReplaceWidget.FindResultsDialog.Element = $('#FindReplaceResults');
        FindReplaceWidget.FindResultsDialog.Params = { width: 700, modal: true, resizable: false, draggable: true, height: 610, title: 'Find Replace Results' };
    }
    
    FindReplaceWidget.CloseReplaceResults = function () {
        FindReplaceWidget.CloseDialog(FindReplaceWidget.FindResultsDialog);
    }

    FindReplaceWidget.ReplaceSaveSuccess = function () {
        FindReplaceWidget.CloseDialog(FindReplaceWidget.FindResultsDialog);
        $('#FindText').val('');
        $('#ReplaceText').val('');
        RaiseNotification('BOE text successfully replaced');
    }

    FindReplaceWidget.registerForEvent("PageFindResults", function (event, data) {
        var dataToSend = JSON.stringify(data);

        $.ajax({
            type: 'POST',
            url: CreatePostURL(
                '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_FIND_REPLACE %>',
                '<%: WebConstants.ACTION_PAGE_FIND_REPLACE_RESULTS %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'html',
            data: dataToSend,
            success: function (response) {
                var SearchResults = $('#FindReplaceResults');
                SearchResults.html(response);
                }
            });
        });

    $(function() {
      

        FindReplaceWidget.Initialize();

        createModule(FindReplaceWidget.Module);

        FindReplaceWidget.InitializeDialog(FindReplaceWidget.FindResultsDialog);
        
        FindReplaceWidget.registerForEvent("REPLACE_SAVE_SUCCESS", FindReplaceWidget.ReplaceSaveSuccess);
        FindReplaceWidget.registerForEvent("CLOSE_REPLACE_RESULTS", FindReplaceWidget.CloseReplaceResults);

        $('#FindReplace-Cancel').click(function () {
            window.location = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_WORKSPACE %>',
                '<%: WebConstants.VIEW_INDEX %>', '')
        });

        $('#FindReplace-FindAll').click(function() {      
            $("#FindReplace-FindAll").addClass("display-none");
            $("#FindReplace-loader").removeClass("display-none");
            
            FindReplaceWidget.FindData = {};
                
            FindReplaceWidget.FindData.FindText = $('#FindText').val();
            FindReplaceWidget.FindData.ReplaceText = $('#ReplaceText').val();
            FindReplaceWidget.data.findParams = FindReplaceWidget.FindData;

            var dataToSend = JSON.stringify(FindReplaceWidget.data);

            FindReplaceWidget.ajaxRequest({
                type: 'POST',
                url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%: WebConstants.CONTROLLER_FIND_REPLACE %>',
                    '<%: WebConstants.ACTION_FIND_REPLACE_RESULTS %>', ''),
                contentType: 'application/json; charset=utf-8',
                dataTypeForced: 'html',
                data: dataToSend,
                success: function (response) {
                    FindReplaceWidget.clearValidationBox();
                    $('#FindReplaceResults').html(response);
                    FindReplaceWidget.OpenDialogAfterInitialize(FindReplaceWidget.FindResultsDialog);
                    $("#FindReplace-FindAll").removeClass("display-none");
                    $("#FindReplace-loader").addClass("display-none");
                    },
                error: function () {
                    $("#FindReplace-FindAll").removeClass("display-none");
                    $("#FindReplace-loader").addClass("display-none");
                }
            });
        });

        refreshModule(FindReplaceWidget.Module);
    });
</script>
<% Html.BeginForm("", "", FormMethod.Post, new { name = "FindReplaceForm", id = "FindReplaceForm" }); %>
<div id="FindReplace" class="module">
    <div class="module-header-data">Find/Replace BOE Text</div>
    <div class="module-content-data">
            <ul class="validation-box"> </ul>    
            <div class="form-row" style="width:650px">
            <div class="form-element">
                <span>This searches the text fields</span>
                <div id="TextFields-Help" class="help-icon" onclick="FindReplaceWidget.ToggleHelp(this);" style="margin-left:-3px;"></div>
                <!-- This comment is needed for the jquery animation to work in IE8... -->
                <div id="TextFields-HelpDialog" class="help-dialog" style="width: 250px;">
                    <div class="help-dialog-close"></div>
                    <div class="help-dialog-text">The following are the fields that are searched:
                        <ul>
                            <li>BOE Title</li>
                            <li>BOE Description</li>
                            <li>Sources of Data</li>
                            <li>Task Title</li>
                            <li>Task Description</li>
                            <li>MOQ Text</li>
                        </ul>
                    </div>
                </div>
                <span>of all BOEs within the Workspace. You will be given a list of where all occurrences were found and will have the opportunity to replace all or selected occurrences.</span>            
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <div class="subtext">Find what:</div>
            </div>
            <div class="form-element">
               <input id="FindText" type="text" maxlength="400" style="width:400px; display:inline-block" />            
            </div>
        </div>
        <div class="form-row">
            <div class="form-label">
                <div class="subtext">Replace With:</div>
            </div>
            <div class="form-element">            
               <input id="ReplaceText" type="text" maxlength="400" style="width:400px; display:inline-block" />                 
            </div>
        </div>
        
        <div class="form-row">
            <div class="form-label"></div>
            <div class="form-element">
                <div class="buttons">
                    <button id="FindReplace-FindAll" class="ies-action" type="button">Find All</button>
                    <div id="FindReplace-loader" class="loader display-none"></div>
                    <button id="FindReplace-Cancel" class="ies" name="cancel-button" type="button">Cancel</button>
                </div>           
            </div>
        </div>
    </div>

</div>
<% Html.EndForm(); %>
<div id="FindReplaceResults" style ="display: none"></div>