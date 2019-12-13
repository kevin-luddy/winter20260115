<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Dtos.WorkspaceActivityReportModelView>" %>

<script type="text/javascript">

    var WorkspaceActivityReport = new GridWidget("WorkspaceActivityReport");

    $(function () {

        createModule($('#WorkspaceActivityReport'));
        refreshModule($('#WorkspaceActivityReport'));
    });

</script>


<div id="WorkspaceActivityReport" class="module">
    <div class="module-header-data">Workspace Activity</div>
    <div class="module-content-data">
        <div>
            <%:Model.DaysLeftUntilProposalSubmittalDate %><br /><br />
        </div>

        <div>
            <div class="workspace-activityreport-left float-left">
                <table id="WorkspaceActivityReportLeftGrid" class="workspace-activityreport-left-table grid readonly full">
                    <thead>
                        <tr>
                            <th class="workspace-activityreport-metric">Metric</th>
                            <th class="workspace-activityreport-value last-child">Value</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Days in Initialization
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.DaysInInitialization%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Days in Working
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.DaysInWorking%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Days in Locked
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.DaysInLocked%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Days from Initialization to Complete
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.DaysFromInitializationToComplete%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Days from Working to Complete
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.DaysFromWorkingToComplete%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Days to Closed
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.DaysToClosed%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Times in Initialization
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfTimesInInitialization%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Times in Working
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfTimesInWorking%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Times in Locked
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfTimesInLocked%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Times in Complete
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfTimesInComplete%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Times in Closed
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfTimesInClosed%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Times Exported to ProPricer Format
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfTimesExportedToProPricer%>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>


            <div class="workspace-activityreport-right float-right">
                <table id="WorkspaceActivityReportRightGrid" class="workspace-activityreport-right-table grid readonly full">
                    <thead>
                        <tr>
                            <th class="workspace-activityreport-metric">Metric</th>
                            <th class="workspace-activityreport-value last-child">Value</th>                                  
                        </tr>
                    </thead>
                    <tbody>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # BOEs
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfBOEs%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                &nbsp;&nbsp;&nbsp;Unassigned
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfBOEsUnassigned%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                &nbsp;&nbsp;&nbsp;Draft
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfBOEsDraft%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                &nbsp;&nbsp;&nbsp;Awaiting Approval
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfBOEsAwaitingApproval%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                &nbsp;&nbsp;&nbsp;Approved
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfBOEsApproved%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Administrators
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfAdministrators%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Authors
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfAuthors%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Approvers
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfApprovers%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                # Reviewers
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.NumberOfReviewers%>
                            </td>
                        </tr>
                        <tr>
                            <td class="workspace-activityreport-metric">
                                Avg # of BOEs/Author
                            </td>
                            <td class="workspace-activityreport-value">
                                <%: Model.AverageNumberOfBOEsPerAuthor%>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>

    </div>
</div>