<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<GenBOE.Web.ModelView.FindReplaceResultsModelView>" %>

<script type="text/javascript">

    var FindReplaceResultsWidget = new Widget("FindReplaceResults", <%: ViewData["READONLY"] %>);

    //Select all results on the page
    FindReplaceResultsWidget.SelectAll = function(){
        if($('#replaceAll:checked').length > 0)
        {
                $('div[name=resultRow]').each(function(){
                    $(this).find('input[type=checkbox]').prop('checked', true);
                });
            }else{
                $('div[name=resultRow]').each(function(){
                    $(this).find('input[type=checkbox]').prop('checked', false);
                });
            }
    }

    //Checks for the text being too long and will display a message appropriately
    FindReplaceResultsWidget.checkForTooLong = function(){
        
        $('div[name=resultRow]').each(function(){
            if($(this).find('input[name=fieldTooLong]').val() == "True")
            {
                $(this).find('div[name=warningDiv]').removeClass("display-none");
                $(this).find('input[type=checkbox]').prop('checked', false).prop('disabled', true);
            }
        })
    }

    //Creates the view links to redirect to the BOE
    FindReplaceResultsWidget.createLinks = function(){

        $('div[name=resultRow]').each(function(){
           //Create href tag here
           var linkBOEID = $(this).children('input[name=BOEId]').val();
           var linkTaskID = $(this).children('input[name=TaskElementID]').val();
           var linkTaskType = $(this).children('input[name=TaskElementType]').val();
    
           var linkTaskURL = '';
           if (linkTaskType == 'BOE')
           {
                linkTaskURL = '#LMLabor/task/';
           }
           else if (linkTaskType == 'Material')
           {
                linkTaskURL ='#Material/material/';
            }
           else if (linkTaskType == 'ODC')
           {
                linkTaskURL = '#ODC/odc/';
           }
           else if (linkTaskType == 'Travel')
           {
                linkTaskURL = '#Travel/travel/';
            }
           else // default
           {
                linkTaskURL ='#LMLabor/task/';
            }

           var link = CreatePostURL('<%:SiteMasterUtilities.GetCurrentWorkspace()%>', '<%: WebConstants.CONTROLLER_BOE %>', '<%: WebConstants.ACTION_EDIT_BOE_INDEX %>', 'boe/' + linkBOEID + linkTaskURL + linkTaskID);
           $(this).find('a').attr('href', link);
        })
    }
    
    //Selects the check all    
    FindReplaceResultsWidget.CheckSelectAll = function(){
        
        $('div[name=resultRow]').each(function(){
            if($(this).find('input[name=fieldTooLong]').val() == "True")
            {
                $(this).find('div[name=warningDiv]').removeClass("display-none");
                $(this).find('input[type=checkbox]').prop('checked', false).prop('disabled', true);
            }
        })
    }
    
    //Saves the replaced values from the modelview data
    FindReplaceResultsWidget.saveReplaceChecked = function(){
        $("#FindReplaceResults-replace").addClass("display-none");
        $("#FindReplaceResults-loader").removeClass("display-none");

        var data = {};
        data.ReplacedTextModelViews = {}
        data.ReplacedTextModelViews.FindResults = [];


        $('div[name=resultRow]').each(function(){
            var toReplaceDTO = {};
            toReplaceDTO.BOEId = $(this).children('input[name=BOEId]').val();
            toReplaceDTO.TaskElementID = $(this).children('input[name=TaskElementID]').val();
            toReplaceDTO.WorkspaceID = $(this).children('input[name=WorkspaceID]').val();
            toReplaceDTO.FieldName = $(this).children('input[name=FieldName]').val();
            toReplaceDTO.TextAfterReplace = $(this).children('input[name=TextAfterReplace]').val();
            toReplaceDTO.FindReplaceTaskElementType = $(this).children('input[name=TaskElementType]').val();

            if($(this).find('input[type=checkbox]:checked').length > 0 || $('#replaceAll:checked').length > 0)
            {
                toReplaceDTO.toSave = true;
            }else
            {
                toReplaceDTO.toSave = false;
            }
            
            data.ReplacedTextModelViews.FindResults.push(toReplaceDTO);
        });

        $.ajax({
            type: 'POST',
            url: CreatePostURL('<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
                        '<%: WebConstants.CONTROLLER_FIND_REPLACE %>',
                        '<%: WebConstants.ACTION_SAVE_REPLACED_VALUES %>', ''),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify(data),
            success: FindReplaceResultsWidget.SaveSuccess,
            error: function () {
                $("#Replace-FindReplace").removeClass("display-none");
                $("#Loader-FindReplace").addClass("display-none");
            }
        });

    }
    //Closes the dialog on success
    FindReplaceResultsWidget.SaveSuccess = function()
    {
        $(document).trigger("REPLACE_SAVE_SUCCESS");
    }

    $(function () {

        pagingData = {};
        pagingData.data = {};
        pagingData.data.CurrentPage = <%: Model.CurrentPage %>;
        pagingData.data.StartArrayIndex = <%: Model.StartArrayIndex %>;
        pagingData.data.EndArrayIndex = <%: Model.EndArrayIndex %>;
        pagingData.data.NumPages = <%: Model.NumPages %>;
        pagingData.data.PagedIndexes = <%= Model.PagedResultsJSArray %>;
        pagingData.data.ResultsPerPage = <%: Model.ResultsPerPage %>;

        pagingData.div = $('[name=PageControls]');
        pagingData.action = function(data) {
            delete data.FindReplaceResults;
            $(document).trigger('PageFindResults', data);
        };
        pagingData.type = "PageNumber";

        FindReplaceResultsWidget.AddPaging(pagingData);
    
        $('#replaceAll').click(FindReplaceResultsWidget.SelectAll);
        $('#FindReplaceResults-replace').click(FindReplaceResultsWidget.saveReplaceChecked);

        $('input[type=checkbox]').click(FindReplaceResultsWidget.CheckSelectAll);
        
        $('#FindReplaceResults-cancel').click(function() {
            $(document).trigger("CLOSE_REPLACE_RESULTS");
        });

         FindReplaceResultsWidget.checkForTooLong();

         FindReplaceResultsWidget.createLinks();

    });

</script>

<div name="PageControls" style="margin-bottom: 20px;"></div>
<hr />

<div id="FindReplaceResults" class="findReplace-Results">
    <% if (Model.FindResults.Count > 0)
       { %>
        
        <div class="text-boe-select">Select occurrences to be replaced then click Replace.</div><br />
        <div ><input type="checkbox" id="replaceAll" />&nbsp;Select All</div>

        <div class="divider"></div>
        <% foreach (GenBOE.Web.ModelView.FindReplaceResult item in Model.FindResults)
           { %>
            <div name="resultRow">
                <input type="checkbox"/>&nbsp;
                <div class="wbs-clin inLineBlock" style="font-weight:bold">WBS: <%: item.WBSInfo %>&nbsp;&nbsp;&nbsp;&nbsp;CLIN: <%: item.ClinInfo%></div>
                <div class="inLineBlock" style="float:right"><a title="The word/phrase occurs <%: item.Occurences%> time(s)"><%: item.Occurences%> Occurrences</a>&nbsp;&nbsp;<a>View</a></div><br />
                <div class="inLineBlock" style="font-weight: bold; margin-left: 29px;">BOE Title: <%: item.BOETitle%></div>
                <div style="width:500px">
                    <span style="font-weight: bold;"><%: item.FieldName %>:</span>
                    <span name="textBeforeFind">&nbsp;<%:item.TextBeforeFind%></span>
                    <span name="foundText" style="background-color:Yellow"><%: item.FoundText%></span>
                    <span name="textAfterFind" ><%:item.TextAfterFind%></span>
                </div>
                <div class="tooLongWarning, display-none" name="warningDiv">Cannot replace. Resultant text would be too long.</div>
                <div class="divider"></div>
                <input type="hidden" name="BOEId" value="<%: item.BOEId%>" />
                <input type="hidden" name="FieldName" value="<%: item.FieldName%>"/>
                <input type="hidden" name="FoundText" value="<%: item.FoundText%>"/>
                <input type="hidden" name="TaskElementID" value="<%: item.TaskElementID%>"/>
                <input type="hidden" name="WorkspaceID" value="<%: item.WorkspaceID%>"/>
                <input type="hidden" name="TextAfterReplace" value="<%: item.TextAfterReplace%>"/>
                <input type="hidden" name="fieldTooLong" value="<%: item.tooLong %>"/>      
                <input type="hidden" name="TaskElementType" value="<%:item.FindReplaceTaskElementType %>" />
            </div>
        <% } %>

    <% } else { %>
        <div class="text-noresults">No results found.</div>
       
        <div class="divider"></div>

        <% } %>
    <div class="form-row">
        <div class="form-label"></div>
        <div class="form-element">
            <div class="buttons" style="width:300px;">
                <button id="FindReplaceResults-replace" class="ies-action" type="button">Replace</button>
                <div id="FindReplaceResults-loader" class="loader display-none"></div>
                <button id="FindReplaceResults-cancel" class="ies" name="cancel-button" type="button">Cancel</button>
            </div>
        </div>
    </div>
</div>

<div name="PageControls" style="margin-top: 10px;"></div>