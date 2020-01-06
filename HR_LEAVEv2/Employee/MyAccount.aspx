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
        <div class="row" id="myTabs">
            <ul class="nav nav-tabs nav-justified" id="myTab" role="tablist" style="width:70%; margin:auto;">
              <li class="nav-item active" id="summaryItem">
                <a class="nav-link active" id="summaryTab" data-toggle="tab" href="#summary" role="tab" aria-controls="summary" aria-selected="true">Summary</a>
              </li>
              <li class="nav-item" id="viewLeaveLogsItem">
                <a class="nav-link" id="viewLeaveLogsTab" data-toggle="tab" href="#viewLeaveLogs" role="tab" aria-controls="viewLeaveLogs" aria-selected="false">View Leave Logs</a>
              </li>
              <li class="nav-item" id="editInfoItem">
                <a class="nav-link" id="editInfoTab" data-toggle="tab" href="#editInfo" role="tab" aria-controls="editInfo" aria-selected="false">Edit Account Info</a>
              </li>
            </ul>
        </div>
        <div class="row tab-content" id="myTabContent">
          <div class="tab-pane fade active in" id="summary" role="tabpanel" aria-labelledby="summaryTab">
              <h3>Summary</h3>
              <TWebControl:LeaveCountUserControlBS4 ID ="LeaveCountUserControl" runat="server"></TWebControl:LeaveCountUserControlBS4>
          </div>
          <div class="tab-pane fade" id="viewLeaveLogs" role="tabpanel" aria-labelledby="viewLeaveLogsTab">
              <h3>My Leave Logs</h3>

              <div id="leaveLogContainer">
                  <TWebControl:GridViewWebControl ID ="GridViewWebControlEmp" gridViewType="emp" runat="server"></TWebControl:GridViewWebControl>
              </div>

          </div>
          <div class="tab-pane fade" id="editInfo" role="tabpanel" aria-labelledby="editInfoTab">
              <h3>Edit Account</h3>
          </div>
        </div>
    </div>

    <script>

        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            var lastActiveItem = sessionStorage.getItem("lastActiveItemId");
            var lastActiveItemContent = sessionStorage.getItem("lastActiveItemContentId");
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

        $('.nav-item').click(function () {
            var itemId = $(this).attr('id').toString();
            var contentId = itemId.replace('Item', '');

            sessionStorage.setItem('lastActiveItemId', itemId);
            sessionStorage.setItem('lastActiveItemContentId', contentId);
        });

    </script>
</asp:Content>
