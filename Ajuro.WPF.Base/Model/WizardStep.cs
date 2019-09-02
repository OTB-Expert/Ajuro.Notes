using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.WPF.Base.Model
{
	public class WizardStep: INotifyPropertyChanged
	{
		private static int IdSeed = 1;
		public WizardStep()
		{
			Id = IdSeed++;
			WizardSections = new ObservableCollection<WizardSection>();
		}

		private bool isEditing { get; set; }
		public bool IsEditing
		{
			get { return isEditing; }
			set
			{
				isEditing = value;
				NotifyPropertyChanged();
			}
		}
		private int id { get; set; }
		public int Id
		{
			get { return id; }
			set
			{
				id = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableCollection<WizardSection> wizardSections { get; set; }
		public ObservableCollection<WizardSection> WizardSections
		{
			get { return wizardSections; }
			set
			{
				wizardSections = value;
				NotifyPropertyChanged();
			}
		}

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

		private string info { get; set; }
		public string Info
		{
			get { return info; }
			set
			{
				info = value;
				NotifyPropertyChanged();
			}
		}

		public enum DataType { Text32, Text64, Text200, Text500, Text2000, Boolean, DateTime }

		private DataType type { get; set; }
		public DataType Type
		{
			get { return type; }
			set
			{
				type = value;
				NotifyPropertyChanged();
			}
		}
		private ObservableCollection<WizardStep> children { get; set; }
		public ObservableCollection<WizardStep> Children
		{
			get { return children; }
			set
			{
				children = value;
				NotifyPropertyChanged();
			}
		}

		internal ObservableCollection<WizardStep> Parent { get; set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
