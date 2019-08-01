using Ajuro.Notes.Model;
using Ajuro.Notes.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.Notes
{
	public class MainModel : INotifyPropertyChanged
	{
		#region Singleton
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
			LastDocumentNames = new ObservableCollection<string>();
		}
		#endregion Singleton

		#region MultifileDocuments

		/// <summary>
		/// Unfiltred collection
		/// </summary>
		public ItemList AllItems { get; set; }

		/// <summary>
		/// Filtred collection
		/// </summary>
		public ItemList FileItems { get; set; }

		#endregion MultifileDocuments

		private ObservableCollection<string> lastDocumentNames { get; set; }
		public ObservableCollection<string> LastDocumentNames
		{
			get { return lastDocumentNames; }
			set
			{
				lastDocumentNames = value;
				NotifyPropertyChanged();
			}
		}



		private string selectedAffectedFileText { get; set; }
		public string SelectedAffectedFileText
		{
			get { return selectedAffectedFileText; }
			set
			{
				selectedAffectedFileText = value;
				NotifyPropertyChanged();
			}
		}

		private AffectedFile selectedAffectedFile { get; set; }
		public AffectedFile SelectedAffectedFile
		{
			get { return selectedAffectedFile; }
			set
			{
				if (value != null && File.Exists(value.Path))
				{
					SelectedAffectedFileText = File.ReadAllText(value.Path);
				}
				else
				{
					SelectedAffectedFileText = string.Empty;
				}
				selectedAffectedFile = value;
				NotifyPropertyChanged();
			}
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