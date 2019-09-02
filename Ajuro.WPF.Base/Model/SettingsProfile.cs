using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.WPF.Base.Model
{
	public class SettingsProfile : INotifyPropertyChanged
	{
		private string name { get; set; }
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				NotifyPropertyChanged();
			}
		}

		public List<KeyValuePair<string, string>> Properties { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
