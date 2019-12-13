<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOE.BOEHeaderISGSModelView>" %>
<script type="text/javascript">
    // Get the read-only attribute passed in from the controller
    var BOEHeaderTitle_ReadOnly = <%= ViewData["READONLY"] %>;

    var BOEHeaderTitle = new Widget("BoeHeaderTitle", BOEHeaderTitle_ReadOnly);

    //bind any events that the objects need to observe to and member functions.
    BOEHeaderTitle.InitializeTitle = function() {
        BOEHeaderTitle.applyReadOnly();

        // because the parent widget may set things to readonly if the parent is readonly
        // we also need to manually set things back to updateable if our widget is not readonly
        if (!BOEHeaderTitle_ReadOnly) {
            $('#Title-element .replacedWidgetText').remove();
            $('#Title-element *').removeClass('display-none');
        } else {
            $('#BoeHeaderForm .buttons button[name=save-button]').addClass('display-none');
            $('#BoeHeaderForm .buttons button[name=cancel-button]').addClass('display-none');
        }

        $('#BoeHeaderForm .buttons').removeClass('display-none');
    };
</script>

<div class="form-row">
    <div class="form-label">BOE Title</div>
    <div class="form-element" id="title-element">
        <%: Html.TextBoxFor(model => model.Title, new { @maxlength = "100", onkeyup = "Helper.textAreaLimit(this, 100)", style="width:533px;" })%>
    </div>
</div>