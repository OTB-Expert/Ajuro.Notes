using System.ComponentModel;
using System.Runtime.CompilerServices;
using Ajuro.Net.Template.Processor;
using System.Collections.ObjectModel;

namespace Ajuro.WPF.Base.Model
{

	public class TemplateEditorModel : INotifyPropertyChanged
	{
		private string originalCode;

		public string OriginalCode
		{
			get
			{
				return originalCode;
			}

			set
			{
				if (originalCode != value)
				{
					originalCode = value;
					NotifyPropertyChanged();
				}
			}
		}

		private bool realTimeUpdates;

		public bool RealTimeUpdates
		{
			get
			{
				return realTimeUpdates;
			}

			set
			{
				realTimeUpdates = value;
				NotifyPropertyChanged();
			}
		}

		private string fragmentName;

		public string FragmentName
		{
			get
			{
				return fragmentName;
			}

			set
			{
				if (fragmentName != value)
				{
					fragmentName = value;
					NotifyPropertyChanged();
				}
			}
		}

		private int selectionStart;

		public int SelectionStart
		{
			get
			{
				return selectionStart;
			}

			set
			{
				if (selectionStart != value)
				{
					selectionStart = value;
					NotifyPropertyChanged();
				}
			}
		}

		private int selectionLength;

		public int SelectionLength
		{
			get
			{
				return selectionLength;
			}

			set
			{
				if (selectionLength != value)
				{
					selectionLength = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string valueExample;

		public string ValueExample
		{
			get
			{
				return valueExample;
			}

			set
			{
				if (valueExample != value)
				{
					valueExample = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string codeFragment;

		public string CodeFragment
		{
			get
			{
				return codeFragment;
			}

			set
			{
				if (codeFragment != value)
				{
					codeFragment = value;
					NotifyPropertyChanged();
				}
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

		private CodeFragment lastFragment;

		public CodeFragment LastFragment
		{
			get
			{
				return lastFragment;
			}

			set
			{
				lastFragment = value;
				NotifyPropertyChanged();
			}
		}

		private string jsonString;

		public string JsonString
		{
			get
			{
				return jsonString;
			}

			set
			{
				if (jsonString != value)
				{
					jsonString = value;
					NotifyPropertyChanged();
				}
			}
		}

		private string inflatedTemplate;

		public string InflatedTemplate
		{
			get
			{
				return inflatedTemplate;
			}

			set
			{
				if (inflatedTemplate != value)
				{
					inflatedTemplate = value;
					NotifyPropertyChanged();
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}