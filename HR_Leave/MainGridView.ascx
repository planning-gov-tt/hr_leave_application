<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="MainGridView.ascx.cs" Inherits="HR_Leave.MainGridView" %>


<asp:GridView AutoGenerateColumns="false" ID="GridView" runat="server">
    <Columns>
        <%--<asp:BoundField HeaderText="ID" DataField="employee_id" />--%>
                
        <%--<asp:TemplateField HeaderText="Name" SortExpression="LastName">
        <ItemTemplate>
           <asp:Label ID="lblName" runat="server" Text='<%# Eval("first_name").ToString()[0] +". "+ Eval("last_name")%>' ></asp:Label>
        </ItemTemplate>
        </asp:TemplateField>--%>

        <asp:BoundField HeaderText="employee_id" DataField="employee_id" InsertVisible="false" />
        

    </Columns>
</asp:GridView>