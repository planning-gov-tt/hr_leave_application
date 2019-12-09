<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyEmployeeLeaveApplications.aspx.cs" Inherits="HR_LEAVEv2.Supervisor.MyEmployeeLeaveApplications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>My Employee Leave Applications</h1>
    <TWebControl:GridViewWebControl ID ="GridViewWebControlSup" gridViewType="sup" runat="server"></TWebControl:GridViewWebControl>    

</asp:Content>
