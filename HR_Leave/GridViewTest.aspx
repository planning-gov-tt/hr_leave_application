﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GridViewTest.aspx.cs" Inherits="HR_Leave.GridViewTest" %>

<%@ Register Src="~/MainGridView.ascx" TagName="GridViewWebControl" TagPrefix="TWebcontrol" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <h1>This is the main GridView</h1>

    <TWebControl:GridViewWebControl ID ="GridView" runat="server"></TWebControl:GridViewWebControl>

</asp:Content>
