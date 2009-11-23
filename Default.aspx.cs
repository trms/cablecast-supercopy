using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Transactions;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

public partial class _Default : System.Web.UI.Page
{
	/*----------------------------------------------------------
	 * Cablecast 'Super Copy' Plugin
	 * Designed and tested with Database Schema: 4.6.0
	 * ---------------------------------------------------------*/
	
	#region Variables
	private bool m_SuppressDateChangeEvent = false;
	private readonly string m_ConnectionString = "Data Source=(local);Initial Catalog=Cablecast40;Integrated Security=SSPI;Persist Security Info=False;Packet Size=4096";
	#endregion

	#region Event Handlers
	protected void Page_Load(object sender, EventArgs e)
    {
		//Get the list of Channels from the Cablecast Database, add them to the Drop Down
		if(SuperCopyWizard.ActiveStepIndex == 0 && this.IsPostBack == false)
			PopulateChannelList();

		//Since we are not ready to continue, disable the Next Button for now.
		NextButtonEnabled(false);
	}

	protected void SuperCopyWizard_NextButtonClick(object sender, WizardNavigationEventArgs e)
	{
		if (SuperCopyWizard.ActiveStepIndex == 0)
		{
			//The user clicked next on the first step, get the second step ready to go
			//Save the selected channel name
			m_CurrentChannelID = int.Parse(uxChannel.SelectedValue);
			m_CurrentChannelName = uxChannel.SelectedItem.Text;

			//Set the text of the various labels
			uxChannelName2.Text = m_CurrentChannelName;
			uxCurrentChannelName.Text = m_CurrentChannelName;
			uxSourceDateName.Text = uxSourceDay.SelectedDate.ToShortDateString();
			uxDestStartDate.Text = uxSourceDay.SelectedDate.AddDays(1).ToShortDateString();
			uxDestEndDate.Text = uxEndDate.SelectedDate.ToShortDateString();

			//Set the highest step completed
			m_StepCompleted = 1;
		}

		if (SuperCopyWizard.ActiveStepIndex == 1)
		{
			//The user clicked next on the second step, actually perform the manipulation

			ClearAndCopyRuns();

			//Set the highest step completed
			m_StepCompleted = 2;
		}
	}

	private void ClearAndCopyRuns()
	{
		using (TransactionScope scope = new TransactionScope())
		{
			try
			{
				//Remove the runs from the schedule for the selected days.
				int RemovedRuns =
					ClearSchedule(
						m_CurrentChannelID,
						uxSourceDay.SelectedDate.AddDays(1),
						uxEndDate.SelectedDate);

				//Set the label for the number of runs removed
				uxRemovedCount.Text = RemovedRuns.ToString();

				//Copy the schedule for the selected days
				int AddedRuns =
					CopySchedule(
						m_CurrentChannelID,
						uxSourceDay.SelectedDate,
						uxEndDate.SelectedDate);

				//Set the label for the number of runs added
				uxAddedCount.Text = AddedRuns.ToString();

				scope.Complete();
			}
			catch (Exception exc)
			{
				uxRemovedCount.Text = "0";
				uxAddedCount.Text = "0";
				uxError.Text = "An error occured.  No changes were made to the database.  Error details: " + exc.Message;
			}
		}
	}

	protected void uxSourceDate_SelectionChanged(object sender, EventArgs e)
	{
		uxNumberOfSourceRuns.Text = NumberOfRunsDescription(
			GetNumberOfRuns(m_CurrentChannelID, uxSourceDay.SelectedDate));
	}

	protected void uxStartDate_SelectionChanged(object sender, EventArgs e)
	{
		//When the source day changes, we want to make sure its a valid combination
		ValidateDateRange();
	}

	protected void uxEndDate_SelectionChanged(object sender, EventArgs e)
	{
		//When the source day changes, we want to make sure its a valid combination		
		ValidateDateRange();
	}

	protected void SuperCopyWizard_FinishButtonClick(object sender, WizardNavigationEventArgs e)
	{
		Response.Redirect("/Cablecast/Default.aspx");
	}

	protected void SuperCopyWizard_ActiveStepChanged(object sender, EventArgs e)
	{
		//The pops the user back to the step after their highest completed step
		if (SuperCopyWizard.ActiveStepIndex > m_StepCompleted)
			SuperCopyWizard.ActiveStepIndex = m_StepCompleted;
	}
	#endregion

	#region Data Manipulation Methods
	/// <summary>
	/// Clears a portion of the cablecast schedule
	/// </summary>
	/// <param name="channelID">The channelID to clear</param>
	/// <param name="startDate">The start date to clear from.  All shows from 0:00:00 on will be removed</param>
	/// <param name="endDate">The end date to clear to.  All shows until 23:59:59 will be removed</param>
	/// <returns>The number of runs removed</returns>
	private int ClearSchedule(int channelID, DateTime startDate, DateTime endDate)
	{
		int result = 0;

		using(SqlConnection SqlConn = new SqlConnection())
		{
			SqlConn.ConnectionString = m_ConnectionString;
			SqlConn.Open();

			//This SQL deletes from the schedule from 0:00:00 on the start day, until 23:59:59 on the end day, on the specified channel
			string sql = @"
				DELETE FROM schedule
				WHERE channelID = " + channelID + @"
				AND runDateTime >= DATEADD(dd,0, datediff(dd,0,'" + startDate.ToShortDateString() + @"'))
				AND runDateTime <= DATEADD([day],1, dateadd(dd,0, datediff(dd,0,'" + endDate.ToShortDateString() + @"')))";

			SqlCommand sqlCmd = new SqlCommand(sql, SqlConn);

			//Run the command
			result = sqlCmd.ExecuteNonQuery();
		}

		return result;
	}

	/// <summary>
	/// Copies all programing from a specific channel on a specific date, to a range of dates.
	/// </summary>
	/// <param name="channelID">The channelID from the Schedule</param>
	/// <param name="sourceDay">The date which to copy from</param>
	/// <param name="endDate">The last day for which schedule should be added</param>
	/// <returns></returns>
	private int CopySchedule(int channelID, DateTime sourceDay, DateTime endDate)
	{
		int result = 0;

		using (SqlConnection SqlConn = new SqlConnection())
		{
			SqlConn.ConnectionString = m_ConnectionString;
			SqlConn.Open();
			SqlCommand sqlCmd;

			//Get the number of days we need to add
			int numberOfDays = (int)((endDate - sourceDay).TotalDays);

			//We need to run one SQL statement per day
			for (int i = 1; i <= numberOfDays; i++)
			{
				//The magic
				string sql = @"
					INSERT INTO schedule (ChannelID, ShowID, RunDateTime, RunBump, RunLock, CGExempt, IDType)
						SELECT ChannelID, ShowID, DATEADD([day], " + i + @", RunDateTime), RunBump, RunLock, CGExempt, IDType
						FROM Schedule
						WHERE (Schedule.ChannelID = " + channelID + @")
						AND runDateTime >= DATEADD(dd,0, datediff(dd,0,'" + sourceDay.ToShortDateString() + @"'))
						AND runDateTime < DATEADD([day],1, dateadd(dd,0, datediff(dd,0,'" + sourceDay.ToShortDateString() + @"')))
						AND (IDType = 1);";

				sqlCmd = new SqlCommand(sql, SqlConn);

				//Add the number of affected rows up, since we are looping through the days
				result += sqlCmd.ExecuteNonQuery();
			}
		}

		return result;
	}

	/// <summary>
	/// Calculates the number of runs on the schedule for a specific day
	/// </summary>
	/// <param name="Date">The date to calculate for</param>
	/// <returns>The number of runs</returns>
	private int GetNumberOfRuns(int channelID, DateTime date)
	{
		int result = 0;

		using (SqlConnection SqlConn = new SqlConnection())
		{
			SqlDataReader SqlDr = null;

			SqlConn.ConnectionString = m_ConnectionString;
			SqlConn.Open();

			string sql = @"
				SELECT Count(*)
				FROM Schedule
				WHERE (DATEADD([second], Schedule.RunBump, Schedule.RunDateTime) >= dateadd(dd,0, datediff(dd,0,'" + date.ToShortDateString() + @"'))) 
				AND (DATEADD([second], Schedule.RunBump, Schedule.RunDateTime) <= DATEADD([day],1, dateadd(dd,0, datediff(dd,0,'" + date.ToShortDateString() + @"')))) 
				AND (Schedule.ChannelID = " + channelID + @") ";

			//Get the channels from the database
			SqlCommand SqlCmd = new SqlCommand(sql, SqlConn);

			//set data reader
			SqlDr = SqlCmd.ExecuteReader();

			//If we were able to get the count, we want to return it.  If this fails, the count will be returned as zero.
			if (SqlDr.Read())
				result = (int)SqlDr[0];
		}

		return result;
	}
	#endregion

	#region Support Methods
	private void PopulateChannelList()
	{
		using (SqlConnection SqlConn = new SqlConnection())
		{
			SqlDataReader SqlDr = null;

			SqlConn.ConnectionString = m_ConnectionString;
			SqlConn.Open();

			//Get the channels from the database
			SqlCommand SqlCmd = new SqlCommand("SELECT * FROM stChannels;", SqlConn);

			//set data reader
			SqlDr = SqlCmd.ExecuteReader();

			//Remove any existing channels from the list
			uxChannel.Items.Clear();

			while (SqlDr.Read())
			{
				//Add each item to the drop down, as a Name Value pair
				ListItem Item = new ListItem(
					SqlDr["ChannelName"].ToString(),
					(SqlDr["ChannelID"]).ToString());
				uxChannel.Items.Add(Item);
			}
		}
	}

	/// <summary>
	/// Gennerates a bit of text to describe how many runs are on the schedule for a specific day
	/// </summary>
	/// <param name="NumberOfRuns">The number of runs</param>
	/// <returns>A string such as "There are 'n' runs on the selected day."</returns>
	private string NumberOfRunsDescription(int NumberOfRuns)
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

	/// <summary>
	/// Validates the selected dates, changes the state of the form accordingly
	/// </summary>
	private void ValidateDateRange()
	{
		if (m_SuppressDateChangeEvent == false) //If we don't do this check, it will clear the message out right away.
		{
			//We will assume that the dates as bad for now
			//This is modified later if the values are good
			NextButtonEnabled(false);
			uxDateCount.Visible = false;

			//Check if the source calendar has a selected day
			if (uxSourceDay.SelectedDates.Count == 0)
				uxStartDateError.Text = "You must select a source date.";

			//Check if the end calendar has a selected day
			if (uxEndDate.SelectedDates.Count == 0)
				uxEndDateError.Text = "You must select an end date.";

			//Check if the start and end dates are the same
			if (uxEndDate.SelectedDate == uxSourceDay.SelectedDate)
				uxEndDateError.Text = "You must select at least one day to copy to.";

			//Check if the end day is before the source day
			if (uxEndDate.SelectedDate < uxSourceDay.SelectedDate)
			{
				m_SuppressDateChangeEvent = true;
				uxEndDateError.Text = "End date must be after the start date.";
				uxEndDate.SelectedDates.Clear();
				m_SuppressDateChangeEvent = false;
			}

			//Success case, end date is after source date
			if (uxEndDate.SelectedDate > uxSourceDay.SelectedDate)
			{
				uxDateCount.Visible = true;
				uxEndDateError.Visible = false;
				uxStartDateError.Visible = false;

				uxDateCount.Text = GetDateSelectionDescription();

				NextButtonEnabled(true);
			}
		}
	}

	/// <summary>
	/// Searches for the 'Next' button, and sets its enabled property
	/// </summary>
	/// <param name="enabled">Determines if the 'Next' button should be enabled or not</param>
	private void NextButtonEnabled(bool enabled)
	{
		Control NextButton = SuperCopyWizard.FindControl("StartNavigationTemplateContainerID").FindControl("StartNextButton");

		if (NextButton != null)
			if (NextButton is Button)
				((Button)NextButton).Enabled = enabled;
	}

	/// <summary>
	/// Provides a description of the number of days selected in the form
	/// "'n' days of schedule will be added.
	/// </summary>
	/// <returns>Empty string if there are no days selected</returns>
	private string GetDateSelectionDescription()
	{
		if ((uxEndDate.SelectedDate - uxSourceDay.SelectedDate).TotalDays < 1)
			return string.Empty;

		if ((uxEndDate.SelectedDate - uxSourceDay.SelectedDate).TotalDays == 1)
			return "One day of schedule will be added";

		return string.Format("{0} days of schedule will be added.", (uxEndDate.SelectedDate - uxSourceDay.SelectedDate).TotalDays);
	}

	/// <summary>
	/// Keeps track of the highest numbered step that the user has completed
	/// </summary>
	private int m_StepCompleted
	{
		get { return ViewState["StepCompleted"] == null ? 0 : (int)ViewState["StepCompleted"]; }
		set { ViewState["StepCompleted"] = value; }
	}

	/// <summary>
	/// Keeps track of the currently selected channel
	/// </summary>
	private int m_CurrentChannelID
	{
		get { return ViewState["CurrentChannelID"] == null ? 0 : (int)ViewState["CurrentChannelID"]; }
		set { ViewState["CurrentChannelID"] = value; }
	}

	/// <summary>
	/// Keeps track of the current channel's name
	/// </summary>
	private string m_CurrentChannelName
	{
		get { return ViewState["CurrentChannelName"] == null ? string.Empty : ViewState["CurrentChannelName"].ToString(); }
		set { ViewState["CurrentChannelName"] = value; }
	}
	#endregion
}

