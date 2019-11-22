<%@ Page Title="My Account" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyAccount.aspx.cs" Inherits="HR_Leave.MyAccount" %>
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
              <li class="nav-item active">
                <a class="nav-link active" id="summaryTab" data-toggle="tab" href="#summary" role="tab" aria-controls="summary" aria-selected="true">Summary</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" id="viewLeaveLogsTab" data-toggle="tab" href="#viewLeaveLogs" role="tab" aria-controls="viewLeaveLogs" aria-selected="false">View Leave Logs</a>
              </li>
              <li class="nav-item">
                <a class="nav-link" id="editInfoTab" data-toggle="tab" href="#editInfo" role="tab" aria-controls="editInfo" aria-selected="false">Edit Account Info</a>
              </li>
            </ul>
        </div>
        <div class="row tab-content" id="myTabContent">
          <div class="tab-pane fade active in" id="summary" role="tabpanel" aria-labelledby="summaryTab">
              <h3>Summary</h3>
          </div>
          <div class="tab-pane fade" id="viewLeaveLogs" role="tabpanel" aria-labelledby="viewLeaveLogsTab">
              <h3>Leave Logs</h3>
          </div>
          <div class="tab-pane fade" id="editInfo" role="tabpanel" aria-labelledby="editInfoTab">
              <h3>Edit Account</h3>
          </div>
        </div>
    </div>
</asp:Content>
