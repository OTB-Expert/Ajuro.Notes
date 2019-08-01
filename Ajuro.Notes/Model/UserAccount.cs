using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.Notes.Model
{
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
			set
			{
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
