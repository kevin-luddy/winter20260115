<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<GenBOE.ActionLogic.ModelView.GenBOEMetricsModelView>" %>
<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<div class="module">
    <div class="module-header-data">GenBOE Metrics -- All Users (<%:Model.usersTotal%>)</div>
    <div class="module-content-data" style="padding-bottom:20px;">
        <script type="text/javascript">
            $(function () {
                $('.collapsiblePanel').click(function () {
                    if ($(this).hasClass('expanded')) {
                        $(this).removeClass('expanded').addClass('collapsed');
                        $(this).parent().parent().parent().find(".list-group").slideUp(400);
                    }
                    else {
                        $(this).removeClass('collapsed').addClass('expanded');
                        $(this).parent().parent().parent().find(".list-group").slideDown(400);
                    }
                });
            });    
        </script>
        <div class="bootstrap container well">
            <div class="row">
                <div class="col-md-3">
                    <div class="panel-group">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <div class="collapsiblePanel expanded">Workspaces*</div> 
                                    <span class="badge" style="float:right"><%:@Model.UsageMetrics.WorkspacesAll.ToString("#,0") %></span>
                                </h4>
                            </div>
                            <div class="panel">
                                <ul class="panel-body list-group" style="padding:0;">
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.WorkspacesInitialization.ToString("#,0") %></span>
                                        Initialization
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.WorkspacesWorking.ToString("#,0") %></span>
                                        Working
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.WorkspacesLocked.ToString("#,0") %></span>
                                        Locked
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.WorkspacesComplete.ToString("#,0") %></span>
                                        Complete
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.WorkspacesClosed.ToString("#,0") %></span>
                                        Closed
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="col-md-3">
                    <div class="panel-group">
                        <div class="panel panel-default">
                            <div class="panel-heading">
                                <h4 class="panel-title">
                                    <div class="collapsiblePanel expanded">BOEs*</div> 
                                    <span class="badge" style="float:right"><%:@Model.UsageMetrics.BoesAll.ToString("#,0") %></span>
                                </h4>
                            </div>
                            <div class="panel">
                                <ul class="panel-body list-group" style="padding:0">
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.BoesUnassigned.ToString("#,0") %></span>
                                        Unassigned
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.BoesDraft.ToString("#,0") %></span>
                                        Draft
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.BoesAwaitingApproval.ToString("#,0") %></span>
                                        Awaiting Approval
                                    </li>
                                    <li class="list-group-item">
                                        <span class="badge"><%:@Model.UsageMetrics.BoesApproved.ToString("#,0") %></span>
                                        Approved
                                    </li>
                                </ul>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</div>
</asp:Content>