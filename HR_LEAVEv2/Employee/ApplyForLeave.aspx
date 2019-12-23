<%@ Page Title="Apply for Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyForLeave.aspx.cs" Inherits="HR_LEAVEv2.Employee.ApplyForLeave"  %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Src="~/UserControls/supervisorSelect.ascx" TagName="SupervisorSelectUserControl" TagPrefix="TWebControl"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>

        #applyForLeaveContainer div.form-group{
            margin-top:50px;
        }
        
    </style>
    <h1><%: Title %></h1>
    <div id="applyForLeaveContainer" class="container-fluid text-center">
        <div class="row form-group" >
            <div style="display:inline-block; margin-right:15%;">
                <label for="txtFrom" style="font-size:1.5em">From:</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="fromCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                <asp:RequiredFieldValidator ID="fromCalendarRequiredValidator" runat="server" ControlToValidate="txtFrom" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" ValidationGroup="applyForLeave"></asp:RequiredFieldValidator>
            </div>
            <div style="display:inline-block;">
                <label for="txtTo" style="font-size:1.5em">To:</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                <i id="toCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" />
                <asp:RequiredFieldValidator ID="toCalendarRequiredValidator" runat="server" ControlToValidate="txtTo" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"  ValidationGroup="applyForLeave"></asp:RequiredFieldValidator>
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
                <asp:ListItem Value="Pre-retirement"></asp:ListItem>
                <asp:ListItem Value="Bereavement"></asp:ListItem>
            </asp:DropDownList>
            <asp:RequiredFieldValidator ID="typeOfLeaveRequiredValidator" runat="server" ControlToValidate="typeOfLeave" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" ValidationGroup="applyForLeave"></asp:RequiredFieldValidator>
        </div>

        <div class="row form-group">
            <label for="supervisor_select" style="font-size:1.5em">Supervisor:</label>
            <TWebControl:SupervisorSelectUserControl ID="supervisor_select" runat="server" validationGroup="applyForLeave"/>
        </div>

        <div class="row form-group">
            <asp:FileUpload ID="FileUpload1" runat="server" Width="475px" style="margin:auto; display:inline-block" />
        </div>

        <div class="row form-group">
            <label for="txtComments" style="font-size:1.5em">Comments</label>
            <textarea runat="server" class="form-control" id="txtComments" rows="4" style="width:45%; margin:0 auto;"></textarea>
        </div>
        <div class="row">
            <asp:UpdatePanel ID="UpdatePanelLeaveCount" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="LeaveCountUserControl" />
                </Triggers>
                <ContentTemplate>
                     <TWebControl:LeaveCountUserControlBS4 ID ="LeaveCountUserControl" runat="server"></TWebControl:LeaveCountUserControlBS4>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <div class="row" id="validationRow" style="margin-top:25px;">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
                    </asp:Panel>
                    <asp:Panel ID="startDateBeforeTodayValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="startDateBeforeTodayValidationMsg" runat="server">Start date cannot be before today</span>
                    </asp:Panel>
                    <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidEndDateValidationMsg" runat="server">End date is not valid</span>
                    </asp:Panel>
                    <asp:Panel ID="dateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="dateComparisonValidationMsg" runat="server">End date cannot precede start date</span>
                    </asp:Panel>
                    <asp:Panel ID="invalidVacationStartDateMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="invalidVacationStartDateMsg" runat="server">You must request vacation leave at least a month before the start date</span>
                    </asp:Panel>
                    <asp:Panel ID="invalidSickLeaveStartDate" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span1" runat="server">Sick leave must already have beeen taken prior to today</span>
                    </asp:Panel>
                    <%--<asp:Panel ID="validationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; width: 500px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="validationMsg" runat="server">End date is not valid</span>
                    </asp:Panel>--%>
                    <asp:Panel ID="successMsgPanel" runat="server" CssClass="row alert alert-success" Style="display: none" role="alert">
                        <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                        <span id="successMsg" runat="server">Application successfully submitted</span>
                        <asp:Button ID="submitAnotherLA" runat="server" Text="Submit another" CssClass="btn btn-success" Style="display: inline; margin-left: 10px" OnClick="refreshForm" />
                    </asp:Panel>
                    <asp:Panel ID="submitButtonPanel" runat="server" CssClass="row form-group">
                        <asp:Button ID="cancelBtn" runat="server" Text="Cancel" Style="margin-right: 35px;" CssClass="btn btn-danger" OnClick="refreshForm" CausesValidation="False" />
                        <asp:Button ID="submitBtn" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="submitLeaveApplication_Click" ValidationGroup="applyForLeave"/>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>
    

</asp:Content>


