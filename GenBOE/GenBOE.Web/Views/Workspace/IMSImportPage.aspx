<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Import IMS
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <script type="text/javascript">

    <% bool WBSCheck = (bool)ViewData["WBSAsset_Check"];  %>

    ImportIMS = new Widget("import_IMS");

    ImportIMS.ImportInProgressDialog = {};
    ImportIMS.ImportErrorDialog = {};
   
    ImportIMS.ImportArtimis =  function(){
        ImportIMS.OpenDialogAfterInitialize(ImportIMS.ImportInProgressDialog);
       
        // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
        $('#ImportDialog-Artemis-DocumentDomain').val(document.domain);
        
        // Remove the old hidden iFrame, if it exists
        $('#ImportDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportDialog-Artemis-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportDialog-UploadTarget').load(ImportIMS.StopUpload);

        $("#ImportDialog-Artemis-Form").submit();
       
    };

    ImportIMS.ImportProject =  function(){

        ImportIMS.OpenDialogAfterInitialize(ImportIMS.ImportInProgressDialog);
       
        // This was setting the document.domain but that's no longer necessary now that IRIS is gone.  Keeping the code incase the model view needs that hidden input.
        $('#ImportDialog-Project-DocumentDomain').val(document.domain);
        
        // Remove the old hidden iFrame, if it exists
        $('#ImportDialog-UploadTarget').remove();

        // Append the iFrame to the body, causing the controller action to fire and
        // the download to occur inside the iFrame
        $('#ImportDialog-Project-Form').append('<iframe id="ImportDialog-UploadTarget" name="ImportDialog-UploadTarget" class="display-none"></iframe>');
        $('#ImportDialog-UploadTarget').load(ImportIMS.StopUpload);

        $("#ImportDialog-Project-Form").submit();
       
    };

     ImportIMS.StopUpload = function() { //Function will be called when iframe is loaded
        var uploadResponseElement = $("#ImportDialog-UploadTarget").contents().find("body #UploadResponse");

        if (uploadResponseElement != undefined && uploadResponseElement.length && uploadResponseElement.html().length) {
            
            var results = eval('(' + uploadResponseElement.html() + ')');
            if (results.Status) {                
                // Show success notification
                RaiseNotification('Import successful and changes saved');
                // Redirect back to jump page
                window.location = window.location.protocol + '//' + window.location.host + '/' + '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
            }
            else {
                
                var errorMessages="";

                if(results.Message=='Errors')
                {
                    for(message in results.Data)
                    {
                        errorMessages += "<b>" + results.Data[message].Title + "</b><br/>";
                        errorMessages += results.Data[message].Message + "<br/><br/>";
                    }
                }else{
                    errorMessages=results.Message;
                }

                $("#IMSImportErrors .errorSpace").html(errorMessages);
                ImportIMS.CloseDialog(ImportIMS.ImportInProgressDialog);
                ImportIMS.OpenDialogAfterInitialize(ImportIMS.ImportErrorDialog);
            }
        }
        else {
            ImportIMS.CloseDialog(ImportIMS.ImportInProgressDialog);
        }
    }

    ImportIMS.showProject = function () {
        $("#IMPORT_ARTEMIS_STEP_TWO").addClass("display-none");
        $("#IMPORT_STEP_ONE .buttons").addClass("display-none");
        $("#IMPORT_PROJECT_STEP_TWO").removeClass("display-none");
    }

    ImportIMS.showArtemis = function () {
        $("#IMPORT_ARTEMIS_STEP_TWO").removeClass("display-none");
        $("#IMPORT_STEP_ONE .buttons").addClass("display-none");
        $("#IMPORT_PROJECT_STEP_TWO").addClass("display-none");
    }


    $(function () {
        createModule($('#import_IMS'));
        refreshModule($('#import_IMS'));

        ImportIMS.ImportInProgressDialog.Element = $('#ImportInProgressDialog');
        ImportIMS.ImportInProgressDialog.Params = { width: 400, height: 80, modal: true, resizable: false, draggable: true, closeOnEscape: false, dialogClass: "ImportInProgress-Dialog" };

        ImportIMS.ImportErrorDialog.Element= $("#IMSImportErrors");
        ImportIMS.ImportErrorDialog.Params = { width: 500, height: 300, modal: true, resizable: false, draggable: true, closeOnEscape: false};

        ImportIMS.InitializeDialog(ImportIMS.ImportInProgressDialog);
        ImportIMS.InitializeDialog(ImportIMS.ImportErrorDialog);

        $("#IMSImportErrors button[name='ok-button']").click(function(){ImportIMS.CloseDialog(ImportIMS.ImportErrorDialog);});

        <%if(WBSCheck){%> $("#formDisabledBox").show();<%}%>

        $("button[name='cancel-button']").click(function () { window.location = window.location.protocol + '//' + window.location.host + '/' + '<%: SiteMasterUtilities.GetCurrentWorkspace() %>'; });

        $("button[name='import-button']").click(function () { 
            if($("input[name=importType]:checked").val()=="artemis")
            {
                ImportIMS.ImportArtimis();
            }else{
                ImportIMS.ImportProject();
            }
        });

    });

   </script>

  <div id="import_IMS" class="import_IMS module" height="400">
    <div class="module-header-data">Import IMS</div>
    <div class="module-content-data">
        <div class="form-row">
            Import of the IMS will create an initial set of WBS Elements, BOEs and Task Elements. 
            CLINS can optionally be created when importing from MS Project. 
            This can be done only if no WBS Elements and no BOEs already exist in the Workspace.
        </div>
        <div id="formDisabledBox" class="validation-box">
            Import of an IMS is not allowed when WBS Elements or BOEs already exist in the Workspace.
        </div>
        <div id="ImportDialog-Error" class="validation-box">
            <div><b>Import Failed.</b>
                <div id="ImportDialog-ErrorText">
                </div>
            </div>
            <div class="clear"></div>
        </div>
        <div id="IMPORT_STEP_ONE" class="form-row">
                <h3>Step 1: Select what to Import</h3>
                <br />
                <input type="radio" name="importType" onclick="ImportIMS.showArtemis()" value="artemis" <%if(WBSCheck){ %>disabled="disabled"<%}%>/> Import from Artemis 
                <br />
                <input type="radio" name="importType" onclick="ImportIMS.showProject()" value="project" <%if(WBSCheck){ %>disabled="disabled"<%}%>/> Import from MS Project
                <br />
                <br />
                <div class="buttons">
                    <button class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
        </div>
        <div id="IMPORT_PROJECT_STEP_TWO" class="form-row display-none">
                <hr />
                <br />
                <h3>Step 2: Select file for import</h3>
                <br />
                <% Html.BeginForm(WebConstants.ACTION_IMPORT_PROJECT_IMS, WebConstants.CONTROLLER_WORKSPACE, new {workspace= SiteMasterUtilities.GetCurrentWorkspace()},FormMethod.Post,  new { enctype = "multipart/form-data", id = "ImportDialog-Project-Form", target = "ImportDialog-UploadTarget" }); %>
                 <input type="hidden" id="ImportDialog-Project-DocumentDomain" name="documentDomain" />
                <input type="file" name="importFile" size="60"/>
                <br />
                <ul>
                    <li>This file contains an export of the IMS data from MS Project.</li>
                    <li>The file is an Excel (.xlsx) file as shown in this  <a target="_blank" href="<%= this.ResolveClientUrl("~/Templates/Export/IMS_MSProject.xlsx") %>">format example</a>.</li>
                </ul>
                <br />
                <br />
                <div class="buttons">
                    <button class="ies-action" name="import-button" type="button">Import</button>
                    <button class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
                <% Html.EndForm(); %>
        </div>
        <div id="IMPORT_ARTEMIS_STEP_TWO" class="form-row display-none">
                <hr />
                <br />
                <h3>Step 2: Select file for import</h3>
                <br />
                <% Html.BeginForm(WebConstants.ACTION_IMPORT_ARTEMIS_IMS, WebConstants.CONTROLLER_WORKSPACE, new {workspace= SiteMasterUtilities.GetCurrentWorkspace()}, FormMethod.Post, new { enctype = "multipart/form-data", id = "ImportDialog-Artemis-Form", target = "ImportDialog-UploadTarget" }); %>
                 <input type="hidden" id="ImportDialog-Artemis-DocumentDomain" name="documentDomain" />
                <h3>WBS File Location</h3><input type="file" name="importFile1" size="60"/>
                <br />
                <ul>
                    <li>This file contains export of the WBS Elements and the WBS names exported from Artemis.</li>
                    <li>The file is a comma separated values (CSV) file containing a row of data for each WBS as shown in the <a target="_blank" href="<%= this.ResolveClientUrl("~/Templates/Export/IMS_Artemis.WBS.csv") %>">format example</a>.</li>
                </ul>
                <br />
                <h3>Tasks File Location</h3><input type="file" name="importFile2" size="60"/>
                <br />
                <ul>
                    <li>This file contains an export of the Task Elements exported from Artemis.</li>
                    <li>The file is a CSV file containing a row of data for each Task Element as shown in this <a target="_blank" href="<%= this.ResolveClientUrl("~/Templates/Export/IMS_Artemis.Tasks.csv") %>">format example</a>.</li>
                </ul>
                <br />
                <br />
                <div class="buttons">
                    <button class="ies-action" name="import-button" type="button">Import</button>
                    <button class="ies" name="cancel-button" type="button">Cancel</button>
                </div>
                <% Html.EndForm(); %>
        </div>
    </div>
</div>

<div id="ImportInProgressDialog" style="display: none; font-size: 18px; font-weight: bold; font-family: Arial, Helvetica; text-align: center;">
    <div class="loader"></div>
    <br />
    Importing IMS Data
</div>

<div id="IMSImportErrors"  title="Errors During Import of IMS">
    Errors occurred during import of the IMS. No changes were made to the Workspace.
    <br /><br />
    <div class="errorSpace">
    </div>
    <button class="ies" name="ok-button" type="button">OK</button>
</div>

</asp:Content>