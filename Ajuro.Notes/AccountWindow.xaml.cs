using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MemoDrops
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
			Sectret2Label.Visibility = Visibility.Collapsed;
			Sectret2TextBox.Visibility = Visibility.Collapsed;
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
					Sectret2Label.Visibility = Visibility.Collapsed;
					Sectret2TextBox.Visibility = Visibility.Collapsed;
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
					Sectret2Label.Visibility = Visibility.Visible;
					Sectret2TextBox.Visibility = Visibility.Visible;
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

					Sectret2Label.Visibility = Visibility.Collapsed;
					Sectret2TextBox.Visibility = Visibility.Collapsed;
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
					if (!Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account"))
					{
						Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account");
						File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/me.json", JsonConvert.SerializeObject(Me));
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
				if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account"))
				{
					Directory.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account", true);
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
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(DataAccess.UrlBase + entityType + "/" + actiontype);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

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
	}

	public class UserAccount : INotifyPropertyChanged
	{
		private string realName { get; set; }
		public string RealName
		{
			get { return realName; }
			set
			{
				realName = value;
				NotifyPropertyChanged();
			}
		}

		private string displayName { get; set; }
		public string DisplayName
		{
			get { return displayName; }
			set
			{
				displayName = value;
				NotifyPropertyChanged();
			}
		}

		private string email { get; set; }
		public string Email
		{
			get { return email; }
			set {
				email = value;
				NotifyPropertyChanged();
			}
		}
		
		private string username { get; set; }
		public string Username
		{
			get { return username; }
			set
			{
				username = value;
				NotifyPropertyChanged();
			}
		}

		private string password { get; set; }
		public string Password
		{
			get { return password; }
			set
			{
				password = value;
				NotifyPropertyChanged();
			}
		}

		private string permalinkId { get; set; }
		public string PermalinkId
		{
			get { return permalinkId; }
			set
			{
				permalinkId = value;
				NotifyPropertyChanged();
			}
		}

		private string rowKey { get; set; }
		public string RowKey
		{
			get { return rowKey; }
			set
			{
				rowKey = value;
				NotifyPropertyChanged();
			}
		}

		private string partitionKey { get; set; }
		public string PartitionKey
		{
			get { return partitionKey; }
			set
			{
				partitionKey = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
