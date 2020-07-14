<%@ Page Title="Employee Details" MaintainScrollPositionOnPostback="true" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmployeeDetails.aspx.cs" Inherits="HR_LEAVEv2.HR.EmployeeDetails" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #addEmploymentRecordContainer th, td {
            text-align: center;
        }

        #addEmploymentRecordContainer th{
            border-top: 1px solid black;
            border-bottom: 1px solid black;
        }

        #addEmploymentRecordContainer tr, td{
            border:none;
        }

        .emp-details-validation-msg{
            display: none; 
            margin: 0 5px;
            margin-bottom:25px;
        }

        .highlighted-record{
            background-color: #e0e0eb;
        }

        #vacationAmtsTable td{
            padding-bottom:25px;
        }

        .switch
        {
            position: relative;
            display: inline-block;
            width: 50px;
            height: 24px;
        }
         
        .switch input
        {
            opacity: 0;
        }
         
        .slider
        {
            position: absolute;
            cursor: pointer;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: #ccc;
            -webkit-transition: .4s;
            transition: .4s;
        }
         
        .slider:before
        {
            position: absolute;
            content: "";
            height: 16px;
            width: 16px;
            left: 4px;
            bottom: 4px;
            background-color: white;
            -webkit-transition: .4s;
            transition: .4s;
        }
         
        input:checked + .slider
        {
            background-color: #2196F3;
        }
         
        input:focus + .slider
        {
            box-shadow: 0 0 1px #2196F3;
        }
         
        input:checked + .slider:before
        {
            -webkit-transform: translateX(26px);
            -ms-transform: translateX(26px);
            transform: translateX(26px);
        }
         
        /* Rounded sliders */
        .slider.round
        {
            border-radius: 34px;
        }
         
        .slider.round:before
        {
            border-radius: 50%;
        }

        table.subOrActingList input {
            width: 20px;
            display: block;
            float: left;
        }
        table.subOrActingList label {
            width: 80px;
            display: block;
            float: left;
            text-align:left;
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
            <asp:UpdatePanel ID="validationMessagesPanel" runat="server">
                <ContentTemplate>
                    <%--NO EDITS MADE---------------------------------------------------------------------------------------------%>
                    <asp:Panel ID="noChangesMadePanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-info">
                            <i class="fa fa-info-circle" aria-hidden="true"></i>
                            <span>No changes made</span>
                        </span>
                    </asp:Panel>

                    <%--No file uploaded but accumulate past max status changed--%>
                    <asp:Panel ID="noChangesMadeToAccStatus" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-info">
                            <i class="fa fa-info-circle" aria-hidden="true"></i>
                            <span>No Files uploaded. Cannot allow employee to accumulate leave past max without letter from PS</span>
                        </span>
                    </asp:Panel>

                    <%--SUCCESSES---------------------------------------------------------------------------------------------%>

                    <%--EDIT EMPLOYEE SUCCESSES-------------------------------------------------------------------------------%>
                    <%--General Successful edit of employee--%>
                    <asp:Panel ID="editFullSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Employee successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee roles--%>
                    <asp:Panel ID="editRolesSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Authorizations successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee leave balances--%>
                     <asp:Panel ID="editLeaveSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Leave balances successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee employment records--%>
                     <asp:Panel ID="editEmpRecordSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Employment record(s) successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of employee files--%>
                    <asp:Panel ID="editEmpFilesPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Employment file(s) successfully uploaded</span>
                        </span>
                    </asp:Panel>

                    <%--Successful edit of accumulate past max--%>
                    <asp:Panel ID="editAccumulatePastMaxSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Accumulate past vacation leave limit status successfully edited</span>
                        </span>
                    </asp:Panel>

                    <%--ADD NEW EMPLOYEE SUCCESSES-------------------------------------------------------------------------------%>

                    <%--General Successful insert of new employee--%>
                    <asp:Panel ID="fullFormSubmitSuccessPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span>Employee successfully created</span>
                        </span>
                    </asp:Panel>

                    <%--ERRORS---------------------------------------------------------------------------------------------------%>

                    <%--EDIT EMPLOYEE ERRORS-------------------------------------------------------------------------------%>
                    <%--General Error in editing employee--%>
                    <asp:Panel ID="editEmpErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Employee not edited</span>
                        </span>
                    </asp:Panel>

                    <asp:Panel ID="editAccumulatePastMaxErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Accumulate past vacation leave limit status not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee files--%>
                    <asp:Panel ID="editEmpFilesErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Files not uploaded</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee roles--%>
                    <asp:Panel ID="editRolesErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Authorizations not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee leave balances--%>
                    <asp:Panel ID="editLeaveBalancesErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Leave Balances not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing employee employment records--%>
                    <asp:Panel ID="editEmpRecordErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Employment Record(s) not edited</span>
                        </span>
                    </asp:Panel>

                    <%--Error in deleting employee employment records--%>
                    <asp:Panel ID="deleteEmpRecordsErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Employment Record(s) not deleted</span>
                        </span>
                    </asp:Panel>

                    <%--Error in adding new employee employment records--%>
                    <asp:Panel ID="addEmpRecordsErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Employment Record(s) not added</span>
                        </span>
                    </asp:Panel>

                    <%--Error in editing end date in employee employment records--%>
                    <asp:Panel ID="editEndDateEmpRecordsPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>End Date not added</span>
                        </span>
                    </asp:Panel>

                    <%--ADD NEW EMPLOYEE ERRORS-------------------------------------------------------------------------------%>

                    <%--General Error in adding new employee--%>
                    <asp:Panel ID="fullFormErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Employee not added</span>
                        </span>
                    </asp:Panel>

                    <%--Duplicate primary key: new employee has same employee id as another in the db--%>
                    <asp:Panel ID="duplicateIdentifierPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>The entered employee ID already exists in database</span>
                        </span>
                    </asp:Panel>

                    <%--Error in finding email in AD--%>
                    <asp:Panel ID="emailNotFoundErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>Active Directory email not found</span>
                        </span>
                    </asp:Panel>

                    <%--Error: No employment records --%>
                    <asp:Panel ID="noEmploymentRecordEnteredErrorPanel" runat="server" CssClass="emp-details-validation-msg" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span>At least one Employment record must be entered</span>
                        </span>
                    </asp:Panel>

                </ContentTemplate>
            </asp:UpdatePanel>
        </asp:Panel>

        <asp:LinkButton ID="clearFormBtn" runat="server" OnClick="refreshForm" CssClass="btn btn-primary" Style="margin-bottom: 15px; outline:none; border:none;" CausesValidation="false">
                <i class="fa fa-refresh" aria-hidden="true"></i>
                <span runat="server" id ="clearFormTxt">Clear Form</span>
        </asp:LinkButton>

        <%--Employee Information--%>
        <%--Employee ID, IHRIS ID, AD Email, Authorization Level--%>
        
        <div class="container" style="width: 50%; margin-bottom: 5px;">
            <asp:Panel ID="empBasicInfoPanel" runat="server" CssClass="row text-center" Style="background-color: #e0e0eb; padding-bottom: 15px; margin-bottom: 5px;">
                <h3>IDs and Email</h3>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" 
                        ID="empIdRegExpValidator" runat="server" 
                        ControlToValidate="employeeIdInput" 
                        ErrorMessage="Please enter valid employee ID" ForeColor="Red" 
                        ValidationExpression="^[0-9]*$" 
                        Display="Dynamic" 
                        Style="float: right;">  
                    </asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="empIdReqFieldValidator" runat="server" ControlToValidate="employeeIdInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="employeeIdInput" style="display: block">Employee ID</label>
                    <asp:TextBox ID="employeeIdInput" runat="server" CssClass="form-control" placeholder="Enter employee ID" Style="display: inline-block"></asp:TextBox>

                </div>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator 
                        ValidationGroup="submitFullFormGroup" 
                        ID="ihrisIDRegExpValidator" 
                        runat="server" 
                        ControlToValidate="ihrisNumInput" 
                        ErrorMessage="Please enter valid IHRIS ID" 
                        ForeColor="Red" 
                        ValidationExpression="^[0-9]*$" 
                        Display="Dynamic" 
                        Style="float: right;">  
                    </asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="ihrisIdReqFieldValidator" runat="server" ControlToValidate="ihrisNumInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="ihrisNumInput" style="display: block">IHRIS ID</label>
                    <asp:TextBox ID="ihrisNumInput" runat="server" CssClass="form-control" placeholder="Enter IHRIS ID" Style="display: inline-block"></asp:TextBox>
                </div>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator 
                        ID="emailRegExpValidator" 
                        runat="server" 
                        ControlToValidate="adEmailInput" 
                        ErrorMessage="Please enter valid email" 
                        ForeColor="Red"
                        ValidationExpression="^[a-zA-Z0-9.!#$%&'*+/=?^_`{|}~-]+@planning.gov.tt$"
                        Display="Dynamic" Style="float: right;">  
                    </asp:RegularExpressionValidator>

                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="emailReqFieldValidator" runat="server" ControlToValidate="adEmailInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
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
        <div class="container text-center" style="width: 50%; background-color: #e0e0eb; margin-bottom: 5px; padding-bottom: 15px;">
            <h3>Leave Balances
                <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-left: 5px; cursor: pointer;font-size: 14px;"
                    data-toggle="tooltip"
                    data-placement="right"
                    title="All empty form fields default to a leave balance of 0"></i>
            </h3>
            <asp:ListView ID="leaveBalancesListView" runat="server" GroupItemCount="10" Style="text-align: left;">
                <EmptyDataTemplate>
                    <div class="alert alert-info text-center" role="alert" style="display: inline-block; margin: 0 auto">
                        <i class="fa fa-info-circle"></i>
                        No Leave Types available
                    </div>
                </EmptyDataTemplate>

                <EmptyItemTemplate>
                    </td>
                </EmptyItemTemplate>

                <GroupTemplate>
                    <div id="itemPlaceholderContainer" runat="server">
                        <div id="itemPlaceholder" runat="server"></div>
                    </div>
                </GroupTemplate>

                <ItemTemplate>
                    <div class="form-group text-left">
                        <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="leaveBalanceRegExpValidator" runat="server" ControlToValidate="LeaveInput" ErrorMessage='<%#Eval("validation") %>' ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                        </asp:RegularExpressionValidator>
                        <label for="LeaveInput"><%#Eval("leave_type") %> Leave</label>
                        <asp:TextBox CssClass="form-control" ID="LeaveInput" placeholder='<%#Eval("placeholder") %>' runat="server"></asp:TextBox>
                        <asp:HiddenField ID="leaveTypeHiddenField" runat="server" Value='<%#Eval("leave_type") %>' />
                    </div>
                </ItemTemplate>

                <LayoutTemplate>
                    <div id="groupPlaceholderContainer" runat="server">
                        <div id="groupPlaceholder" runat="server"></div>
                    </div>
                </LayoutTemplate>

            </asp:ListView>
        </div>
       
        <%--Employment Record form--%>
        <div id="addEmploymentRecordContainer" class="text-center" style="background-color: #f0f0f5; padding-bottom: 10px; padding-top:5px; padding-right:10px; padding-left:10px; width:100%; margin: 0 auto; margin-bottom: 5px;">
            <h3>Employment Record</h3>
            <asp:UpdatePanel ID="empRecordUpdatePanel" runat="server">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="addEmpRecordForm">
                        <div>
                            <div class="form-group" style="margin-top: 25px;">
                                <label for="empTypeList">Employment Type</label>
                                <asp:DropDownList ID="empTypeList" runat="server" CssClass="form-control" Width="225px" AutoPostBack="true" DataSourceID="employmentTypeDataSource" DataTextField="type_id" Style="display: inline-block;"></asp:DropDownList>
                                <asp:SqlDataSource ID="employmentTypeDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [type_id] FROM [employmenttype] ORDER BY [type_id]"></asp:SqlDataSource>
                            </div>
                            <div class="form-group" style="margin-top: 35px;">
                                <label for="deptList">Department</label>
                                <asp:DropDownList ID="deptList" runat="server" CssClass="form-control" Width="225px" DataSourceID="deptDataSource" DataValueField="dept_id" DataTextField="dept_name" Style="display: inline-block; margin-right: 7%;"></asp:DropDownList>
                                <asp:SqlDataSource ID="deptDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [dept_id], [dept_name] FROM [department] ORDER BY [dept_name]"></asp:SqlDataSource>

                                <label for="positionList">Position</label>
                                <asp:DropDownList ID="positionList" runat="server" CssClass="form-control" Width="225px" DataSourceID="positionDataSource" DataValueField="pos_id" DataTextField="pos_name" Style="display: inline-block"></asp:DropDownList>
                                <asp:SqlDataSource ID="positionDataSource" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [pos_id], [pos_name] FROM [position] ORDER BY [pos_name]"></asp:SqlDataSource>
                            </div>
                            <div class="form-group text-center" style="margin-top: 35px; display:flex; justify-content:space-around; flex-wrap:wrap;">
                                <span>
                                    <label for="txtStartDate">Start date</label>
                                    <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="startDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="startDateCalendarExtender" TargetControlID="txtStartDate" PopupButtonID="startDateCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
                                    <asp:RequiredFieldValidator ValidationGroup="empRecord" ID="startDateRequiredValidator" runat="server" ControlToValidate="txtStartDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                                </span>

                                <span>
                                    <label for="txtEndDate">
                                        Expected end date
                                    </label>
                                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="endDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="expectedEndDateCalendarExtender" TargetControlID="txtEndDate" PopupButtonID="endDateCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
                                    <asp:RequiredFieldValidator 
                                        ValidationGroup="empRecord" 
                                        ID="endDateRequiredValidator" 
                                        runat="server" 
                                        ControlToValidate="txtEndDate" 
                                        Display="Dynamic" 
                                        ErrorMessage="Required"
                                        Style="margin-right:2px;"
                                        ForeColor="Red">

                                    </asp:RequiredFieldValidator>
                                </span>
                                <span runat="server" id="actualEndDateSpan" style="display:none;">
                                    <label for="txtActualEndDate">
                                        Actual end date
                                    </label>
                                    <asp:TextBox ID="txtActualEndDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="actualEndDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="actualEndDateCalendarExtender" TargetControlID="txtActualEndDate" PopupButtonID="actualEndDateCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
                                </span>
                            </div>

                            <div style="margin-top: 25px; display:flex; justify-content:center;">
                                <table id="vacationAmtsTable">
                                    <tr>
                                        <td style="text-align:right;">
                                            <label for="annualAmtOfLeaveTxt">Annual amount of vacation leave</label>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="annualAmtOfLeaveTxt" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:RequiredFieldValidator
                                                ValidationGroup="empRecord"
                                                ID="annualAmtOfLeaveReqValidator"
                                                runat="server"
                                                ControlToValidate="annualAmtOfLeaveTxt"
                                                Display="Dynamic"
                                                ErrorMessage="Required"
                                                ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                        </td>
                                        <td>
                                            <asp:RegularExpressionValidator ID="annualAmtOfLeaveRegExpValidator" runat="server" ValidationGroup="empRecord" ControlToValidate="annualAmtOfLeaveTxt" ErrorMessage="Input must be numerical" ValidationExpression="^\d+$" ForeColor="Red">
                                            </asp:RegularExpressionValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td style="text-align:right;">
                                            <label for="maxAmtOfLeaveTxt">Maximum accumulated vacation leave</label>
                                        </td>
                                        <td>
                                            <asp:TextBox ID="maxAmtOfLeaveTxt" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                        </td>
                                        <td>
                                            <asp:RequiredFieldValidator
                                                ValidationGroup="empRecord"
                                                ID="maxAmtOfLeaveReqValidator"
                                                runat="server"
                                                ControlToValidate="maxAmtOfLeaveTxt"
                                                Display="Dynamic"
                                                ErrorMessage="Required"
                                                ForeColor="Red">
                                            </asp:RequiredFieldValidator>
                                        </td>
                                        <td>
                                             <asp:RegularExpressionValidator ID="maxAmtOfLeaveRegExpValidator" runat="server" ValidationGroup="empRecord" ControlToValidate="maxAmtOfLeaveTxt" ErrorMessage="Input must be numerical" ValidationExpression="^\d+$" ForeColor="Red">
                                            </asp:RegularExpressionValidator>
                                        </td>
                                    </tr>
                                </table>
                            </div>

                            <div style="display:flex; justify-content: center; margin-top:10px;">
                                <asp:RadioButtonList ID="subsOrActingRadioBtnList" runat="server" CssClass="subOrActingList" RepeatLayout="Table" RepeatDirection="Vertical">

                                    <asp:ListItem Selected="True">Substantive</asp:ListItem>
                                    <asp:ListItem>Acting</asp:ListItem>
                                </asp:RadioButtonList>
                            </div>

                            <div id="validationDiv">
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~INVALID ANNUAL OR MAX VACATION LEAVE AMT MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                <asp:Panel ID="invalidAnnualOrMaximumVacationLeaveAmtPanel" runat="server" CssClass="row alert alert-warning" Style="display: none; margin: 0px 5px;" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Annual amount of vacation leave cannot be more than maximum accumulated vacation leave</span>
                                </asp:Panel>
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~INVALID ANNUAL OR MAX VACATION LEAVE AMT MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>


                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~DATE VALIDATION MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
                                </asp:Panel>

                                <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="invalidEndDateValidationMsg" runat="server">Expected end date is not valid</span>
                                </asp:Panel>

                                <asp:Panel ID="dateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span id="dateComparisonValidationMsg" runat="server">Expected end date cannot precede start date</span>
                                </asp:Panel>

                                <asp:Panel ID="startDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Start date is on the weekend</span>
                                </asp:Panel>

                                <asp:Panel ID="endDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Expected end date is on the weekend</span>
                                </asp:Panel>

                                <asp:Panel ID="recordEditEndDateBeforeStartDate" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Actual end date cannot be before start date</span>
                                </asp:Panel>

                                <asp:Panel ID="recordEditEndDateInvalidPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Actual end date is not valid</span>
                                </asp:Panel>

                                <asp:Panel ID="recordEditEndDateOnWeekend" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Actual end date is on the weekend</span>
                                </asp:Panel>
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~DATE VALIDATION MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>


                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EMPLOYMENT RECORD VALIDATION MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                              
                                <asp:Panel ID="startDateClashEditRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not edited since the current employment record's start date falls within the time period of another employment record
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="employmentRecordClashPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not edited since the edit would result in a clash between employment records. This means that the edit causes more than one employment
                                        record to overlap
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="startDateClashAddRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not added since the current employment record's period would result in an overlap with another employment record. Employment records must be non
                                        intersecting
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="multipleActiveRecordsAddRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not added since this would result in two employment records of the same type being marked active simultaneously. Edit employment records accordingly 
                                        to ensure only one active record
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="multipleActiveRecordsEditRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not edited since this would result in two employment records of the same type being marked active simultaneously. Edit employment records accordingly 
                                        to ensure only one active record
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="noSubstantiveRecordAddRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not added since employee must have at least one (1) active substantive employment record in order to add an acting record
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="noSubstantiveRecordEditRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not edited since employee must have at least one (1) active substantive employment record
                                    </span>
                                </asp:Panel>
                                
                                <asp:Panel ID="actingStartDateBeforeSubAdd" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not added since the start date of the acting record is before the start date of the active substantive record
                                    </span>
                                </asp:Panel>

                                <asp:Panel ID="actingStartDateBeforeSubEdit" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>
                                        Record not edited since this would result in the substantive record's start date being later than the acting record's start date
                                    </span>
                                </asp:Panel>
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EMPLOYMENT RECORD VALIDATION MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                

                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~FAILURE MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                <asp:Panel ID="editUnsuccessful" runat="server" CssClass="row alert alert-danger validation-msg" role="alert">
                                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                    <span>Edits could not be made</span>
                                </asp:Panel>
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~FAILURE MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>


                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SUCCESS MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                <asp:Panel ID="editEmploymentRecordSuccessful" runat="server" CssClass="row alert alert-success validation-msg" role="alert">
                                    <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                                    <span id="editEmpRecordSuccTxt" runat="server"></span>
                                </asp:Panel>
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~SUCCESS MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>


                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NO EDITS MADE MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                <asp:Panel ID="noEditsToRecordsMade" runat="server" CssClass="row alert alert-info validation-msg" role="alert">
                                    <i class="fa fa-info-circle" aria-hidden="true"></i>
                                    <span>No Edits made</span>
                                </asp:Panel>
                                <%--~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~NO EDITS MADE MESSAGES~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~--%>
                                
                            </div>
                        </div>
                        <div class="text-center" style="margin-top: 10px; margin-bottom: 45px;">
                            <asp:LinkButton runat="server" ID="cancelNewRecordBtn" CssClass="btn btn-danger" Text="Cancel" CausesValidation="false" Style="margin-right: 35px;" OnClick="cancelNewRecordBtn_Click">
                                 <i class="fa fa-times" aria-hidden="true"></i>
                                 Cancel
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" ID="addNewRecordBtn" CssClass="btn btn-primary" Text="Add new Record" OnClick="addNewRecordBtn_Click" ValidationGroup="empRecord">
                                 <i class="fa fa-plus" aria-hidden="true"></i>
                                 Add new record
                            </asp:LinkButton>
                            <asp:LinkButton runat="server" ID="editRecordBtn" CssClass="btn btn-primary" Text="Edit new Record" OnClick="editRecordBtn_Click" ValidationGroup="empRecord" Visible="false">
                                 <i class="fa fa-floppy-o" aria-hidden="true"></i>
                                 Save record
                            </asp:LinkButton>
                        </div>

                    </asp:Panel>
                    <asp:GridView ID="empRecordGridView" 
                        runat="server" 
                        BorderStyle="None" CssClass="table" 
                        GridLines="Horizontal" 
                        OnRowDataBound="empRecordGridView_RowDataBound" OnDataBound="empRecordGridView_DataBound" OnRowDeleting="empRecordGridView_RowDeleting" OnRowCommand="empRecordGridView_RowCommand"
                        AutoGenerateColumns="false" 
                        AutoGenerateDeleteButton="false" 
                        AllowSorting="true" AllowPaging="true" 
                        PageSize="5" OnPageIndexChanging="empRecordGridView_PageIndexChanging">
                        <Columns>
                            <asp:TemplateField HeaderText="Action" Visible="true">
                                <ItemTemplate>
                                    <%--edit emp record button--%>
                                    <asp:LinkButton ID="editEmpRecordActionBtn"
                                        runat="server"
                                        CssClass="btn btn-warning content-tooltipped"
                                        data-toggle="tooltip"
                                        data-placement="bottom"
                                        title="Edit employment record"
                                        CausesValidation="false" 
                                        CommandName="editEmpRecord"
                                        CommandArgument =" <%# ((GridViewRow) Container).RowIndex %>"
                                        >
                                        <i class="fa fa fa-pencil" aria-hidden="true"></i>
                                    </asp:LinkButton>

                                    <%--delete button--%>
                                    <asp:LinkButton ID="deleteEmpRecordActionBtn"
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
                                    <asp:LinkButton ID="endEmpRecordActionBtn"
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
                                <ItemStyle Width="140px" />
                                <HeaderStyle  Width="140px" />
                            </asp:TemplateField>

                            <%--Employment record ID--%>
                            <asp:BoundField HeaderText="Employment Record ID" DataField="record_id" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden" />

                            <%--Employment Type--%>
                            <asp:BoundField HeaderText="Employment Type" DataField="employment_type" />

                            <%--Department ID--%>
                            <asp:BoundField HeaderText="Department ID" DataField="dept_id" Visible="false" />

                            <%--Department--%>
                            <asp:BoundField HeaderText="Department" DataField="dept_name" />

                            <%--Position ID--%>
                            <asp:BoundField HeaderText="Position ID" DataField="pos_id" Visible="false" />

                            <%--Position--%>
                            <asp:BoundField HeaderText="Position" DataField="pos_name" />

                            <%--Start--%>
                            <asp:BoundField HeaderText="Start Date" DataField="start_date" DataFormatString="{0:d/MM/yyyy}" />

                            <%--Expected End Date--%>
                            <asp:BoundField HeaderText="Expected End Date" DataField="expected_end_date" DataFormatString="{0:d/MM/yyyy}"/>

                            <%--isChanged--%>
                            <asp:BoundField HeaderText="isChanged" DataField="isChanged" HeaderStyle-CssClass="hidden" ItemStyle-CssClass="hidden"/>

                            <%--Actual End Date--%>
                            <asp:BoundField HeaderText="Actual End Date" DataField="actual_end_date" />   
                            
                             
                            <%--Annual Vacation Amt--%>
                            <asp:BoundField HeaderText="Annual Vacation" DataField="annual_vacation_amt" />

                            <%--Max Vacation Accumulation--%>
                            <asp:BoundField HeaderText="Max Vacation" DataField="max_vacation_accumulation" />

                            <%--Is substantive or Acting--%>
                            <%--<asp:BoundField HeaderText="Type" DataField="is_substantive_or_acting" Visible="false"/>--%>

                            <%--Status--%>
                            <asp:TemplateField HeaderText="Status">
                                <ItemTemplate>
                                    <span id="status-label" class="label <%# Eval("status_class") %>"><%# Eval("status") %></span>
                                    <span id="type-label" class="label label-primary"><%# Eval("is_substantive_or_acting") %></span>
                                    <asp:Label ID="new_record_label" runat="server" CssClass="label label-primary" Text="New" Visible ="false"></asp:Label>
                                    <asp:Label ID="edited_record_label" runat="server" CssClass="label label-warning" Text="Edited" Visible ="false"></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>

                    <asp:Panel ID="addEmpRecordBtn" runat="server" CssClass="btn-group" role="group">
                        <asp:LinkButton runat="server" ID="showFormBtn" class="btn btn-primary" Text="Add" OnClick="showFormBtn_Click" CausesValidation="false">
                                 <i class="fa fa-plus" aria-hidden="true"></i>
                                 Add
                        </asp:LinkButton>
                    </asp:Panel>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
    </div>

    <asp:Panel ID="accPastLimitContainerPanel" runat="server" Visible="false" class="container text-center" style="width: 65%; background-color: #e0e0eb;padding-bottom: 15px;">
        <h3 style="margin-bottom:15px;">Allow Accumulation Past Max
            <i class="fa fa-info-circle content-tooltipped" aria-hidden="true" style="margin-left: 5px; cursor: pointer; font-size: 14px;"
            data-toggle="tooltip"
            data-placement="right"
            title="Allow employee to accumulate vacation leave past their maximum amount if a letter from PS is uploaded"></i>
        </h3>

        <%-- Create, Edit mode: shows a downloadable list of all the files associated with an employee--%>
        <asp:Panel ID="filesToDownloadPanel" runat="server" Style="margin-bottom:15px;" Visible="false">

            <label for="filesToDownloadList" style="font-size: 1.05em; display: inline;">Previously uploaded files:</label>

            <asp:DropDownList ID="filesToDownloadList" runat="server"></asp:DropDownList>

            <asp:LinkButton ID="btnDownloadFiles" runat="server" OnClick="btnDownloadFiles_Click" CssClass="btn btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Download file" Style="display: inline-block; margin-left: 5px;">
                <i class="fa fa-download" aria-hidden="true"></i>
            </asp:LinkButton>

            <asp:LinkButton ID="btnDeleteFile" runat="server" OnClick="btnDeleteFile_Click" OnClientClick="return confirm('Delete file?');" CssClass="btn btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete file" Style="display: inline-block; margin-left: 5px;">
            <i class="fa fa-trash-o" aria-hidden="true"></i>
            </asp:LinkButton>

        </asp:Panel>

        <label class="switch">
            <asp:CheckBox ID="chkOnOff" runat="server" AutoPostBack="true" OnCheckedChanged="chkOnOff_CheckedChanged" />
            <span class="slider round"></span>
        </label>

        <asp:UpdatePanel ID="disclaimerForNoFilesUploadedUpdatePanel" runat="server">
            <ContentTemplate>
                <asp:Panel ID="noFilesUploadedDisclaimerPanel" runat="server" CssClass="row alert alert-info" Style="display: inline-block; margin: 0px 5px; width:450px;" role="alert" Visible="false">
                    <i class="fa fa-info-circle" aria-hidden="true"></i>
                    <span>No Files currently uploaded. Without a letter from PS, the employee will not be able to accumulate vacation leave past their max</span>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>
        
        <asp:Panel ID="fileUploadPanel" runat="server" Style="margin: 15px auto; text-align: center" CssClass="row form-group" Visible="false">

            <label for="FileUpload1" style="font-size: 1.05em; display: inline;">Upload letter from PS:</label>

            <asp:FileUpload ID="FileUpload1" runat="server" Width="475px" Style="margin: 0 auto; display: inline-block; background-color: lightgrey" AllowMultiple="true" />

            <asp:LinkButton ID="uploadFilesBtn" runat="server" OnClick="uploadFilesBtn_Click" CssClass="btn btn-sm btn-primary content-tooltipped" data-toggle="tooltip" data-placement="top" title="Upload files">
                <i class="fa fa-upload" aria-hidden="true"></i>
            </asp:LinkButton>

            <asp:LinkButton ID="clearAllFilesBtn" runat="server" OnClientClick="return confirm('Clear all files?');" CssClass="btn btn-sm btn-danger content-tooltipped" data-toggle="tooltip" data-placement="top" title="Clear all uploaded files" OnClick="clearAllFilesBtn_Click">
                <i class="fa fa-times" aria-hidden="true"></i>
            </asp:LinkButton>

            <br />

            <%--Shows the files uploaded to Session--%>
            <asp:Panel ID="filesUploadedPanel" runat="server" Style="text-align: left; margin: 0 auto; display: inline-block;" Visible="false">
                <asp:ListView ID="filesUploadedListView" runat="server" GroupItemCount="10">
                    <GroupTemplate>
                        <div id="itemPlaceholderContainer" runat="server">
                            <div id="itemPlaceholder" runat="server"></div>
                        </div>
                    </GroupTemplate>

                    <ItemTemplate>
                        <div style="margin-top: 5px;">
                            <span>
                                <asp:LinkButton ID="clearIndividualFileBtn" data-id='<%#Eval("file_name") %>' OnClick="clearIndividualFileBtn_Click" runat="server" CssClass="btn btn-danger btn-sm content-tooltipped" data-toggle="tooltip" data-placement="left" title="Clear file">
                                    <i class="fa fa-times" aria-hidden="true"></i>
                                </asp:LinkButton>
                            </span>
                            <span><%#Eval("file_name") %></span>
                        </div>
                    </ItemTemplate>


                    <LayoutTemplate>
                        <div id="groupPlaceholderContainer" runat="server">
                            <div id="groupPlaceholder" runat="server"></div>
                        </div>
                    </LayoutTemplate>

                </asp:ListView>
            </asp:Panel>
        </asp:Panel>

        <asp:UpdatePanel ID="filesUpdatePanel" runat="server" Style="text-align:center;">
            <ContentTemplate>
                <asp:Panel ID="duplicateFileNamesPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="duplicateFileNameTxt" runat="server">File name already exists</span>
                </asp:Panel>

                <asp:Panel ID="invalidFileTypePanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="invalidFileTypeErrorTxt" runat="server">Invalid file type</span>
                </asp:Panel>

                <asp:Panel ID="fileUploadedTooLargePanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span id="fileUploadTooLargeTxt" runat="server">File upload too large</span>
                </asp:Panel>

                <asp:Panel ID="noFileUploaded" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                    <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                    <span>No file was uploaded</span>
                </asp:Panel>
            </ContentTemplate>
        </asp:UpdatePanel>

    </asp:Panel>

    <div class="container text-center" style="display:flex; justify-content:center">
        <asp:Panel ID="submitFullFormPanel" runat="server" CssCclass="row text-center" Style="margin-top: 30px;">
            <asp:LinkButton type="submit" ID="submitBtn" CssClass="btn btn-success" runat="server" OnClick="submitBtn_Click" ValidationGroup="submitFullFormGroup">
            <i class="fa fa-send" aria-hidden="true"></i>
            Submit new employee
            </asp:LinkButton>
        </asp:Panel>

        <asp:Panel ID="editFormPanel" runat="server" CssCclass="row text-center" Style="margin-top: 30px;">
            <asp:LinkButton ID="editBtn" CssClass="btn btn-success" runat="server" ValidationGroup="submitFullFormGroup" OnClick="editBtn_Click">
            <i class="fa fa-floppy-o" aria-hidden="true"></i>
            Save employee
            </asp:LinkButton>
        </asp:Panel>
    </div>

     <%-- End Employment Record Modal--%>
    <div class="modal fade" id="cancelEmpRecordModal" tabindex="-1" role="dialog" aria-labelledby="cancelEmpRecordTitle" aria-hidden="true">

        <div class="modal-dialog" role="document" style="width: 50%;">
            <asp:UpdatePanel ID="endEmpRecordUpdatePanel" runat="server">
                <ContentTemplate>
                    <div class="modal-content">
                        <div class="modal-header text-center">
                            <h2 class="modal-title" id="cancelEmpRecordTitle" style="display: inline; width: 150px;">End Employment Record</h2>
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
                                    <ajaxToolkit:CalendarExtender ID="fromCalendarExtender" TargetControlID="txtEmpRecordEndDate" PopupButtonID="empRecordEndDateCalendar" runat="server" Format="d/MM/yyyy"></ajaxToolkit:CalendarExtender>
                                </div>
                            </div>

                            <asp:Panel ID="invalidEndDatePanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>End date is not valid</span>
                            </asp:Panel>

                            <asp:Panel ID="emptyEndDatePanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>No end date is entered</span>
                            </asp:Panel>

                            <asp:Panel ID="actualEndDateIsWeekendPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>Actual end date is on the weekend</span>
                            </asp:Panel>

                            <asp:Panel ID="endDateBeforeStartDatePanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>End date cannot be before start date</span>
                            </asp:Panel>

                            <asp:Panel ID="multipleActiveRecordsEndRecordPanel" runat="server" CssClass="row alert alert-warning validation-msg" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>
                                    Actual end date not edited since the date entered would result in two employment records being marked active simultaneously. Edit employment records accordingly 
                                    to ensure only one active record
                                </span>
                            </asp:Panel>

                        </div>
                        <div class="modal-footer">
                            <asp:Button ID="closeEndEmpRecordModalBtn" runat="server" Text="Close" CssClass="btn btn-secondary" OnClick="closeEndEmpRecordModalBtn_Click"/>
                            <asp:LinkButton runat="server" ID="submitEndEmpRecordBtn" class="btn btn-primary" OnClick="submitEndEmpRecordBtn_Click" CausesValidation="false">
                                 <i class="fa fa-send" aria-hidden="true"></i>
                                 Submit
                            </asp:LinkButton>
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
