<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Master/Site.Master" Inherits="System.Web.Mvc.ViewPage<GenBOE.Web.ModelView.DateShiftIndexModelView>" %>
<%@ Import Namespace="System.Web.Script.Serialization" %>
<%@ Import namespace="System.Web.Optimization" %>
<%@ Import Namespace="IES.Common.classes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
	 Date Shift
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
<%: Styles.Render("~/Content/genCss") %>
<%: Scripts.Render("~/bundles/dateshift") %>
<style>
.ui-datepicker-calendar {
	display: none;
}
</style>
<% 
	var serializer = new JavaScriptSerializer { MaxJsonLength = Int32.MaxValue };
%>
<script type="text/javascript">
	app.value('DateShiftModel', {
		workspace: '<%: SiteMasterUtilities.GetCurrentWorkspace() %>',
		controller: '<%:WebConstants.CONTROLLER_DATESHIFT%>',
		dateShiftAction: '<%:WebConstants.ACTION_APPLY_DATE_SHIFT %>',
		id: '<%: Model.Id %>',
		level: '<%: Model.DateShiftLevel %>',
		startDate: '<%: Model.StartDate %>',
		endDate: '<%: Model.EndDate %>',
		containsDiscrete: '<%: Model.ContainsDiscrete.ToString().ToLower() %>'.isTrue(),
        discreteOutsidePop: '<%: Model.DiscreteOutsidePop.ToString().ToLower() %>'.isTrue(),
		spreadCurves: <%=serializer.Serialize(Model.SpreadCurves)%>,
        returnUrl: '<%: Model.ReturnUrl%>',
        isRMS: '<%: SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST%>'.isTrue(),
        hoursLabel: '<%: ViewData["HoursLabel"]%>',
	});
	var DateShiftWidget;
	var HelpImagesWidget;
	
	$(function () {
		
		// Create widget
		var widgetConfig = {};
		widgetConfig.ContextID = "DateShift";
		widgetConfig.isReadOnly = false; // used so that way we can manually override readonly behaviors
		widgetConfig.IsModule = false;

		DateShiftWidget = new GenWidget(widgetConfig);

		widgetConfig = {};
		widgetConfig.ContextID = "HelpImages";
		widgetConfig.isReadOnly = false; // used so that way we can manually override readonly behaviors
        widgetConfig.IsModule = false;
        widgetConfig.ApplyHelpPopouts = false;

        HelpImagesWidget = new GenWidget(widgetConfig); //Widget("HelpImages", false);

        widgetConfig = {};
        widgetConfig.ContextID = "DateShiftOutput";
        widgetConfig.isReadOnly = false; // used so that way we can manually override readonly behaviors
        widgetConfig.IsModule = false;
        widgetConfig.ApplyHelpPopouts = false;

        DateShiftOutputWidget = new GenWidget(widgetConfig);         

		DateShiftWidget.openWindow = function (currentWorkspace, boeLaborController) {
			var url = CreatePostURL(currentWorkspace, boeLaborController, '<%:WebConstants.ACTION_DISPLAY_LABOR_CURVES%>');
			var name = 'Curves';
			var width = 840;
			var height = 560;

			var newwindow = window.open(url, name, 'scrollbars=1,toolbar=no,resizeable=no,status=no,width=' + width + ',height=' + height);
		}; 
	});
</script>
<div data-ng-controller="DateShiftController" data-ng-cloak style="word-spacing:normal;">
    <script type="text/ng-template" id="curveTemplate.html">
        <div class="modal-header">
        <div class="modal-close-box" data-ng-click="$close()"><i class="glyphicon glyphicon-remove"></i></div>    
        <h3 class="modal-title" id="modal-title">Spread Curves</h3>
        </div>
        <div class="modal-body" id="modal-body">
            <div class="curve-page-holder">
                <div class="curve-table-holder">
                <table style="border:none">
                    <tr>
            <%
            for (int x = 1; x <= 53; x++)
            {
                int curveId = x + 1;
                if (curveId > 51) {
                    curveId += 2;
                }
                        %>
                        <td>Curve <%:x %><br /><img class="pointer" data-ng-click="selectCurve('<%:curveId %>');" src="<%= this.ResolveClientUrl("~/Resources/css/images/curves/Curve")%><%:x%>.jpg" /></td>
                        <%
                         if (x % 5 == 0)
                        {
                            %></tr><tr><%
                        }
                    } 
                 %>
                    </tr>
                 </table>
                </div>
            </div>
        </div>
    </script>
    <div id="DateShift" class="module">
		<div class="module-header-data">Date Adjust - <%: Model.Title %></div>
		<div class="module-content-data">
			<div class="form-row css3pie-position-fix" data-ng-hide="errors.length === 0">
				<gen-validation data-errors="errors"></gen-validation>
			</div>
			<div data-ng-switch="step">
                <div data-ng-switch-when="1">
                    <div data-ng-include src="'/Resources/DateShiftStep1.html'"></div>
                </div>
                <div data-ng-switch-when="2">
                    <div data-ng-include src="'/Resources/DateShiftStep2.html'"></div>
                </div>
                <div data-ng-switch-when="3">
                    <div data-ng-include src="'/Resources/DateShiftStep3.html'"></div>
                </div>
                <div data-ng-switch-when="4">
                    <div data-ng-include src="'/Resources/DateShiftStep4.html'"></div>
                </div>
                <div data-ng-switch-when="5">
                    <div data-ng-include src="'/Resources/DateShiftStep5.html'"></div>
                </div>
            </div>
	        <div class="bootstrap" data-ng-show="showErrorHandling && (step === 2 || step === 3)">
				<div class="form-row">
					<hr />
					<h2>Error Handling</h2>
				</div>
				<div class="form-row">
					<div class="form-label">Automatically Fix Negative Date Ranges (to Single Month) 
						<input type="checkbox" data-ng-model="errorHandling.error1" style="vertical-align:bottom" />
					</div>
				</div>
				<div class="form-row">
					<div class="form-label">Fix child outside Parent POP 
						<select data-ng-model="errorHandling.error2">
							<option data-ng-value="undefined"></option>
							<% if (SystemConfiguration.Instance().CompanyMode == CompanyConfiguration.MST)
								{ %>
							<option data-ng-value="0">No Change (Author to manually correct)</option>
							<%  } %>
							<option data-ng-value="2">To POP (Period of Performance)</option>
							<option data-ng-value="3">To Start</option>
							<option data-ng-value="4">To End</option>
						</select>
					</div>
				</div>
			</div>
			<div class="form-row last-form-row" style="height:48px">
		        <hr />
                <br />
		        <div class="buttons">
			        <button type="button" data-ng-disabled="backButtonDisabled()" data-ng-show="showBackButton()" data-ng-click="back()" class="ies">Back</button>
			        <button type="button" data-ng-disabled="validateButtonDisabled()" data-ng-show="showValidateButton()" data-ng-click="validate()" class="ies-action">Validate</button>
			        <button type="button" data-ng-show="showFinishButton()" class="ies-action" data-ng-click="finish()">Finish</button>
			        <button type="button" data-ng-show="showReturnButton()" class="ies-action" data-ng-click="return()">Return</button>
                    <button type="button" data-ng-disabled="nextButtonDisabled()" data-ng-show="showNextButton()" data-ng-click="next()" class="ies-action">Next</button>
                    <button type="button" data-ng-show="showCancelButton()" class="ies left-margin" data-ng-click="return()">Cancel</button>
		        </div>
	        </div>
		</div>
	</div>
    <div class="boe-summary">
	    <div id="DateShiftOutput">
		<div class="module">
			<div class="module-header-data">Results<span data-ng-hide="step === 5"> - Verify for correctness</span></div>
			<div class="module-content-data">
                <div class="bootstrap">
                    <div class="form-row">
                        <div class="row">
                            <div class="col-md-3 col-sm-3 bold">&nbsp;</div>
                        </div>
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold"></div>
			                <div class="col-md-2 col-sm-2 bold underline">Start Date</div>
			                <div class="col-md-1 col-sm-1 bold"></div>
			                <div class="col-md-5 col-sm-5 bold underline">End Date</div>
		                </div>
	                </div>
                    <% if (Model.DateShiftLevel != Level.Workspace)
                        { %>
	                <div class="form-row">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">Parent <%: Model.ParentLevel.GetDescription() %></div>
			                <div class="col-md-2 col-sm-2"><%: Model.ParentStartDate %></div>
			                <div class="col-md-1 col-sm-1 bold"></div>
			                <div class="col-md-5 col-sm-5"><%: Model.ParentEndDate %></div>
		                </div>
	                </div>
                    <% } %>
	                <div class="form-row">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">Original <%: Model.DateShiftLevel.GetDescription() %></div>
			                <div class="col-md-2 col-sm-2"><%: Model.StartDate %></div>
			                <div class="col-md-1 col-sm-1 bold"></div>
			                <div class="col-md-5 col-sm-5"><%: Model.EndDate %></div>
		                </div>
	                </div>
	                <div class="form-row" data-ng-show="step > 1">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">Adjusted<span data-ng-hide="finished"> Calculated</span> <%: Model.DateShiftLevel.GetDescription() %></div>
			                <div class="col-md-2 col-sm-2 bold" style="font-size:16px;color:green">{{shift.startDate}}</div>
			                <div class="col-md-1 col-sm-1 bold"></div>
			                <div class="col-md-5 col-sm-5 bold" style="font-size:16px;color:green">{{durationChange.endDate}}</div>
		                </div>
	                </div>
	                <div class="form-row" data-ng-show="showActual">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">Actual <%: Model.DateShiftLevel.GetDescription() %> (different because of Error Handling)</div>
			                <div class="col-md-2 col-sm-2 bold" style="font-size:16px;color:red">{{results.startDate}}</div>
			                <div class="col-md-1 col-sm-1 bold"></div>
			                <div class="col-md-5 col-sm-5 bold" style="font-size:16px;color:red">{{results.endDate}}</div>
		                </div>
	                </div>
                    <div class="form-row">
                        <div class="row">
                            <div class="col-md-3 col-sm-3 bold">&nbsp;</div>
                        </div>
                    </div>
                    <div class="form-row" data-ng-show="step > 2 && showShift">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">{{getShiftCaption()}}</div>
			                <div class="col-md-9 col-sm-9">Offset {{shift.offset}} months, {{getShiftText()}}</div>
		                </div>
	                </div>
                    <div class="form-row" data-ng-show="step > 2 && showDuration">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">{{getDurationCaption()}}</div>
			                <div class="col-md-9 col-sm-9">Offset {{durationChange.offset}} months, {{getDurationText()}}</div>
		                </div>
	                </div>
                    <div class="form-row" data-ng-show="step > 3 && showDiscreteStep()">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">Discrete Handling</div>
			                <div class="col-md-9 col-sm-9">{{getDiscreteText()}}</div>
		                </div>
	                </div>
                    <div class="form-row" data-ng-show="step > 3 && showErrorHandling && errorHandling.error1">
		                <div class="row">
                            <div class="col-md-3 col-sm-3 bold">Error Handling</div>
			                <div class="col-md-9 col-sm-9">Automatically Fix Negative Date Ranges</div>
		                </div>
	                </div>
                    <div class="form-row" data-ng-show="step > 3 && showErrorHandling && errorHandling.error2 >= 0">
		                <div class="row">
			                <div class="col-md-3 col-sm-3 bold">Error Handling</div>
			                <div class="col-md-9 col-sm-9">{{getError2Text()}}</div>
		                </div>
	                </div>
                </div>
			</div>
		</div>
	</div>
	    <div id="HelpImages">
		    <div class="module">
			    <div class="module-header-data">Child Handling Images</div>
			    <div class="module-content-data">
                    <div class="section">
                        <br />
                        <span>Note: Children can be CLINs, BOEs, Tasks, {{::note}} depending on the Parent.</span>
                    </div>
				    <div class="section help-images" data-ng-repeat="mod in help" data-ng-show="showImages(mod)">
					    <hr data-ng-if="$index > 0" />
					    <h1>{{::mod.caption}}</h1>
					    <div data-ng-repeat="img in mod.images" class="image-holder" data-ng-class="{'selected-image-holder': isImageSelected(mod.type, img)}" data-ng-show="showImage(mod.type, img)">
						    <span data-ng-class="{'pointer caption': step == 2}" data-ng-click="selectChildHandling(mod.type, img.mod)" helptext="{{::img.help}}">{{::img.caption}}</span>
                            <img data-ng-click="currentHelpSlide($index);openHelpModal($parent.$index)" data-ng-src="/Content/images/dateshift/{{::img.src}}.PNG" alt="{{img.caption}}" />
					    </div>
				    </div>
			    </div>
		    </div>
	    </div>
    </div>
	<div id="HelpModal" class="modal" data-ng-if="showHelpModal">
	    <span class="close cursor" data-ng-click="closeHelpModal()">&times;</span>
	    <div class="modal-content">
        <div class="caption-container">
		    <p id="caption">{{selectedHelp.caption}}</p>
	    </div>
	    <div data-ng-repeat="img in selectedHelp.images" data-ng-show="$index === helpSlideIndex" class="mySlides">
		    <div class="numbertext">{{$index + 1}} / 4</div>
		    <img data-ng-src="/Content/images/dateshift/{{img.src}}.png" style="width:100%">
	    </div>

	    <!-- Next/previous controls -->
	    <a class="prev" data-ng-click="plusHelpSlides(-1)">&#10094;</a>
	    <a class="next" data-ng-click="plusHelpSlides(1)">&#10095;</a>

	    <!-- Caption text -->
	    <div class="caption-container">
		    <p id="caption">{{selectedHelp.images[helpSlideIndex].caption}}</p>
	    </div>

	    <!-- Thumbnail image controls -->
	    <div data-ng-repeat="thumb in selectedHelp.images" class="column">
		    <img class="demo" data-ng-class="{'active': $index === helpSlideIndex}" data-ng-src="/Content/images/dateshift/{{thumb.src}}.png" data-ng-click="currentHelpSlide($index)" alt="{{thumb.caption}}">
	    </div>
	</div>
</div>
</div>
<script type="text/javascript">
	angular.element(document).ready(function () {
		angular.bootstrap(document, ['genboe']);
	});
</script> 
</asp:Content>
