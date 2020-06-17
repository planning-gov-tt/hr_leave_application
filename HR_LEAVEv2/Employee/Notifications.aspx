<%@ Page Title="Notifications" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Notifications.aspx.cs" Inherits="HR_LEAVEv2.Employee.Notifications" MaintainScrollPositionOnPostback="true" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
    <h1><%: Title %></h1>

    <asp:Panel ID="notifsPanel" runat="server" Style="height: 85%; width: 50%; margin: 0 auto; margin-top: 30px; text-align:center">

        <asp:LinkButton ID="markAllAsRead" runat="server" CssClass="btn btn-primary" Style="margin-bottom:15px; display:inline-block;" OnClick="markAllAsRead_Click">
            <i class="fa fa-check" aria-hidden="true"></i>
            Mark all as read
        </asp:LinkButton>

        <asp:LinkButton ID="deleteAllNotifsBtn" runat="server" OnClick="deleteAllNotifsBtn_Click" OnClientClick="return confirm('Delete all notifications?');" CssClass="btn btn-danger" Style="margin-bottom:15px; display:inline-block;">
            <i class="fa fa-trash" aria-hidden="true"></i>
            Clear all notifications
        </asp:LinkButton>    

        <asp:LinkButton ID="markAllAsUnread" runat="server" CssClass="btn btn-primary" Style="margin-bottom:15px; display:inline-block;" OnClick="markAllAsUnread_Click">
            <i class="fa fa-times" aria-hidden="true"></i>
            Mark all as unread
        </asp:LinkButton>

        <asp:ListView ID="notificationsListView" runat="server" OnPagePropertiesChanging="notificationsListView_PagePropertiesChanging" GroupItemCount="10" Style="text-align:left;">
            <EmptyDataTemplate>
                <div class="alert alert-info text-center" role="alert" style="display:inline-block; margin:0 auto">
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
                <div class="panel panel-info" style="text-align:left;">
                    <div class="panel-heading">
                        <span style="font-size: 1.1em;">
                            <%#Eval("notification_header") %>
                            <span class="label <%#Eval("bootstrap_class") %>"><%#Eval("status") %></span>
                        </span>
                        <span style="float: right">
                            <asp:LinkButton ID="readBtn" runat="server" Style="margin-right: 5px;" OnClick="readBtn_Click" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-check-circle-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Mark as Read" aria-hidden="true" style="color:#47a447; border-color: #398439;"></i>
                            </asp:LinkButton>
                            <asp:LinkButton ID="unreadBtn" runat="server" Style="margin-right: 5px;" OnClick="unreadBtn_Click" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-times-circle-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Mark as Unread" aria-hidden="true" style="color:#d2322d; border-color: #ac2925;"></i>
                            </asp:LinkButton>
                            <asp:LinkButton ID="deleteBtn" runat="server" OnClick="deleteBtn_Click" OnClientClick="return confirm('Delete notification?');" data-id='<%#Eval("id") %>'>
                            <i class="fa fa-trash-o fa-2x content-tooltipped" data-toggle="tooltip" data-placement="top" title="Delete" aria-hidden="true"></i>
                            </asp:LinkButton>
                        </span>
                        <br />
                        <%#Eval("created_at") %>
                    </div>
                    <div class="panel-body">
                        <%#Eval("notification") %>
                    </div>
                </div>
            </ItemTemplate>

            <LayoutTemplate>
                <div id="groupPlaceholderContainer" runat="server">
                    <div id="groupPlaceholder" runat="server"></div>
                </div>
            </LayoutTemplate>

        </asp:ListView>

        <asp:DataPager ID="notificationsDataPager" PagedControlID="notificationsListView" PageSize="10" runat="server" >
            <Fields>
                <asp:NumericPagerField ButtonType="Link" />
            </Fields>
        </asp:DataPager>

    </asp:Panel>
    
</asp:Content>
