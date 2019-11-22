<%@ Page Title="Apply for Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyForLeave.aspx.cs" Inherits="HR_Leave.ApplyForLeave" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #fromCalendar, #toCalendar{
            cursor:pointer;
            font-size: 1.3em;
        }

        div.row{
            margin-top:50px;
        }
    </style>
    <h1><%: Title %></h1>
    <div class="container-fluid text-center">
        <div class="row form-group" >
            <div style="display:inline-block; margin-right:15%;">
                <label for="txtFrom" style="font-size:1.5em">From:</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="fromCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server" />
            </div>
            <div style="display:inline-block;">
                <label for="txtTo" style="font-size:1.5em">To:</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="toCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
            </div>
        </div>
        <div class="row form-group" >
            <label for="typeOfLeave" style="font-size:1.5em">Type:</label>
            <select class="form-control" id="typeOfLeave" style="width:150px; display:inline;">
              <option>Sick</option>
              <option>Personal</option>
              <option>Vacation</option>
              <option>Maternity</option>
              <option>Bereavement</option>
            </select>
        </div>

        <div class="row form-group" style="background-color:lightblue;">
            Head of Division/Section goes here
        </div>

        <div class="row form-group">
            <label for="comments" style="font-size:1.5em">Comments</label>
            <textarea class="form-control" id="comments" rows="4" style="width:45%; margin:0 auto;"></textarea>
        </div>

         <div class="row form-group" style="background-color:lightblue;">
            Leave Balance goes here
        </div>

        <div class="row form-group">
            <button class="btn btn-danger" style="margin-right:35px;">Cancel</button>
            <button type="submit" class="btn btn-success">Submit</button>
        </div>
    </div>
    

</asp:Content>

