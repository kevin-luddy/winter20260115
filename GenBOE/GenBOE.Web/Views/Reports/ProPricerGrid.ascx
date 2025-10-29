<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<IEnumerable<ExportToProPricerModelView>>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%
    bool enableSendDirectly = (bool) ViewData["EnableSendDirectly"];
    bool isSystemAdmin = (bool)ViewData["IsSystemAdmin"];

    SendToProPricerModelView sendData = (SendToProPricerModelView)ViewData["SendToProPricerData"];
    JavaScriptSerializer serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
    // Create Discrepancy Validator.
    var validateDiscrepancyUrl = CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                '<%: WebConstants.CONTROLLER_REPORTS %>',
		'<%: WebConstants.ACTION_VALIDATE_BOES_FOR_DISCREPANCIES %>', '');
    var DiscrepancyValidator = InitializeDiscrepancyValidator(validateDiscrepancyUrl);

    // Create Grid Widget
    var ExportToProPricerGridWidget = new GridWidget("ExportToProPricerGrid", "pkid", '<%= ViewData["READONLY"] %>'.isTrue());
    ExportToProPricerGridWidget.Module = {};
    ExportToProPricerGridWidget.EnableSendDirectly = '<%=enableSendDirectly %>'.isTrue();

    ExportToProPricerGridWidget.Initialize = function () {
        ExportToProPricerGridWidget.Module = $('.export-to-propricer-grid').parents('.module');
        ExportToProPricerGridWidget.SetCopyFromFormatNameSelections();

        if (!ExportToProPricerGridWidget.EnableSendDirectly) {
            $(".send").addClass("display-none");
            $(".export").addClass("last-child");
        }
    };

    ExportToProPricerGridWidget.BindEvents = function () {
        // Bind Select All Checkbox change event
        $('#ExportToProPricerGrid thead th.select-checkbox input.checkbox').change(function () {
            if ($(this).is(':checked')) {
                $('#ExportToProPricerGrid tbody input.checkbox').prop('checked', true);
            }
            else {
                $('#ExportToProPricerGrid tbody input.checkbox').prop('checked', false);
            }

            ExportToProPricerGridWidget.CheckForDeleteButtonEnabled();
        });

        // Bind individual row Checkbox change events
        $('#ExportToProPricerGrid tbody input.checkbox').change(function () {
            ExportToProPricerGridWidget.CheckForDeleteButtonEnabled();
        });

        $('#ExportToProPricerGrid tbody tr[pkid] td[name=Edit]').click(function () {
            $(document).trigger('EDIT_PROPRICER_EXPORT_FORMAT', $(this).parents('tr').get());
        });

        $('#ExportToProPricerGrid tbody tr[pkid] td[name=Export]').click(function () {
            var reportId = $(this).attr('formatID');
            var reportName = $(this).attr('formatName');
            var scope = $(this).attr('scope');

            ExportToProPricerGridWidget.ExportCsvFiles(reportId, reportName, scope);
        });

        $('#ExportCsvFiles').click(function () {
            var reportId = $(this).attr('formatID');
            var reportName = $(this).attr('formatName');
            var scope = $(this).attr('scope');

            ExportToProPricerGridWidget.ExportCsvFiles(reportId, reportName, scope);
        });

        $('#ExportToProPricerGrid tbody tr[pkid] td[name=Send]').click(function () {
            var reportId = $(this).attr('formatID');
            var reportName = $(this).attr('formatName');
            var scope = $(this).attr('scope');

            ExportToProPricerGridWidget.SendDirectly(reportId, reportName, scope);
        })
    };

    ExportToProPricerGridWidget.ExportCsvFiles = function (reportId, reportName, scope) {
        // Establish the function to run if there are no discrepancies or if the user elects to proceed.
        var proceedWithExportFunction = function () {
            var format = { formatID: reportId, formatName: reportName, scope: scope };
            $(document).trigger('EXPORT_PROPRICER_EXPORT_FORMAT', format);
        };
        DiscrepancyValidator.ValidateDiscrepancies(proceedWithExportFunction);
    };

    ExportToProPricerGridWidget.SendDirectly = function (reportId, reportName, scope) {
        // Establish function to call SendToProPricerClicked in ProPricerController.js if there are no discrepancies or if the user elects to proceed
        var angularScope = angular.element($('#ProPricerController')).scope();

        var proceedWithSendFunction = function () {
            angularScope.$apply(function () {
                angularScope.SendToProPricerClicked(reportId, reportName, scope);
            })
        };

        DiscrepancyValidator.ValidateDiscrepancies(proceedWithSendFunction);
    };

    // Trigger an event to tell the main UI which formats can be copied from
    ExportToProPricerGridWidget.SetCopyFromFormatNameSelections = function () {
        var availableFormatsJSON = '[<%= String.Join(",", Model.Select(x =>  "{ \"ID\" : " + x.ID + ", \"Name\" : \"" + x.Name + "\"}")) %>]';

        $(document).trigger('SET_PROPRICER_COPY_FROM_FORMATS', availableFormatsJSON);
    };

    ExportToProPricerGridWidget.CheckForDeleteButtonEnabled = function () {
        $(document).trigger('ENABLE_PROPRICER_DELETE_BUTTON', $('#ExportToProPricerGrid tbody input.checkbox:checked').length > 0);
    };

    $(function () {
        ExportToProPricerGridWidget.Initialize();
        ExportToProPricerGridWidget.BindEvents();
        ExportToProPricerGridWidget.refreshModule();
    });

    angular.module('genboe').value('ProPricerModel', {
        BaseUrl: '<%=this.ResolveClientUrl("~/")%>',
        SendToProPricerData: <%=serializer.Serialize(sendData)%>,
        ProPricerUrl: '<%= ConfigurationUtilities.GetAppSetting("ProPricerApiUrl")%>',
        LastProposalAction: '<%= WebConstants.ACTION_UPDATE_WS_PROPRICER_LAST_PROPOSAL%>',
        LastInstanceAction: '<%= WebConstants.ACTION_UPDATE_WS_PROPRICER_LAST_INSTANCE%>',
        PreviewAction: '<%= WebConstants.ACTION_WS_PROPRICER_PREVIEW%>',
        ExportAction: '<%= WebConstants.ACTION_WS_PROPRICER_EXPORT%>',
        Controller: '<%= WebConstants.CONTROLLER_REPORTS%>',
        Workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
        CopyToSystemFormatAction: '<%= WebConstants.ACTION_COPY_FORMAT_TO_SYSTEM_LEVEL %>',
    });
</script>
<div id="ProPricerController" data-ng-controller="ProPricerController">
    <gen-validation data-errors="errors"></gen-validation>
    <table id="ExportToProPricerGrid" class="export-to-propricer-grid readonly grid full-width">
        <thead>
            <tr>
                <th class="select-checkbox"><input type="checkbox" class="checkbox" id="SelectAllCheckbox" /></th>
                <th class="scope">
                    Scope
                    <div id="ExportToProPricerGrid-ScopeHelp" class="help-icon" style="margin-left: 0px;"
                        onclick="ExportToProPricerGridWidget.ToggleHelp(this);">
                    </div>
                    <!-- This comment is needed for the jquery animation to work in IE8... -->
                    <div id="ExportToProPricerGrid-ScopeHelpDialog" class="help-dialog" style="width: 250px;">
                        <div class="help-dialog-close"></div>
                        <div class="help-dialog-text">
                            Scope indicates if the defined ProPricer Export Format is for use only in the current Workspace or for the all Workspaces in the genBOE System.
                        </div>
                    </div>
                </th>
                <th class="format">Template Name</th>
                <th class="copySystemLevel"><% if (isSystemAdmin) { %>System Copy<% } %></th>
                <th class="export">Export Files</th>
                <th class="send last-child">Send Directly</th>
            </tr>
        </thead>
        <tbody>
            <% foreach (var item in Model)
               { %>
               <tr pkid="<%: item.ID %>">
                    <td>
                        <% if (item.Scope == ProPricerScope.Workspace)
                           { %>
                            <input type="checkbox" class="checkbox" />
                        <% } %>
                        <input type="hidden" name="Name" value="<%: item.Name %>" />
                        <input type="hidden" name="Scope" value="<%: (int)item.Scope %>" />
                        <input type="hidden" name="OrderedTasks" value="[<%: String.Join(",", item.OrderedTasks.Select(t => "\"" + t + "\"")) %>]" />
                        <input type="hidden" name="OrderedResources" value="[<%: String.Join(",", item.OrderedResources.Select(t => "\"" + t + "\"")) %>]" />
                        <input type="hidden" name="UpdateDateLong" value="<%: item.UpdateDateLong %>" />
                    </td>
                    <td><%: item.Scope.ToString() %></td>
                    <% if (item.InvalidCustomFields.Count > 0)
                      { %>
                            <td class="disabled" title="This ProPricer export format uses the following custom fields that are not defined in the current Workspace: '<%:String.Join("', '", item.InvalidCustomFields)%>'.  This export format cannot be used until these custom fields are defined in this Workspace."><%: item.Name%></td>
                            <td></td>
                            <td></td>
                            <td class="send"></td>
                    <% }
                      else
                      { %>
                            <td name="Edit"><a><%: item.Name%></a></td>
                            <td><% if (isSystemAdmin && item.Scope == ProPricerScope.Workspace) { %><a data-ng-click="CopyToSystemLevel(<%: item.ID %>)" class='CopySystemLevel'>Copy To System Level</a><% } %></td>
                            <td name="Export" formatID="<%: item.ID %>" formatName="<%: item.Name%>" scope="<%: (int)item.Scope %>"><a>Export Files</a></td>
                            <td name="Send" class="send" formatID="<%: item.ID %>" formatName="<%: item.Name%>" scope="<%: (int)item.Scope %>"><div data-ng-show="showInstanceLoader" class="loader"></div><a data-ng-hide="showInstanceLoader">Send Directly</a></td>
                    <% } %>                
               </tr>
            <% } %>
        </tbody>
    </table>
    
    <div gen-dialog id="SendToProPricerDialog" data-title="Export To ProPricer" class="dialog form bootstrap modal-content" data-width="865" data-height="600" data-open="model.sendToProPricerModalOpen" data-resizable="true" data-on-open="dialogInitialize()" data-on-close="closeDialog()">
        <div id="propricer-model-content-area" class="dialog-text modal-body">
            <form class="permission-form">
                <div class="form-row">
                    <div class="form-label">Selected Export Format:</div>
                    <div class="form-element">{{model.SendToProPricerData.formatName}}</div>    
                </div>
                <div class="form-row">
                    <div class="form-label">Selected Proposal:</div>
                    <div class="form-element">{{model.SendToProPricerData.SelectedProposalName}}</div>                
                </div>
                <ul class="validation-box"> </ul>
                <gen-validation data-errors="apiErrors"></gen-validation>
                <div id="InstanceSelectRow"  data-ng-show="model.SendToProPricerData.step == 1 && model.SendToProPricerData.Instances.length > 1" class="form-row">
                    <div class="form-label">ProPricer Schema</div>
                    <div class="form-element">
                        <select id="InstanceSelect" class="instance-select" data-ng-model="model.SendToProPricerData.SelectedInstance" data-ng-change="selectedInstanceChanged()">
                            <option></option>
                            <option data-ng-repeat="instance in model.SendToProPricerData.Instances" data-ng-value="{{instance.Id}}">{{instance.FriendlyName}}</option>
                        </select>
                    </div>     
                </div>
            
                <div data-ng-show="model.SendToProPricerData.step == 1 && instanceSelected()" class="form-row" style="margin: 0 0 10px 0;">
                    <span class="ui-helper-hidden-accessible"><input type="text"/></span>
                    <div class="form-label">Proposal Search</div>
                    <input type="text" data-ng-model="model.SendToProPricerData.searchText" placeholder="Search Filter (at least 3 characters)" id="ProPricerSearchText" class="ignore-dirty" data-ng-change="searchProposals()">
                </div>

                <div id="ProposalSelectRow" data-ng-show="model.SendToProPricerData.step == 1 && instanceSelected()" class="form-row">
                    <div class="form-element" data-ng-hide="proPricerDataIsLoading">
                        <div>
                            <a data-ng-click="collapseAll()">Collapse All</a>&nbsp;&nbsp;
                            <a data-ng-show="filtering" title="This may take several seconds, please be patient.." data-ng-click="expandAll()">Expand All</a>
                        </div>
                        <div class="proposal-tree">
                            <ol class="angular-ui-tree-nodes">
                                <li class="angular-ui-tree-node" data-ng-repeat="node in model.SendToProPricerData.Proposals" data-ng-show="node.matched" data-ng-include="'/Resources/nodes_renderer.html'"></li>
                            </ol>
                        </div>
                    </div>
                    <div class="form-element" data-ng-show="proPricerDataIsLoading"><div class="loader"></div></div>
                </div>
            
                <div id="PreviewRow" data-ng-show="model.SendToProPricerData.step == 2" class="form-row row">
                    <div class="col-md-12">
                        <div class="row">
                            <div class="col-md-12">
                                <div class="row">
                                    <div class="col-md-10 small">
                                        <br />
                                        <p>
                                            <span>* Note: A handling duplicate selection is required for both Task and Resource Duplicates.</span>
                                        </p>
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-5">
                                        <div class="well sm-well radio-height">
                                            <h5>Handling Task duplicates: *</h5>
                                            <input type="radio" data-ng-disabled="model.SendToProPricerData.taskReplaceAll" id="taskDuplicates_1" data-ng-model="model.SendToProPricerData.taskDuplicates" value="file">&nbsp;<label for="taskDuplicates_1" data-ng-class="{'disabled':model.SendToProPricerData.taskReplaceAll}">Do not overwrite, make exception file</label><br />
                                            <input type="radio" data-ng-disabled="model.SendToProPricerData.taskReplaceAll" id="taskDuplicates_2" data-ng-model="model.SendToProPricerData.taskDuplicates" value="overwrite">&nbsp;<label for="taskDuplicates_2" data-ng-class="{'disabled':model.SendToProPricerData.taskReplaceAll}">Overwrite existing duplicates</label><br />
                                            <input type="checkbox" data-ng-model="model.SendToProPricerData.taskReplaceAll" id="taskDuplicates_3" value="true" ng-change="taskReplaceAllChanged()" />&nbsp;<label for="taskDuplicates_3">Replace all existing with imported</label>
                                        </div>
                                    </div> 
                                    <div class="col-md-5 col-md-offset-1">
                                    <div class="well sm-well radio-height">
                                        <h5>Handling Resource duplicates: *</h5>
                                        <input type="radio" data-ng-disabled="model.SendToProPricerData.resourceReplaceAll" id="resourceDuplicates_1" data-ng-model="model.SendToProPricerData.resourceDuplicates" value="file">&nbsp;<label for="resourceDuplicates_1" data-ng-class="{'disabled':model.SendToProPricerData.resourceReplaceAll}">Do not overwrite, make exception file</label><br />
                                        <input type="radio" data-ng-disabled="model.SendToProPricerData.resourceReplaceAll" id="resourceDuplicates_2" data-ng-model="model.SendToProPricerData.resourceDuplicates" value="overwrite">&nbsp;<label for="resourceDuplicates_2" data-ng-class="{'disabled':model.SendToProPricerData.resourceReplaceAll}">Overwrite existing duplicates</label><br />
                                        <input type="radio" data-ng-disabled="model.SendToProPricerData.resourceReplaceAll" id="resourceDuplicates_3" data-ng-model="model.SendToProPricerData.resourceDuplicates" value="add">&nbsp;<label for="resourceDuplicates_3" data-ng-class="{'disabled':model.SendToProPricerData.resourceReplaceAll}">Add import value to duplicate</label><br />
                                        <input type="checkbox" data-ng-model="model.SendToProPricerData.resourceReplaceAll" id="resourceDuplicates_4" value="true" ng-change="resourceReplaceAllChanged()" />&nbsp;<label for="resourceDuplicates_4">Replace all existing with imported</label>
                                    </div>             
                                </div>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-12" data-ng-show="showPreview">
                                <div class="loader" data-ng-show="previewIsLoading"></div>
                                <div data-ng-hide="previewIsLoading">
                                    <i>* genBOE header row will be excluded automatically</i>
                                    <div class="table-responsive">
                                        <h4>Task Data</h4>
                                        <table class="ppPreviewTable table table-bordered table-striped table-condensed">
                                            <thead>
                                                <tr>
                                                    <th data-ng-repeat="taskHeader in taskHeaders track by $index">{{taskHeader}}</th>
                                                </tr>
                                                <tr>
                                                    <th data-ng-repeat="formatTaskHeader in formatTaskHeaders track by $index">{{formatTaskHeader}}</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr data-ng-repeat="taskRow in previewTaskRows">
                                                    <td data-ng-repeat="task in taskRow track by $index">{{task}}</td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                    <div class="table-responsive">
                                        <h4>Resource Hours / Cost</h4>
                                        <table class="ppPreviewTable table table-bordered table-striped table-condensed">
                                            <thead style="font-weight:bold;">
                                                <tr>
                                                    <th data-ng-repeat="resourceHeader in resourceHeaders track by $index">{{resourceHeader}}</th>
                                                </tr>
                                                <tr>
                                                    <th data-ng-repeat="formatResourceHeader in formatResourceHeaders track by $index">{{formatResourceHeader}}</th>
                                                </tr>
                                            </thead>
                                            <tbody>
                                                <tr data-ng-repeat="resourceRow in previewResourceRows">
                                                    <td data-ng-repeat="resource in resourceRow track by $index">{{resource}}</td>
                                                </tr>
                                            </tbody>
                                        </table>
                                    </div>
                                </div>
                    </div>
                        </div>
                    </div>
                </div>
            
                <div id="ProPricerResultsRow" data-ng-show="model.SendToProPricerData.step == 3" class="form-row row">
                    <div data-ng-hide="showButtonLoader || !model.SendToProPricerData.success" id="ExportCsvFiles" formatID="{{model.SendToProPricerData.formatId}}" formatName="{{model.SendToProPricerData.formatName}}" scope="{{model.SendToProPricerData.scope}}">
                        <a>Download a copy of the ProPricer Export files</a>
                    </div>
                    <div class="col-md-12" data-ng-show="model.SendToProPricerData.sendMessage !== ''">
                        <div class="export-to-propricer-error">{{model.SendToProPricerData.sendMessage}}</div>
                    </div>
                    <div class="col-md-12" data-ng-hide="showButtonLoader || !model.SendToProPricerData.success">
                        <div class="well">
                            <h3>Task Results:</h3>
                            <div class="bg-danger" data-ng-hide="taskExportResults.ProcessExceptionFileError == undefined || taskExportResults.ProcessExceptionFileError == '' || taskExportResults.ProcessExceptionFileError == null"><div class="warning-row-icon"></div>{{taskExportResults.ProcessExceptionFileError}}</div>
                            <ul>
                                <li>Total Records in the Data Transfer - {{taskExportResults.TotalRecordsInImportFile}}</li>
                                <li>Total Records Successfully Transferred - {{taskExportResults.TotalRecordsSuccessfullyExported}}</li>
                                <li>Total Records in Exception File - {{taskExportResults.TotalRecordsInException}}</li>
                            </ul>
                            <div data-ng-show="showTaskResultsError">
                                <h4>Task Id - Task Error</h4>
                                <ul class="validation-box" style="display:inline-block">
                                    <li data-ng-repeat="(key, value) in taskExportResults.TaskIdErrorMessages">{{key}} - {{value}}</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-12">
                        <div class="well" data-ng-hide="showButtonLoader || !model.SendToProPricerData.success">
                            <h3>Resource Results:</h3>
                            <div class="bg-danger" data-ng-hide="resourceExportResults.ProcessExceptionFileError == undefined || resourceExportResults.ProcessExceptionFileError == '' || resourceExportResults.ProcessExceptionFileError == null"><div class="warning-row-icon"></div>{{resourceExportResults.ProcessExceptionFileError}}</div>
                            <ul>
                                <li>Total Records in the Data Transfer - {{resourceExportResults.TotalRecordsInImportFile}}</li>
                                <li>Total Records Successfully Transferred - {{resourceExportResults.TotalRecordsSuccessfullyExported}}</li>
                                <li>Total Records in Exception File - {{resourceExportResults.TotalRecordsInException}}</li>
                            </ul>
                            <div data-ng-show="showResourceResultsError">
                                <h4>Task Id - Resource Error</h4>
                                <ul class="validation-box" style="display:inline-block">
                                    <li data-ng-repeat="(key, value) in resourceExportResults.TaskIdErrorMessages">{{key}} - {{value}}</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </div>
        <div class="modal-footer dialog-footer">
            <div id="ProPricerButtonRow" class="row">
                <div class="col-md-5">
                    <button name="preview-button" class="ies ies-action" data-ng-show="model.SendToProPricerData.step == 2" data-ng-disabled="showPreview || sendButtonDisabled()" data-ng-class="{'disabled':showPreview}" data-ng-click="preview()" type="button">Preview</button>
                </div>
                <div class="col-md-6">
                    <button name="back-button" class="ies left-margin" data-ng-disabled="model.SendToProPricerData.showButtonLoader" data-ng-class="{'disabled':model.SendToProPricerData.showButtonLoader}" data-ng-hide="backButtonHidden()" data-ng-click="previousStep()" type="button">Back</button>
                    <div class="loader" data-ng-show="model.SendToProPricerData.showButtonLoader"></div>
                    <button name="next-button" class="ies-action" data-ng-disabled="nextButtonDisabled()" data-ng-class="{'disabled':nextButtonDisabled()}" data-ng-show="!model.SendToProPricerData.showButtonLoader && model.SendToProPricerData.step == 1" data-ng-click="goToStep2()" type="button">Next</button>
                    <button name="send-button" class="ies-action" data-ng-disabled="sendButtonDisabled()" data-ng-class="{'disabled':sendButtonDisabled()}" data-ng-show="!model.SendToProPricerData.showButtonLoader && model.SendToProPricerData.step == 2" data-ng-click="sendToPropricer()" type="button">Export to ProPricer</button>
                    <button name="cancel-button" class="ies left-margin" data-ng-disabled="model.SendToProPricerData.showButtonLoader" data-ng-class="{'disabled':model.SendToProPricerData.showButtonLoader}" data-ng-click="cancel()" type="button">{{model.SendToProPricerData.step == 3 ? 'Close' : 'Cancel'}}</button>
                </div>
            </div>
        </div>
    </div>
</div>        
