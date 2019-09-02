using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.WPF.Base.Model
{
	public class AffectedFile: INotifyPropertyChanged
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
		private string path { get; set; }
		public string Path
		{
			get { return path; }
			set
			{
				path = value;
				NotifyPropertyChanged();
			}
		}

		private long size { get; set; }
		public long Size
		{
			get { return size; }
			set
			{
				size = value;
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