using MemoDrops.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops
{
	public class MainModel : INotifyPropertyChanged
	{
		private static MainModel instance { get; set; }
		public static MainModel Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MainModel();
				}
				return instance;
			}
		}

		private static void CustomInitialize()
		{
			
		}

		private MainModel()
		{
		}

		private SettingsProfile selectedProfile { get; set; }
		public SettingsProfile SelectedProfile
		{
			get { return selectedProfile; }
			set
			{
				selectedProfile = value;
				NotifyPropertyChanged();
			}
		}

		private string jsonRepresentation { get; set; }
		public string JsonRepresentation
		{
			get { return jsonRepresentation; }
			set
			{
				jsonRepresentation = value;
				NotifyPropertyChanged();
			}
		}

		public ObservableCollection<SettingsProfile> SettingProfiles { get; set; }

		// Curent user info
		public static UserAccount Me { get; set; }

		public static string UrlBase = "https://ajuro.azurewebsites.net/api/";
		// public static string UrlBase = "https://localhost:44351/api/";



		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
