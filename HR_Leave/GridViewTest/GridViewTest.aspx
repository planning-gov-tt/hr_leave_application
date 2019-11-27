<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GridViewTest.aspx.cs" Inherits="HR_Leave.GridViewTest" %>

<%--<%@ Register Src="~/MainGridView.ascx" TagName="GridViewWebControl" TagPrefix="TWebcontrol" %>--%>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>This is the main GridView</h1>

    <TWebControl:GridViewWebControl ID ="GridViewWebControl1" gridViewType="emp" runat="server"></TWebControl:GridViewWebControl>


    <%--<TWebControl:GridViewWebControl ID ="GridViewWebControl2" runat="server"></TWebControl:GridViewWebControl>--%>


</asp:Content>
