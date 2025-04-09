<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<GenBOE.Web.ModelView.BOEFormMasterModelView>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Update Boe Form
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% 
        var partialName = string.Format("Forms/{0}_{1}", Model.BOEFormType.GetDescription(), Model.Version.ToString());
        GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView boeFormModel = (Model.BOEFormType == BOEFormType.IBOE) ? (GenBOE.ActionLogic.ModelView.BOE.BOEFormModelView)Model.IBOEModel : Model.PBOEModel;
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    %>
    <script type="text/javascript">
        var ManageBOEFormsWidget;
        $(function () {
            
            var formConfigs = [];
            formConfigs.push({
                ElementID: 'ManageBOEForm',
                Buttons: [
                {
                    ButtonClass: 'ies-action',
                    ButtonText: 'Save',
                    ButtonName: 'save-button',
                    ButtonAction: function (buttonPressed) {
                        ManageBOEFormsWidget.Save($(buttonPressed));
                    },
                    Stateful: true
                }, {
                    ButtonClass: 'ies',
                    ButtonText: 'Cancel',
                    ButtonName: 'cancel-button',
                    ButtonAction: function (buttonPressed) {
                        if (ManageBOEFormsWidget.isDirty()) {
                            Session.confirmDialog("Cancel", "Are you sure you want to cancel all changes?", ManageBOEFormsWidget.CancelToMainGrid, null);
                        }
                        else {
                            ManageBOEFormsWidget.CancelToMainGrid();
                        }
                    },
                    Stateful: false
                }, {
                    ButtonClass: ' ies boe-validate-button',
                    ButtonText: 'Validate',
                    ButtonAction: function (buttonPressed) {
                        ManageBOEFormsWidget.ValidateForm($(buttonPressed));
                    },
                    Stateful: false
                }],
				ContainsOCI: false,
				HideOCI: false,
				// We Load both OCI and NON OCI texts so that the Generation.JS will use the ContainsOCI to display the correct text
				BannerTextWithOCI: '<%: SiteMasterUtilities.GetBannerText() %>',
				BannerTextWithoutOCI: '<%: SiteMasterUtilities.GetBannerText(true) %>'
			});
            var widgetConfig = {
                ContextID: "UpdateBOEFormsForm",
                isReadOnly: <%:ViewData["READONLY"]%>, 
                IsModule: true,
                FormConfigs: formConfigs,
                DialogConfigs: []
            };
            ManageBOEFormsWidget = new GenWidget(widgetConfig);

            ManageBOEFormsWidget.SubResources = <%= serializer.Serialize(Model.SubResources)%>;
            ManageBOEFormsWidget.IWTAResources = <%= serializer.Serialize(Model.IWTAResources)%>;
            ManageBOEFormsWidget.SubInUseResourceIds = <%= serializer.Serialize(Model.SubInUseResourceIds)%>;
            ManageBOEFormsWidget.IWTAInUseResourceIds = <%= serializer.Serialize(Model.IWTAInUseResourceIds)%>;
            ManageBOEFormsWidget.IsIBOE = function() {
                // Get the Form Type from the Model.
                var boeFormType = <%:(int)Model.BOEFormType%>;

                // Get the Form Types once.
                var boeFormTypePBOE = <%:(int)BOEFormType.PBOE%>;
                var boeFormTypeIBOE = <%:(int)BOEFormType.IBOE%>;

                return boeFormType === boeFormTypeIBOE;
            };

            ManageBOEFormsWidget.GetData = function() {
                var data = ManageBOEFormsWidget.getForm('ManageBOEForm').getData();
                // there seems to be an issue with the jquery serializer when dealing with rich text pasted from excel so we need to get these field contents again
                if (ManageBOEFormsWidget.IsIBOE())
                {
                    // IBOE TinyMCE Editors.
                
                    if (tinyMCE.EditorManager.editors.Description_IBOE) {
                        data.Description = tinyMCE.EditorManager.editors.Description_IBOE.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.BasisAndRationale_IBOE) {
                        data.BasisAndRationale = tinyMCE.EditorManager.editors.BasisAndRationale_IBOE.getContent();
                    }
                } else {
                    // PBOE TinyMCE Editors.
                    if (tinyMCE.EditorManager.editors.Description_PBOE) {
                        data.Description = tinyMCE.EditorManager.editors.Description_PBOE.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.Description_SourceSelection) {
                        data.SourceSelectionDescription = tinyMCE.EditorManager.editors.Description_SourceSelection.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.Description_Commerciality) {
                        data.CommercialityDescription = tinyMCE.EditorManager.editors.Description_Commerciality.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.Description_TechnicalEvaluation) {
                        data.TechnicalEvaluationDescription = tinyMCE.EditorManager.editors.Description_TechnicalEvaluation.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.Description_PriceAnalysis) {
                        data.PriceAnalysisDescription = tinyMCE.EditorManager.editors.Description_PriceAnalysis.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.Description_CostAnalysis) {
                        data.CostAnalysisDescription = tinyMCE.EditorManager.editors.Description_CostAnalysis.getContent();
                    }

                    if (tinyMCE.EditorManager.editors.Description_RationaleValueSummary) {
                        data.RationaleValueSummary = tinyMCE.EditorManager.editors.Description_RationaleValueSummary.getContent();
                    }
                }
                
                var dataToSend = JSON.stringify(data);
                return dataToSend;
            };

            ManageBOEFormsWidget.StripCurrencyFormatting = function () {
				const proposedVal = $('#SupplierProposedValue').val();
				if (proposedVal !== undefined) {
					const unformattedValue = proposedVal.replace(',', '').replace('$', '');
					$('#SupplierProposedValue').val(unformattedValue);
				}
            }

            ManageBOEFormsWidget.Save = function (button) {
                ManageBOEFormsWidget.StripCurrencyFormatting();

                var dataToSend = ManageBOEFormsWidget.GetData();
                console.log(dataToSend);

                var action = (ManageBOEFormsWidget.IsIBOE()) ? '<%:WebConstants.ACTION_SAVE_IBOE_FORM%>' : '<%:WebConstants.ACTION_SAVE_PBOE_FORM%>';

                var saveUrl = GenSession.CreateUrl({
                    workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    controller: '<%:WebConstants.CONTROLLER_BOE_FORMS %>',
                    action: action});
                ManageBOEFormsWidget.saveRequest({
                    url: saveUrl,
                    data: dataToSend,
                    success: ManageBOEFormsWidget.CancelToMainGrid,
                    error: function (response) {
                        $('#SupplierProposedValue').trigger('change');
                    }
                }, button);
            };

            ManageBOEFormsWidget.ValidateForm = function (button) {
                ManageBOEFormsWidget.StripCurrencyFormatting();

                var dataToSend = ManageBOEFormsWidget.GetData();
                var action = (ManageBOEFormsWidget.IsIBOE()) ? '<%:WebConstants.ACTION_VALIDATE_IBOE_FORM%>' : '<%:WebConstants.ACTION_VALIDATE_PBOE_FORM%>';
                var validateUrl = GenSession.CreateUrl({
                    workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                    controller: '<%:WebConstants.CONTROLLER_BOE_FORMS %>',
                    action: action});

                ManageBOEFormsWidget.saveRequest({
                    url: validateUrl,
                    performDirtyProcessing: false,
                    data: dataToSend,
                    success: function(response) {
                        Session.alertDialog('INL Form Validation', 'No Validation errors found.');
                        $('#SupplierProposedValue').trigger('change'); // reset the currency formatting
                    },
                    error: function(response) {
                        // nothing to do since genvalidation exceptions get automatically handled
                        $('#SupplierProposedValue').trigger('change');
                    }
                }, button);
            };

            ManageBOEFormsWidget.CancelToMainGrid = function () {
                ManageBOEFormsWidget.cleanDirty();
                $("#UpdateBOEFormsForm").html('<div class="loader"></div>');
                var url = "/<%: SiteMasterUtilities.GetCurrentWorkspace() %>/BOEForm";
                window.location.href = url; 
            };

            if (!ManageBOEFormsWidget.isReadOnly()) {
                // PBOE
                InitializeRTE('Description_PBOE', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('Description_SourceSelection', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('Description_Commerciality', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('Description_TechnicalEvaluation', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('Description_PriceAnalysis', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('Description_CostAnalysis', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('Description_RationaleValueSummary', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                // IBOE
                InitializeRTE('Description_IBOE', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                InitializeRTE('BasisAndRationale_IBOE', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
            }
            else  /* read-only */
            {
                // PBOE
                HandleRTEDataForReadOnly("#Description_PBOE", ".replacedWidgetText");
                HandleRTEDataForReadOnly('#Description_SourceSelection', 'replacedWidgetText');
                HandleRTEDataForReadOnly('#Description_Commerciality', 'replacedWidgetText');
                HandleRTEDataForReadOnly('#Description_TechnicalEvaluation', 'replacedWidgetText');
                HandleRTEDataForReadOnly('#Description_PriceAnalysis', 'replacedWidgetText');
                HandleRTEDataForReadOnly('#Description_CostAnalysis', 'replacedWidgetText');
                HandleRTEDataForReadOnly('#Description_RationaleValueSummary', 'replacedWidgetText');
                // IBOE
                HandleRTEDataForReadOnly("#Description_IBOE", ".replacedWidgetText");
                HandleRTEDataForReadOnly("#BasisAndRationale_IBOE", ".replacedWidgetText");
            }

            ManageBOEFormsWidget.refreshModule();

            // After the page loads, easy thing would be to call validate to show the validation errors at the top of the page 
            //  except tinymce is not always done loading so we add this manually
            <% if (boeFormModel.IsIncomplete)
            {
                string messages = string.Empty;
                foreach(string message in boeFormModel.IncompleteMessages)
                {
                    messages += "<li>" + message + "</li>";
                }
            %>
            var validations = '<%= messages%>';
            $('#ManageBOEForm .validation-box').html(validations).show();
            <%  } %>
    });
     
	</script>
    <div id="UpdateBOEFormsForm" class="manage-BOEForms module">
        <div class="module-header-data">Edit Integrated Non-Labor Form</div>
        <div class="module-content-data" style="width: 900px;">
            <div class="form-row">
                <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "ManageBOEForm", onSubmit = "return false" }))
                    {
                        Html.RenderPartial(partialName, Model, this.ViewData);
                    }%>
            </div>
        </div>
    </div>
</asp:Content>
















