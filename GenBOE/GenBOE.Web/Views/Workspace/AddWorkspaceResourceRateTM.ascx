<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.Workspace.WorkspaceResourceRateTMModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>

<% JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue }; %>

<script type="text/javascript">
    $(function() {
        var model = <%= serializer.Serialize(Model) %>;

        function getData() {
            var toReturn = {};
            toReturn = AddRateTMWidget.getForm("AddEditWorkspaceResourceForm").getData();
            if(toReturn.ResourceRateID>0)
            {
                toReturn.ResourceID = model.ResourceID;
            }
            return JSON.stringify([toReturn]);
        };

        function saveSuccess() {
            window.location.href = '#ResourceRatesTM';
        };

        function clearData() {
            AddRateTMWidget.getForm("AddEditWorkspaceResourceForm").resetForm();
            $('input[name=ResourceRateID]').val('-1');
            $('input[name=ToDelete]').val('false');
        };

        var saveURL = GenSession.CreatePostURL(
                        '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%:WebConstants.CONTROLLER_WORKSPACE %>',
                        '<%:WebConstants.ACTION_SAVE_WORKSPACE_RESOURCE_RATE_TM %>', '');

        var DialogConfigs = [];

        var FormConfigs = [];
        FormConfigs.push({
            ElementID: "AddEditWorkspaceResourceForm",
            ContainsOCI: <%= ViewData["ContainsOCI"] %>,
            Buttons: [{
                ButtonClass: 'ies-action',
                ButtonText: 'Save',
                ButtonName: 'save-button',
                Stateful: true,
                ButtonAction: function (buttonPressed) {
                    AddRateTMWidget.saveRequest({
					    url: saveURL,
						data: getData,
						success: function () { saveSuccess(); }
				    }, buttonPressed);
                }
            }, {
                ButtonClass: 'ies',
                ButtonText: 'Save & add another',
                ButtonName: "save-add-another-button",
                Stateful: true,
                ButtonAction: function (buttonPressed) {
                    AddRateTMWidget.saveRequest({
					    url: saveURL,
						data: getData,
						success: function () {
						  	clearData();
						}
				    }, buttonPressed);
                }
            }, {
                ButtonClass: 'ies',
                ButtonText: 'Cancel',
                ButtonName: "cancel-button",
                Stateful: false,
                ButtonAction: function (buttonPressed) {
                    window.location.href = '#ResourceRatesTM';
                    }, 
                }
			]
        });

        var widgetConfig = {};
        widgetConfig.ContextID = "AddEditWorkspaceResourceRate";
        widgetConfig.IsModule = true;
        widgetConfig.DialogConfigs = DialogConfigs;
        widgetConfig.FormConfigs = FormConfigs;

        AddRateTMWidget = new GenWidget(widgetConfig);

        if (model.ResourceRateID > 0) {
            ShowLoadingBox();

            // disable the segment/resource ids but also set the values from the model before disabling            
            $('#AddEditWorkspaceResourceForm select[name=ResourceID]').val(model.ResourceID).prop('disabled', true);

            $('#AddEditWorkspaceResourceForm input[name=StartDate]').val(model.StartDate);
            $('#AddEditWorkspaceResourceForm input[name=StartDate]').removeClass('placeholder');
            $('#AddEditWorkspaceResourceForm input[name=EndDate]').val(model.EndDate);
            $('#AddEditWorkspaceResourceForm input[name=EndDate]').removeClass('placeholder');
            $('#AddEditWorkspaceResourceForm input[name=ResourceRate]').val(model.ResourceRate);

            HideLoadingBox();
        }

        $("#AddEditWorkspaceResourceForm select[name=ResourceID]").change(function () {  });

        AddRateTMWidget.registerForEvent('CLEAN_WORKSPACE_SETTINGS_DIRTY', function () { AddRateTMWidget.cleanDirty(); });
    });

</script>

<div id="AddEditWorkspaceResourceRate" class="add-edit-workspace-resource-rate module">
    <div class="module-header-data">
        <% if (Model.ResourceRateID > 0) { Response.Write("Edit Rate"); }
           else { Response.Write("Add Rate"); } %>
    </div>
    <div class="module-content-data">
        <form id="AddEditWorkspaceResourceForm">
            <input type="hidden" name="ResourceRateID" value="<%: Model.ResourceRateID %>" />
            <input type="hidden" name="UpdateDateLong" value="<%: Model.UpdateDateLong %>"/>
            <input type="hidden" name="ToDelete" value="false" />
            <div class="form-row">
                <div class="form-label">Resource ID*</div>
                <div class="form-element">
                    <select name="ResourceID">
                        <% Response.Write(ViewData["WORKSPACE_RATES"]); %>
                    </select>
                </div>  
            </div>
            <div class="form-row">
                <div class="form-label">Start Date</div>
                <div class="form-element">
                    <input type="text" maxlength="7" type="text" placeholder="mm/yyyy" class="datepicker" name="StartDate" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">End Date</div>
                <div class="form-element">
                    <input type="text" maxlength="7" type="text" placeholder="mm/yyyy" class="datepicker" name="EndDate" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label">Rate $</div>
                <div class="form-element">
                    <input type="text" name="ResourceRate" />
                </div>
            </div>
            <div class="form-row">
                <div class="form-label"></div>
                <div class="form-element button-container-wide"></div>
            </div>
        </form>
    </div>
</div>