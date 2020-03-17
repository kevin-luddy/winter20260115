<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<ICollection<string>>" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import namespace="Newtonsoft.Json" %>

<script type="text/javascript">
    var ManageOverdueTrainingWidget = new Widget('ManageOverdueTraining', false);

    $(function () {

        ManageOverdueTrainingWidget.registerForEvent('MANAGE_OVERDUE_TRAINING_LOADED', function () {
             return true;
        });

        SortableGrid('.OverdueTraining');
    });
    
</script>

<div class="section">
    <div class="title">Manage Overdue Training</div>
    <div class="data">
        <div data-ng-app="genboe" data-ng-controller="manageOverdueTrainingController">
            <form name="overdueTrainingForm" id="overdueTrainingForm">
                <div data-ng-cloak>
                    <div class="form-row css3pie-position-fix">
                        <gen-validation data-errors="errors"></gen-validation>                    
                    </div>
                    <div class="form-row">
                        <div class="form-label">
                            Training Course *
                        </div>
                        <div class="form-element">
                            <% foreach (string courseName in Model) { %>
                            <div>
                                <input id="Select-<%:courseName %>" data-ng-model="courseName" type="radio" value="<%:courseName %>" />&nbsp;&nbsp;
                                <label for="Select-<%:courseName %>"><%:courseName %></label>
                                <br />
                            </div>
                            <% } %>
                        </div>
                    </div>
                    <div class="form-row">
                        <input type="hidden" id="ImportDialog-DocumentDomain" name="documentDomain" />
                        <input type="file" size="60" id="ImportDialog-File" name="file"  onchange="angular.element(this).scope().fileUploadChange(this)" />
                        <button data-ng-click="import()" data-ng-disabled="disableImport || importWorking" type="button" class="ies-action">Import</button>
                        <button data-ng-click="exportCsv()" data-ng-disabled="disableImport || importWorking || !showImportResults" type="button" class="ies-action">Export Results</button>
                    </div>
                    <div class="form-row css3pie-position-fix last-form-row">
                        <div class="loader" data-ng-show="importWorking"></div>
                        <div class="OverdueTraining" data-ng-show="showImportResults">
                            <table id="OverdueTrainingTable" class="sortable grid readonly">
                                <thead>
                                    <tr>
                                        <th class="sort">Name</th>
                                        <th class="sort">NTID</th>
                                        <th class="sort">Last Completed</th>
                                        <th class="sort">Course Id</th>
                                    </tr>
                                </thead>
                                <tbody>
                                    <tr data-ng-repeat="user in data">
                                        <td>{{user.UserDisplayName}}</td>
                                        <td>{{user.NTID}}</td>
                                        <td>{{user.LastCompletedString}}</td>
                                        <td>{{user.CourseId}}</td>
                                    </tr>
                                </tbody>
                            </table>
                        </div>
                    </div>
                </div>
            </form>
        </div>   
    </div>
</div>
