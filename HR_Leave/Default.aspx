<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="HR_Leave._Default" %>
<%@ Register Src="~/supervisorSelect.ascx" TagName="WebControl" TagPrefix="TWebControl"%>
<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    We don't do it for love, we don't do it for a kiss. We do it bc we're us, built by Chris and Tris
    <style>
        .test{
            background-color:red;
        }
    </style>
    <TWebControl:WebControl ID="Header" runat="server" />
</asp:Content>
