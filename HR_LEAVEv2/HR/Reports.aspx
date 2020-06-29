<%@ Page Title="Reports" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="HR_LEAVEv2.HR.Reports" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>
    <div class="container-fluid">
        <%--Tab labels--%>
        <div class="row" id="myTabs">
            <ul class="nav nav-tabs nav-justified" id="myTab" role="tablist" style="width: 70%; margin: auto;">
                <li class="nav-item active" id="ihrisItem">
                    <a class="nav-link active" id="ihrisTab" data-toggle="tab" href="#ihris" role="tab" aria-controls="ihris" aria-selected="true">IHRIS</a>
                </li>
                <li class="nav-item" id="hrReportsItem">
                    <a class="nav-link" id="hrReportsTab" data-toggle="tab" href="#hrReports" role="tab" aria-controls="hrReports" aria-selected="false">HR</a>
                </li>
            </ul>
        </div>

        <%-- Tab content --%>
        <div class="row tab-content" id="myTabContent" style="text-align:center;">
            <div class="tab-pane fade active in" id="ihris" role="tabpanel" aria-labelledby="ihrisTab">
                <h3>IHRIS</h3>

                <asp:Button ID="getIhrisReportBtn" runat="server" Text="Get data on active employees' leave" CssClass="btn btn-primary" OnClick="getIhrisReportBtn_Click" />

                <asp:Panel ID="filesToDownloadPanel" runat="server" Visible="false" Style="margin-top:15px;">
                    <asp:Label ID="fileToDownloadLabel" runat="server" Text=""></asp:Label>
                    <asp:LinkButton ID="btnDownloadFiles" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Download file" OnClick="btnDownloadFiles_Click" Style="display: inline-block; margin-left: 5px;">
                        <i class="fa fa-download" aria-hidden="true"></i>
                    </asp:LinkButton>
                </asp:Panel>

                <asp:Panel ID="msgPanel" runat="server">
                    <asp:Panel ID="errorPanel" runat="server" CssClass="row alert alert-danger validation-msg" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span>Error occurred. Contact IT for more information</span>
                    </asp:Panel>
                </asp:Panel>

                <%--Put in error messages--%>
            </div>
            <div class="tab-pane fade" id="hrReports" role="tabpanel" aria-labelledby="hrReportsTab">
                <h3>HR</h3>
                <div class="alert alert-info text-center" role="alert" style="width:200px; margin: 0 auto">
                    <i class="fa fa-cogs"></i>
                    Under development
                </div>
            </div>
        </div>
    </div>

    <script>

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            var storage = sessionStorage;
            var lastActiveItem = storage.getItem("lastActiveItemId");
            var lastActiveItemContent = storage.getItem("lastActiveItemContentId");
            if (lastActiveItem != null && lastActiveItemContent != null) {
                // remove active class from item tab
                $('.nav-item').removeClass('active');

                // remove 'active' and 'in' classes from tab content
                $('.tab-pane').removeClass('active');
                $('.tab-pane').removeClass('in');

                // add classes to relevant element
                $('#' + lastActiveItem).addClass("active");
                $('#' + lastActiveItemContent).addClass("active");
                $('#' + lastActiveItemContent).addClass("in");
            }
        });

        // store the current tab id in session storage
        $('.nav-item').click(function () {
            var itemId = $(this).attr('id').toString();
            var contentId = itemId.replace('Item', '');

            var storage = sessionStorage;
            storage.setItem('lastActiveItemId', itemId);
            storage.setItem('lastActiveItemContentId', contentId);
        });

    </script>
</asp:Content>
