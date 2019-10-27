using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.WPF.Base.Model
{


	/// <summary>
	/// The structure of a note
	/// </summary>
	public class MultiFileDocument : BaseModel, INotifyPropertyChanged
	{
		public MultiFileDocument()
		{
			Tags = new ObservableCollection<string>();
			AffectedFiles = new ObservableCollection<AffectedFile>();

			Versions = new ObservableCollection<VersionModel>();
			VersionItems = new VersionList(new ObservableCollection<VersionModel>{ });
			AllVersionItems = new VersionList(Versions);
			Projects = new ObservableCollection<ProjectModel>();
			ProjectItems = new ProjectList(new ObservableCollection<ProjectModel>{ });
			AllProjectItems = new ProjectList(Projects);
		}


		[JsonIgnore]
		public VersionList AllVersionItems { get; set; }
		[JsonIgnore]
		public ProjectList AllProjectItems { get; set; }
		[JsonIgnore]
		public VersionList VersionItems { get; set; }
		[JsonIgnore]
		public ProjectList ProjectItems { get; set; }

		public ObservableCollection<string> Tags { get; set; }
		private ObservableCollection<VersionModel> versions { get; set; }
		public ObservableCollection<VersionModel> Versions
		{
			get { return versions; }
			set
			{
				versions = value;
				NotifyPropertyChanged();
			}
		}
		private ObservableCollection<ProjectModel> projects { get; set; }
		public ObservableCollection<ProjectModel> Projects
		{
			get { return projects; }
			set
			{
				projects = value;
				NotifyPropertyChanged();
			}
		}

		private string selectedProject { get; set; }
		public string SelectedProject
		{
			get { return selectedProject; }
			set
			{
				selectedProject = value;
				NotifyPropertyChanged();
			}
		}
		public string selectedVersion { get; set; }
		public string SelectedVersion
		{
			get { return selectedVersion; }
			set
			{
				selectedVersion = value;
				NotifyPropertyChanged();
			}
		}
		private ObservableCollection<VersionedFile> files { get; set; }
		public ObservableCollection<VersionedFile> Files
		{
			get { return files; }
			set
			{
				files = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableCollection<AffectedFile> affectedFiles { get; set; }
		public ObservableCollection<AffectedFile> AffectedFiles
		{
			get { return affectedFiles; }
			set
			{
				affectedFiles = value;
				NotifyPropertyChanged();
			}
		}

		private VisibilityLevel visibility { get; set; }
		public VisibilityLevel Visibility
		{
			get { return visibility; }
			set
			{
				visibility = value;
				NotifyPropertyChanged();
			}
		}
		private DateTime lastUpdated { get; set; }
		public DateTime LastUpdated
		{
			get { return lastUpdated; }
			set
			{
				lastUpdated = value;
				NotifyPropertyChanged();
			}
		}

		private int status { get; set; }
		public int Status
		{
			get { return status; }
			set
			{
				status = value;
				NotifyPropertyChanged();
			}
		}
		public string Key { get; set; }
		public string Author { get; set; }
		public DateTime Synced { get; set; }
		private string path { get; set; }
		/// <summary>
		/// Name of the note, is also the name of the file storing the content of the note.
		/// </summary>
		public string Path
		{
			get { return path; }
			set
			{
				path = value;
				NotifyPropertyChanged();
			}
		}

		private string label { get; set; }
		/// <summary>
		/// Used as dirty flag for the note's name or content. Is emptied on save.
		/// </summary>
		public string Label
		{
			get { return label; }
			set
			{
				label = value;
				NotifyPropertyChanged();
			}
		}

		public TemplaterInstruction TemplaterInstruction { get; set; }
	}

	public class VersionedFile
	{
		private string name { get; set; }
		/// <summary>
		/// Given name of the file
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				NotifyPropertyChanged();
			}
		}
		private string key { get; set; }
		/// <summary>
		/// Unique identifier of the file
		/// </summary>
		public string Key
		{
			get { return key; }
			set
			{
				key = value;
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