using Ajuro.WPF.Base;
using Ajuro.WPF.Base.Model;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Ajuro.Notes.View
{
	/// <summary>
	/// Interaction logic for AccountWindow.xaml
	/// </summary>
	public partial class AccountWindow : Window
	{
		public UserAccount Me { get; set; }
		public int UserAction { get; private set; }

		public AccountWindow()
		{
			InitializeComponent();
		}

		public void WireUpData(UserAccount userAccount)
		{
			Me = userAccount;
			DataContext = Me;

			if (string.IsNullOrEmpty(Me.PermalinkId))
			{
				SwitchView(0);
			}
			else
			{
				SwitchView(2);
			}
		}

		private void LogInSwitchButton_Click(object sender, RoutedEventArgs e)
		{
			LogInSwitchButton.Background = Brushes.Wheat;
			SignInSwitchButton.Background = Brushes.AliceBlue;
			NameLabel.Visibility = Visibility.Collapsed;
			NameTextBox.Visibility = Visibility.Collapsed;
			MailLabel.Visibility = Visibility.Collapsed;
			MailTextBox.Visibility = Visibility.Collapsed;
			// Sectret2Label.Visibility = Visibility.Collapsed;
			// Sectret2TextBox.Visibility = Visibility.Collapsed;
			ClientLabel.Visibility = Visibility.Collapsed;
			ClientText.Visibility = Visibility.Collapsed;
			ActionButton.Content = "LogIn";
			Height = 250;
			UserAction = 0;
		}

		private void SwitchView(int action)
		{
			ErrorLabel.Content = string.Empty;
			switch (action)
			{
				case 0:

					// Logout
					LogInSwitchButton.Visibility = Visibility.Visible;
					SignInSwitchButton.Visibility = Visibility.Visible;
					UsernameLabel.Visibility = Visibility.Visible;
					UsernameTextBok.Visibility = Visibility.Visible;
					SectretLabel.Visibility = Visibility.Visible;
					SectretTextBox.Visibility = Visibility.Visible;

					LogInSwitchButton.Background = Brushes.Wheat;
					SignInSwitchButton.Background = Brushes.AliceBlue;
					NameLabel.Visibility = Visibility.Collapsed;
					NameTextBox.Visibility = Visibility.Collapsed;
					MailLabel.Visibility = Visibility.Collapsed;
					MailTextBox.Visibility = Visibility.Collapsed;
					// Sectret2Label.Visibility = Visibility.Collapsed;
					// Sectret2TextBox.Visibility = Visibility.Collapsed;
					ClientLabel.Visibility = Visibility.Collapsed;
					ClientText.Visibility = Visibility.Collapsed;
					ActionButton.Content = "LogIn";
					Height = 250;
					break;

				case 1:

					LogInSwitchButton.Background = Brushes.AliceBlue;
					SignInSwitchButton.Background = Brushes.Wheat;
					NameLabel.Visibility = Visibility.Visible;
					NameTextBox.Visibility = Visibility.Visible;
					MailLabel.Visibility = Visibility.Visible;
					MailTextBox.Visibility = Visibility.Visible;
					// Sectret2Label.Visibility = Visibility.Visible;
					// Sectret2TextBox.Visibility = Visibility.Visible;
					ClientLabel.Visibility = Visibility.Collapsed;
					ClientText.Visibility = Visibility.Collapsed;
					Height = 350;
					ActionButton.Content = "SignIn";
					break;

				case 2:

					// Login
					LogInSwitchButton.Visibility = Visibility.Collapsed;
					SignInSwitchButton.Visibility = Visibility.Collapsed;
					UsernameLabel.Visibility = Visibility.Collapsed;
					UsernameTextBok.Visibility = Visibility.Collapsed;
					SectretLabel.Visibility = Visibility.Collapsed;
					SectretTextBox.Visibility = Visibility.Collapsed;

					// Sectret2Label.Visibility = Visibility.Collapsed;
					// Sectret2TextBox.Visibility = Visibility.Collapsed;
					NameLabel.Visibility = Visibility.Visible;
					NameTextBox.Visibility = Visibility.Visible;
					MailLabel.Visibility = Visibility.Visible;
					MailTextBox.Visibility = Visibility.Visible;
					ClientLabel.Visibility = Visibility.Visible;
					ClientText.Visibility = Visibility.Visible;

					ActionButton.Content = "LogOut";
					Height = 250;
					break;
			}

			UserAction = action;
		}

		private void SignInSwitchButton_Click(object sender, RoutedEventArgs e)
		{
			SwitchView(1);
		}

		private void ActionButton_Click(object sender, RoutedEventArgs e)
		{
			UpdateUserSession();
		}

		private void UpdateUserSession()
		{
			ErrorLabel.Content = string.Empty;
			if (UserAction < 2)
			{
				if (Me.DisplayName == "Log In")
				{
					Me.DisplayName = Me.RealName;
				}
				var response = PostItem("user", "set", Me);
				if(!string.IsNullOrEmpty(response.RowKey))
				{
					// Replace empty or default with PermaLink
					if(string.IsNullOrEmpty(response.RealName) || response.DisplayName == "Log In")
					{
						response.RealName = response.PermalinkId;
					}
					Me = response;
					if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account"))
					{
						Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account");
						File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account/me.json", JsonConvert.SerializeObject(Me));
					}
					Close();
				}
				else
				{
					ErrorLabel.Content = response.RealName;
				}					
			}
			else
			{
				Me.PermalinkId = null;
				if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account"))
				{
					Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account", true);
				}
				Me.RealName = string.Empty;
				Me.Password = string.Empty;
				Me.Email = string.Empty;
				Me.DisplayName = string.Empty;
				SwitchView(0);
				Close();
			}
		}

		private UserAccount PostItem(string entityType, string actiontype, object entity)
		{
			UserAccount account = null;
			string url = MainModel.UrlBase + entityType + "/" + actiontype;
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

#if DEBUG
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("-------------------------------------------------------------");
			sb.AppendLine(httpWebRequest.Method + " - " + DateTime.UtcNow.ToShortTimeString());
			sb.AppendLine(JsonConvert.SerializeObject(entity, Formatting.Indented));
			if(!Directory.Exists("C://Logs//OTB//Ajuro.Notes"))
			{
				Directory.CreateDirectory("C://Logs//OTB//Ajuro.Notes");
			}
			File.AppendAllText("C://Logs//OTB//Ajuro.Notes//http_trace.txt", sb.ToString());
#endif

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				streamWriter.Write(JsonConvert.SerializeObject(entity));
				streamWriter.Flush();
				streamWriter.Close();

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					account = JsonConvert.DeserializeObject<UserAccount>(result);
				}
			}
			return account;
		}

		private void SectretTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
		{
			Me.Password = SectretTextBox.Text.Trim();
		}

		private void SectretTextBox_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if(e.Key == System.Windows.Input.Key.Enter)
			{
				Me.Password = SectretTextBox.Text.Trim();
				UpdateUserSession();
			}
		}
	}
}
