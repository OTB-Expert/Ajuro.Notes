using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.WPF.Base.Model
{
	public class VersionModel: BaseModel, INotifyPropertyChanged
	{
		public VersionModel()
		{
			Files = new ObservableCollection<VersionedFile>();
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
	}
}