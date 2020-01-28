﻿<%@ Page Title="Employee Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmployeeDetails.aspx.cs" Inherits="HR_LEAVEv2.HR.EmployeeDetails" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #addEmploymentRecordContainer th, td {
            text-align: center;
        }

        .emp-details-validation-msg{
            display: none; 
            margin: 0 5px;
            margin-bottom:25px;
        }

        .new-record{
            background-color: #e0e0eb;
        }
    </style>

    <asp:LinkButton ID="returnToPreviousBtn" runat="server" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="right" title="Return to all employees" OnClick="returnToPreviousBtn_Click">
        <i class="fa fa-arrow-left" aria-hidden="true"></i>
    </asp:LinkButton>

    <%--create mode--%>
    <asp:Panel ID="createModeTitle" runat="server">
        <h1>Create New Employee</h1>
    </asp:Panel>

    <%--edit mode--%>
    <asp:Panel ID="editModeTitle" runat="server">
        <h1>Edit Employee</h1>
    </asp:Panel>

   <%-- employee name--%>
    <asp:Panel ID="empNamePanel" runat="server" Style="text-align:center">
        <h3 id="empNameHeader" runat="server"></h3>
    </asp:Panel>

    <div id="empDetailsContainer" runat="server" class="container-fluid text-center">
        <asp:Panel ID="validationRowPanel" runat="server" CssClass="row text-center" Style="margin-top: 25px;">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <%--NO EDITS MADE---------------------------------------------------------------------------------------------%>
                    <asp:Panel ID="noChangesMadePanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-info">
                            <i class="fa fa-info-circle" aria-hidden="true"></i>
                            <span id="Span18" runat="server">No changes made</span>
                        </span>
                    </asp:Panel>

                    <%--SUCCESSES---------------------------------------------------------------------------------------------%>

                    <%--EDIT EMPLOYEE SUCCESSES-------------------------------------------------------------------------------%>
                    <%--General Successful edit of employee--%>
                    <asp:Panel ID="editFullSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span id="Span9" runat="server">Employee successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee roles--%>
                    <asp:Panel ID="editRolesSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span id="Span5" runat="server">Authorizations successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee leave balances--%>
                     <asp:Panel ID="editLeaveSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span id="Span7" runat="server">Leave balances successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee employment records--%>
                     <asp:Panel ID="editEmpRecordSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span id="Span8" runat="server">Employment record(s) successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--ADD NEW EMPLOYEE SUCCESSES-------------------------------------------------------------------------------%>

                    <%--General Successful insert of new employee--%>
                    <asp:Panel ID="fullFormSubmitSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span id="Span1" runat="server">Employee successfully created</span>
                        </span>
                    </asp:Panel>

                    <%--ERRORS---------------------------------------------------------------------------------------------------%>

                    <%--EDIT EMPLOYEE ERRORS-------------------------------------------------------------------------------%>
                    <%--General Error in editing employee--%>
                    <asp:Panel ID="editEmpErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span6" runat="server">Employee not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee roles--%>
                    <asp:Panel ID="editRolesErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span10" runat="server">Authorizations not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee leave balances--%>
                    <asp:Panel ID="editLeaveBalancesErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span11" runat="server">Leave Balances not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee employment records--%>
                    <asp:Panel ID="editEmpRecordErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span12" runat="server">Employment Record(s) not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in deleting employee employment records--%>
                    <asp:Panel ID="deleteEmpRecordsErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span13" runat="server">Employment Record(s) not deleted</span>
                        </span>
                    </asp:Panel>

                    <%--Error in adding new employee employment records--%>
                    <asp:Panel ID="addEmpRecordsErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span14" runat="server">Employment Record(s) not added</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing end date in employee employment records--%>
                    <asp:Panel ID="editEndDateEmpRecordsPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span16" runat="server">End Date not added</span>
                        </span>
                    </asp:Panel>

                    <%--ADD NEW EMPLOYEE ERRORS-------------------------------------------------------------------------------%>

                    <%--General Error in adding new employee--%>
                    <asp:Panel ID="fullFormErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span2" runat="server">Employee not added</span>
                        </span>
                    </asp:Panel>

                    <%--Duplicate primary key: new employee has same employee id as another in the db--%>
                    <asp:Panel ID="duplicateIdentifierPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span15" runat="server">The entered employee ID already exists in database</span>
                        </span>
                    </asp:Panel>

                    <%--Error in finding email in AD--%>
                    <asp:Panel ID="emailNotFoundErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span3" runat="server">Active Directory email not found</span>
                        </span>
                    </asp:Panel>

                    <%--Error: No employment records --%>
                    <asp:Panel ID="noEmploymentRecordEnteredErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span4" runat="server">At least one Employment record must be entered</span>
                        </span>
                    </asp:Panel>

                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:LinkButton ID="clearFormBtn" runat="server" OnClick="refreshForm" CssClass="btn btn-primary" Style="margin-bottom: 15px;">
                <i class="fa fa-times" aria-hidden="true"></i>
                Clear Form
        </asp:LinkButton>

        <%--Employee Information--%>
        <%--Employee ID, IHRIS ID, AD Email, Authorization Level--%>
        
        <div class="container" style="width: 50%; margin-bottom: 5px;">
            <asp:Panel ID="empBasicInfoPanel" runat="server" CssClass="row text-center" Style="background-color: #e0e0eb; padding-bottom: 15px; margin-bottom: 5px;">
                <h3>IDs and Email</h3>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" 
                        ID="RegularExpressionValidator11" runat="server" 
                        ControlToValidate="employeeIdInput" 
                        ErrorMessage="Please enter valid employee ID" ForeColor="Red" 
                        ValidationExpression="^[0-9]*$" 
                        Display="Dynamic" 
                        Style="float: right;">  
                    </asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="RequiredFieldValidator4" runat="server" ControlToValidate="employeeIdInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="employeeIdInput" style="display: block">Employee ID</label>
                    <asp:TextBox ID="employeeIdInput" runat="server" CssClass="form-control" placeholder="Enter employee ID" Style="display: inline-block"></asp:TextBox>

                </div>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator 
                        ValidationGroup="submitFullFormGroup" 
                        ID="RegularExpressionValidator12" 
                        runat="server" 
                        ControlToValidate="ihrisNumInput" 
                        ErrorMessage="Please enter valid IHRIS ID" 
                        ForeColor="Red" 
                        ValidationExpression="^[0-9]*$" 
                        Display="Dynamic" 
                        Style="float: right;">  
                    </asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="RequiredFieldValidator5" runat="server" ControlToValidate="ihrisNumInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="ihrisNumInput" style="display: block">IHRIS ID</label>
                    <asp:TextBox ID="ihrisNumInput" runat="server" CssClass="form-control" placeholder="Enter IHRIS ID" Style="display: inline-block"></asp:TextBox>
                </div>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator 
                        ID="RegularExpressionValidator13" 
                        runat="server" 
                        ControlToValidate="adEmailInput" 
                        ErrorMessage="Please enter valid email" 
                        ForeColor="Red"
                        ValidationExpression="^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@planning.gov.tt$"
                        Display="Dynamic" Style="float: right;">  
                    </asp:RegularExpressionValidator>

                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="RequiredFieldValidator6" runat="server" ControlToValidate="adEmailInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="adEmailInput" style="display: block">
                        Active Directory Email
                        <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-left: 5px; cursor: pointer;"
                            data-toggle="tooltip"
                            data-placement="right"
                            title="This email is used to get employee's first name and last name from Active Directory"></i>
                    </label>

                    <asp:TextBox ID="adEmailInput" runat="server" CssClass="form-control" AutoCompleteType="Email" placeholder="Enter Active Directory email" Style="display: inline-block"></asp:TextBox>
                </div>
            </asp:Panel>

            <%--Authorization Level--%>
            <asp:Panel ID="authorizationLevelPanel" runat="server">
                <div class="row text-center" style="background-color: #f0f0f5;">
                    <h3>Authorization Level
                    <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-left: 5px; cursor: pointer; font-size: 14px;"
                        data-toggle="tooltip"
                        data-placement="right"
                        title="Click on any checkbox below to grant higher privileges to an employee"></i>
                    </h3>
                    <div class="form-group" id="authLevelDiv">
                        <asp:Panel ID="supervisorCheckDiv" runat="server" CssClass="form-check">
                            <label class="form-check-label" for="supervisorCheck">
                                <asp:CheckBox ID="supervisorCheck" runat="server" CssClass="form-check-input" />
                                <span>Supervisor</span>
                            </label>
                        </asp:Panel>
                        <asp:Panel CssClass="form-check" ID="hr1CheckDiv" runat="server">
                            <label class="form-check-label" for="hr1Check">
                                <asp:CheckBox ID="hr1Check" runat="server" CssClass="form-check-input" />
                                <span>HR Level 1</span>
                            </label>
                        </asp:Panel>
                        <asp:Panel CssClass="form-check" ID="hr2CheckDiv" runat="server">
                            <label class="form-check-label" for="hr2Check">
                                <asp:CheckBox ID="hr2Check" runat="server" CssClass="form-check-input" />
                                <span>HR Level 2</span>
                            </label>
                        </asp:Panel>
                        <asp:Panel CssClass="form-check" ID="hr3CheckDiv" runat="server">
                            <label class="form-check-label" for="hr2Check">
                                <asp:CheckBox ID="hr3Check" runat="server" CssClass="form-check-input" />
                                <span>HR Level 3</span>
                            </label>
                        </asp:Panel>
                    </div>
                </div>
                <div class="row text-center" style="display: none; width: 80%; margin: 0 auto; margin-top: 5px; background-color: #f0f0f5" id="furtherDetailsForHrDiv">
                    <h4 style="padding-top: 25px;">Type of employee dealt with</h4>
                    <div class="form-group">
                        <div class="form-check" id="contractCheckDiv">
                            <label class="form-check-label" for="contractCheck">
                                <asp:CheckBox ID="contractCheck" runat="server" CssClass="form-check-input" />
                                <span>Contract</span>
                            </label>
                        </div>
                        <div class="form-check" id="publicServiceCheckDiv">
                            <label class="form-check-label" for="publicServiceCheck">
                                <asp:CheckBox ID="publicServiceCheck" runat="server" CssClass="form-check-input" />
                                <span>Public Service</span>
                            </label>
                        </div>
                    </div>
                </div>
            </asp:Panel>      
        </div>

        <%--Leave Balances--%>
        <div class="container text-center" style="width: 50%; background-color: #e0e0eb;  margin-bottom: 5px; padding-bottom: 15px;">
            <h3>Leave Balances
                <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-left: 5px; cursor: pointer; font-size: 14px;"
                    data-toggle="tooltip"
                    data-placement="right"
                    title="All empty form fields default to a leave balance of 0"></i>
            </h3>
            <%--Sick Leave--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator7" runat="server" ControlToValidate="sickLeaveInput" ErrorMessage="Please enter valid sick leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="sickLeaveInput">Sick Leave</label>
                <asp:TextBox CssClass="form-control" ID="sickLeaveInput" placeholder="Enter sick leave balance" runat="server"></asp:TextBox>
            </div>

            <%--Vacation--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator5" runat="server" ControlToValidate="vacationLeaveInput" ErrorMessage="Please enter valid vacation leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="vacationLeaveInput">Vacation Leave</label>
                <asp:TextBox CssClass="form-control" ID="vacationLeaveInput" placeholder="Enter vacation leave balance" runat="server"></asp:TextBox>
            </div>

            <%--Personal--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator4" runat="server" ControlToValidate="personalLeaveInput" ErrorMessage="Please enter valid personal leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="personalLeaveInput">Personal Leave</label>
                <asp:TextBox CssClass="form-control" ID="personalLeaveInput" placeholder="Enter personal leave balance" runat="server"></asp:TextBox>
            </div>

            <%--Casual--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator6" runat="server" ControlToValidate="casualLeaveInput" ErrorMessage="Please enter valid casual leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="casualLeaveInput">Casual Leave</label>
                <asp:TextBox CssClass="form-control" ID="casualLeaveInput" placeholder="Enter casual leave balance" runat="server"></asp:TextBox>
            </div>

            <%--Bereavement--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator8" runat="server" ControlToValidate="bereavementLeaveInput" ErrorMessage="Please enter valid bereavement leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="bereavementLeaveInput">Bereavement Leave</label>
                <asp:TextBox CssClass="form-control" ID="bereavementLeaveInput" placeholder="Enter bereavement leave balance" runat="server"></asp:TextBox>
            </div>

            <%--Maternity--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator9" runat="server" ControlToValidate="maternityLeaveInput" ErrorMessage="Please enter valid maternity leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="maternityLeaveInput">Maternity Leave</label>
                <asp:TextBox CssClass="form-control" ID="maternityLeaveInput" placeholder="Enter maternity leave balance" runat="server"></asp:TextBox>
            </div>

            <%--Pre-Retirement--%>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator10" runat="server" ControlToValidate="preRetirementLeaveInput" ErrorMessage="Please enter valid pre-retirement leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="preRetirementLeaveInput">Pre-retirement Leave</label>
                <asp:TextBox CssClass="form-control" ID="preRetirementLeaveInput" placeholder="Enter pre-retirement leave balance" runat="server"></asp:TextBox>
            </div>
        </div>

        <%--Add new Employment Record--%>
        <div id="addEmploymentRecordContainer" class="container text-center" style="background-color: #f0f0f5; padding-bottom: 10px;">
            <h3>Employment Record</h3>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="addEmpRecordForm" Style="display: inline-block; width: 100%">
                        <div class="container" style="width: 80%; height: 95%">
                            <div class="form-group" style="margin-top: 25px;">
                                <label for="empTypeList">Employment Type</label>
                                <asp:DropDownList ID="empTypeList" runat="server" CssClass="form-control" Width="225px" AutoPostBack="true" DataSourceID="SqlDataSource3" DataTextField="type_id" Style="display: inline-block;"></asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [type_id] FROM [employmenttype] ORDER BY [type_id]"></asp:SqlDataSource>
                            </div>
                            <div class="form-group" style="margin-top: 45px;">
                                <label for="deptList">Department</label>
                                <asp:DropDownList ID="deptList" runat="server" CssClass="form-control" Width="225px" DataSourceID="SqlDataSource2" DataValueField="dept_id" DataTextField="dept_name" Style="display: inline-block; margin-right: 15%;"></asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [dept_id], [dept_name] FROM [department] ORDER BY [dept_name]"></asp:SqlDataSource>

                                <label for="positionList">Position</label>
                                <asp:DropDownList ID="positionList" runat="server" CssClass="form-control" Width="225px" DataSourceID="SqlDataSource1" DataValueField="pos_id" DataTextField="pos_name" Style="display: inline-block"></asp:DropDownList>
                                <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [pos_id], [pos_name] FROM [position] ORDER BY [pos_name]"></asp:SqlDataSource>
                            </div>
                            <div class="form-group text-center" style="margin-top: 45px;">
                                <span style="margin-right: 15%;">
                                    <label for="txtStartDate">Start date</label>
                                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="startDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtStartDate" PopupButtonID="startDateCalendar" runat="server" Format="MM/dd/yyyy"></ajaxToolkit:CalendarExtender>
                                    <asp:RequiredFieldValidator ValidationGroup="empRecord" ID="startDateRequiredValidator" runat="server" ControlToValidate="txtStartDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                                </span>

                                <span>
                                    <label for="txtEndDate">
                                        Expected end date
                                    </label>
                                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="endDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtEndDate" PopupButtonID="endDateCalendar" runat="server" Format="MM/dd/yyyy"></ajaxToolkit:CalendarExtender>
                                    <asp:RequiredFieldValidator 
                                        ValidationGroup="empRecord" 
                                        ID="endDateRequiredValidator" 
                                        runat="server" 
                                        ControlToValidate="txtEndDate" 
                                        Display="Dynamic" 
                                        ErrorMessage="Required"
                                        ForeColor="Red">

                                    </asp:RequiredFieldValidator>
                                </span>
                            </div>

                            <div id="validationDiv" style="margin-top: 25px;">
                                <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
                                </asp:Panel>
                                <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="invalidEndDateValidationMsg" runat="server">End date is not valid</span>
                                </asp:Panel>
                                <asp:Panel ID="dateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="dateComparisonValidationMsg" runat="server">End date cannot precede start date</span>
                                </asp:Panel>
                                <asp:Panel ID="duplicateRecordPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="Span17" runat="server">Cannot add duplicate record</span>
                                </asp:Panel>
                            </div>
                        </div>
                        <div class="text-center" style="margin-top: 35px; margin-bottom: 45px;">
                            <asp:LinkButton runat="server" ID="cancelNewRecordBtn" CssClass="btn btn-danger" Text="Cancel" CausesValidation="false" Style="margin-right: 35px;" OnClick="cancelNewRecordBtn_Click">
                                 <i class="fa fa-times" aria-hidden="true"></i>
                                 Cancel
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" ID="addNewRecordBtn" CssClass="btn btn-primary" Text="Add new Record" OnClick="addNewRecordBtn_Click" ValidationGroup="empRecord">
                                 <i class="fa fa-plus" aria-hidden="true"></i>
                                 Add new record
                            </asp:LinkButton>
                        </div>

                    </asp:Panel>
                    <asp:GridView ID="GridView1" 
                        runat="server" 
                        BorderStyle="None" CssClass="table" Style="margin: 0 auto;" 
                        GridLines="Horizontal" 
                        OnRowDataBound="GridView1_RowDataBound" OnDataBound="GridView1_DataBound" OnRowDeleting="GridView1_RowDeleting" OnRowCommand="GridView1_RowCommand"
                        AutoGenerateColumns="false" 
                        AutoGenerateDeleteButton="false" 
                        AllowSorting="true" AllowPaging="true" 
                        PageSize="5" OnPageIndexChanging="GridView1_PageIndexChanging">
                        <Columns>
                            <asp:TemplateField HeaderText="Action" Visible="true">
                                <ItemTemplate>
                                    <%--delete button--%>
                                    <asp:LinkButton ID="deleteBtn"
                                        runat="server"
                                        CssClass="btn btn-danger content-tooltipped"
                                        data-toggle="tooltip"
                                        data-placement="bottom"
                                        title="Delete employment record"
                                        OnClientClick="return confirm('Delete record?');"
                                        CommandName="delete"
                                        CausesValidation="false">
                                        <i class="fa fa-trash-o" aria-hidden="true"></i>
                                    </asp:LinkButton>

                                    <%--end emp record button--%>
                                    <asp:LinkButton ID="endEmpRecordBtn"
                                        runat="server"
                                        CssClass="btn btn-primary content-tooltipped"
                                        data-toggle="tooltip"
                                        data-placement="bottom"
                                        title="End employment record"
                                        CausesValidation="false" 
                                        CommandName="endEmpRecord"
                                        CommandArgument =" <%# ((GridViewRow) Container).RowIndex %>"
                                        >
                                        <i class="fa fa fa-ban" aria-hidden="true"></i>
                                    </asp:LinkButton>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <%--Index 0: Employment record ID--%>
                            <asp:BoundField HeaderText="Employment Record ID" DataField="record_id" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" />

                            <%--Index 1: Employment Type--%>
                            <asp:BoundField HeaderText="Employment Type" DataField="employment_type" />

                            <%--Index 2: Department ID--%>
                            <asp:BoundField HeaderText="Department ID" DataField="dept_id" Visible="false" />

                            <%--Index 3: Department--%>
                            <asp:BoundField HeaderText="Department" DataField="dept_name" />

                            <%--Index 4: Position ID--%>
                            <asp:BoundField HeaderText="Position ID" DataField="pos_id" Visible="false" />

                            <%--Index 5: Position--%>
                            <asp:BoundField HeaderText="Position" DataField="pos_name" />

                            <%--Index 6: Start--%>
                            <asp:BoundField HeaderText="Start Date" DataField="start_date" DataFormatString="{0:MM/dd/yyyy}" />

                            <%--Index 7: Expected End Date--%>
                            <asp:BoundField HeaderText="Expected End Date" DataField="expected_end_date" DataFormatString="{0:MM/dd/yyyy}"/>

                            <%--Index 8: isDeleted--%>
                            <asp:BoundField HeaderText="isChanged" DataField="isChanged" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" />

                            <%--Index 9: actual_end_date--%>
                            <asp:BoundField HeaderText="Actual End Date" DataField="actual_end_date"/>

                        </Columns>
                    </asp:GridView>

                    <asp:Panel ID="addEmpRecordBtn" runat="server" CssClass="btn-group" role="group" Style="margin-top: 10px;">
                        <asp:LinkButton runat="server" ID="showFormBtn" class="btn btn-primary" Text="Add" OnClick="showFormBtn_Click" CausesValidation="false">
                                 <i class="fa fa-plus" aria-hidden="true"></i>
                                 Add
                        </asp:LinkButton>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>

        <asp:Panel ID="submitFullFormPanel" runat="server" CssCclass="row text-center" Style="margin-top: 50px;">
            <asp:LinkButton type="submit" ID="submitBtn" CssClass="btn btn-success" runat="server" OnClick="submitBtn_Click" ValidationGroup="submitFullFormGroup">
                <i class="fa fa-send" aria-hidden="true"></i>
                Submit new employee
            </asp:LinkButton>
        </asp:Panel>

        <asp:Panel ID="editFormPanel" runat="server" CssCclass="row text-center" Style="margin-top: 50px;">
            <asp:LinkButton ID="editBtn" CssClass="btn btn-success" runat="server" ValidationGroup="submitFullFormGroup" OnClick="editBtn_Click">
                <i class="fa fa-pencil" aria-hidden="true"></i>
                Edit employee
            </asp:LinkButton>
        </asp:Panel>
    </div>

     <%-- End Employment Record Modal--%>
    <div class="modal fade" id="cancelEmpRecordModal" tabindex="-1" role="dialog" aria-labelledby="cancelEmpRecordTitle" aria-hidden="true">

        <div class="modal-dialog" role="document" style="width: 50%;">
            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                <ContentTemplate>
                    <div class="modal-content">
                        <div class="modal-header text-center">
                            <h2 class="modal-title" id="cancelEmpRecordTitle" style="display: inline; width: 150px;">End Employment Record
                            </h2>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body text-center">
                            <div class="row form-group">

                                <%--End Date--%>
                                <div>
                                    <label for="txtEmpRecordEndDate">Enter end date:</label>
                                    <asp:TextBox ID="txtEmpRecordEndDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="empRecordEndDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="fromCalendarExtender" TargetControlID="txtEmpRecordEndDate" PopupButtonID="empRecordEndDateCalendar" runat="server" Format="MM/dd/yyyy"></ajaxToolkit:CalendarExtender>
                                </div>
                            </div>

                            <asp:Panel ID="invalidEndDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>End date is not valid</span>
                            </asp:Panel>
                            <asp:Panel ID="emptyEndDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>No end date is entered</span>
                            </asp:Panel>
                            <asp:Panel ID="endDateBeforeStartDatePanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>End date cannot be before start date</span>
                            </asp:Panel>

                        </div>
                        <div class="modal-footer">
                            <asp:LinkButton runat="server" ID="submitEndEmpRecordBtn" class="btn btn-primary" OnClick="submitEndEmpRecordBtn_Click" CausesValidation="false">
                                 <i class="fa fa-send" aria-hidden="true"></i>
                                 Submit
                            </asp:LinkButton>
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                        </div>
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>

        </div>
    </div>

    

    <script>
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {
            // show section to choose whether new HR 2,3 employee is dealing with contract or public service 
            if ($('#<%= hr2CheckDiv.ClientID %> input[type="checkbox"], #<%= hr3CheckDiv.ClientID %> input[type="checkbox"]').is(':checked')) {
                $('#furtherDetailsForHrDiv').css("display", "block");
            }
            else {
                $('#furtherDetailsForHrDiv').css("display", "none");
                $('#furtherDetailsForHrDiv input[type="checkbox"]').prop('checked', false);
            }

            $('#authLevelDiv input[type="checkbox"]').click(function (e) {
                // show section to choose whether new HR 2,3 employee is dealing with contract or public service 
                if ($('#<%= hr2CheckDiv.ClientID %> input[type="checkbox"], #<%= hr3CheckDiv.ClientID %> input[type="checkbox"]').is(':checked')) {
                    $('#furtherDetailsForHrDiv').css("display", "block");
                }
                else {
                    $('#furtherDetailsForHrDiv').css("display", "none");
                    $('#furtherDetailsForHrDiv input[type="checkbox"]').prop('checked', false);
                }
            });
        });

    </script>
</asp:Content>
