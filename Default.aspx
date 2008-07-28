<%@ Page Language="C#" AutoEventWireup="true"  CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title>Untitled Page</title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:Wizard ID="SuperCopyWizard" runat="server" ActiveStepIndex="2" Height="336px" Width="655px" OnNextButtonClick="SuperCopyWizard_NextButtonClick" BackColor="#E6E2D8" BorderColor="#999999" BorderStyle="Solid" BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" OnFinishButtonClick="SuperCopyWizard_FinishButtonClick" OnActiveStepChanged="SuperCopyWizard_ActiveStepChanged">
            <WizardSteps>
                    <asp:WizardStep runat="server" Title="Select" ID="Start">
                        <table>
                            <tr>
                                <td colspan="2">
                                    Current Channel:<br />
                                    &nbsp;<asp:DropDownList ID="uxChannel" runat="server">
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr valign =top>
                                <td>
                                    Source Day:<br />
                                    <asp:Calendar ID="uxSourceDay" runat="server" OnSelectionChanged="uxStartDate_SelectionChanged">
                                        <TodayDayStyle BackColor="Silver" />
                                        <SelectedDayStyle BackColor="Navy" />
                                    </asp:Calendar>
                                    <asp:Label ID="uxStartDateError" runat="server" ForeColor="Red"></asp:Label>
                                    <br />
                                    <asp:Label ID="uxNumberOfSourceRuns" runat="server" Text="Please select a source day to continue."></asp:Label>
                                </td>
                                <td>
                                    End date:<br />
                                    <asp:Calendar ID="uxEndDate" runat="server" OnSelectionChanged="uxEndDate_SelectionChanged">
                                        <TodayDayStyle BackColor="Silver" />
                                        <SelectedDayStyle BackColor="Navy" />
                                    </asp:Calendar>
                                    <asp:Label ID="uxEndDateError" runat="server" ForeColor="Red"></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td colspan="2">
                                    <asp:Label ID="uxDateCount" runat="server"></asp:Label>
                                    &nbsp;
                                </td>
                            </tr>
                        </table>
                </asp:WizardStep>
                
                <asp:WizardStep ID="Confirm" runat="server" Title="Confirm">
                    SuperCopy is ready to run.<br />
                    <br />
                    You are about to copy the programing on
                    <asp:Label ID="uxSourceDateName" runat="server"></asp:Label>
                    &nbsp;on
                    <asp:Label ID="uxCurrentChannelName" runat="server"></asp:Label>
                    .<br />
                    <p style="font-weight:bold; color:Red">The programing on 
                    <asp:Label ID="uxChannelName2" runat="server"></asp:Label>
                    &nbsp;will be deleted, and replaced, from
                    <asp:Label ID="uxDestStartDate" runat="server"></asp:Label>
                    &nbsp;to
                    <asp:Label ID="uxDestEndDate" runat="server"></asp:Label>
                    .<p/>
                    If you are sure you want to do this, press Next</asp:WizardStep>
                <asp:WizardStep ID="Review" runat="server" Title="Review">
                    Removed
                    <asp:Label ID="uxRemovedCount" runat="server"></asp:Label>
                    &nbsp;runs from the schedule.<br />
                    <br />
                    Added
                    <asp:Label ID="uxAddedCount" runat="server"></asp:Label>
                    runs to the schedule.<br />
                    <br />
                    <asp:Label ID="uxError" runat="server" ForeColor="Red"></asp:Label>
                    <br />
                    <br />
                    Click finish to return to the main menu.</asp:WizardStep>
            </WizardSteps>
            <HeaderTemplate>
                Welcome to SuperCopy
            </HeaderTemplate>
            <StepStyle BackColor="#F7F6F3" BorderColor="#E6E2D8" BorderStyle="Solid" BorderWidth="2px" />
            <SideBarStyle BackColor="#1C5E55" Font-Size="0.9em" VerticalAlign="Top" Width="100px"/>
            <NavigationButtonStyle BackColor="White" BorderColor="#C5BBAF" BorderStyle="Solid"
                BorderWidth="1px" Font-Names="Verdana" Font-Size="0.8em" ForeColor="#1C5E55" />
            <SideBarButtonStyle ForeColor="White" />
            <HeaderStyle BackColor="#666666" BorderColor="#E6E2D8" BorderStyle="Solid" BorderWidth="2px"
                Font-Bold="True" Font-Size="0.9em" ForeColor="White" HorizontalAlign="Center" />
        </asp:Wizard>
        &nbsp;</div>
    </form>
</body>
</html>
