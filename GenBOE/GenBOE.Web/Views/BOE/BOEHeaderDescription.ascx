<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOEHeaderDescriptionModelView>" %>

<%
    int rteFieldSize = ViewBag.RteFieldSize;    
%>

<script type="text/javascript">
    $(function() {
        // Get the read-only attribute passed in from the controller
        var BOEHeaderDescription_ReadOnly = <%= ViewData["READONLY"] %>;

        var BOEHeaderDescription = AfterDomLoadBoeHeaderDescription(BOEHeaderDescription_ReadOnly, <%:rteFieldSize%>);
    });
</script>

<div id="BoeHeaderDescription" class="form-row">
    <div class="form-label">Description *</div>
    <div class="form-element" id="description-element">
        <div class="wrapper">
            <%: Html.TextAreaFor(model => model.Description, new { @maxlength = Constants.MAX_RTE_LENGTH, onkeyup = "Helper.textAreaLimit(this, " + Constants.MAX_RTE_LENGTH + ")" })%>
        </div>
    </div>
            
</div>
