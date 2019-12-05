<%@ Page Title="Employee Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmployeeDetails.aspx.cs" Inherits="HR_LEAVEv2.HR.EmployeeDetails" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>
    <div class="container-fluid">
        <div class="container" style="width:45%; margin-top:25px;">
            <div class="row text-center">
                <h3>IDs and Email</h3>
                <div class="form-group text-left">
                    <label for="employeeIdInput">Employee ID</label>
                    <input class="form-control" id="employeeIdInput" aria-describedby="emailHelp" placeholder="Enter employee ID">
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
                <button type="button" class="btn btn-primary">Add</button>
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

    </script>
</asp:Content>
