using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class _Default : System.Web.UI.Page 
{
	private bool m_SuppressDateChangeEvent = false;

    protected void Page_Load(object sender, EventArgs e)
    {
		PopulateChannelList();
    }

	private void PopulateChannelList()
	{
		System.Data.SqlClient.SqlConnection SqlConn = new System.Data.SqlClient.SqlConnection();
		System.Data.SqlClient.SqlDataReader SqlDr = null;

		try
		{
			SqlConn.ConnectionString = "Data Source=(local);Initial Catalog=Cablecast40;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096";
			SqlConn.Open();

			//Get the channels from the database
			System.Data.SqlClient.SqlCommand SqlCmd
				= new System.Data.SqlClient.SqlCommand(
					"SELECT * FROM stChannels;", SqlConn);

			//set data reader
			SqlDr = SqlCmd.ExecuteReader();

			uxChannel.Items.Clear();

			while (SqlDr.Read())
			{
				ListItem Item = new ListItem((string)SqlDr["ChannelName"], (SqlDr["ChannelID"]).ToString());
				uxChannel.Items.Add(Item);
			}
		}
		catch(Exception exc)
		{
		}
	}

	protected void SuperCopyWizard_NextButtonClick(object sender, WizardNavigationEventArgs e)
	{
		if(SuperCopyWizard.ActiveStepIndex == 0)
			uxCurrentChannel.Text = uxChannel.SelectedItem.Text;

		if (SuperCopyWizard.ActiveStepIndex == 2)
		{
			uxChannelName1.Text = uxChannel.SelectedItem.Text;
			uxChannelName2.Text = uxChannel.SelectedItem.Text;
			uxSourceDateName.Text = uxSourceDate.SelectedDate.ToShortDateString();
			uxDestStartDate.Text = uxStartDate.SelectedDate.ToShortDateString();
			uxDestEndDate.Text = uxEndDate.SelectedDate.ToShortDateString();
		}

		if (SuperCopyWizard.ActiveStepIndex == 3)
		{
			int RemovedRuns = 
				ClearSchedule(
					int.Parse(uxChannel.SelectedValue),
					uxStartDate.SelectedDate,
					uxEndDate.SelectedDate);

			uxRemovedRunCount.Text = RemovedRuns.ToString();

			int AddedRuns = 
				CopySchedule(
					int.Parse(uxChannel.SelectedValue),
					uxSourceDate.SelectedDate,
					uxStartDate.SelectedDate,
					uxEndDate.SelectedDate);
		}
	}



	private int ClearSchedule(int channelID, DateTime startDate, DateTime endDate)
	{
		int result = 0;

		using(System.Data.SqlClient.SqlConnection SqlConn = new System.Data.SqlClient.SqlConnection())
		{
			SqlConn.ConnectionString = "Data Source=(local);Initial Catalog=Cablecast40;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096";
			SqlConn.Open();

			string sql = @"
				DELETE FROM schedule
				WHERE channelID = " + channelID + @"
				AND runDateTime >= DATEADD(dd,0, datediff(dd,0,'" + startDate.ToShortDateString() + @"'))
				AND runDateTime <= DATEADD([day],1, dateadd(dd,0, datediff(dd,0,'" + endDate.ToShortDateString() + @"')))";

			System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand(sql, SqlConn);

			 result = sqlCmd.ExecuteNonQuery();
		}

		return result;
	}

	private int CopySchedule(int channelID, DateTime sourceDay, DateTime startDate, DateTime endDate)
	{
		int result = 0;

		using (System.Data.SqlClient.SqlConnection SqlConn = new System.Data.SqlClient.SqlConnection())
		{
			SqlConn.ConnectionString = "Data Source=(local);Initial Catalog=Cablecast40;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096";
			SqlConn.Open();

			int numberOfDays = (int)((endDate - startDate).TotalDays + 1);

			for (int i = 1; i <= numberOfDays; i++)
			{
				string sql = @"
				INSERT INTO schedule (ChannelID, ShowID, RunDateTime, RunBump, RunLock, CGExempt, IDType)
					SELECT ChannelID, ShowID, DATEADD([day], " + i + @", RunDateTime), RunBump, RunLock, CGExempt, IDType
					FROM Schedule
					WHERE (Schedule.ChannelID = " + channelID + @")
					AND runDateTime >= DATEADD(dd,0, datediff(dd,0,'" + sourceDay.ToShortDateString() + @"'))
					AND runDateTime <= DATEADD([day],1, dateadd(dd,0, datediff(dd,0,'" + sourceDay.ToShortDateString() + @"')))
					AND (IDType = 1);";

				System.Data.SqlClient.SqlCommand sqlCmd = new System.Data.SqlClient.SqlCommand(sql, SqlConn);

				result += sqlCmd.ExecuteNonQuery();
			}
		}

		return result;
	}

	protected void uxSourceDate_SelectionChanged(object sender, EventArgs e)
	{
		uxNumberOfSourceRuns.Text = NumberOfRunsDescriptions(GetNumberOfRuns(uxSourceDate.SelectedDate));
	}

	private int GetNumberOfRuns(DateTime Date)
	{
		System.Data.SqlClient.SqlConnection SqlConn = new System.Data.SqlClient.SqlConnection();
		System.Data.SqlClient.SqlDataReader SqlDr = null;

		try
		{
			SqlConn.ConnectionString = "Data Source=(local);Initial Catalog=Cablecast40;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096";
			SqlConn.Open();


		string sql = @"
SELECT Count(*)
FROM Schedule
WHERE (DATEADD([second], Schedule.RunBump, Schedule.RunDateTime) >= dateadd(dd,0, datediff(dd,0,'" + Date.ToShortDateString() + @"'))) 
AND (DATEADD([second], Schedule.RunBump, Schedule.RunDateTime) <= DATEADD([day],1, dateadd(dd,0, datediff(dd,0,'" + Date.ToShortDateString() + @"')))) 
AND (Schedule.ChannelID = " + int.Parse(uxChannel.SelectedValue) + @") ";

			//Get the channels from the database
			System.Data.SqlClient.SqlCommand SqlCmd = new System.Data.SqlClient.SqlCommand(sql, SqlConn);

			//set data reader
			SqlDr = SqlCmd.ExecuteReader();

			if (SqlDr.Read())
			{
				return (int)SqlDr[0];
			}
		}
		catch (Exception exc)
		{
		}

		return 0;
	}

	private string NumberOfRunsDescriptions(int NumberOfRuns)
	{
		string result = string.Empty;

		if (NumberOfRuns <= 0)
			result = "There are no runs on the selected day.";
		if (NumberOfRuns == 1)
			result = "There is one run on the selected day.";
		if (NumberOfRuns > 1)
			result = string.Format("There are {0} runs on the selected day.", NumberOfRuns);

		return result;
	}

	protected void uxStartDate_SelectionChanged(object sender, EventArgs e)
	{
		ValidateDateRange();
	}

	protected void uxEndDate_SelectionChanged(object sender, EventArgs e)
	{
		ValidateDateRange();
	}

	private void ValidateDateRange()
	{
		if (m_SuppressDateChangeEvent == false) //If we don't do this check, it will clear the message out right away.
		{
			if (uxStartDate.SelectedDates.Count == 0)
			{
				uxStartDateError.Text = "You must select a start date.";
				uxDateCount.Text = string.Empty;
			}
			else if (uxEndDate.SelectedDates.Count == 0)
			{
				uxEndDateError.Text = "You must select an end date.";
				uxDateCount.Text = string.Empty;
			}
			else if (uxEndDate.SelectedDate < uxStartDate.SelectedDate)
			{
				m_SuppressDateChangeEvent = true;
				uxEndDateError.Text = "End date must be on or after the start date.";
				uxEndDate.SelectedDates.Clear();
				m_SuppressDateChangeEvent = false;
			}
			else if (uxEndDate.SelectedDate >= uxStartDate.SelectedDate)
			{
				uxEndDateError.Text = string.Empty;
				uxStartDateError.Text = string.Empty;

				uxDateCount.Text = GetDateSelectionDescription();
			}
		}

	}

	/// <summary>
	/// Provides a description of the number of days selected in the form
	/// "There are 'n' days selected"
	/// </summary>
	/// <returns>Empty string if there are no days selected</returns>
	private string GetDateSelectionDescription()
	{
		if ((uxEndDate.SelectedDate - uxStartDate.SelectedDate).TotalDays < 1)
			return string.Empty;
		
		if ((uxEndDate.SelectedDate - uxStartDate.SelectedDate).TotalDays == 0)
			return "There is one day selected.";

		return string.Format("There are {0} days selected.", (uxEndDate.SelectedDate - uxStartDate.SelectedDate).TotalDays + 1);
	}
}

