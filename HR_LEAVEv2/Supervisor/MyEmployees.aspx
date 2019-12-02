<%@ Page Title="My Employees" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyEmployees.aspx.cs" Inherits="HR_LEAVEv2.Supervisor.MyEmployees" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .custom-card{
            border:1px solid black; 
            height:390px; 
            width:250px;
            text-align:center;
            margin-bottom:35px;
        }

        .custom-card-header{
            background-color:#f0f0f0; 
            height:45%;
        }

        .custom-no-dp-header-icon{
            font-size:8em; 
            margin-top:15%;
        }

        .custom-card-body{
            margin:5px 0px 15px 0px;
        }
    </style>
    <h1><%: Title %></h1>
    <div class="container-fluid">
        <div class="container" style="width:65%;">
            <div class="row" style="margin-top:25px;">
                    <div class="input-group" style="width:510px;margin:auto;">
                        <asp:TextBox ID="searchTxtbox" runat="server" CssClass="form-control"  placeholder="Search Employee" aria-label="Search Employee" aria-describedby="basic-addon2" OnTextChanged="searchTxtbox_TextChanged"></asp:TextBox>
                        <div class="input-group-addon">
                            <asp:LinkButton ID="searchBtn" runat="server" OnClick="searchBtn_Click">
                                <span class="input-group-text" id="basic-addon2">
                                    <i class="fa fa-search"></i>
                                </span>
                            </asp:LinkButton>
                        </div>
                    </div>
                
                <%--<div class="col-lg-4">
                    <label for="filterEmpSearchBy" style="font-size:1.0em">Filter by:</label>
                    <asp:DropDownList ID="filterEmpSearchBy" runat="server" CssClass="form-control" Width="150px" style="display:inline;" >
                        <asp:ListItem Value=""></asp:ListItem>
                        <asp:ListItem Value="Name"></asp:ListItem>
                        <asp:ListItem Value="Position"></asp:ListItem>
                    </asp:DropDownList>
                
                </div>--%>
            </div>
        </div>
        <div class="container" style="width:100%; margin-top:55px;">
            <asp:ListView ID="ListView1" runat="server" OnPagePropertiesChanging="ListView1_PagePropertiesChanging" GroupItemCount="4" style="height:85%;">  
                <EmptyDataTemplate>
                    <table >
                        <tr>
                            <td>No data was returned.</td>
                        </tr>
                    </table>
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
                        <div class="custom-card">
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
                            <div>
                                <a href="#" class="btn btn-primary">Employee Details</a>
                            </div>
                        </div>
                    </td>  
                </ItemTemplate>   
                 
                <AlternatingItemTemplate>  
                    <td align="center">  
                        <div class="custom-card" style="margin-left:10px; margin-right:10px;">
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
                            <div>
                                <a href="#" class="btn btn-primary">Employee Details</a>
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

        </div>
    </div>
    
  
</asp:Content>
