using Ajuro.Notes.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.Notes.Model
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
		}

		public ObservableCollection<string> Tags { get; set; }
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

		public TemplaterInstruction TemplaterInstruction { get; internal set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}