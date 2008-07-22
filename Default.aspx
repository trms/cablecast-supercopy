<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Wizard ID="SuperCopyWizard" runat="server" ActiveStepIndex="0" Height="508px" Width="655px" OnNextButtonClick="SuperCopyWizard_NextButtonClick">
            <WizardSteps>
                <asp:WizardStep runat="server" Title="Step 1" ID="Step1">
                    Please select a Channel<br />
                    <asp:DropDownList ID="uxChannel" runat="server">
                    </asp:DropDownList>
                </asp:WizardStep>
                <asp:WizardStep runat="server" Title="Step 2" ID="Step2">
                    Current Channel:<br />
                    <asp:Label ID="uxCurrentChannel" runat="server" Text="None"></asp:Label>
                    <br />
                    <br />
                    Please select the source day:<br />
                    <asp:Calendar ID="uxSourceDate" runat="server" OnSelectionChanged="uxSourceDate_SelectionChanged">
                        <SelectedDayStyle BackColor="Navy" />
                    </asp:Calendar>
                    <br />
                    <asp:Label ID="uxNumberOfSourceRuns" runat="server" Text="Please select a source day to continue."></asp:Label>
                </asp:WizardStep>
                <asp:WizardStep runat="server" Title="Step 3" ID="Step3">
                    Please select destination dates<br />
                    <br />
                    Start:<br />
                    <asp:Calendar ID="uxStartDate" runat="server" OnSelectionChanged="uxStartDate_SelectionChanged">
                        <TodayDayStyle BackColor="Silver" />
                        <SelectedDayStyle BackColor="Navy" />
                    </asp:Calendar>
                    <asp:Label ID="uxStartDateError" runat="server" ForeColor="Red"></asp:Label>
                    <br />
                    <br />
                    <br />
                    End:<asp:Calendar ID="uxEndDate" runat="server" OnSelectionChanged="uxEndDate_SelectionChanged">
                        <TodayDayStyle BackColor="Silver" />
                        <SelectedDayStyle BackColor="Navy" />
                    </asp:Calendar>
                    <asp:Label ID="uxEndDateError" runat="server" ForeColor="Red"></asp:Label>
                    <br />
                    <br />
                    <asp:Label ID="uxDateCount" runat="server"></asp:Label>
                    <br />
                </asp:WizardStep>
                <asp:WizardStep ID="Step4" runat="server" Title="Step 4">
                    SuperCopy is ready to run.<br />
                    <br />
                    You are about to copy the programing on
                    <asp:Label ID="uxSourceDateName" runat="server"></asp:Label>
                    on
                    <asp:Label ID="uxChannelName1" runat="server"></asp:Label>
                    .<br />
                    <br />
                    The programing on 
                    <asp:Label ID="uxChannelName2" runat="server"></asp:Label>
                    &nbsp;will be deleted, and replaced, from
                    <asp:Label ID="uxDestStartDate" runat="server"></asp:Label>
                    &nbsp;to
                    <asp:Label ID="uxDestEndDate" runat="server"></asp:Label>
                    .<br />
                    <br />
                    If you are sure you want to do this, press Next</asp:WizardStep>
                <asp:WizardStep ID="Step5" runat="server" Title="Confirm">
                    Removed
                    <asp:Label ID="uxRemovedRunCount" runat="server"></asp:Label>
                    &nbsp;runs from the schedule.</asp:WizardStep>
            </WizardSteps>
            <HeaderTemplate>
                Welcome to SuperCopy
            </HeaderTemplate>
        </asp:Wizard>
        &nbsp;</div>
    </form>
</body>
</html>
