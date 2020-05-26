<%@ Page Title="My Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyAccount.aspx.cs" Inherits="HR_LEAVEv2.Employee.MyAccount" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #myTabs{
            margin-top:35px;
        }

        #myTabContent{
            padding-top:25px;
        }

    </style>
    <h1><%: Title %></h1>
    <div id="myAccountContainer" class="container-fluid text-center">
        <%--Tab labels--%>
        <div class="row" id="myTabs">
            <ul class="nav nav-tabs nav-justified" id="myTab" role="tablist" style="width: 70%; margin: auto;">
                <li class="nav-item active" id="summaryItem">
                    <a class="nav-link active" id="summaryTab" data-toggle="tab" href="#summary" role="tab" aria-controls="summary" aria-selected="true">Summary</a>
                </li>
                <li class="nav-item" id="viewLeaveLogsItem">
                    <a class="nav-link" id="viewLeaveLogsTab" data-toggle="tab" href="#viewLeaveLogs" role="tab" aria-controls="viewLeaveLogs" aria-selected="false">View Leave Logs</a>
                </li>
            </ul>
        </div>
        <%-- Tab content --%>
        <div class="row tab-content" id="myTabContent">
            <div class="tab-pane fade active in" id="summary" role="tabpanel" aria-labelledby="summaryTab">
                <h3>Summary</h3>
                <TWebControl:LeaveCountUserControlBS4 ID="LeaveCountUserControl" runat="server"></TWebControl:LeaveCountUserControlBS4>
            </div>
            <div class="tab-pane fade" id="viewLeaveLogs" role="tabpanel" aria-labelledby="viewLeaveLogsTab">
                <h3>My Leave Logs</h3>

                <div id="leaveLogContainer" style="margin-top:35px;">
                    <TWebControl:GridViewWebControl ID="GridViewWebControlEmp" gridViewType="emp" runat="server"></TWebControl:GridViewWebControl>
                </div>
            </div>
        </div>
    </div>

    <script>

        /** 
        * When filtering data in the Gridview in the 'My Leave Logs' tab, a partial postback occurs (the gridview
        * must be updated). This would reset the current tab to the first tab, Summary, when the postback occurs.
        *
        * The following function is run when the page load occurs to reset the tab to the last active tab. Session storage is used to store the id of the last active tab.
        * Furthermore, any future code which triggers a postback will also benefit from this code.
        */
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
