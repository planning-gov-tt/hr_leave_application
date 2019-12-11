<%@ Page Title="Employee Details" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="EmployeeDetails.aspx.cs" Inherits="HR_LEAVEv2.HR.EmployeeDetails" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="ajaxToolkit" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>
    <div id="empDetailsContainer" runat="server" class="container-fluid">
        <div class="row text-center" style="margin-top: 25px;">
            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                <ContentTemplate>
                    <asp:Panel ID="fullFormSubmitSuccessPanel" runat="server" Style="display: none; margin: 0 5px;" role="alert">
                        <span class="alert alert-success">
                            <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                            <span id="Span1" runat="server">Employee successfully added</span>
                        </span>
                    </asp:Panel>
                    <asp:Panel ID="fullFormErrorPanel" runat="server" Style="display: none; margin: 0 5px;" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span2" runat="server">Employee not added</span>
                        </span>
                    </asp:Panel>
                    <asp:Panel ID="emailNotFoundErrorPanel" runat="server" Style="display: none; margin: 0 5px;" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span3" runat="server">Active Directory email not found</span>
                        </span>
                    </asp:Panel>
                    <asp:Panel ID="noEmploymentRecordEnteredErrorPanel" runat="server" Style="display: none; margin: 0 5px;" role="alert">
                        <span class="alert alert-danger">
                            <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                            <span id="Span4" runat="server">At least one Employment record must be entered</span>
                        </span>
                    </asp:Panel>
                    <asp:Button ID="Button2" runat="server" CssClass="btn btn-primary" Text="Clear Form" Style="margin-left: 10px;" OnClick="refreshForm" />
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div class="container" style="width: 50%;">
            <div class="row text-center" style="background-color: #e0e0eb; margin-top: 15px; padding-bottom:15px;">

                <h3>IDs and Email</h3>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator1" runat="server" ControlToValidate="employeeIdInput" ErrorMessage="Please enter valid employee ID" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                    </asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="RequiredFieldValidator1" runat="server" ControlToValidate="employeeIdInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="employeeIdInput" style="display: block">Employee ID</label>
                    <asp:TextBox ID="employeeIdInput" runat="server" CssClass="form-control" placeholder="Enter employee ID" Style="display: inline-block"></asp:TextBox>

                </div>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator2" runat="server" ControlToValidate="ihrisNumInput" ErrorMessage="Please enter valid IHRIS ID" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                    </asp:RegularExpressionValidator>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="RequiredFieldValidator2" runat="server" ControlToValidate="ihrisNumInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="ihrisNumInput" style="display: block">IHRIS ID</label>
                    <asp:TextBox ID="ihrisNumInput" runat="server" CssClass="form-control" placeholder="Enter IHRIS ID" Style="display: inline-block"></asp:TextBox>
                </div>
                <div class="form-group text-left" style="width: 75%; margin: 0 auto; padding-bottom: 15px;">
                    <%--<asp:RegularExpressionValidator ID="RegularExpressionValidator3" runat="server" ControlToValidate="adEmailInput" ErrorMessage="Please enter valid email" ForeColor="Red" 
                         ValidationExpression="[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?" 
                         Display="Dynamic" style="float:right;">  
                    </asp:RegularExpressionValidator> --%>
                    <asp:RequiredFieldValidator ValidationGroup="submitFullFormGroup" ID="RequiredFieldValidator3" runat="server" ControlToValidate="adEmailInput" Display="Dynamic" ErrorMessage="Required" ForeColor="Red" Style="float: right;"></asp:RequiredFieldValidator>
                    <label for="adEmailInput" style="display: block">Active Directory Email</label>
                    <asp:TextBox ID="adEmailInput" runat="server" CssClass="form-control" AutoCompleteType="Email" placeholder="Enter Active Directory email" Style="display: inline-block"></asp:TextBox>
                </div>
            </div>
            <div class="row text-center" style="background-color: #f0f0f5; margin-top: 5px;">
                <h3>Authorization Level</h3>
                <div class="form-group" id="authLevelDiv">
                    <div class="form-check" id="supervisorCheckDiv">
                        <label class="form-check-label" for="supervisorCheck">
                            <asp:CheckBox ID="supervisorCheck" runat="server" CssClass="form-check-input" />
                            <span>Supervisor</span>
                        </label>
                    </div>
                    <div class="form-check" id="hr1CheckDiv">
                        <label class="form-check-label" for="hr1Check">
                            <asp:CheckBox ID="hr1Check" runat="server" CssClass="form-check-input" />
                            <span>HR Level 1</span>
                        </label>
                    </div>
                    <div class="form-check" id="hr2CheckDiv">
                        <label class="form-check-label" for="hr2Check">
                            <asp:CheckBox ID="hr2Check" runat="server" CssClass="form-check-input" />
                            <span>HR Level 2</span>
                        </label>
                    </div>
                    <div class="form-check" id="hr3CheckDiv">
                        <label class="form-check-label" for="hr2Check">
                            <asp:CheckBox ID="hr3Check" runat="server" CssClass="form-check-input" />
                            <span>HR Level 3</span>
                        </label>
                    </div>
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
        </div>
        <div class="container text-center" style="width: 50%; background-color: #e0e0eb; margin-top: 5px; padding-bottom:15px;">
            <h3>Leave Balances</h3>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator4" runat="server" ControlToValidate="personalLeaveInput" ErrorMessage="Please enter valid personal leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="personalLeaveInput">Personal Leave</label>
                <asp:TextBox CssClass="form-control" ID="personalLeaveInput" placeholder="Enter personal leave balance" runat="server"></asp:TextBox>
            </div>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator5" runat="server" ControlToValidate="vacationLeaveInput" ErrorMessage="Please enter valid vacation leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="vacationLeaveInput">Vacation Leave</label>
                <asp:TextBox CssClass="form-control" ID="vacationLeaveInput" placeholder="Enter vacation leave balance" runat="server"></asp:TextBox>
            </div>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator6" runat="server" ControlToValidate="casualLeaveInput" ErrorMessage="Please enter valid casual leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="casualLeaveInput">Casual Leave</label>
                <asp:TextBox CssClass="form-control" ID="casualLeaveInput" placeholder="Enter casual leave balance" runat="server"></asp:TextBox>
            </div>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator7" runat="server" ControlToValidate="sickLeaveInput" ErrorMessage="Please enter valid sick leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="sickLeaveInput">Sick Leave</label>
                <asp:TextBox CssClass="form-control" ID="sickLeaveInput" placeholder="Enter sick leave balance" runat="server"></asp:TextBox>
            </div>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator8" runat="server" ControlToValidate="bereavementLeaveInput" ErrorMessage="Please enter valid sick leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="bereavementLeaveInput">Bereavement Leave</label>
                <asp:TextBox CssClass="form-control" ID="bereavementLeaveInput" placeholder="Enter bereavement leave balance" runat="server"></asp:TextBox>
            </div>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator9" runat="server" ControlToValidate="maternityLeaveInput" ErrorMessage="Please enter valid sick leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="maternityLeaveInput">Maternity Leave</label>
                <asp:TextBox CssClass="form-control" ID="maternityLeaveInput" placeholder="Enter maternity leave balance" runat="server"></asp:TextBox>
            </div>
            <div class="form-group text-left">
                <asp:RegularExpressionValidator ValidationGroup="submitFullFormGroup" ID="RegularExpressionValidator10" runat="server" ControlToValidate="preRetirementLeaveInput" ErrorMessage="Please enter valid sick leave number" ForeColor="Red" ValidationExpression="^[0-9]*$" Display="Dynamic" Style="float: right;">  
                </asp:RegularExpressionValidator>
                <label for="preRetirementLeaveInput">Pre-retirement Leave</label>
                <asp:TextBox CssClass="form-control" ID="preRetirementLeaveInput" placeholder="Enter pre-retirement leave balance" runat="server"></asp:TextBox>
            </div>
        </div>
        <div class="container text-center" style="background-color: #f0f0f5; margin-top: 5px; padding-bottom: 10px;">
            <h3>Employment Record</h3>
            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                <ContentTemplate>
                    <asp:Panel runat="server" ID="addEmpRecordForm" Style="display: none; width: 100%">
                        <div class="container" style="width: 80%; height: 95%">
                            <div class="form-group" style="margin-top: 25px;">
                                <label for="empTypeList">Employment Type</label>
                                <asp:DropDownList ID="empTypeList" runat="server" CssClass="form-control" Width="225px" DataSourceID="SqlDataSource3" DataTextField="type_id" Style="display: inline-block;"></asp:DropDownList>
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
                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtStartDate" PopupButtonID="startDateCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                                    <asp:RequiredFieldValidator ValidationGroup="empRecord" ID="startDateRequiredValidator" runat="server" ControlToValidate="txtStartDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                                </span>

                                <span>
                                    <label for="txtEndDate">End date</label>
                                    <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" Style="width: 150px; display: inline;"></asp:TextBox>
                                    <i id="endDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                    <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtEndDate" PopupButtonID="endDateCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                                    <asp:RequiredFieldValidator ValidationGroup="empRecord" ID="endDateRequiredValidator" runat="server" ControlToValidate="txtEndDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
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
                            </div>
                        </div>
                        <div class="text-center" style="margin-top: 35px; margin-bottom: 45px;">
                            <asp:Button runat="server" ID="cancelNewRecordBtn" CssClass="btn btn-danger" Text="Cancel" CausesValidation="false" Style="margin-right: 35px;" OnClick="cancelNewRecordBtn_Click" />
                            <asp:Button runat="server" ID="addNewRecordBtn" CssClass="btn btn-primary" Text="Add new Record" OnClick="addNewRecordBtn_Click" ValidationGroup="empRecord" />
                        </div>

                    </asp:Panel>

                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="false" Style="margin: 0 auto;" AutoGenerateDeleteButton="false"
                        AllowPaging="true" PageSize="2" OnPageIndexChanging="GridView1_PageIndexChanging" OnRowDeleting="GridView1_RowDeleting">
                        <Columns>
                            <asp:CommandField ShowDeleteButton="true" CausesValidation="false" />
                            <asp:TemplateField HeaderText="Employment Type">
                                <ItemTemplate>
                                    <asp:Label ID="Label1" runat="server" Text='<%# Bind("employment_type") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Department ID" Visible="false">
                                <ItemTemplate>
                                    <asp:Label ID="Label2" runat="server" Text='<%# Bind("dept_id") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Department">
                                <ItemTemplate>
                                    <asp:Label ID="Label3" runat="server" Text='<%# Bind("dept_name") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Position ID" Visible="false">
                                <ItemTemplate>
                                    <asp:Label ID="Label4" runat="server" Text='<%# Bind("pos_id") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Department">
                                <ItemTemplate>
                                    <asp:Label ID="Label5" runat="server" Text='<%# Bind("pos_name") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Start">
                                <ItemTemplate>
                                    <asp:Label ID="Label6" runat="server" Text='<%# Bind("start_date") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                            <asp:TemplateField HeaderText="Expected End Date">
                                <ItemTemplate>
                                    <asp:Label ID="Label7" runat="server" Text='<%# Bind("expected_end_date") %>'></asp:Label>
                                </ItemTemplate>
                            </asp:TemplateField>

                        </Columns>
                    </asp:GridView>

                    <div class="btn-group" role="group" style="margin-top: 10px;">
                        <asp:Button runat="server" ID="showFormBtn" class="btn btn-primary" Text="Add" OnClick="showFormBtn_Click" CausesValidation="false" />
                    </div>
                </ContentTemplate>
            </asp:UpdatePanel>
        </div>
        <div id="submitFullFormPanel" runat="server" class="row text-center" style="margin-top: 50px;">
            <asp:Button type="submit" ID="submitBtn" CssClass="btn btn-success" runat="server" Text="Submit new employee" OnClick="submitBtn_Click" ValidationGroup="submitFullFormGroup" />
        </div>
    </div>

     <%--Modal--%>

   <%-- <div class="modal fade" id="addEmpRecordModal" tabindex="-1" role="dialog" aria-labelledby="addEmpRecordTitle" aria-hidden="true" style="margin-top: 7%;">
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
                                <asp:TextBox ID="txtStartDate" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                                <i id="startDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                <ajaxToolkit:CalendarExtender ID="CalendarExtender1" TargetControlID="txtStartDate" PopupButtonID="startDateCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                                <asp:RequiredFieldValidator ID="startDateRequiredValidator" runat="server" ControlToValidate="txtStartDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                            </span>

                            <span>
                                <label for="txtEndDate">End date</label>
                                <asp:TextBox ID="txtEndDate" runat="server" CssClass="form-control" style="width:150px; display:inline;"></asp:TextBox> 
                                <i id="endDateCalendar" class="fa fa-calendar fa-lg calendar-icon"></i>
                                <ajaxToolkit:CalendarExtender ID="CalendarExtender2" TargetControlID="txtEndDate" PopupButtonID="endDateCalendar" runat="server"></ajaxToolkit:CalendarExtender>
                                <asp:RequiredFieldValidator ID="endDateRequiredValidator" runat="server" ControlToValidate="txtEndDate" Display="Dynamic" ErrorMessage="Required" ForeColor="Red"></asp:RequiredFieldValidator>
                            </span>
                        </div>

                        <div id="validationDiv" style="margin-top:25px;">
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
                            <asp:Panel ID="successMsgPanel" runat="server" CssClass="row alert alert-success" style="display:none" role="alert">
                                <i class="fa fa-thumbs-up" aria-hidden="true"></i>
                                <span id="successMsg" runat="server">Record successfully added</span>
                            </asp:Panel>
                        </div>
                    </div>
                </div>
                <div class="text-center" style="margin-bottom:45px;">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal" style="margin-right:35px;">Cancel</button>
                    <button type="submit" id="addEmpRecordSubmitBtn" class="btn btn-success">Submit</button>
                </div>
            </div>
        </div>
    </div>--%>

    <script>
        $('#authLevelDiv input[type="checkbox"]').click(function (e) {
            // show section to choose whether cupervising contract or public service when creating HR 2,3 employee
            if ($('#hr2CheckDiv input[type="checkbox"], #hr3CheckDiv input[type="checkbox"]').is(':checked')) {
                $('#furtherDetailsForHrDiv').css("display", "block");
            }
            else {
                $('#furtherDetailsForHrDiv').css("display", "none");
                $('#furtherDetailsForHrDiv input[type="checkbox"]').prop('checked', false);
            }
        });

        function testClick() {
            alert("test");
        }

       <%-- $('#addEmpRecordSubmitBtn').click(function (e) {
            // validate dates when adding new employment record

            $("#<%= invalidStartDateValidationMsgPanel.ClientID %>").css("display", "none");
            $("#<%= invalidEndDateValidationMsgPanel.ClientID %>").css("display", "none");
            $("#<%= dateComparisonValidationMsgPanel.ClientID %>").css("display", "none");
            $("#<%= successMsgPanel.ClientID %>").css("display", "none");
            e.preventDefault();
            var startDate = $("#<%= txtStartDate.ClientID %>").val();
            var endDate = $("#<%= txtEndDate.ClientID %>").val();
            var datesToValidate = {
                'startDate': startDate,
                'endDate': endDate
            };

            $.ajax({
                type: "POST",
                url:  '<%= ResolveUrl("EmployeeDetails.aspx/validateDates") %>',
                contentType: "application/json; charset=utf-8",
                data: JSON.stringify({ 'datesToValidate': JSON.stringify(datesToValidate) }),
                dataType: "json",
                success: function (data) {
                    var validationIds = data.d.toString().split(';');
                    //change display of relevant validation messages
                    validationIds.forEach(function (validationMsgId) {
                        var id = "";
                        if (validationMsgId == "invalidStartDateValidationMsgPanel")
                            id = "#<%= invalidStartDateValidationMsgPanel.ClientID %>";
                        if (validationMsgId == "invalidEndDateValidationMsgPanel")
                             id = "#<%= invalidEndDateValidationMsgPanel.ClientID %>";
                        if (validationMsgId == "dateComparisonValidationMsgPanel")
                             id = "#<%= dateComparisonValidationMsgPanel.ClientID %>";
                        if (validationMsgId == "successMsgPanel")
                            id = "#<%= successMsgPanel.ClientID %>";
                        $(id).css("display", "inline-block");
                    });

                    if (validationIds[0] == "successMsgPanel") {

                        var newEmploymentRecord = {
                            'emp_id': $("#<%= employeeIdInput.ClientID %>").val(),
                            'empType': $("#<%= empTypeList.ClientID %> option:selected").val() ,
                            'dept': $("#<%= deptList.ClientID %> option:selected").val() ,
                            'position': $("#<%= positionList.ClientID %> option:selected").val() ,
                            'startDate': $("#<%= txtStartDate.ClientID %>").val(),
                            'expectedEndDate': $("#<%= txtEndDate.ClientID %>").val()
                        }

                        alert(JSON.stringify(newEmploymentRecord));

                        var row = $("#<%= GridView1.ClientID %> tr:last-child").clone(true);
                        $("#<%= GridView1.ClientID %> tr").not($("#<%= GridView1.ClientID %> tr:first-child")).remove();

                        $("td", row).eq(0).html(newEmploymentRecord.emp_id);
                        $("td", row).eq(1).html(newEmploymentRecord.empType);
                        $("td", row).eq(2).html(newEmploymentRecord.dept);
                        $("td", row).eq(3).html(newEmploymentRecord.position);
                        $("td", row).eq(4).html(newEmploymentRecord.startDate);
                        $("td", row).eq(5).html(newEmploymentRecord.expectedEndDate);

                        $("#<%= GridView1.ClientID %>").append(row);

                    }

                },
                error: function (result) {
                    alert('Unable to load data: ' + result.responseText);
                }
            });

        });--%>


    </script>
</asp:Content>
