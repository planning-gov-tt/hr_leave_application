<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HR_LEAVEv2._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        #leaveCountHeader{
            font-size:2em;
        }
    </style>
    <div class="container">
        <TWebControl:LeaveCountUserControlBS4 ID ="LeaveCountUserControl" runat="server" ></TWebControl:LeaveCountUserControlBS4>
        <asp:Panel ID="applyForLeaveCtaPanel" Style="margin: 30px auto; width: 100%; text-align:center;" runat="server">
            <asp:LinkButton ID="applyForLeaveRedirectBtn" runat="server" CssClass="btn btn-primary" Style="font-size:1.5em" OnClick="applyForLeaveRedirectBtn_Click">
                <i class="fa fa-file" aria-hidden="true"></i>
                Apply for Leave
            </asp:LinkButton>
        </asp:Panel>
        <asp:Panel ID="yearsWorkedPanel" runat="server" Style="margin: 50px auto; width: 100%; text-align:center;" Visible="false">
            <i class="fa fa-trophy fa-2x" style="display:inline; margin-right:5px;"></i>
            <span style="font-size:2.2em;">Years Worked : <%= numYearsWorked %></span>
        </asp:Panel>
    </div>

    
</asp:Content>
