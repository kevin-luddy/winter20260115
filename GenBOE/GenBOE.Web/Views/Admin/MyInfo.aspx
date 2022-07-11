<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Home/Master/Home.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>
<%@ Import namespace="System.Web.Optimization" %>
<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    System Administration
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
	<%: Scripts.Render("~/bundles/angularapp") %>
    <%: Scripts.Render("~/bundles/systemAdmin") %>
    <%: Styles.Render("~/Content/siteCss") %>
	<script type="text/javascript">
		var myInfoWidget;

		$(function () {

			var myInfoWidgetConfig = { ContextID: "myInfo", IsModule: false, isReadOnly: true }; // always mark this as false so we can essentially override the GenListWidget readonly rules
			myInfoWidget = new GenWidget(myInfoWidgetConfig);
		});
	</script>
    <div id="myInfo" class="my-info-module module">
		<div class="module-header-data">My Info</div>
        <div class="module-content-data">
			<div class="form-row">
				<div class="container bootstrap">
				<div class="row">
					<div class="col-md-2 bold">
						Display Name
					</div>
					<div class="col-md-10">
						<%= ((MyInfoModelView)Model).DisplayName %>
					</div>
				</div>
				<div class="row">
					<div class="col-md-12">&nbsp;</div>
				</div>
				<div class="row">
					<div class="col-md-2 bold">
						NTID
					</div>
					<div class="col-md-10">
						<%= ((MyInfoModelView)Model).NtId %>
					</div>
				</div>
				<div class="row">
					<div class="col-md-12">&nbsp;</div>
				</div>
				<div class="row">
					<div class="col-md-2 bold">
						IES Bearer Token
					</div>
					<div class="col-md-10" style="word-wrap: break-word;">
						<%= ((MyInfoModelView)Model).BearerToken %>
					</div>
				</div>
			</div>
			</div>
		</div>
    </div>
</asp:Content>
