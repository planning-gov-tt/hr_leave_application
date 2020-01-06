<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HR_LEAVEv2._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <style>
        #leaveCountHeader{
            font-size:2em;
        }
    </style>
    <div class="container">
        <TWebControl:LeaveCountUserControlBS4 ID ="LeaveCountUserControl" runat="server" ></TWebControl:LeaveCountUserControlBS4>
    </div>

    
</asp:Content>
