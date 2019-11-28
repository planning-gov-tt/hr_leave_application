<%@ Page Title="" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="GridViewTest.aspx.cs" Inherits="HR_Leave.GridViewTest" %>

<%--<%@ Register Src="~/MainGridView.ascx" TagName="GridViewWebControl" TagPrefix="TWebcontrol" %>--%>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
 

    <h1>GridView Demo</h1>
    <center><%:Session["first_name"]%> <%: Session["last_name"]%></center>
    <br /><br />

    <center>

        <h2>Employee View</h2>
        <TWebControl:GridViewWebControl ID ="GridViewWebControlEmp" gridViewType="emp" runat="server"></TWebControl:GridViewWebControl>
        <br /><br />
        
        <h2>Supervisor View</h2>
        <TWebControl:GridViewWebControl ID ="GridViewWebControlSup" gridViewType="sup" runat="server"></TWebControl:GridViewWebControl>
        <br /><br />

        <h2>HR View</h2>
        <TWebControl:GridViewWebControl ID ="GridViewWebControlHr" gridViewType="hr" runat="server"></TWebControl:GridViewWebControl>
        <br /><br />
         
    </center>
        

</asp:Content>
