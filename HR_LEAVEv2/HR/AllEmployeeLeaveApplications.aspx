<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AllEmployeeLeaveApplications.aspx.cs" Inherits="HR_LEAVEv2.HR.AllEmployeeLeaveApplications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>All Employee Leave Applications</h1>
    <TWebControl:GridViewWebControl ID ="GridViewWebControlHr" gridViewType="hr" runat="server"></TWebControl:GridViewWebControl>

</asp:Content>
