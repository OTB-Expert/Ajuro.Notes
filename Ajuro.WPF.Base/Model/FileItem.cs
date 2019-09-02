using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.WPF.Base.Model
{


	/// <summary>
	/// The structure of a note
	/// </summary>
	public class MultiFileDocument : INotifyPropertyChanged
	{
		public MultiFileDocument()
		{
			Tags = new ObservableCollection<string>();
			AffectedFiles = new ObservableCollection<AffectedFile>();

			VersionItems = new VersionList(
				new List<VersionModel>
				{ }
				);
			AllVersionItems = new VersionList(
				new List<VersionModel>
				{ }
				);
			ProjectItems = new ProjectList(
				new List<ProjectModel>
				{ }
				);
			AllProjectItems = new ProjectList(
				new List<ProjectModel>
				{ }
				);
		}


		public VersionList AllVersionItems { get; set; }
		public ProjectList AllProjectItems { get; set; }
		public VersionList VersionItems { get; set; }
		public ProjectList ProjectItems { get; set; }

		public ObservableCollection<string> Tags { get; set; }
		public ObservableCollection<VersionModel> versions { get; set; }
		public ObservableCollection<VersionModel> Versions0
		{
			get { return versions; }
			set
			{
				versions = value;
				NotifyPropertyChanged();
			}
		}
		public ObservableCollection<ProjectModel> projects { get; set; }
		public ObservableCollection<ProjectModel> Projects0
		{
			get { return projects; }
			set
			{
				projects = value;
				NotifyPropertyChanged();
			}
		}

		public string selectedProject { get; set; }
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
		public ObservableCollection<string> files { get; set; }
		public ObservableCollection<string> Files
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

		public VisibilityLevel visibility { get; set; }
		public VisibilityLevel Visibility
		{
			get { return visibility; }
			set
			{
				visibility = value;
				NotifyPropertyChanged();
			}
		}
		public DateTime lastUpdated { get; set; }
		public DateTime LastUpdated
		{
			get { return lastUpdated; }
			set
			{
				lastUpdated = value;
				NotifyPropertyChanged();
			}
		}

		public int status { get; set; }
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
		private string name { get; set; }
		/// <summary>
		/// Name of the note, is also the name of the file storing the content of the note.
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

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}