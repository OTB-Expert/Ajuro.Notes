using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.WPF.Base.Model
{
	public class NoteEntity
	{
		public string RowKey { get; set; }
		public ObservableCollection<string> Tags { get; set; }
		public ObservableCollection<string> Files { get; set; }
		public ObservableCollection<VersionModel> versions { get; set; }
		public ObservableCollection<VersionModel> Versions
		{
			get { return versions; }
			set
			{
				versions = value;
			}
		}
		public ObservableCollection<ProjectModel> projects { get; set; }
		public ObservableCollection<ProjectModel> Projects
		{
			get { return projects; }
			set
			{
				projects = value;
			}
		}
		public DateTime Synced { get; set; }
		public string Author { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
	}
}
