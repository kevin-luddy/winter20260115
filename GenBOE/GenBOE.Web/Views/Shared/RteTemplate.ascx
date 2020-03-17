<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.RteTemplateModelView>" %>
<%
    int rteFieldSize = ViewBag.RteFieldSize;
    bool showCustomQuestions = Model.Templates.Any();
    int numberQuestions = showCustomQuestions ? Model.Templates.Count : 1;
    string defaultTemplateName = Model.TemplateName + "_0";
%>

<div class="wrapper">
    <% if (showCustomQuestions) { 
        int index = 0;
        foreach(GenBOE.Dtos.RTECustomTemplateQuestionAnswerModelView answer in Model.Templates.OrderBy(t => t.SortOrder)) { %>
        <span class="form-label template-label"><%: answer.QuestionText %><%: answer.Required ? "*" : string.Empty %></span>
        <textarea cols="20" id="<%:Model.TemplateName %>_<%:index.ToString() %>" isrequired="<%: answer.Required.ToString() %>" taskId="<%: answer.TaskId.HasValue ? answer.TaskId.ToString() : "" %>" updateDateLong="<%: answer.UpdateDateLong.ToString()%>" name="<%:Model.TemplateName %>_<%:index.ToString()%>" boeId="<%:answer.BoeId.ToString() %>" sourceId="<%:answer.SourceId.ToString() %>" questionId="<%:answer.QuestionId.ToString() %>" pkid="<%:answer.Id.ToString() %>" maxlength="<%:Constants.MAX_RTE_LENGTH%>" onkeyup="Helper.textAreaLimit(this, <%:Constants.MAX_RTE_LENGTH%>)"><%:answer.AnswerText %></textarea>
    <% index++; 
        } } else { %>
        <textarea name="<%:defaultTemplateName %>" id="<%:defaultTemplateName %>" maxlength="<%:Constants.MAX_RTE_LENGTH%>" onkeyup = "Helper.textAreaLimit(this, <%:Constants.MAX_RTE_LENGTH%>)"><%:Model.NonTemplateValue %></textarea>
    <% } %>
</div>