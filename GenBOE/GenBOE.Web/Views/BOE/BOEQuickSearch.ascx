<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.ActionLogic.ModelView.BOEQuickSearchModelView>" %>

<script type="text/javascript">
    BOEQuickSearchWidget = new Widget("QuickForm", <%: ViewData["READONLY"] %>);

    BOEQuickSearchWidget.ClearForm = function() {
        $('#Search-QuickSearch').removeClass('display-none').addClass('disabled');
        $('#Loader-QuickSearch').addClass('display-none');
        $('#QuickForm #SelectedCategory').val(0);
        $('#QuickForm #QuickSearchText').val('');
        BOEQuickSearchWidget.clearValidationBox();
    }

    $(function () {
        BOEQuickSearchWidget.afterDOMLoad();
        BOEQuickSearchWidget.registerForEvent("CLEAR_FORM", BOEQuickSearchWidget.ClearForm);

        // Click events get bound on page load and when .dialog is called on the parent, call unbind to prevent them from happening twice.
        $('#Cancel-QuickSearch').unbind('click');
        $('#Cancel-QuickSearch').click(function () { $(document).trigger("CANCEL_SEARCH"); });

        $("#QuickForm").unbind('keypress');
        $("#QuickForm").keypress(function (e) {
            if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)){
                    $("#Search-QuickSearch").click();
        			return false;
		    } else {
			        return true;
		    }
        });

        $('#Search-QuickSearch').unbind('click');
        $('#Search-QuickSearch').click(function (e) {
            var button = this;

            if( $(this).hasClass('disabled') ) {
                return;
            }

            if (true) {
                BOESearchWidget.QuickData = {};
                BOESearchWidget.QuickData.IsQuickSearch = true;
                BOESearchWidget.QuickData.SelectedCategory = $('#QuickForm #SelectedCategory').val();
                BOESearchWidget.QuickData.QuickSearchText = $('#QuickForm #QuickSearchText').val();
                BOESearchWidget.QuickData.IsCopyFromBoeContext = $('#IsCopyFromBoeContext').val();
                BOESearchWidget.data = BOESearchWidget.QuickData;
                BOESearchWidget.type = "QuickSearch";

                $('#Search-QuickSearch').addClass('display-none');
                $('#Loader-QuickSearch').removeClass('display-none');
                BOEQuickSearchWidget.clearValidationBox();

                var dataToSend = JSON.stringify(BOESearchWidget.data);

                BOEQuickSearchWidget.ajaxUpdatedRequest({
                    type: 'POST',
                    url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_BOE %>',
                        '<%: WebConstants.ACTION_QUICK_SEARCH_FOR_BOES %>', 'boe/' + BOESearchWidget.BoeID),
                    contentType: 'application/json; charset=utf-8',
                    dataType: 'html',
                    data: dataToSend,
                    success: function (response) {
                        $(document).trigger("DISPLAY_RESULTS", response);
                        BOESearchWidget.clearValidationBox();
                    },
                    error: function () {
                        $('#Search-QuickSearch').removeClass('display-none');
                        $('#Loader-QuickSearch').addClass('display-none');
                    }
                }, $(button));
            }
        });
    });

</script>

<div id="QuickSearchDiv">

<% Html.BeginForm("", "", FormMethod.Post, new { name = "QuickForm", id = "QuickForm" }); %>
    <div class="form-header">Quick Search</div>
    <ul class="validation-box"></ul>
    <div class="form-row" id="QuickSearchCategoryRow">
        <div class="form-label">Search For</div>
        <div class="form-element">
            <%: Html.DropDownList("SelectedCategory", Model.Categories) %>
            <%: Html.TextBox("QuickSearchText", null, new { maxlength = "200" })%>
        </div>
    </div>
    <div class="form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="buttons">
                <button id="Search-QuickSearch" class="ies-action disabled" name="save-button" type="button">Search</button>
                <div id="Loader-QuickSearch" class="loader display-none"></div>
                <button id="Cancel-QuickSearch" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        </div>
    </div>
<% Html.EndForm(); %>
</div>