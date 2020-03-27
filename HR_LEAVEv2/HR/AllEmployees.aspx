<%@ Page Title="All Employees" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AllEmployees.aspx.cs" Inherits="HR_LEAVEv2.HR.AllEmployees" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .custom-no-dp-header-icon {
            font-size: 4em;
            margin-top: 5%;
        }
    </style>
    <h1><%: Title %></h1>

    <div class="container-fluid">

        <div class="container" style="width: 65%;">

            <div class="row text-center" style="margin-top: 20px;">

                <%--Search bar--%>
                <asp:Panel ID="Panel1" runat="server" DefaultButton="searchBtn" CssClass="input-group" Width="510px" Style="margin: 0 auto;">
                    <asp:TextBox ID="searchTxtbox" runat="server" CssClass="form-control" placeholder="Search Employee" aria-label="Search Employee" aria-describedby="basic-addon2" OnTextChanged="searchTxtbox_TextChanged"></asp:TextBox>
                    <div class="input-group-addon">
                        <asp:LinkButton ID="searchBtn" runat="server" OnClick="searchBtn_Click">
                            <span class="input-group-text" id="basic-addon2">
                                <i class="fa fa-search"></i>
                            </span>
                        </asp:LinkButton>
                    </div>
                </asp:Panel>

                <button id="clearSearchBtn" class="btn btn-primary" style="display: none; margin-top: 15px; margin-right: 5px;">
                    <i class="fa fa-times"></i>
                    Clear Search
                </button>

                <asp:LinkButton ID="LinkButton1" runat="server" CssClass="btn btn-primary" OnClick="newEmployeeBtn_Click" Style="margin-top: 15px; margin-right:5px;">
                    <i class="fa fa-plus"></i>
                    Add new Employee
                </asp:LinkButton>

                <%--Dropdown list where HR can select which type of employee to view: Active or Inactive--%>
                <asp:DropDownList ID="employeeStatusDropDown" runat="server" OnSelectedIndexChanged="employeeStatusDropDown_SelectedIndexChanged" AutoPostBack="true"
                    CssClass="form-control" Width="100px" Style="margin: 0 auto; margin-top:15px; display:inline;">
                    <asp:ListItem Selected="True">Active</asp:ListItem>
                    <asp:ListItem>Inactive</asp:ListItem>
                </asp:DropDownList>
                
            </div>
        </div>

        <div class="container" style="margin-top: 30px;">

            <asp:ListView ID="ListView1" runat="server" OnPagePropertiesChanging="ListView1_PagePropertiesChanging" GroupItemCount="4" Style="height: 85%;">
                <EmptyDataTemplate>
                    <div class="alert alert-info text-center" role="alert" style="width: 30%; margin: auto">
                        <i class="fa fa-info-circle"></i>
                        No data on employees available
                    </div>
                </EmptyDataTemplate>

                <EmptyItemTemplate>
                    </td>
                </EmptyItemTemplate>

                <GroupTemplate>
                    <tr id="itemPlaceholderContainer" runat="server">
                        <td id="itemPlaceholder" runat="server"></td>
                    </tr>
                </GroupTemplate>

                <ItemTemplate>
                    <td align="center">

                        <div class="custom-card" style="position: relative;">

                            <%--employee icon--%>
                            <div class="custom-card-header"><i class="fa fa-user-circle custom-no-dp-header-icon"></i></div>

                            <%--employee name--%>
                            <h4 style="margin-top: 10px; margin-bottom:0px;">
                                <strong><asp:Label runat="server" ID="Label4" Text='<%#Eval("Name") %>'></asp:Label></strong>
                            </h4>

                            <%--employee details--%>
                            <div class="custom-card-body">

                                <%--badge showing whether employee is active or inactive--%>
                                <div class='<%#Eval("bootstrapClass") %>'><%#Eval("isActive") %></div>

                                <%--employee ID--%>
                                <div>
                                    <h5 style="display: inline;" class="custom-card-body-header-text">Employee ID:</h5>
                                    <asp:Label runat="server" ID="Label3" Text='<%#Eval("employee_id") %>' CssClass="custom-card-body-text"></asp:Label>
                                </div>

                                <%--IHRIS ID--%>
                                <div>
                                    <h5 style="display: inline;" class="custom-card-body-header-text">IHRIS ID:</h5>
                                    <asp:Label runat="server" ID="Label5" Text='<%#Eval("ihris_id") %>' CssClass="custom-card-body-text"></asp:Label>
                                </div>

                                <%--email--%>
                                <div>
                                    <h5 class="custom-card-body-header-text">Email:</h5>
                                    <asp:Label runat="server" ID="Label6" Text='<%#Eval("email") %>' CssClass="custom-card-body-text"></asp:Label>
                                </div>

                            </div>

                            <div class="custom-card-footer">

                                <%--button to show employee details--%>
                                <span class="content-tooltipped" data-toggle="tooltip" data-placement="top" title="Details">
                                    <button emp_id='<%#Eval("employee_id") %>' type="button" class="btn btn-primary show-details-btn" data-toggle="modal" data-target="#empDetailsModal">
                                        <i class="fa fa-address-card-o" aria-hidden="true"></i>
                                    </button>
                                </span>

                                <%--button to edit employee--%>
                                <a href="EmployeeDetails.aspx?mode=edit&empId=<%#Eval("employee_id") %>" class="content-tooltipped" data-toggle="tooltip" data-placement="top" title="Edit">
                                    <button emp_id='<%#Eval("employee_id") %>' type="button" class="btn btn-warning edit-employee-btn" style="margin-left: 5px; color: black;">
                                        <i class="fa fa-pencil" aria-hidden="true"></i>
                                    </button>
                                </a>

                            </div>

                        </div>

                    </td>
                </ItemTemplate>

                <AlternatingItemTemplate>
                    <td align="center">

                        <div class="custom-card" style="margin-left: 20px; margin-right: 20px; position: relative;">

                            <%--employee icon--%>
                            <div class="custom-card-header"><i class="fa fa-user-circle custom-no-dp-header-icon"></i></div>

                            <%--employee name--%>
                            <h4 style="margin-top: 10px; margin-bottom:0px;">
                                <strong><asp:Label runat="server" ID="Label2" Text='<%#Eval("Name") %>'></asp:Label></strong>
                            </h4>

                            <%--employee details--%>
                            <div class="custom-card-body">

                                <%--badge showing whether employee is active or inactive--%>
                                <div class='<%#Eval("bootstrapClass") %>'><%#Eval("isActive") %></div>

                                <%--employee ID--%>
                                <div>
                                    <h5 style="display: inline;" class="custom-card-body-header-text">Employee ID:</h5>
                                    <asp:Label runat="server" ID="Label3" Text='<%#Eval("employee_id") %>' CssClass="custom-card-body-text"></asp:Label>
                                </div>

                                <%--IHRIS ID--%>
                                <div>
                                    <h5 style="display: inline;" class="custom-card-body-header-text">IHRIS ID:</h5>
                                    <asp:Label runat="server" ID="Label5" Text='<%#Eval("ihris_id") %>' CssClass="custom-card-body-text"></asp:Label>
                                </div>

                                <%--email--%>
                                <div>
                                    <h5 class="custom-card-body-header-text">Email:</h5>
                                    <asp:Label runat="server" ID="Label6" Text='<%#Eval("email") %>' CssClass="custom-card-body-text"></asp:Label>
                                </div>

                            </div>

                            <div class="custom-card-footer">

                                <%--button to show employee details--%>
                                <span class="content-tooltipped" data-toggle="tooltip" data-placement="top" title="Details">
                                    <button emp_id='<%#Eval("employee_id") %>' type="button" class="btn btn-primary show-details-btn" data-toggle="modal" data-target="#empDetailsModal">
                                        <i class="fa fa-address-card-o" aria-hidden="true"></i>
                                    </button>
                                </span>

                                <%--button to edit employee--%>
                                <a href="EmployeeDetails.aspx?mode=edit&empId=<%#Eval("employee_id") %>" class="content-tooltipped" data-toggle="tooltip" data-placement="top" title="Edit">
                                    <button emp_id='<%#Eval("employee_id") %>' type="button" class="btn btn-warning edit-employee-btn" style="margin-left: 5px; color: black;">
                                        <i class="fa fa-pencil" aria-hidden="true"></i>
                                    </button>
                                </a>

                            </div>

                        </div>

                    </td>
                </AlternatingItemTemplate>

                <LayoutTemplate>
                    <table style="width: 100%;">
                        <tbody>
                            <tr>
                                <td>
                                    <table id="groupPlaceholderContainer" runat="server" style="width: 100%">
                                        <tr id="groupPlaceholder"></tr>
                                    </table>
                                </td>
                            </tr>
                            <tr>
                                <td></td>
                            </tr>
                            <tr></tr>
                        </tbody>
                    </table>
                </LayoutTemplate>

            </asp:ListView>

            <asp:DataPager ID="DataPager1" PagedControlID="ListView1" PageSize="8" runat="server">
                <Fields>
                    <asp:NumericPagerField ButtonType="Link" />
                </Fields>
            </asp:DataPager>

            <%--Modal to view employee details--%>
            <div class="modal fade" id="empDetailsModal" tabindex="-1" role="dialog" aria-labelledby="empDetailsTitle" aria-hidden="true">
                <div class="modal-dialog" role="document" style="width: 65%;">
                    <div class="modal-content">

                        <div class="modal-header text-center">
                            <h2 class="modal-title" id="empDetailsTitle" style="display: inline; width: 150px;">
                                <span id="empNameDetails"></span>
                            </h2>
                            <button type="button" class="close" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>

                        <div class="modal-body text-center">
                            <h3>Details</h3>

                            <div>
                                <h4 style="display: inline">Employee ID:</h4>
                                <span id="empIdDetails"></span>
                            </div>

                            <div>
                                <h4 style="display: inline">IHRIS ID:</h4>
                                <span id="ihrisIdDetails"></span>
                            </div>

                            <div>
                                <h4 style="display: inline">Email:</h4>
                                <span id="emailDetails"></span>
                            </div>

                            <div id="positionDetails">
                                <div>
                                    <h4 style="display: inline">Employee Type:</h4>
                                    <span id="empTypeDetails"></span>
                                </div>
                                <div>
                                    <h4 style="display: inline">Employee Position:</h4>
                                    <span id="empPositionDetails"></span>
                                </div>
                            </div>

                            <div id="errorPanel" class="alert alert-info" style="margin: 5px 0px; display: inline-block; font-size: 0.90em">
                                <i class="fa fa-exclamation-triangle" aria-hidden="true"></i>
                                <span>Employment Type and employee position info could not be loaded</span>
                            </div>


                            <hr style="width: 45%;" />


                            <h3>Leave Balances</h3>

                            <div>
                                <h4 style="display: inline">Vacation Leave Balance:</h4>
                                <span id="vacationDetails"></span>
                            </div>

                            <div>
                                <h4 style="display: inline">Personal Leave Balance:</h4>
                                <span id="personalDetails"></span>
                            </div>

                            <div>
                                <h4 style="display: inline">Casual Leave Balance:</h4>
                                <span id="casualDetails"></span>
                            </div>

                            <div>
                                <h4 style="display: inline">Sick Leave Balance:</h4>
                                <span id="sickDetails"></span>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button type="button" class="btn btn-secondary" data-dismiss="modal">Close</button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <script>
        /*
        * The following function runs at page load. This is necessary when the user changes page number or searches for an employee. When either of the previously stated events 
        * occur, the listview rerenders the new cards with the new employee info. The previously attached jquery event handlers are then cleared. As a result, at every new page 
        * load, the event handlers must be rebinded. 
        */
        Sys.WebForms.PageRequestManager.getInstance().add_pageLoaded(function () {

            $('.show-details-btn').click(function () {
                $('#empNameDetails').text("");
                $('#empIdDetails').text("");
                $('#ihrisIdDetails').text("");
                $('#emailDetails').text("");
                $('#vacationDetails').text("");
                $('#personalDetails').text("");
                $('#casualDetails').text("");
                $('#sickDetails').text("");
                $('#empTypeDetails').text("");
                $('#empPositionDetails').text("");
                $.ajax({
                    type: "POST",
                    url: '<%= ResolveUrl("AllEmployees.aspx/getEmpDetails") %>',
                    contentType: "application/json; charset=utf-8",
                    data: "{'emp_id':'" + $(this).attr("emp_id") + "'}",
                    dataType: "json",
                    success: function (data) {
                        populateModal(JSON.parse(data.d));
                    },
                    error: function (result) {
                        alert('Unable to load data: ' + result.responseText);
                    }
                });
            });

            if ($('#<%= searchTxtbox.ClientID %>').val() != "")
                $('#clearSearchBtn').show();
            else
                $('#clearSearchBtn').hide();

            $('#<%= searchTxtbox.ClientID %>').on('keypress', function () {
                $('#clearSearchBtn').show();
            });

            $('#clearSearchBtn').click(function () {
                $('#<%= searchTxtbox.ClientID %>').val("");
                $(this).hide();
            });
           
        });

        function populateModal(data) {
            //populate modal
            $('#empNameDetails').text(data.name);
            $('#empIdDetails').text(data.emp_id);
            $('#ihrisIdDetails').text(data.ihris_id);
            $('#emailDetails').text(data.email);
            $('#vacationDetails').text(data.vacation);
            $('#personalDetails').text(data.personal);
            $('#casualDetails').text(data.casual);
            $('#sickDetails').text(data.sick);
            if (data.isCompleteRecord == '1') {
                $('#errorPanel').hide();
                $('#positionDetails').show();

                $('#empTypeDetails').text(data.employment_type);
                $('#empPositionDetails').text(data.position);
            } else {
                $('#positionDetails').hide();
                $('#errorPanel').show();
            }

        }
        
    </script>

</asp:Content>
