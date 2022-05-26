<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<GenBOE.Web.ModelView.BOEFormMasterModelView>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Create Form
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <% 
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
    %>
    <script type="text/javascript">

        var ManageBOEFormsWidget;
        $(function () {

            var formConfigs = [];
            formConfigs.push({
                ElementID: 'CreateBOEForm',
                Buttons: [
                {
                    ButtonClass: 'ies-action',
                    ButtonText: '+ Add',
                    ButtonName: 'add-button',
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
                }],
                ContainsOCI: false,
            });

            var widgetConfig = {
                ContextID: "CreateBOEFormsForm",
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

            ManageBOEFormsWidget.Save = function (button) {
                // Check that the select is actually selecting something
                var boeFormType = $('#BOEFormType').val();
                if (boeFormType == '<%:(int)BOEFormType.NotSet%>') {
                    Session.alertDialog("Form Type", "A Form Type must be selected to save.");
                } else {
                    // Ensure any currency formatting is stripped
					const proposedVal = $('#SupplierProposedValue').val();
					if (proposedVal !== undefined) {
						const unformattedValue = proposedVal.replace(',', '').replace('$', '');
						$('#SupplierProposedValue').val(unformattedValue);
					}

                    var data = ManageBOEFormsWidget.getForm('CreateBOEForm').getData();

                    // Get the Form Types once.
                    var boeFormTypePBOE = <%:(int)BOEFormType.PBOE%>;
                    var boeFormTypeIBOE = <%:(int)BOEFormType.IBOE%>;
                    
                    // there seems to be an issue with the jquery serializer when dealing with rich text pasted from excel so we need to get these field contents again

                    // PBOE TinyMCE Editors.
                    if (boeFormType == boeFormTypePBOE)
                    {
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

                    // IBOE TinyMCE Editors.
                    if (boeFormType == boeFormTypeIBOE)
                    {
                        if (tinyMCE.EditorManager.editors.Description_IBOE) {
                            data.Description = tinyMCE.EditorManager.editors.Description_IBOE.getContent();
                        }

                        if (tinyMCE.EditorManager.editors.BasisAndRationale_IBOE) {
                            data.BasisAndRationale = tinyMCE.EditorManager.editors.BasisAndRationale_IBOE.getContent();
                        }
                    }

                    var dataToSend = JSON.stringify(data);
                    var action = (boeFormType == boeFormTypeIBOE) ? '<%:WebConstants.ACTION_SAVE_IBOE_FORM%>' : '<%:WebConstants.ACTION_SAVE_PBOE_FORM%>';

                    var saveUrl = GenSession.CreateUrl({
                        workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        controller: '<%:WebConstants.CONTROLLER_BOE_FORMS %>',
                        action: action});
                    ManageBOEFormsWidget.saveRequest({
                        url: saveUrl,
                        data: dataToSend,
                        success: ManageBOEFormsWidget.CancelToMainGrid,
                        error: function(response) {
                        }
                    }, button);
                }
            };

            ManageBOEFormsWidget.CancelToMainGrid = function () {
                ManageBOEFormsWidget.cleanDirty();
                $("#CreateBOEFormsForm").html('<div class="loader"></div>');
                var url = "/<%: SiteMasterUtilities.GetCurrentWorkspace() %>/BOEForm";
                window.location.href = url; 
            };

            ManageBOEFormsWidget.BindEvents = function() {
                $('#BOEFormType').change(function(e) {
                    $('#boeFormContainer div.boeForm').addClass('display-none');
                    $('#boeFormContainer div.display-none').html("");
                    var boeDiv = $('#boeFormType'+e.target.value);
                    if ($('#BOEFormType').val() != '0'){
                        boeDiv.removeClass('display-none');
                        boeDiv.html('<div class="loader"></div>');

                        var boeFormUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                            '<%:WebConstants.CONTROLLER_BOE_FORMS %>',
                            '<%: WebConstants.ACTION_CREATE_BOE_FORM %>',
                            '?boeFormType=' + e.target.value);

                        $.ajax({
                            type: 'POST',
                            url: boeFormUrl,
                            dataType: 'html',
                            success: function(response)
                            {             
                                boeDiv.html(response);
                                if (!ManageBOEFormsWidget.isReadOnly()) {
                                    InitializeRTE('Description_PBOE', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_SourceSelection', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_Commerciality', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_TechnicalEvaluation', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_PriceAnalysis', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_CostAnalysis', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_RationaleValueSummary', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('Description_IBOE', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    InitializeRTE('BasisAndRationale_IBOE', { maxlen: <%: Constants.MAX_RTE_LENGTH %> }, ManageBOEFormsWidget);
                                    ManageBOEFormsWidget.refreshModule();
                                }
                                else  // read-only
                                {
                                    HandleRTEDataForReadOnly("#Description_PBOE", ".replacedWidgetText");
                                    HandleRTEDataForReadOnly('#Description_SourceSelection', 'replacedWidgetText');
                                    HandleRTEDataForReadOnly('#Description_Commerciality', 'replacedWidgetText');
                                    HandleRTEDataForReadOnly('#Description_TechnicalEvaluation', 'replacedWidgetText');
                                    HandleRTEDataForReadOnly('#Description_PriceAnalysis', 'replacedWidgetText');
                                    HandleRTEDataForReadOnly('#Description_CostAnalysis', 'replacedWidgetText');
                                    HandleRTEDataForReadOnly('#Description_RationaleValueSummary', 'replacedWidgetText');
                                    HandleRTEDataForReadOnly("#Description_IBOE", ".replacedWidgetText");
                                    HandleRTEDataForReadOnly("#BasisAndRationale_IBOE", ".replacedWidgetText");
                                }

                                ManageBOEFormsWidget.refreshModule($('#CreateBOEFormsForm'));
                                ManageBOEFormsWidget.applyHelpPopouts();
                            }
                        });
                    } else {
                        ManageBOEFormsWidget.refreshModule($('#CreateBOEFormsForm'));
                    }
                });
            }

            ManageBOEFormsWidget.BindEvents();
    });
     
	</script>
    <div id="CreateBOEFormsForm" class="manage-BOEForms module">
        <div class="module-header-data">Add Integrated Non-Labor Form</div>
        <div class="module-content-data" style="width: 900px;">
            <div class="form-row">
                <div class="form-label">Form Type *</div>
                <div class="form-element"><%: Html.EnumDropDownListFor(m => m.BOEFormType, new { id = "BOEFormType" }) %></div>
            </div>
            <div id="boeFormContainer">
                <% using (Html.BeginForm("", "", FormMethod.Post, new { id = "CreateBOEForm", onSubmit = "return false" }))
                    {
                        %>
                        <div id="boeFormType<%: (int)BOEFormType.IBOE %>" class="display-none boeForm"></div>
                        <div id="boeFormType<%: (int)BOEFormType.PBOE %>" class="display-none boeForm"></div>
                        <%
                    }%>
            </div>
        </div>
    </div>
</asp:Content>