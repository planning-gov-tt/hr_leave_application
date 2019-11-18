<%@ Page Title="Apply for Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyForLeave.aspx.cs" Inherits="HR_Leave.ApplyForLeave" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>
    <div class="row" style="margin-top:25px;">
        <div style="width:50%; margin:auto; position:relative;">
            <div style="display:inline-block;">
                <span style="font-size:1.5em">From:</span>
                <asp:Calendar ID="Calendar1" runat="server"></asp:Calendar>
<%--                <p>Date: <input type="text" id="datepicker"></p>--%>
            </div>
            <div style="display:inline-block; position:absolute;right:0px; top:0px;">
                <span style="font-size:1.5em">To:</span>
                <asp:Calendar ID="Calendar2" runat="server"></asp:Calendar>
            </div>
        </div>
    </div>
<%--    <script>
        $(document).ready(function () {
            $("#datepicker").datepicker();
        });
    </script>--%>
</asp:Content>
