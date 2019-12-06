<%@ Page Title="Employee Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmployeeDetails.aspx.cs" Inherits="HR_LEAVEv2.HR.EmployeeDetails" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>
    <div class="container-fluid">
        <div class="container" style="width:45%; margin-top:25px;">
            <div class="row text-center">
                <h3>IDs and Email</h3>
                <div class="form-group text-left">
                    <label for="employeeIdInput">Employee ID</label>
<%--                    <asp:TextBox ID="employeeIdInput" runat="server"></asp:TextBox>--%>
                    <input class="form-control" runat="server" id="employeeIdInput" aria-describedby="emailHelp" placeholder="Enter employee ID">
    <%--                <small id="emailHelp" class="form-text text-muted">Press tab to move between inputs in the form</small>--%>
                </div>
                <div class="form-group text-left">
                    <label for="ihrisNumInput">IHRIS ID</label>
                    <input class="form-control" id="ihrisNumInput" placeholder="Enter IHRIS ID">
                </div>
                 <div class="form-group text-left">
                    <label for="adEmailInput">Active Directory Email</label>
                    <input type="email" class="form-control" id="adEmailInput" placeholder="Enter Active Directory email">
                </div>
            </div>

            <div class="row text-center">
                <h3 style="margin-top:25px;">Authorization Level</h3>
                <div class="form-group" id="authLevelDiv">
                    <div class="form-check" id="supervisorCheckDiv">
                        <label class="form-check-label" for="supervisorCheck">
                            <input type="checkbox" class="form-check-input" id="supervisorCheck">
                            <span>Supervisor</span>
                        </label>
                    </div>
                    <div class="form-check" id="hr1CheckDiv">
                        <label class="form-check-label" for="hr1Check">
                            <input type="checkbox" class="form-check-input" id="hr1Check">
                            <span>HR Level 1</span>
                        </label>
                    </div>
                    <div class="form-check" id="hr2CheckDiv">
                        <label class="form-check-label" for="hr2Check">
                            <input type="checkbox" class="form-check-input" id="hr2Check">
                            <span>HR Level 2</span>
                        </label>
                    </div>
                    <div class="form-check" id="hr3CheckDiv">
                        <label class="form-check-label" for="hr2Check">
                            <input type="checkbox" class="form-check-input" id="hr3Check">
                            <span>HR Level 3</span>
                        </label>
                    </div>
                </div>
            </div>
            <div class="text-center" style="display:none" id="furtherDetailsForHrDiv">
                <h4 style="margin-top:25px;">Type of employee dealt with</h4>
                 <div class="form-group">
                    <div class="form-check" id="contractCheckDiv">
                        <label class="form-check-label" for="contractCheck">
                            <input type="checkbox" class="form-check-input" id="contractCheck">
                            <span>Contract</span>
                        </label>
                    </div>
                    <div class="form-check" id="publicServiceCheckDiv">
                        <label class="form-check-label" for="publicServiceCheck">
                            <input type="checkbox" class="form-check-input" id="publicServiceCheck">
                            <span>Public Service</span>
                        </label>
                    </div>
                </div>
            </div>
        </div>
        <hr style="width:45%;"/>
        <div class="container text-center" style="width:20%;">
            <h3>Leave Balances</h3>
            <div class="form-group text-left">
                <label for="personalLeaveInput">Personal Leave</label>
                <input class="form-control" id="personalLeaveInput" placeholder="Enter personal leave balance">
            </div>
            <div class="form-group text-left">
                <label for="casualLeaveInput">Casual Leave</label>
                <input class="form-control" id="casualLeaveInput" placeholder="Enter casual leave balance">
            </div>
                <div class="form-group text-left">
                <label for="sickLeaveInput">Sick Leave</label>
                <input type="email" class="form-control" id="sickLeaveInput" placeholder="Enter sick leave balance">
            </div>
        </div>
        <hr style="width:45%;"/>
        <div class="container text-center">
            <h3>Employment Record</h3>

            <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" style="margin: 0 auto;">
                <Columns>
                    <asp:BoundField HeaderText="Employment Type" DataField="employment_type"/>
                    <asp:BoundField HeaderText="Department" DataField="dept_name"/>
                    <asp:BoundField HeaderText="Position" DataField="pos_name"/>
                    <asp:BoundField HeaderText="Start" DataField="start_date"/>
                    <asp:BoundField HeaderText="End" DataField="expected_end_date"/>
                </Columns>
            </asp:GridView>
            <div class="btn-group" role="group" aria-label="Basic example" style="margin-top:10px; margin-left:35%;">
                <button type="button" class="btn btn-primary" data-toggle="modal" data-target="#addEmpRecordModal">Add</button>
            </div>
        </div>
    </div>

     <%--Modal--%>

    <div class="modal fade" id="addEmpRecordModal" tabindex="-1" role="dialog" aria-labelledby="addEmpRecordTitle" aria-hidden="true" style="margin-top: 7%;">
        <div class="modal-dialog" role="document" style="width: 55%;">
            <div class="modal-content">
                <div class="modal-header text-center">
                    <h2 class="modal-title" id="addEmpRecordTitle" style="display: inline; width: 150px;">
                        Add Record
                    </h2>
                    <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="modal-body text-center">
                    <div class="container" style="width:80%; height:95%">
                        <div class="form-group" style="margin-top:25px;">
                             <label for="empTypeList">Employment Type</label>
                            <asp:DropDownList ID="empTypeList" runat="server" CssClass="form-control" Width="225px" DataSourceID="SqlDataSource3" DataTextField="type_id" style="display:inline-block;"></asp:DropDownList>
                            <asp:SqlDataSource ID="SqlDataSource3" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [type_id] FROM [employmenttype] ORDER BY [type_id]"></asp:SqlDataSource>
                        </div>
                        <div class="form-group" style="margin-top:45px;">
                            <label for="deptList">Department</label>
                            <asp:DropDownList ID="deptList" runat="server" CssClass="form-control" Width="225px" DataSourceID="SqlDataSource2" DataValueField="dept_id" DataTextField="dept_name" style="display:inline-block; margin-right:15%;"></asp:DropDownList>
                            <asp:SqlDataSource ID="SqlDataSource2" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [dept_id], [dept_name] FROM [department] ORDER BY [dept_name]"></asp:SqlDataSource>

                            <label for="positionList" >Position</label>
                            <asp:DropDownList ID="positionList" runat="server" CssClass="form-control" Width="225px" DataSourceID="SqlDataSource1" DataValueField="pos_id" DataTextField="pos_name" style="display:inline-block"></asp:DropDownList>
                            <asp:SqlDataSource ID="SqlDataSource1" runat="server" ConnectionString="<%$ ConnectionStrings:dbConnectionString %>" ProviderName="System.Data.SqlClient" SelectCommand="SELECT [pos_id], [pos_name] FROM [position] ORDER BY [pos_name]"></asp:SqlDataSource>
                        </div>
                        <div class="form-group text-center" style="margin-top:45px;">
                            <span style="margin-right:15%;">
                                <label for="txtStartDate">Start date</label>
                                <asp:TextBox ID="txtStartDate" ClientID="txtStartDate" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                                <i id="startDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtStartDate" PopupButtonID="startDateCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                                <asp:RequiredFieldValidator ID="startDateRequiredValidator" runat="server" ControlToValidate="txtStartDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                            </span>

                            <span>
                                <label for="txtEndDate">End date</label>
                                <asp:TextBox ID="txtEndDate" ClientID="txtEndDate" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                                <i id="endDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtEndDate" PopupButtonID="endDateCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                                <asp:RequiredFieldValidator ID="endDateRequiredValidator" runat="server" ControlToValidate="txtEndDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                            </span>
                        </div>

                        <div id="validationDiv">
                             <asp:Panel ID="invalidStartDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none; margin:0px 5px;" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span id="invalidStartDateValidationMsg" runat="server">Start date is not valid</span>
                            </asp:Panel>
                            <asp:Panel ID="invalidEndDateValidationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none;margin:0px 5px;" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span id="invalidEndDateValidationMsg" runat="server">End date is not valid</span>
                            </asp:Panel>
                            <asp:Panel ID="dateComparisonValidationMsgPanel" runat="server" CssClass="row alert alert-warning" style="display:none; margin:0px 5px;" role="alert">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span id="dateComparisonValidationMsg" runat="server">End date cannot precede start date</span>
                            </asp:Panel>

                        </div>
                    </div>
                </div>
                <div class="text-center" style="margin-bottom:45px; margin-top:25px;">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal" style="margin-right:35px;">Cancel</button>
                    <button type="submit" id="addEmpRecordSubmitBtn" class="btn btn-success">Submit</button>
<%--                    <asp:Button ID="addEmpRecordSubmitBtn" runat="server" Text="Submit" CssClass="btn btn-success"/>--%>
                </div>
            </div>
        </div>
    </div>

    <script>
        $('#authLevelDiv input[type="checkbox"]').click(function (e) {
            if ($('#hr2CheckDiv input[type="checkbox"], #hr3CheckDiv input[type="checkbox"]').is(':checked')) {
                $('#furtherDetailsForHrDiv').css("display", "block");
            }
            else {
                $('#furtherDetailsForHrDiv').css("display", "none");
                $('#furtherDetailsForHrDiv input[type="checkbox"]').prop('checked', false);
            }
        });

        //$('#addEmpRecordSubmitBtn').click(function () {
        //    alert($('#txtStartDate').val() + " to " + $('#txtEndDate').val());
        //});


    </script>
</asp:Content>
