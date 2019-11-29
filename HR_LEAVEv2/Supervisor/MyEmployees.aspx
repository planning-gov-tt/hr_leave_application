<%@ Page Title="My Employees" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="MyEmployees.aspx.cs" Inherits="HR_LEAVEv2.Supervisor.MyEmployees" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        .test{
            background-color:red;
        }
    </style>
    <h1><%: Title %></h1>
    <div class="container-fluid">
        <div class="container" style="width:65%;">
            <div class="row" style="margin-top:25px;">
                <div class="col-lg-8" >
                    <div class="input-group" style="width:510px;">
                        <input type="text" class="form-control" placeholder="Search Employee" aria-label="Search Employee" aria-describedby="basic-addon2">
                        <div class="input-group-addon">
                            <span class="input-group-text" id="basic-addon2">
                                <i class="fa fa-search"></i>
                            </span>
                        </div>
                    </div>
                
                </div>
                <div class="col-lg-4">
                    <label for="filterEmpSearchBy" style="font-size:1.0em">Filter by:</label>
                    <asp:DropDownList ID="filterEmpSearchBy" runat="server" CssClass="form-control" Width="150px" style="display:inline;" >
                        <asp:ListItem Value=""></asp:ListItem>
                        <asp:ListItem Value="Name"></asp:ListItem>
                        <asp:ListItem Value="Position"></asp:ListItem>
                    </asp:DropDownList>
                
                </div>
            </div>
        </div>
        <div class="container" style="width:85%; margin-top:35px;">
            <asp:ListView ID="ListView1" runat="server" OnPagePropertiesChanging="ListView1_PagePropertiesChanging" GroupItemCount="4" GroupPlaceholderID="groupPlaceHolder1" ItemPlaceholderID="itemPlaceHolder1">  
                <EmptyDataTemplate>
                    <table runat="server" style="">
                        <tr>
                            <td></td>
                        </tr>
                    </table>
                </EmptyDataTemplate>
                <LayoutTemplate>
                    <div class="row text-center">
                        <asp:PlaceHolder runat="server" ID="groupPlaceHolder1"></asp:PlaceHolder>
                    </div>
                </LayoutTemplate>
                <GroupTemplate>
                    <div class="row text-center">
                        <asp:PlaceHolder runat="server" ID="itemPlaceHolder1"></asp:PlaceHolder>
                    </div>
                </GroupTemplate>
                <ItemTemplate>
                    <div class="col-lg-3">
                        <div class="card test">
                            <div class="card-header">
                                <asp:Label runat="server" ID="Label4" Text='<%#Eval("Name") %>'></asp:Label>
                            </div>
                            <div class="card-body">
                                <asp:Label runat="server" ID="Label3" Text='<%#Eval("ID") %>'></asp:Label>  
                                <asp:Label runat="server" ID="Label2" Text='<%#Eval("Mobile") %>'></asp:Label>  
                                <asp:Label runat="server" ID="lblName" Text='<%#Eval("College") %>'></asp:Label>  
                            </div>
                            <div class="card-footer">
                                Footer
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
        <%--<EmptyDataTemplate>
                    <table >
                        <tr>
                            <td>No data was returned.</td>
                        </tr>
                    </table>
                </EmptyDataTemplate>
                <EmptyItemTemplate>
                    <td/>
                </EmptyItemTemplate>
                <GroupTemplate>
                    <tr id="itemPlaceholderContainer" runat="server">
                        <td id="itemPlaceholder" runat="server"></td>
                    </tr>
                </GroupTemplate>

                <ItemTemplate>  
                    <td>  
                        <div class="card">
                            <h5 class="card-header"><asp:Label runat="server" ID="Label4" Text='<%#Eval("Name") %>'></asp:Label>  </h5>
                            <div class="card-body">
                                <asp:Label runat="server" ID="Label3" Text='<%#Eval("ID") %>'></asp:Label>  
                                <asp:Label runat="server" ID="Label2" Text='<%#Eval("Mobile") %>'></asp:Label>  
                                <asp:Label runat="server" ID="lblName" Text='<%#Eval("College") %>'></asp:Label>  
                                <a href="#" class="btn btn-primary">Go somewhere</a>
                            </div>
                        </div>
                    </td>  
                </ItemTemplate>  
                <AlternatingItemTemplate>  
                    <td>  
                        <asp:Label runat="server" ID="Label3" Text='<%#Eval("ID") %>'></asp:Label>  
                        <asp:Label runat="server" ID="Label1" Text='<%#Eval("Name") %>'></asp:Label>  
                        <asp:Label runat="server" ID="Label2" Text='<%#Eval("Mobile") %>'></asp:Label>  
                        <asp:Label runat="server" ID="lblName" Text='<%#Eval("College") %>'></asp:Label>  
                        <br />  
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
                </LayoutTemplate>--%>

            </asp:ListView> 
            <asp:DataPager ID="DataPager1" PagedControlID="ListView1" PageSize="4" runat="server">  
                <Fields>  
                    <asp:NumericPagerField ButtonType="Link" />  
                </Fields>  
            </asp:DataPager> 

        </div>
    </div>
    
  
</asp:Content>
