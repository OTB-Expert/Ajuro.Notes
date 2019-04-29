using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops.Model
{


	/// <summary>
	/// The structure of a note
	/// </summary>
	public class FileItem : INotifyPropertyChanged
	{
		public FileItem()
		{
			Tags = new List<string>();
		}

		public List<string> Tags { get; set; }
		public List<string> Files { get; set; }

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

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}