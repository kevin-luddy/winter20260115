<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOEHeaderDescriptionModelView>" %>

<%
    int rteFieldSize = ViewBag.RteFieldSize;
    bool showCustomQuestions = Model.RteTemplateAnswers.Any();
    int numberQuestions = showCustomQuestions ? Model.RteTemplateAnswers.Count : 1;
%>

<script type="text/javascript">
    $(function() {
        // Get the read-only attribute passed in from the controller
        var BOEHeaderDescription_ReadOnly = <%= ViewData["READONLY"] %>;

        var BOEHeaderDescription = AfterDomLoadBoeHeaderDescription(BOEHeaderDescription_ReadOnly, <%:rteFieldSize%>, '<%:showCustomQuestions.ToString()%>'.isTrue(), <%:numberQuestions%>);
    });
</script>

<div id="BoeHeaderDescription" class="form-row">
    <div class="form-label">BOE Description *</div>
    <div class="form-element" id="description-element">
        <% Html.RenderPartial(WebConstants.VIEW_RTE_TEMPLATE, new GenBOE.Web.ModelView.RteTemplateModelView(Model.RteTemplateAnswers, "Description", Model.Description));  %>
    </div>
            
</div>
