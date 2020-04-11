<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyEmployeeLeaveApplications.aspx.cs" Inherits="HR_LEAVEv2.Supervisor.MyEmployeeLeaveApplications"%>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%--Allows user to return to last page they were on before --%>
    <asp:LinkButton ID="returnToPreviousBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Return to previous page" OnClick="returnToPreviousBtn_Click">
        <i class="fa fa-arrow-left" aria-hidden="true"></i>
    </asp:LinkButton>

    <h1>My Employee Leave Applications</h1>
    <br /><br />
    <TWebControl:GridViewWebControl ID ="GridViewWebControlSup" gridViewType="sup" runat="server"></TWebControl:GridViewWebControl>    

</asp:Content>
