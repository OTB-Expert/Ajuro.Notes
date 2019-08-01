using Ajuro.Net.Template.Processor;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Ajuro.Notes.Views
{
	public class FragmentSelectorViewModel : INotifyPropertyChanged
	{
		private string selection { get; set; }
		public string Selection
		{
			get { return selection; }
			set
			{
				selection = value;
				NotifyPropertyChanged();
			}
		}
		private string originalCode { get; set; }
		public string OriginalCode
		{
			get { return originalCode; }
			set
			{
				originalCode = value;
				NotifyPropertyChanged();
			}
		}
		private object jsonSample { get; set; }
		public object JsonSample
		{
			get { return jsonSample; }
			set
			{
				jsonSample = value;
				NotifyPropertyChanged();
			}
		}
		private string jsonSampleString { get; set; }
		public string JsonSampleString
		{
			get { return jsonSampleString; }
			set
			{
				jsonSampleString = value;
				NotifyPropertyChanged();
			}
		}
		private string structureJson { get; set; }
		public string StructureJson
		{
			get { return structureJson; }
			set
			{
				structureJson = value;
				NotifyPropertyChanged();
			}
		}

		private CodeFragment currentFragment;
		public CodeFragment CurrentFragment
		{
			get
			{
				return currentFragment;
			}
			set
			{
				currentFragment = value;
				NotifyPropertyChanged();
			}
		}
		private CodeFragment rootCodeFragment;
		public CodeFragment RootCodeFragment
		{
			get
			{
				return rootCodeFragment;
			}
			set
			{
				rootCodeFragment = value;
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