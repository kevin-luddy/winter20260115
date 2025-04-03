<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>

<script type="text/javascript">

    var ResourceRatesTMWidget;

    $(function () {

        function onDeletePressed() {
            var button = this;
            var elementsTODelete = ResourceRatesTMWidget.getElement('input[name=toDelete]:checked');
            var data = [];
            GenSession.confirmDialog('Delete Resources', 'Are you sure you want to delete these Resources?', function () {
                elementsTODelete.each(function () {
                    var row = $(this).parents('tr');

                    data.push({
                        ResourceRateID: $(row).attr("pkid"),
                        ResourceID: $(row).find("input[name=ResourceID]").val(),
                        ToDelete: true,
                        UpdateDateLong: $(row).find("input[name=UpdateDateLong]").val().toString()
                    })
                });
                ResourceRatesTMWidget.saveRequest(
			    {
			        url: saveURL,
			        data: JSON.stringify(data),
			        success: function () {
			            delete ResourceRatesTMWidget.pagingData.data;
			            ResourceRatesTMWidget.pageResults();
			        }
			    }, button);
            });
        };

        function openAddEdit() {
            window.location.hash = "#AddResourceRateTM";
        };

        var importURL = GenSession.CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%:WebConstants.ACTION_IMPORT_WORKSPACE_RESOURCE_RATES_TM %>', '');

        var importPreviewURL = GenSession.CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%:WebConstants.ACTION_IMPORT_PREVIEW_WORKSPACE_RESOURCE_RATES_TM %>', '');

        var saveURL = GenSession.CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%:WebConstants.ACTION_SAVE_WORKSPACE_RESOURCE_RATE_TM %>', '');

        var dataURL = GenSession.CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%:WebConstants.ACTION_DISPLAY_WORKSPACE_RESOURCE_RATES_GRID_TM %>', '');

        var exportURL = GenSession.CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%:WebConstants.ACTION_EXPORT_WORKSPACE_RESOURCE_RATES_TM %>', '');

        var importTemplateURL = GenSession.CreatePostURL(
                    '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                    '<%:WebConstants.ACTION_EXPORT_BLANK_WORKSPACE_RESOURCE_RATES_TM %>', '');

        $("#ImportWorkspaceResourceRatesTMDialog a.exportTemplateLink").click(function () { ResourceRatesTMWidget.performExport(importTemplateURL); });
        $("#ImportWorkspaceResourceRatesTMDialog a.exportLink").click(function () { ResourceRatesTMWidget.performExport(exportURL); });

        //Paging Data Config.
        var pagingData = {};
        pagingData.ContentDiv = $('#WorkspaceResourceRatesTM .paging-area');
        pagingData.pagingUrl = dataURL,
        pagingData.type = "Arrow";
        pagingData.searchFilter = true;

        DialogConfigs = [];
        DialogConfigs.push({
            ElementID: "ImportWorkspaceResourceRatesTMDialog",
            Params: {
                width: 690,
                height: 620,
                title: "Import T&M Resource Rates",
                onOpen: function () { $("#ImportWorkspaceResourceRatesTMDialog .importOption").hide(); }
            }
        });
        DialogConfigs.push({
            ElementID: "ImportWorkspaceResourceRatesTMDialogResults",
            Params: {
                width: 700,
                title: "Import T&M Resource Rates"
            }
        });

        var FormConfigs = [];
        FormConfigs.push({
            ElementID: "ImportWorkspaceResourceRatesTMForm",
            Buttons: [{
                ButtonClass: "ies-action",
                ButtonText: 'Import',
                ButtonName: 'import-button',
                Stateful: true,
                ButtonAction: function (buttonPressed) {
                    ResourceRatesTMWidget.performImportPreview(importPreviewURL, "#ImportWorkspaceResourceRatesTMForm", UploadComplete, buttonPressed);
                }
			}],
			HideOCI: false,
			// We Load both OCI and NON OCI texts so that the Generation.JS will use the ContainsOCI to display the correct text
			BannerTextWithOCI: <%: SiteMasterUtilities.GetBannerText() %>,
			BannerTextWithoutOCI: <%: SiteMasterUtilities.GetBannerText(true) %>
        });
        FormConfigs.push({
            ElementID: "FinishImportWorkspaceResourceRatesTMForm",
            Buttons: [
            {
                ButtonClass: "ies",
                ButtonText: 'Back',
                ButtonName: 'back-button',
                ButtonAction: function (buttonPressed) {
                    ResourceRatesTMWidget.getDialog("ImportWorkspaceResourceRatesTMDialog").openDialog();
                    ResourceRatesTMWidget.getDialog("ImportWorkspaceResourceRatesTMDialogResults").closeDialog();
                    ResourceRatesTMWidget.setDirty("ImportWorkspaceResourceRatesTMForm");
                }
            }, {
                ButtonClass: "ies-action",
                ButtonText: 'Complete import',
                ButtonName: 'complete-import-button',
                ButtonAction: function (buttonPressed) {
                    ResourceRatesTMWidget.saveRequest(
                    {
                        url: importURL,
                        data: JSON.stringify(JSON.cloneDataAsStrings(ResourceRatesTMWidget.getImportData())),
                        success: function (response) {
                            ResourceRatesTMWidget.getDialog("ImportWorkspaceResourceRatesTMDialogResults").closeDialog();
                            delete ResourceRatesTMWidget.pagingData.data;
                            ResourceRatesTMWidget.pageResults("down");
                            ResourceRatesTMWidget.cleanDirty("ImportWorkspaceResourceRatesTMForm");
                        }
                    }, buttonPressed);
                }
			}],
			HideOCI: false,
			// We Load both OCI and NON OCI texts so that the Generation.JS will use the ContainsOCI to display the correct text
			BannerTextWithOCI: <%: SiteMasterUtilities.GetBannerText() %>,
			BannerTextWithoutOCI: <%: SiteMasterUtilities.GetBannerText(true) %>
        });

        var widgetConfig = {};
        widgetConfig.ContextID = "WorkspaceResourceRatesTM";
        widgetConfig.IsModule = true;
        widgetConfig.deleteable = onDeletePressed;
        widgetConfig.PagingData = pagingData;
        widgetConfig.DialogConfigs = DialogConfigs;
        widgetConfig.FormConfigs = FormConfigs;
        widgetConfig.isReadOnly = <%: ViewData["READONLY"] %>;

        ResourceRatesTMWidget = new GenListWidget(widgetConfig);

        $("#ImportWorkspaceResourceRatesTMForm input[type=radio]").click(function () {
            $("input[type=file]").val("");
            $(".importOption").hide();
            $(".importOption." + $(this).val()).show();
            $("#ImportWorkspaceResourceRatesTMForm input[type=file]").prop("disabled", true);
            $("#ImportWorkspaceResourceRatesTMForm input[type=file]:visible").prop('disabled', false);
        });

        //export Button Click.
        ResourceRatesTMWidget.registerForDelegateEvent('click', "button[name='export-button']:not(.disabled)", function () {

            // Remove the old hidden iFrame, if it exists
            $('iframe#ExportTarget').remove();

            // Create a new hidden iFrame and set it's source to the chosen report's URL
            var targetIFrame = $('<iframe />', {
                'id': 'ExportTarget',
                'class': 'display-none',
                'src': exportURL
            });

            // Append the iFrame to the body, causing the controller action to fire and
            // the download to occur inside the iFrame
            targetIFrame.appendTo('body');
        });

        //Add Button Click.
        ResourceRatesTMWidget.getElement("button[name='add-button']").click(openAddEdit);

        ResourceRatesTMWidget.ValidateFileInput = function () {
            $("#ImportWorkspaceResourceRatesTMForm button[name='import-button']").removeClass('disabled');
        };

        $('#ImportWorkspaceResourceRatesTMForm input[type=file]').change(ResourceRatesTMWidget.ValidateFileInput);

        ResourceRatesTMWidget.getElement("button[name='import-button']").click(function () {
            ResourceRatesTMWidget.getForm("ImportWorkspaceResourceRatesTMForm").resetForm();
            ResourceRatesTMWidget.getDialog("ImportWorkspaceResourceRatesTMDialog").openDialog();
        });

        function UploadComplete(uploadResponse) { //Function will be called when iframe is loaded
            $("#ImportWorkspaceResourceRatesTMDialogResults div.import-text-place").html(uploadResponse);
            ResourceRatesTMWidget.getDialog("ImportWorkspaceResourceRatesTMDialog").closeDialog();
            ResourceRatesTMWidget.getDialog("ImportWorkspaceResourceRatesTMDialogResults").openDialog();
        }

        ResourceRatesTMWidget.pageResults("down");
    });
</script>

<div id="WorkspaceResourceRatesTM" class="module">
    <div class="module-header-data">
        <%= Model.WorkspaceResourceRateTMHeadingText %>
    </div>
    <div class="module-content-data">
        <div class="form-row">
            <%= Model.WorkspaceResourceRateTMControlDescription %>
        </div>
        <form id="DeleteWorkspaceResourceRatesTMForm">
            <div class="form-row widget-menu">
                <div class="buttons inline">
                    <button name="add-button" class="ies" id="Add-ResourceRateTM" type="button">+ Add</button>
                    <button class="ies" id="Import-ResourceRateTM" name="import-button" type="button">Import</button>
                    <button class="ies" id="Export-ResourceRateTM" name="export-button" type="button">Export</button>
                </div>
            </div>
        </form>
        <div class="paging-area"></div>
    </div>

    <div id="ImportWorkspaceResourceRatesTMDialog" style="height: 600px">
        <form id="ImportWorkspaceResourceRatesTMForm">
            To import T&M Resource Rates, follow the steps below.
                <div class="step one">
                    <div class="title">Step 1: Select what to import</div>
                    <div>
                        <input class="ignore-dirty" type="radio" value="replaceAll" name="importOption" id="ResourceRateTMReplaceAll" /><label for="ResourceRateTMReplaceAll">Import <i>and</i> replace all T&M Resource Rates for this workspace.</label>
                    </div>
                    <div>
                        <input class="ignore-dirty" type="radio" value="importNew" name="importOption" id="ResourceRateTMImportNew" /><label for="ResourceRateTMImportNew">Import <i>only</i> new T&M Resource Rates for this workspace.</label>
                    </div>
                    <div>
                        <input class="ignore-dirty" type="radio" value="importUpdates" name="importOption" id="ResourceRateTMImportUpdates" /><label for="ResourceRateTMImportUpdates">Import <i>new</i> T&M Resource Rates <i>and</i> import updates to existing T&M Resource Rates for this workspace.</label>
                    </div>
                </div>
            <div class="importOption replaceAll">
                <div class="step two">
                    <div class="title">Step 2: Verify the T&M Resource Rate file is correct</div>
                    <div>
                        Start by downloading the T&M Resource Rate template file. This file has the correct column headings BOE needs to import the T&M Resource Rates.
                    </div>
                    <div>
                        If you have T&M Resource Rates in a different file, in order to use it, it must have the same exact headings as the T&M Resource Rate template file 
                            and the format of the Resource ID, Start Date, and End Date cells must be set to Text. The file type must be an Excel .xlsx file.
                    </div>
                    <div>
                        <a class="exportTemplateLink">Download T&M Resource Rate template file</a>
                    </div>
                    <div class="important">
                        IMPORTANT: Do not change the column headings or fields in the file. 
                    </div>
                </div>
                <div class="step three">
                    <div class="title">Step 3: Verify the T&M Resource Rate file is correct</div>
                    Choose a file to import. The file you import must be an Excel file that ends in .xlsx.
                        <input type="file" size="60" name="file" />
                </div>
            </div>
            <div class="importOption importNew">
                <div class="step two">
                    <div class="title">Step 2: Verify the T&M Resource Rate file is correct</div>
                    <div>
                        Start by downloading the T&M Resource Rate template file. This file has the correct column headings BOE needs to import the T&M Resource Rates.
                    </div>
                    <div>
                        If you have T&M Resource Rates in a different file, in order to use it, it must have the same exact headings as the T&M Resource Rate template file 
                            and the format of the Resource ID, Start Date, and End Date cells must be set to Text. The file type must be an Excel .xlsx file.
                    </div>
                    <div>
                        <a class="exportTemplateLink">Download T&M Resource Rate template file</a>
                    </div>
                </div>
                <div class="step three">
                    <div class="title">Step 3: Enter T&M Resource Rates into the file</div>
                    <div>
                        Enter T&M resource rates into the file. If you have a T&M resource rate in a different file, you can copy and paste T&M resource rates from it into the 
                        BOE T&M Resource Rates template. Make sure the T&M resource rates data you copy matches the column headings provided in the template.<br />
                    </div>
                    <div>
                        A Resource ID must exist for the Workspace.
                        <br />
                        Start and End dates must be in the following format:  mm/yyyy.
                    </div>
                    <div class="important">
                        IMPORTANT: Do not change the column headings or options in the file. Do not enter 
                        a value for BOE Rate ID in column A. BOE Rate IDs are unique identifiers for T&M Resource Rates. 
                        One will be automatically generated for each new T&M Resource Rate once the import is complete. 
                        Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.
                    </div>
                </div>
                <div class="step four">
                    <div class="title">Step 4: Import the updated T&M Resource Rates file</div>
                    Choose a file to import. The file you import must be an Excel file that ends in .xlsx.
                        <input type="file" size="60" name="file" />
                </div>
            </div>
            <div class="importOption importUpdates">
                <div class="step two">
                    <div class="title">Step 2: Export the existing T&M Resource Rates</div>
                    <div>
                        Start by exporting the existing T&M Resource Rates. This file has the correct column headings and BOE needs to import new T&M Resource Rates and updates.
                    </div>
                    <a class="exportLink">Export existing T&M Resource Rates</a>
                </div>
                <div class="step three">
                    <div class="title">Step 3: Enter T&M Resource Rates into the file</div>
                    <div>
                        Enter or update T&M resource rates in the file. If you have a T&M resource rate in a different file, you can copy and paste T&M resource rates from it into the BOE T&M Resource Rates template. 
                            Make sure the T&M resource rates data you copy matches the column headings provided in the template.<br />
                    </div>
                    <div>
                        A Resource ID must exist for the Workspace.
                            <br />
                        Start and End dates must be in the following format:  mm/yyyy.
                    </div>
                    <div class="important">
                        IMPORTANT: Do not change the column headings or options in the file. Do not enter 
                            a value for BOE Rate ID in column A. BOE Rate IDs are unique identifiers for T&M Resource Rates. 
                            One will be automatically generated for each new T&M Resource Rate once the import is complete. 
                            Column A and the list of options have been hidden to prevent accidental edits. These need to be unchanged for the import to work.
                    </div>
                    <div class="important">
                        If you delete a T&M Resource Rate from the file, the T&M Resource Rate will not be removed in BOE. To delete a T&M Resource Rate, it must be 
                            deleted directly in BOE to properly remove all associations, or select the Import and replace all T&M Resource Rates option on this import screen. 
                    </div>
                </div>
                <div class="step four">
                    <div class="title">Step 4: Import the updated T&M Resource Rates file</div>
                    Choose a file to import. The file you import must be an Excel file that ends in .xlsx.
                        <input type="file" size="60" name="file" />
                </div>
            </div>
            <div class="form-element button-container" style="text-align: center; width: 100%;" />
        </form>
    </div>

    <div id="ImportWorkspaceResourceRatesTMDialogResults">
        <form id="FinishImportWorkspaceResourceRatesTMForm">
            <div class="import-text-place"></div>


        </form>
    </div>

</div>
