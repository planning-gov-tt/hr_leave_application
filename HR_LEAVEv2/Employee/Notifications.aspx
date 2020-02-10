﻿<%@ Page Title="Notifications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="HR_LEAVEv2.Employee.Notifications" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>
    <asp:ListView ID="ListView1" runat="server" OnPagePropertiesChanging="ListView1_PagePropertiesChanging" GroupItemCount="5" Style="height: 85%;">
        <EmptyDataTemplate>
            <div class="alert alert-info text-center" role="alert" style="width: 30%; margin: auto">
                <i class="fa fa-info-circle"></i>
                No notifications available
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
            <div class="panel panel-info">
                <div class="panel-heading">
                    <span style="font-size:1.1em;">
                        <%#Eval("notification_header") %>
                        <span class="label <%#Eval("bootstrap_class") %>"><%#Eval("status") %></span>
                    </span>
                    <span style="float:right">
                        <asp:LinkButton ID="readBtn" runat="server" Style="margin-right:5px;" OnClick="readBtn_Click" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-check-circle-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Mark as Read" aria-hidden="true" style="color:#47a447; border-color: #398439;"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="unreadBtn" runat="server" Style="margin-right:5px;" OnClick="unreadBtn_Click" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-times-circle-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Mark as Unread" aria-hidden="true" style="color:#d2322d; border-color: #ac2925;"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="deleteBtn" runat="server" OnClick="deleteBtn_Click" OnClientClick ="return confirm('Delete notification?');" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-trash-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete" aria-hidden="true"></i>
                        </asp:LinkButton>
                    </span>
                </div>
                <div class="panel-body" >
                    <%#Eval("notification") %>
                </div>
            </div>
        </ItemTemplate>

        <AlternatingItemTemplate>
            <div class="panel panel-info">
                <div class="panel-heading">
                    <span style="font-size:1.1em;">
                        <%#Eval("notification_header") %>
                        <span class="label <%#Eval("bootstrap_class") %>"><%#Eval("status") %></span>
                    </span>
                    <span style="float:right">
                        <asp:LinkButton ID="readBtn" runat="server" Style="margin-right:5px;" OnClick="readBtn_Click" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-check-circle-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Mark as Read" aria-hidden="true" style="color:#47a447; border-color: #398439;"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="unreadBtn" runat="server" Style="margin-right:5px;" OnClick="unreadBtn_Click" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-times-circle-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Mark as Unread" aria-hidden="true" style="color:#d2322d; border-color: #ac2925;"></i>
                        </asp:LinkButton>
                        <asp:LinkButton ID="deleteBtn" runat="server" OnClick="deleteBtn_Click" OnClientClick ="return confirm('Delete notification?');" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-trash-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete" aria-hidden="true"></i>
                        </asp:LinkButton>
                    </span>
                </div>
                <div class="panel-body" >
                    <%#Eval("notification") %>
                </div>
            </div>
        </AlternatingItemTemplate>
        

        <LayoutTemplate>
            <div id="groupPlaceholderContainer" runat="server" style="width: 50%; margin: 0 auto;">
                <div id="groupPlaceholder" runat="server"></div>
            </div>
        </LayoutTemplate>

    </asp:ListView>
    <asp:DataPager ID="DataPager1" PagedControlID="ListView1" PageSize="5" runat="server">
        <Fields>
            <asp:NumericPagerField ButtonType="Link" />
        </Fields>
    </asp:DataPager>
</asp:Content>
