<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_Leave.MainGridView" %>

<h1>This is the main GridView</h1>

<asp:GridView AutoGenerateColumns="false" ID="MainGridView" runat="server">
    <Columns>
        <asp:BoundField HeaderText="ID" DataField="employee_id" />
    </Columns>
</asp:GridView>