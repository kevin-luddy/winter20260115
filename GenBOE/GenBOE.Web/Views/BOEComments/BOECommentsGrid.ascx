<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.BOECommentsModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import Namespace="GenBOE.ActionLogic.ModelView.BOE" %>

<%
    int CurrentUserID = Html.GetViewDataValue<int>("CurrentUserID", 0);
    bool ApprovalsReadOnly = Html.GetViewDataValue<bool>("Approvals_ReadOnly", true);
    bool CommentsReadOnly = Html.GetViewDataValue<bool>("Comments_ReadOnly", true);
    bool ResponsesReadOnly = Html.GetViewDataValue<bool>("Responses_ReadOnly", true);
    int workspaceState = Html.GetViewDataValue<int>("WorkspaceState", (int)WorkspaceState.None);
    var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>

<script type="text/javascript">
    BOECommentsGrid = new Widget("BOECommentsDiv");

    BOECommentsGrid.ContainsOCI = <%:Html.GetJSValue(Html.GetViewDataValue<bool>("ContainsOCI", false))%>;
    BOECommentsGrid.CurrentUserApproverInfo = null;
    BOECommentsGrid.CurrentUserApproverInfo = <%= serializer.Serialize(Model.Approver) %>;
    BOECommentsGrid.ApprovalsReadOnly = null;
    BOECommentsGrid.ApprovalsReadOnly = <%:Html.GetJSValue(ApprovalsReadOnly)%>;
    BOECommentsGrid.CommentsReadOnly = null;
    BOECommentsGrid.CommentsReadOnly = <%:Html.GetJSValue(CommentsReadOnly)%>;
    BOECommentsGrid.ResponsesReadOnly = null;
    BOECommentsGrid.ResponsesReadOnly = <%:Html.GetJSValue(ResponsesReadOnly)%>;
    var workspace = '<%: SiteMasterUtilities.GetCurrentWorkspace() %>';
    var originalApproverInfo = <%= serializer.Serialize(Model.Approver) %>;
    var workspaceLocked = '<%:workspaceState%>' == '<%:(int)WorkspaceState.Locked%>';
    var reloadCommentGridEvent = '<%:WebConstants.EVENT_BOEDETAILS_RELOAD_BOE_COMMENT_GRID %>';
    var saveBOECommentsUrl = CreatePostURL(workspace,
                            '<%:WebConstants.CONTROLLER_BOE_COMMENTS%>',
                            '<%:WebConstants.ACTION_SAVE_BOE_COMMENTS%>',
                            'boe/<%: ViewData["BOEID"] %>');
    var reloadHistoryGridEvent = '<%:WebConstants.EVENT_BOEDETAILS_RELOAD_BOE_HISTORY_GRID %>';
    
    //console.log('BOECommentsGrid: ApprovalsReadOnly = ' + BOECommentsGrid.ApprovalsReadOnly + ', CommentsReadOnly = ' + BOECommentsGrid.CommentsReadOnly + ', ResponsesReadOnly = ' + BOECommentsGrid.ResponsesReadOnly);

    InitializeBOECommentsGridWidget(BOECommentsGrid, originalApproverInfo,workspaceLocked, reloadCommentGridEvent, saveBOECommentsUrl, 
        reloadHistoryGridEvent, workspace);        

    $(function () {
        AfterDomLoadBOECommentsGridWidget(BOECommentsGrid);
    });
</script>

<div id="BOECommentsDiv" class="boe-comments module addable inner-collapsible">
    <div class="module-header-data"></div>
    <div class="module-content-data">
    <div class="warning-box" style="display: block">
        <div class="warning-message" style="padding-right: 10px;">
            Note: All comments entered are permanently retained with the workspace, and may be treated as cost and pricing data. Comments and responses may be reviewed/audited by the government.
        </div>
    </div>
    <ul class="validation-box"></ul>
        <% if (!ApprovalsReadOnly)
       { %>
        <div class="approve-reject">
            Approver's Response:&nbsp;&nbsp;&nbsp;&nbsp;
            <input type="radio" name="approval" value="1" id="ResponseApprove" /> <label for="ResponseApprove">Approve</label>&nbsp;&nbsp;&nbsp;
            <input type="radio" name="approval" value="0" id="ResponseReject" /> <label for="ResponseReject">Reject</label>&nbsp;(Select to enter comments below)
        </div>
    <% } %>
        <table class="editable grid">
            <thead>
                <tr>
                    <th class="description">Comments</th>
                    <th class="description last-child">Author's Response</th>
                </tr>
            </thead>
            <tbody>
                <% foreach (var item in Model.Comments) { %>  
                <tr>
                    <td>
                    <% if (item.CommentType == BOECommentType.Comment)
                       { %>
                        <div class="comment-header">Last Updated <%:Html.DisplayFor(i => item.ReviewerCommentUpdateDT) %> by <%: item.ReviewerName%></div>
                        <% if (item.AuthorResponse.Length > 0 || item.ReviewerID != CurrentUserID || CommentsReadOnly || !ApprovalsReadOnly)
                           { %>
                                <div class="static-comment-row"><%: item.ReviewerComment%></div>
                        <% }
                           else
                           { %>
                                <div class="editable-comment" pkid="<%: item.ReviewerCommentID%>" updateDT="<%: item.ReviewerCommentUpdateDTLong %>" existing="true"><%: item.ReviewerComment%></div><div><textarea rows="6" onkeypress="return BOECommentsGrid.TextAreaMaxLength(this, 500);"><%: item.ReviewerComment%></textarea></div>
                        <% }
                       }
                       else
                       { %>
                        <div class="comment-header"><%: item.ReviewerComment %> <%:Html.DisplayFor(i => item.ReviewerCommentUpdateDT) %> by <%: item.ReviewerName%></div>
                    <% } %>
                    </td>
                    <td>
                        <% if (item.CommentType == BOECommentType.Approval)
                           { %>
                                <div class="static-response-row">N/A</div>
                        <% }
                           else if (item.AuthorResponse.Length > 0)
                           { %>
                           <div class="response-header">Last Updated <%:Html.DisplayFor(i => item.AuthorResponseUpdateDT) %> by <%: item.AuthorName%></div> 
                        <%     if (ResponsesReadOnly || !ApprovalsReadOnly)
                               { %>
                                    <div class="static-response-row"><%: item.AuthorResponse %></div>
                        <%     }
                               else
                               { %>
                                    <div class="editable-response" pkid="<%: item.AuthorResponseID%>" fkid="<%: item.ReviewerCommentID%>" updateDT="<%: item.AuthorResponseUpdateDTLong %>" existing="true"><%: item.AuthorResponse%></div><div><textarea rows="6" onkeypress="return BOECommentsGrid.TextAreaMaxLength(this, 500);"><%: item.AuthorResponse%></textarea></div>
                        <%     }
                           }
                           else if (!ResponsesReadOnly && ApprovalsReadOnly)
                           { %>
                                <div class="response-header-new display-none default-text">Add a Response</div>
                                <div class="editable-response" fkid="<%: item.ReviewerCommentID%>"></div><div><textarea rows="6" onkeypress="return BOECommentsGrid.TextAreaMaxLength(this, 500);"></textarea></div>
                        <% } %>
                    </td>
                </tr>
                <% } %>
                <tr class="display-none" id="BOECommentsGrid-NewCommentRow">
                    <td><div class="comment-header-add default-text">Add a comment</div></td>
                    <td></td>
                </tr>
                <tr id="BOECommentsGrid-AddedRowToClone">
                    <td>
                        <div class="comment-header-new">Unsaved Comment</div>
                        <div class="editable-comment"></div><div><textarea rows="6" onkeypress="return BOECommentsGrid.TextAreaMaxLength(this, 500);"></textarea></div>
                    </td>
                    <td></td>
                </tr>
            </tbody>
        </table>
        <br />
        <div id="OCINote" class="oci-note"></div> 
        <% if (!CommentsReadOnly || !ApprovalsReadOnly || !ResponsesReadOnly)
           { %>
        <div class="buttons">
            <button id="BOECommentsSave" class="ies-action disabled" name="save-button" type="button">Save</button>
            <div id="BOECommentsLoader" class="loader display-none"></div>
            <button id="BOECommentsCancel" class="ies" name="cancel-button" type="button">Cancel</button>
        </div>
        <% } %>
    </div>
</div>

