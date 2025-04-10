<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	genBOE - <%: ((GenBOEMasterModelView)Model).ProposalName %>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <%: Styles.Render("~/Content/genCss") %>
    <%: Scripts.Render("~/bundles/manageRTETemplates") %>
    <%  bool isWorkingState = (((GenBOEMasterModelView)Model).WorkspaceState == "Working" || ((GenBOEMasterModelView)Model).WorkspaceState == "Initialization" || ((GenBOEMasterModelView)Model).WorkspaceState == "Locked");
        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
		bool containsOCI = ((GenBOEMasterModelView)Model).HeaderFooter.Contains("Organizational Conflict of Interest");
    %>
    <script type="text/javascript">
        var RteWidget = new Widget('RteWidget', false);
        $(function () {
            // pulls out the error message from the query..
            var errorMessage = sessionStorage.errorMessage;
            if (errorMessage) {
                $('#errorMessage').addClass('validation-box').css('display', 'block').html(errorMessage);
            }
            sessionStorage.errorMessage = '';

            $(".main").addClass("workspace");
        });

    </script>

    <script type="text/javascript">
        app.value('RTETemplateModel', {
            workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
            controller: '<%:WebConstants.CONTROLLER_RTE_TEMPLATES %>',
            action: '<%:WebConstants.ACTION_GET_RTE_TEMPLATES %>',
            isWorkingState: '<%:isWorkingState%>'.isTrue(),
            saveAction: '<%:WebConstants.ACTION_SAVE_RTE_TEMPLATES %>',
            searchAction: '<%:WebConstants.ACTION_SEARCH_RTE_TEMPLATES %>',
            copyAction: '<%:WebConstants.ACTION_COPY_RTE_TEMPLATE %>',
            workspaceId: '<%:ViewData["WorkspaceId"]%>',
			templateSources: <%= serializer.Serialize(ViewData["TemplateSources"])%>,
			containsOCI: '<%: containsOCI%>'.isTrue()
        });
    </script>

    <div id="workspace-home" data-ng-controller="RTETemplatesController" data-ng-cloak="">
        <div class="module workspace-home" id="boesmodule">
            <div class="module-header-data">Custom Rich Text Editor Templates</div>
            <div class="module-content-data">
                <div class="form-row css3pie-position-fix">
                    <gen-validation data-errors="errors"></gen-validation>
                    <div class="buttons inline css3pie-position-fix" style="line-height: 28px;">
                        <button class="ies-action" data-ng-disabled="disableDelete()" data-ng-click="delete()" data-ng-show="isWorkingState" type="button">Delete</button>
                        <button class="ies-action" id="Add-RTETemplate" data-ng-disabled="isLoading" data-ng-show="isWorkingState" data-ng-click="addRTETemplate()" type="button">+ Add</button>
                        <button class="ies" data-ng-if="isWorkingState" id="Search-RTETemplate" data-ng-disabled="isLoading" data-ng-show="isWorkingState" data-ng-click="onSearchOpen()" type="button">Copy Templates</button>
                    </div>
                </div>
                <div class="form-row">
                    <div class="manage-rte-templates-grid container">
                        <table id="RteTemplateGrid" class="grid readonly">
                            <thead>
                                <tr>
                                    <th class="delete-checkbox"><input data-ng-if="isWorkingState" data-ng-disabled="isLoading" data-ng-model="deleteAll.deleteAll" type="checkbox" data-ng-change="toggleDeleteAll()" /></th>
                                    <th class="bootstrap">Template Name</th>
                                    <th class="bootstrap">Created By</th>
                                    <th class="bootstrap">Date of Creation</th>
                                    <th class="bootstrap">Last Updated</th>
                                    <th class="bootstrap">In Use</th>
                                    <th class="bootstrap">Assign Template</th>
                                    <th class="bootstrap">Currently Assigned</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr data-ng-show="isLoading"><td colspan="6"><div class="loader"></div></td></tr>
                                <tr data-ng-show="!isLoading && (data.length === 0)"><td colspan="6"><div class="empty-grid-text">There are no Custom Rich Text Editor Templates for the Workspace.</div></td></tr>
                                <tr pkid="{{::template.Id}}" data-ng-repeat="template in data">
                                    <td class="text delete-checkbox">
                                        <div>
                                            <input class="delete-chck" type="checkbox" data-ng-if="isWorkingState" data-ng-model="template.Deleted" data-ng-click="$event.stopPropagation()" />
                                        </div>
                                    </td>
                                    <td><a title="{{template.Description}}" data-ng-click="editTemplate(template)">{{template.Description}}</a></td>
                                    <td>{{template.Author}}</td>
                                    <td>{{template.CreationDate}}</td>
                                    <td>{{template.LastUpdatedDate}}</td>
                                    <td>{{template.InUse | yesNo}}</td>
                                    <td><a title="Assign Template" data-ng-click="assignTemplate(template)" ><i class="fa fa-list-ul" style="font-size:16px;color:black"></i></a></td>
                                    <td title="{{getCurrentlyAssignedText(template)}}">{{getCurrentlyAssignedText(template)}}</td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>

            <div gen-dialog  id="SearchTemplateDialog" data-width="750" data-height="600" data-title="Copy Templates" data-open="search.open" data-on-close="onSearchClose()">
                <gen-validation data-errors="search.errors"></gen-validation>
                <div class="form-row">
                        <div class="form-label">&nbsp;</div>
                </div>
                <div class="form-header">Search</div>
                <form data-ng-submit="onSearchTemplate()">
                    <div class="form-row">
                        <div class="form-label">(Workspace Name, Template Creator, Template Name)</div>
                        <div class="form-element">
                            <input type="text" data-ng-model="search.searchText" maxlength="50" />
                        </div>
                    </div>
                    <div class="form-row">
                        <div class="form-label"></div>
                        <div class="form-element">
                            <div class="buttons">
                                <button data-ng-disabled="searchButtonDisabled()" data-ng-hide="search.isLoading" data-ng-click="onSearchTemplate()" class="ies-action" name="save-button" type="submit">Search</button>
                                <div data-ng-show="search.isLoading" class="loader"></div>
                                <button data-ng-click="onSearchClose()" class="ies" name="cancel-button" type="button">Cancel</button>
                            </div>
                        </div>
                    </div>
                </form>
                <div class="divider"></div>
                <div class="form-row" id="newTemplateNameDiv" data-ng-if="search.showResults">
                    <gen-validation data-errors="newTemplateErrors"></gen-validation>
                    <div class="form-label">New Template Name: *</div>
                    <div class="form-element">
                        <input type="text" id="newTemplateName" placeholder="New Template Name" maxlength="200" />
                    </div>
                </div>
                <div class="divider" data-ng-if="search.showResults"></div>
                <div class="form-row" data-ng-if="search.showResults">
                    <div class="form-element full-width">
                        <table class="search-template-table full-width">
                            <thead>
                                <tr>
                                    <th>Workspace</th>
                                    <th>Created By</th>
                                    <th>Template Name</th>
                                    <th>Copy</th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr data-ng-show="search.templates.length === 0"><td colspan="4"><div class="empty-grid-text">There are no Custom Rich Text Editor Templates matches.</div></td></tr>
                                <tr data-ng-repeat="template in search.templates | limitTo:search.pageSize:search.currentPage*search.pageSize">
                                    <td>{{template.WorkspaceName}}</td>
                                    <td>{{template.Author}}</td>
                                    <td>{{template.Description | limitTo:50}}</td>
                                    <td>
                                        <a title="Copy Template" data-ng-click="copyTemplate(template)" ><i class="fa fa-copy" style="font-size:16px;color:black"></i></a>
                                    </td>
                                </tr>
                            </tbody>
                            <tfoot data-ng-if="search.templates.length !== 0">
                                <tr>
                                    <td colspan="4">
                                        <div id="Home-SearchRow2" class="search-box full-width" data-ng-hide="isLoading">          
                                            <div class="paging-control" genpaging data-num-pages="{{ numberOfSearchPages() }}" data-current-page="search.currentPage"></div>
                                        </div>
                                    </td>
                                </tr>
                            </tfoot>
                        </table>
                    </div>
                </div>
            </div>

            <div gen-dialog  id="TemplateDialog" data-width="750" data-height="600" data-title="Manage Template" data-open="edit.open" data-on-close="onEditClose()">
                <gen-validation data-errors="edit.errors"></gen-validation>
                <div class="form-row">
                        <div class="form-label">&nbsp;</div>
                </div>
                <div class="form-row">
                    <div class="form-label">Template Name 
                        <div class="help-icon" style="margin-top:3px; margin-left:0;" onclick="RteWidget.ToggleHelp(this, 'right', false);"></div>
                        <!-- This comment is needed for the jquery animation to work in IE8... -->
                        <div class="help-dialog" style="width: 130px;">
                            <div class="help-dialog-close"></div>
                            <div class="help-dialog-text">Enter in a Template Name up to 200 characters.</div>
                        </div>
                    </div>
                    <div class="form-element">
                        <input type="text" data-ng-if="isWorkingState" maxlength="200" rows="3" class="template-description" data-ng-model="edit.template.Description" data-ng-blur="onEditBlur()"></input>
                        <span data-ng-if="!isWorkingState">{{edit.template.Description}}</span>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-element">
                        <button class="ies-action" id="Add-Question" data-ng-disabled="isLoading" data-ng-show="isWorkingState" data-ng-click="addQuestion()" type="button">+ Add Prompt</button>
                    </div>
                </div>
                <div class="form-row">
                    <div class="form-element question-element">
                        <table class="question-table">
                            <thead>
                                <tr>
                                    <th class="question">Prompt
                                        <div class="help-icon" style="margin-top:3px; margin-left:0;" onclick="RteWidget.ToggleHelp(this, 'right', false);"></div>
                                        <!-- This comment is needed for the jquery animation to work in IE8... -->
                                        <div class="help-dialog" style="width: 130px;">
                                            <div class="help-dialog-close"></div>
                                            <div class="help-dialog-text">Enter in text up to 500 characters.</div>
                                        </div>
                                    </th>
                                    <th>Required</th>
                                    <th data-ng-if="isWorkingState">Order</th>
                                    <th data-ng-if="isWorkingState">Delete</th>
                                </tr>
                            </thead>
                            <tbody data-ng-if="!isWorkingState">
                                <tr data-ng-repeat="question in edit.template.Questions">
                                    <td class="question">
                                        <span class="question">{{question.Text}}</span>
                                    </td>
                                    <td>
                                        <span data-ng-if="!isWorkingState">{{question.Required | yesNo}}</span>
                                    </td>
                                </tr>
                            </tbody>
                            <tbody data-ng-if="isWorkingState" ui-sortable="sortableOptions" data-ng-model="edit.template.Questions">
                                <tr data-ng-repeat="question in edit.template.Questions">
                                    <td>
                                        <textarea rows="3" data-ng-change="setEditDirty()" class="question" maxlength="500" data-ng-model="question.Text"></textarea>
                                    </td>
                                    <td>
                                        <input type="checkbox" data-ng-change="setEditDirty()" data-ng-model="question.Required" />
                                    </td>
                                    <td>
                                        <a title="Drag and Drop to change order">
                                            <i class="fa fa-sort" style="font-size:16px;color:black;"></i>
                                            <i class="fa fa-bars" style="font-size:16px;color:black;"></i>
                                        </a>
                                    </td>
                                    <td>
                                        <div data-ng-click="deleteQuestion($index)" class="delete DeleteButton" />
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="form-row last-form-row">
                    <div class="form-element">
                        <div id="OCINote" class="oci-note">
                            <span data-ng-show="containsOCI"><b>Note:</b> <%: SiteMasterUtilities.GetBannerText() %></span>
                            <span data-ng-hide="containsOCI"><b>Note:</b> <%: SiteMasterUtilities.GetBannerText(true) %></span>
                        </div>
                        <div class="button-container">
                            <div class="buttons">
                                <button data-ng-if="isWorkingState" data-ng-click="onEditSave()" data-ng-disabled="!edit.isDirty" name="save-button" type="button" class="ies-action stateful_button">Save</button>
                                <button class="ies" type="button" name="cancel-button" data-ng-click="onEditClose()">Cancel</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div gen-dialog  id="AssignRTETemplateDialog" class="dialog form" data-width="350" data-height="280" data-title="Assign Custom RTE Template" data-open="assign.open" data-on-close="onAssignClose()">
                <gen-validation data-errors="assign.errors"></gen-validation>
                <div class="form-row">
                    <div class="dialog-text">Note: When assigning a Template, data currently in the text field being assigned will be added to the first Prompt of the Rich Text Editor Template. When unassigning a Template, the Prompts and associated text will be moved to the respective text field (listed below).</div>
                </div>
                <div class="form-row" data-ng-repeat="source in assign.sources">
                    <div class="form-label">{{source.Description}}</div>
                    <div class="form-element">
                        <input data-ng-disabled="isLoading || !isWorkingState" data-ng-model="source.checked" type="checkbox" data-ng-change="setAssignDirty()" />
                    </div>
                </div>
                <div class="form-row last-form-row">
                    <div class="form-element">
                        <div class="button-container">
                            <div class="buttons">
                                <button data-ng-if="isWorkingState" data-ng-click="onAssignSave()" data-ng-disabled="!assign.isDirty" name="save-button" type="button" class="ies-action stateful_button">Save</button>
                                <button class="ies" type="button" name="cancel-button" data-ng-click="onAssignClose()">Cancel</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div gen-dialog id="DeleteRTETemplatePromptDialog" class="dialog form" data-width="500" data-open="deletePrompt.open" data-on-close="onDeleteClose()" data-title="Handle Deleted Template Prompts">
                <gen-validation></gen-validation>
                <div class="form-row">
                    <div class="dialog-text">This Template is in use. The following Prompts are being deleted. Please select how to handle the text associated with these Prompts throughout the workspace. This cannot be undone.</div>
                    <div class="dialog-text">
                        <ul>
                            <li data-ng-repeat="prompt in edit.deletedQuestions">{{prompt.Text}}</li>
                        </ul>
                    </div>
                </div>
                <div class="form-row">
                    <html xmlns="http://www.w3.org/1999/xhtml">
                    <head><title></title></head>
                    <div class="dialog-text">
                        <span style="display: block; padding: 5px;">
                            <input type="radio" id="DeletePrompt-DeleteData" name="HandleData" data-ng-click="onDeleteSelect()" />
                            <label for="DeletePrompt-DeleteData">Delete the data</label>
                        </span>
                        <span style="display: block; padding: 5px;">
                            <input type="radio" id="DeletePrompt-MoveData" name="HandleData" data-ng-click="onMoveDataSelect()" />
                            <label for="DeletePrompt-MoveData">Move the data to the selected Prompt</label>
                        </span>
                        <div style="margin-left: 20px" data-ng-if="deletePrompt.moveData">
                            <span data-ng-repeat="question in deletePrompt.notDeletedQuestions" style="display: block; padding: 5px;">
                                <input type="radio" id="DeletePrompt-Prompt{{question.Id}}" pkid="{{question.Id}}" name="MoveTo" data-ng-click="onMoveToSelect(question.Id)" />
                                <label for="DeletePrompt-Prompt{{question.Id}}">{{question.Text}}</label>
                            </span>
                        </div>
                    </div>
                    </html>
                </div>
                <div class="form-row last-form-row">
                    <div class="form-element">
                        <div class="button-container">
                            <div class="buttons">
                                <button data-ng-click="onDeleteSave()" data-ng-disabled="!deletePrompt.isDirty" name="save-button" type="button" class="ies-action stateful_button">Save</button>
                                <button class="ies" type="button" name="cancel-button" data-ng-click="onDeleteClose()">Cancel</button>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        angular.element(document).ready(function () {
            angular.bootstrap(document, ['genboe']);
        });
    </script>
</asp:Content>