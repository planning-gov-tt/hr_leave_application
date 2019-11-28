<%@ Page Title="Apply for Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyForLeave.aspx.cs" Inherits="HR_Leave.ApplyForLeave" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Src="~/supervisorSelect.ascx" TagName="SupervisorSelectUserControl" TagPrefix="TWebControl"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #fromCalendar, #toCalendar{
            cursor:pointer;
            font-size: 1.3em;
        }

        #applyForLeaveContainer div.row{
            margin-top:50px;
        }


        
    </style>
    <h1><%: Title %></h1>
    <div id="applyForLeaveContainer" class="container-fluid text-center">
        <div class="row form-group" >
            <div style="display:inline-block; margin-right:15%;">
                <label for="txtFrom" style="font-size:1.5em">From:</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="fromCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtFrom" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
            </div>
            <div style="display:inline-block;">
                <label for="txtTo" style="font-size:1.5em">To:</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="toCalendar" class="fa fa-calendar fa-lg"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
                <asp:RequiredFieldValidator runat="server" ControlToValidate="txtTo" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
            </div>
        </div>
        <div class="row form-group" >
            <label for="typeOfLeave" style="font-size:1.5em">Type:</label>
            <asp:DropDownList ID="typeOfLeave" runat="server" CssClass="form-control" Width="150px" style="display:inline;">
                <asp:ListItem Value=""></asp:ListItem>
                <asp:ListItem Value="Sick"></asp:ListItem>
                <asp:ListItem Value="Personal"></asp:ListItem>
                <asp:ListItem Value="Vacation"></asp:ListItem>
                <asp:ListItem Value="Maternity"></asp:ListItem>
                <asp:ListItem Value="Bereavement"></asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator runat="server" ControlToValidate="typeOfLeave" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
        </div>

        <div class="row form-group">
            <label for="supervisor_select" style="font-size:1.5em">Supervisor:</label>
            <TWebControl:SupervisorSelectUserControl ID="supervisor_select" runat="server" />
        </div>

        <div class="row" style="background-color:lightblue;">
            File upload goes here
        </div>

        <div class="row form-group">
            <label for="txtComments" style="font-size:1.5em">Comments</label>
            <textarea runat="server" class="form-control" id="txtComments" rows="4" style="width:45%; margin:0 auto;"></textarea>
        </div>

         <div class="row" style="background-color:lightblue;">
            Leave Balance goes here
<%--            <ajaxToolkit:PieChart ID="PieChart1" runat="server" ChartHeight="250px" ChartTitle="Test Pie" ChartWidth="250px" Height="250px" Width="250px" BorderWidth="250px" ForeColor="White" >
                <PieChartValues>
                    <ajaxToolkit:PieChartValue Category="C1" Data="20" PieChartValueColor="red" PieChartValueStrokeColor="" />
                    <ajaxToolkit:PieChartValue Category="C2" Data="35" PieChartValueColor="blue" PieChartValueStrokeColor="" />
                    <ajaxToolkit:PieChartValue Category="C3" Data="40" PieChartValueColor="green" PieChartValueStrokeColor="" />
                </PieChartValues>
             </ajaxToolkit:PieChart>--%>
        </div>

        <div class="row">
            <asp:Panel ID="dateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none" role="alert">
                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                <span id="dateComparisonValidationMsg" runat="server">Start date must be a date preceding the end date</span>
            </asp:Panel>
            <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none; margin:0px 5px;" role="alert">
                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
            </asp:Panel>
            <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none;margin:0px 5px;" role="alert">
                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                <span id="invalidEndDateValidationMsg" runat="server">End date is not valid</span>
            </asp:Panel>
            <asp:Panel ID="validationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none; width:500px;" role="alert">
                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                <span id="validationMsg" runat="server">End date is not valid</span>
            </asp:Panel>
            <asp:Panel ID="successMsgPanel" runat="server" CssClass="row alert alert-success" style="display:none" role="alert">
                <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                <span id="successMsg" runat="server"></span>
                <asp:Button ID="submitAnotherLA" runat="server" Text="Submit another" CssClass="btn btn-success" style="display:inline; margin-left:10px" OnClick="refreshForm" />
            </asp:Panel>
            <asp:Panel ID="submitButtonPanel" runat="server" CssClass="row form-group">
                <asp:Button ID="cancelBtn" runat="server" Text="Cancel" style="margin-right:35px;" CssClass="btn btn-danger" OnClick="refreshForm" CausesValidation="False"/>
                <button type="submit" class="btn btn-success" runat="server" onserverclick="submitLeaveApplication_ServerClick">Submit</button>
            </asp:Panel>
        </div>
    </div>
    

</asp:Content>

