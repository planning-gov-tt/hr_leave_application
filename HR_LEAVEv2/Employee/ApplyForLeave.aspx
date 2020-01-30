<%@ Page Title="Apply for Leave" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="ApplyForLeave.aspx.cs" Inherits="HR_LEAVEv2.Employee.ApplyForLeave" MaintainScrollPositionOnPostback="true" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>
<%@ Register Src="~/UserControls/supervisorSelect.ascx" TagName="SupervisorSelectUserControl" TagPrefix="TWebControl"%>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>

        #applyForLeaveContainer div.form-group{
            margin-top:25px;
        }
    </style>
    
    <%--View mode: Allows user to return to last page they were on before they clicked on a details button for a leave application--%>
    <asp:LinkButton ID="returnToPreviousBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Return to previous page" OnClick="returnToPreviousBtn_Click">
        <i class="fa fa-arrow-left" aria-hidden="true"></i>
    </asp:LinkButton>

    <%--apply mode--%>
    <asp:Panel ID="applyModeTitle" runat="server">
        <h1 style="margin: 0 auto;"><%: Title %></h1>
    </asp:Panel>

    <%--view mode--%>
    <asp:Panel ID="viewModeTitle" runat="server">
        <h1>View Leave Application</h1>
    </asp:Panel>

    <%--edit mode--%>
    <asp:Panel ID="editModeTitle" runat="server">
        <h1>Edit Leave Application</h1>
    </asp:Panel>

    <%--Edit mode: id of employee--%>
    <asp:TextBox ID="empIdTxt" runat="server" Visible ="false"></asp:TextBox>

    <%--View mode, Edit mode: shows name of employee who submitted the leave application--%>
    <asp:Panel ID="empNamePanel" runat="server">
        <h2 id="empNameHeader" runat="server"></h2>
    </asp:Panel>
    <div id="applyForLeaveContainer" class="container-fluid text-center">
        <%--View mode: Shows date and time leave application was submitted--%>
        <asp:Panel ID="submittedOnPanel" CssClass="row" runat="server">
            <asp:Label ID="submittedOnTxt" runat="server"></asp:Label>
        </asp:Panel>
        <%--View mode: Shows status of leave application--%> 
        <asp:Panel ID="statusPanel" CssClass="row form-group" runat="server">
            <label for="statustxt" style="font-size:1.2em">Status:</label>
            <asp:TextBox ID="statusTxt" runat="server" Style="display:block; margin:0 auto; text-align:center;"></asp:TextBox> 
        </asp:Panel>

         <%--Apply mode: Shows the current leave balance of employee --%>
        <asp:Panel ID="leaveCountPanel" runat="server" CssClass="row" Style="margin-top:15px;height:225px;" >
            <asp:UpdatePanel ID="UpdatePanelLeaveCount" runat="server">
                <Triggers>
                    <asp:AsyncPostBackTrigger ControlID="LeaveCountUserControl" />
                </Triggers>
                <ContentTemplate>
                    <TWebControl:LeaveCountUserControlBS4 ID="LeaveCountUserControl" runat="server"></TWebControl:LeaveCountUserControlBS4>
                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>

        <%--View mode: Shows the start and end date of leave application--%>
        <%--Apply mode: Allows user to enter start and end date of application--%>
        <div class="row">
            <%--Start Date--%>
            <div style="display:inline-block; margin-right:15%;">
                <label for="txtFrom" style="font-size:1.2em">From:</label>
                <asp:TextBox ID="txtFrom" runat="server" CssClass="form-control" style="width:150px; height:auto; display:inline;" AutoPostBack="true" OnTextChanged="datesEntered"></asp:TextBox> 
                <i id="fromCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                <ajaxToolkit:CalendarExtender ID="fromCalendarExtender" TargetControlID="txtFrom" PopupButtonID="fromCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
                <asp:RequiredFieldValidator ID="fromCalendarRequiredValidator" runat="server" ControlToValidate="txtFrom" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" ValidationGroup="applyForLeave"></asp:RequiredFieldValidator>
            </div>
            <%--End Date--%>
            <div style="display:inline-block;">
                <label for="txtTo" style="font-size:1.2em">To:</label>
                <asp:TextBox ID="txtTo" runat="server" CssClass="form-control" style="width:150px; height:auto; display:inline;" AutoPostBack="true" OnTextChanged="datesEntered"></asp:TextBox> 
                <i id="toCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                <ajaxToolkit:CalendarExtender ID="toCalendarExtender" TargetControlID="txtTo" PopupButtonID="toCalendar" runat="server" Format="d/MM/yyyy"/>
                <asp:RequiredFieldValidator ID="toCalendarRequiredValidator" runat="server" ControlToValidate="txtTo" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"  ValidationGroup="applyForLeave"></asp:RequiredFieldValidator>
            </div>
        </div>
        <asp:Panel ID="numDaysAppliedForPanel" CssClass="row form-group" runat="server">
            <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-right: 5px; cursor: pointer"
                data-toggle="tooltip"
                data-placement="left"
                title="This count is a literal count and does not take into consideration holidays and weekends. Consult with HR to find out the exact amount of days taken"></i>
            <label for="numDaysAppliedFor" style="font-size: 1.2em">Days applied for:</label>
            <asp:Label ID="numDaysAppliedFor" runat="server" Text="0" Style="font-size: 1.1em"></asp:Label>
        </asp:Panel>

        <%--View mode: Shows the type of leave applied for--%>
        <%--Apply mode: Allows user to enter type of leave to apply for--%>
        <div class="row form-group" >
            <label for="typeOfLeave" style="font-size:1.2em;display:inline;">Type:</label>

            <%--apply mode--%>
            <asp:Panel ID="typeOfLeaveDropdownPanel" runat="server" Style="display:inline;">
                <asp:DropDownList ID="typeOfLeave" runat="server" CssClass="form-control" Width="150px" Height="27px" Style="display: inline;" AutoPostBack ="true" OnSelectedIndexChanged="typeOfLeave_SelectedIndexChanged">
                    <asp:ListItem Value=""></asp:ListItem>
                    <asp:ListItem Value="Sick"></asp:ListItem>
                    <asp:ListItem Value="Personal"></asp:ListItem>
                    <asp:ListItem Value="Vacation"></asp:ListItem>
                    <asp:ListItem Value="Maternity"></asp:ListItem>
                    <asp:ListItem Value="Pre-retirement"></asp:ListItem>
                    <asp:ListItem Value="Bereavement"></asp:ListItem>
                </asp:DropDownList>
                <asp:RequiredFieldValidator ID="typeOfLeaveRequiredValidator" runat="server" ControlToValidate="typeOfLeave" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" ValidationGroup="applyForLeave"></asp:RequiredFieldValidator>
            </asp:Panel>

            <%--view mode--%>
            <asp:Panel ID="typeOfLeavePanel" runat="server" Style="display:inline;">
                <asp:TextBox ID="typeOfLeaveTxt" runat="server" Style="text-align:center;"></asp:TextBox> 
            </asp:Panel>
        </div>

        <%--View mode: Shows the supervisor the leave application was submitted to--%>
        <%--Apply mode: Allows user to enter supervisor to send leave application to--%>
        <div class="row form-group">
            <label for="supervisor_select" style="font-size:1.2em; display:inline;">Supervisor:</label>
            <%--apply mode--%>
            <asp:Panel ID="supervisorSelectUserControlPanel" runat="server" Style="display:inline;">
                <TWebControl:SupervisorSelectUserControl ID="supervisor_select" runat="server" validationGroup="applyForLeave"/>
            </asp:Panel>

            <%--view mode--%>
            <asp:Panel ID="supervisorPanel" runat="server"  Style="display:inline;">
                <asp:TextBox ID="supervisorNameTxt" runat="server" Style="text-align:center;"></asp:TextBox>
            </asp:Panel>
        </div>

        <%--Apply mode: Allows user to upload docs--%>
        <div class="row form-group">
            <label for="FileUpload1" style="font-size:1.2em; display:inline;">Upload Files:</label>
            <asp:Panel ID="fileUploadPanel" runat="server" Style="display:inline;">
                <asp:FileUpload ID="FileUpload1" runat="server" Width="475px" Style="margin: auto; display: inline-block" />
            </asp:Panel>
        </div>
        

        <%--View mode: Shows the comments made by employee when submitting the leave application --%>
        <%--Apply mode: Allows employee to enter comments to explain anything necessary as to why they need the leave etc.--%>
        <asp:Panel ID="empCommentsPanel" runat="server" CssClass="row form-group">
            <label for="empCommentsTxt" style="font-size:1.2em">Employee Comments</label>
            <textarea runat="server" class="form-control" id="empCommentsTxt" rows="4" style="width:45%; margin:0 auto;"></textarea>
        </asp:Panel>

        <%--View mode: Shows the comments made by supervisor as to why they recommended or did not recommend the leave application --%>
        <asp:Panel ID="supCommentsPanel" runat="server" CssClass="row form-group">
            <label for="supCommentsTxt" style="font-size:1.2em">Supervisor Comments</label>
            <textarea runat="server" class="form-control" id="supCommentsTxt" rows="4" style="width:45%; margin:0 auto;"></textarea>
        </asp:Panel>

        <%--View mode: Shows the comments made by hr as to why they approved or did not approve the leave application --%>
        <asp:Panel ID="hrCommentsPanel" runat="server" CssClass="row form-group">
            <label for="hrCommentsTxt" style="font-size:1.2em">HR Comments</label>
            <textarea runat="server" class="form-control" id="hrCommentsTxt" rows="4" style="width:45%; margin:0 auto;"></textarea>
        </asp:Panel>

        <%--Apply mode: Shows any necessary validation messages to user --%>
        <div class="row" id="validationRow">
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="invalidSupervisor" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span3" runat="server">Could not verify supervisor</span>
                    </asp:Panel>

                    <asp:Panel ID="startDateIsWeekend" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span4" runat="server">Start date is on the weekend</span>
                    </asp:Panel>

                    <asp:Panel ID="endDateIsWeekend" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span5" runat="server">End date is on the weekend</span>
                    </asp:Panel>

                    <asp:Panel ID="moreThan2DaysConsecutiveSickLeave" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                        <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                        <span id="Span6" runat="server">More than 2 days of consecutive sick leave requires a medical leave of absence</span>
                    </asp:Panel>

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
                        <span id="Span1" runat="server">Sick leave cannot be taken in advance</span>
                    </asp:Panel>
                    <asp:Panel ID="successMsgPanel" runat="server" CssClass="row alert alert-success" Style="display: none;" role="alert">
                        <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                        <span id="successMsg" runat="server">Application successfully submitted</span>
                        <asp:Button ID="submitAnotherLA" runat="server" Text="Submit another" CssClass="btn btn-success" Style="display: inline; margin-left: 10px" OnClick="refreshForm" />
                    </asp:Panel>
                    <asp:Panel ID="submitButtonPanel" runat="server" CssClass="row form-group">
                        <asp:LinkButton ID="cancelBtn" runat="server" Text="Cancel" Style="margin-right: 35px;" CssClass="btn btn-danger" OnClick="refreshForm" CausesValidation="False">
                                 <i class="fa fa-times" aria-hidden="true"></i>
                                 Cancel
                        </asp:LinkButton>
                        <asp:LinkButton ID="submitBtn" runat="server" Text="Submit" CssClass="btn btn-success" OnClick="submitLeaveApplication_Click" ValidationGroup="applyForLeave">
                                 <i class="fa fa-send" aria-hidden="true"></i>
                                 Submit
                        </asp:LinkButton>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <asp:Panel ID="submitCommentsPanel" runat="server" CssClass="row" Style="margin-top: 15px;">
            <asp:LinkButton ID="submitCommentsBtn" runat="server" Text="Submit comment(s)" CssClass="btn btn-success" OnClick="submitCommentsBtn_Click">
                <i class="fa fa-send" aria-hidden="true"></i>
                Submit comments
            </asp:LinkButton>
            <asp:Panel ID="successfulSubmitCommentsMsgPanel" runat="server" CssClass="row alert alert-success" Style="display: inline-block;" role="alert" Visible="false">
                <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                <span id="Span2" runat="server">Comments successfully added to application</span>
                <asp:Button ID="Button1" runat="server" Text="Go back" CssClass="btn btn-primary" Style="margin-left:3px;" OnClick="returnToPreviousBtn_Click"/>
            </asp:Panel>
        </asp:Panel>

    </div>
    </label>
</asp:Content>


