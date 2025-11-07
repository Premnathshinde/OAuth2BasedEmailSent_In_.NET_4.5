<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Alert.aspx.cs" Inherits="EmailAlertUsingToken.Alert" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button ID="btnSendEmail" runat="server" Text="Alert Shoot" OnClick="btnSendEmail_Click" />
            <br />
            <br />
            <asp:Label ID="lblStatus" runat="server" Text="" />
        </div>
    </form>
</body>
</html>
