<%@ Page Title="All Employees" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="AllEmployees.aspx.cs" Inherits="HR_LEAVEv2.HR.AllEmployees" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .custom-no-dp-header-icon{
            font-size:8em; 
            margin-top:15%;
        }
    </style>

    <h1><%: Title %></h1>
      <div class="container-fluid">
        <div class="container" style="width:65%;">
            <div class="row text-center" style="margin-top: 25px;">
                <asp:Panel ID="Panel1" runat="server" DefaultButton="searchBtn" CssClass="input-group" Width="510px" style="margin:0 auto;">
                    <asp:TextBox ID="searchTxtbox" runat="server" CssClass="form-control" placeholder="Search Employee" aria-label="Search Employee" aria-describedby="basic-addon2" OnTextChanged="searchTxtbox_TextChanged"></asp:TextBox>
                    <div class="input-group-addon">
                        <asp:LinkButton ID="searchBtn" runat="server" OnClick="searchBtn_Click">
                            <span class="input-group-text" id="basic-addon2">
                                <i class="fa fa-search"></i>
                            </span>
                        </asp:LinkButton>
                    </div>
                </asp:Panel>
                <asp:LinkButton ID="LinkButton1" runat="server" CssClass="btn btn-primary" OnClick="newEmployeeBtn_Click" style="margin-top:15px">
                    <i class="fa fa-plus"></i>
                    Add new Employee
                </asp:LinkButton>
            </div>
        </div>
        <div class="container" style="width:100%; margin-top:55px;">
            <asp:ListView ID="ListView1" runat="server" OnPagePropertiesChanging="ListView1_PagePropertiesChanging" GroupItemCount="4" style="height:85%;" >  
                <EmptyDataTemplate>
                    <div class="alert alert-info text-center" role="alert" style=" width: 30%; margin:auto ">
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
                        <div class="custom-card" style="position:relative;">
                            <div class="custom-card-header" ><i class="fa fa-user-circle custom-no-dp-header-icon"></i></div>
                            <h3 style="margin-top:10px;"><asp:Label runat="server" ID="Label4" Text='<%#Eval("Name") %>'></asp:Label></h3>
                            <div class="custom-card-body">
                                <span>
                                    <h5 style="display:inline;">Employee ID:</h5>
                                    <asp:Label runat="server" ID="emp_idLabel" Text='<%#Eval("employee_id") %>'></asp:Label> <br />
                                </span>
                                <span>
                                    <h5 style="display:inline;">IHRIS ID:</h5>
                                    <asp:Label runat="server" ID="ihris_idLabel" Text='<%#Eval("ihris_id") %>'></asp:Label> <br />
                                </span>
                                <span>
                                    <h5 style="margin-bottom:5px;">Email:</h5>
                                    <asp:Label runat="server" ID="Label1" Text='<%#Eval("email") %>'></asp:Label> <br />
                                </span>
                            </div>
                            <div style="position:absolute; bottom:20px; left:25%;">
                                <button emp_id='<%#Eval("employee_id") %>' type="button" class="btn btn-primary show-details-btn" data-toggle="modal" data-target="#empDetailsModal" >Employee Details</button>
                            </div>
                        </div>
                    </td>  
                </ItemTemplate>   
                 
                <AlternatingItemTemplate>  
                    <td align="center">  
                        <div class="custom-card" style="margin-left:20px; margin-right:20px; position:relative;">
                            <div class="custom-card-header" ><i class="fa fa-user-circle custom-no-dp-header-icon"></i></div>
                            <h3 style="margin-top:10px;"><asp:Label runat="server" ID="Label4" Text='<%#Eval("Name") %>'></asp:Label></h3>
                            <div class="custom-card-body">
                                <span>
                                    <h5 style="display:inline;">Employee ID:</h5>
                                    <asp:Label runat="server" ID="emp_idLabel" Text='<%#Eval("employee_id") %>'></asp:Label> <br />
                                </span>
                                <span>
                                    <h5 style="display:inline;">IHRIS ID:</h5>
                                    <asp:Label runat="server" ID="ihris_idLabel" Text='<%#Eval("ihris_id") %>'></asp:Label> <br />
                                </span>
                                <span>
                                    <h5 style="margin-bottom:5px;">Email:</h5>
                                    <asp:Label runat="server" ID="Label1" Text='<%#Eval("email") %>'></asp:Label> <br />
                                </span>
                            </div> 
                            <div style="position:absolute; bottom:20px; left:25%;">
                                <button emp_id='<%#Eval("employee_id") %>' type="button" class="btn btn-primary show-details-btn" data-toggle="modal" data-target="#empDetailsModal" >Employee Details</button>
                            </div>
                        </div>
                    </td>  
                </AlternatingItemTemplate> 


                <LayoutTemplate>
                    <table style="width:100%;">
                        <tbody>
                            <tr>
                                <td>
                                    <table id="groupPlaceholderContainer" runat="server" style="width:100%">
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

            <%--Modal--%>

            <div class="modal fade" id="empDetailsModal" tabindex="-1" role="dialog" aria-labelledby="empDetailsTitle" aria-hidden="true" style="margin-top: 10%;">
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
                            <div>
                                <h4 style="display: inline">Employee Type:</h4>
                                <span id="empTypeDetails"></span>
                            </div>
                            <div>
                                <h4 style="display: inline">Employee Position:</h4>
                                <span id="empPositionDetails"></span>
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
        //Sys.WebForms.PageRequestManager.getInstance()

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
                url:  '<%= ResolveUrl("AllEmployees.aspx/getEmpDetails") %>',
                contentType: "application/json; charset=utf-8",
                data: "{'emp_id':'" + $(this).attr("emp_id") +"'}",
                dataType: "json",
                success: function (data) {
                    populateModal(JSON.parse(data.d));
                },
                error: function (result) {
                    alert('Unable to load data: ' + result.responseText);
                }
            });
        })

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
            $('#empTypeDetails').text(data.employment_type);
            $('#empPositionDetails').text(data.position);
        }
    </script>
    
</asp:Content>
