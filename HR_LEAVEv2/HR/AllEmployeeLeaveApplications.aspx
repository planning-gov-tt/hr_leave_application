<%@ Page Title="All Employee Leave Applications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AllEmployeeLeaveApplications.aspx.cs" Inherits="HR_LEAVEv2.HR.AllEmployeeLeaveApplications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <%--Allows user to return to last page they were on before --%>
    <asp:LinkButton ID="returnToPreviousBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Return to previous page" OnClick="returnToPreviousBtn_Click">
        <i class="fa fa-arrow-left" aria-hidden="true"></i>
    </asp:LinkButton>
    <h1><%: Title %></h1>
    <br /><br />
    <TWebControl:GridViewWebControl ID ="GridViewWebControlHr" gridViewType="hr" runat="server"></TWebControl:GridViewWebControl>

</asp:Content>
