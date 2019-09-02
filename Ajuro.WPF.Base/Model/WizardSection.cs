using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace Ajuro.WPF.Base.Model
{
	public class WizardSection : INotifyPropertyChanged
	{
		private static int IdSeed = 1;
		public WizardSection()
		{
			Id = IdSeed++;
			this.Type = DataType.Text32;
			CurrentValues = new ObservableCollection<EditorMeta>();
			this.Items = new ObservableCollection<StepEntry>();
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

		private string currentValue { get; set; }
		public string CurrentValue
		{
			get { return currentValue; }
			set
			{
				currentValue = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableCollection<EditorMeta> currentValues { get; set; }
		public ObservableCollection<EditorMeta> CurrentValues
		{
			get { return currentValues; }
			set
			{
				currentValues = value;
				NotifyPropertyChanged();
			}
		}

		private string xLabel { get; set; }
		public string XLabel
		{
			get { return xLabel; }
			set
			{
				xLabel = value;
				NotifyPropertyChanged();
			}
		}

		private string xPath { get; set; }
		public string XPath
		{
			get { return xPath; }
			set
			{
				xPath = value;
				NotifyPropertyChanged();
			}
		}

		public ObservableCollection<StepEntry> Items { get; set; }
		internal WizardStep Parent { get;  set; }

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
