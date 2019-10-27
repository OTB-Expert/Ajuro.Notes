using Ajuro.WPF.Base.Commands;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Ajuro.WPF.Base.Model
{
	public class WizardModel : INotifyPropertyChanged
	{

		#region Singleton
		private static WizardModel instance { get; set; }
		public static WizardModel Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new WizardModel();
				}
				return instance;
			}
		}

		public static string DataJsonPath { get; set; }
		public static string MetaJsonPath { get; set; }

		private static void CustomInitialize()
		{

		}

		private WizardModel()
		{
			WizardSteps = new ObservableCollection<WizardStep>();
			SwitchLibraryCommand = new RelayCommand(SwitchLibrary, param => true);
			AddStepCommand = new RelayCommand(AddStep, param => true);

			AddSectionCommand = new RelayCommand(AddSection, param => true);

			RemoveStepCommand = new RelayCommand(RemoveStep, param => true);

			RemoveEntryCommand = new RelayCommand(RemoveEntry, param => true);

			AddEntryCommand = new RelayCommand(AddEntry, param => true);

			RemoveSectionCommand = new RelayCommand(RemoveSection, param => true);

			EditStepCommand = new RelayCommand(EditStep, param => true);

			EditSectionCommand = new RelayCommand(EditSection, param => true);

			PreviewsStepCommand = new RelayCommand(PreviewsStep, param => true);

			PreviewsSectionCommand = new RelayCommand(PreviewsSection, param => true);

			PreviewsEntryCommand = new RelayCommand(PreviewsEntry, param => true);

			NextEntryCommand = new RelayCommand(NextEntry, param => true);

			NextSectionCommand = new RelayCommand(NextSection, param => true);

			NextStepCommand = new RelayCommand(NextStep, param => true);


		}

		public void SwitchLibrary(object obj)
		{
			ShowLibrary = !ShowLibrary;
		}
		public void EditStep(object obj)
		{
			if (SelectedStep != null)
			{
				SelectedStep.IsEditing = !SelectedStep.IsEditing;
			}
		}

		public void AddStep(object obj)
		{
			var newStep = new WizardStep() { Name = "Step " + (WizardSteps.Count + 1) };
			newStep.Parent = WizardSteps;
			WizardSteps.Add(newStep);
			SelectedStep = newStep;
		}

		public void NextEntry(object obj)
		{
			if (SelectedStep == null)
			{
				SelectedStep = WizardSteps[0];
			}
			if (SelectedSection == null)
			{
				SelectedSection = SelectedStep.WizardSections[0];
			}
			SelectedEntryIndex++;
			if (SelectedEntryIndex >= SelectedSection.CurrentValues.Count)
			{
				SelectedEntryIndex = 0;
				// Next step
				NextSection(null);
			}
			if (SelectedSection != null && SelectedSection.CurrentValues.Count > 0 && SelectedEntryIndex > -1)
				SelectedEntry = SelectedSection.CurrentValues[SelectedEntryIndex];
		}
		public void NextSection(object obj)
		{
			if (SelectedStep == null)
			{
				SelectedStep = WizardSteps[0];
			}
			SelectedSectionIndex++;
			if (SelectedSectionIndex >= SelectedStep.WizardSections.Count)
			{
				SelectedSectionIndex = 0;
				// Next step
				NextStep(null);
			}
			if (SelectedSectionIndex > -1)
				SelectedSection = SelectedStep.WizardSections[SelectedSectionIndex];
		}
		public void PreviewsEntry(object obj)
		{
			var newStep = new WizardStep() { Name = "Step " + (WizardSteps.Count + 1) };
			newStep.Parent = WizardSteps;
			WizardSteps.Add(newStep);
			SelectedStep = newStep;
		}
		public void NextStep(object obj)
		{
			selectedStepIndex++;
			if (selectedStepIndex >= WizardSteps.Count)
			{
				selectedStepIndex = 0;
			}
			SelectedStep = WizardSteps[selectedStepIndex];
		}
		public void PreviewsSection(object obj)
		{
			if (SelectedStep == null)
			{
				SelectedStep = WizardSteps[0];
			}
			SelectedSectionIndex--;
			if (SelectedSectionIndex < 0)
			{
				// Next step
				PreviewsStep(null);
				SelectedSectionIndex = SelectedStep.WizardSections.Count-1;
			}
			if (SelectedSectionIndex > -1)
				SelectedSection = SelectedStep.WizardSections[SelectedSectionIndex];
		}
		public void PreviewsStep(object obj)
		{
			selectedStepIndex--;
			if (selectedStepIndex < 0)
			{
				selectedStepIndex = WizardSteps.Count - 1;
			}
			SelectedStep = WizardSteps[selectedStepIndex];
		}
		public void AddSection(object obj)
		{
			if (SelectedStep == null)
			{
				return;
			}

			var newSection = new WizardSection() {
				Name = "Section " + (WizardSteps.Count + 1),
				IsEditing = true,
				Info= "This is a new step, you can edit it and press ok to exit edit mode." 
			};
			newSection.Parent = SelectedStep;
			SelectedStep.WizardSections.Add(newSection);
			// SelectedWizardSection = newSection;
		}
		public void RemoveStep(object obj)
		{
			int id = -1;
			if (int.TryParse(obj.ToString(), out id))
			{
				for (int i = 0; i < WizardSteps.Count; i++)
				{
					if (WizardSteps[i].Id == id)
					{
						var response = MessageBox.Show("Delete this step?", "Remove Step", MessageBoxButton.OKCancel);
						if (response == MessageBoxResult.OK)
						{
							WizardSteps.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}
		public void AddEntry(object obj)
		{
			if (SelectedStep == null)
			{
				return;
			}

			WizardSection selectedSection = null;
			int id = -1;
			if (int.TryParse(obj.ToString(), out id))
			{
				for (int i = 0; i < SelectedStep.WizardSections.Count; i++)
				{
					if (SelectedStep.WizardSections[i].Id == id)
					{
						selectedSection = SelectedStep.WizardSections[i];
						break;
					}
				}
			}

			if (selectedSection != null)
			{
				var newEditorMeta = new EditorMeta()
				{
					CurrentLabel = "New Entry",
					CurrentValue = "New Entry value"
				};
				selectedSection.CurrentValues.Add(newEditorMeta);
			}
		}
		public void RemoveEntry(object obj)
		{
			int id = -1;
			if (int.TryParse(obj.ToString(), out id))
			{
				for (int i = 0; i < WizardSteps.Count; i++)
				{
					if (WizardSteps[i].Id == id)
					{
						var response = MessageBox.Show("Delete this step?", "Remove Step", MessageBoxButton.OKCancel);
						if (response == MessageBoxResult.OK)
						{
							WizardSteps.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}
		public void RemoveSection(object obj)
		{
			int id = -1;
			if (int.TryParse(obj.ToString(), out id))
			{
				for(int i=0; i< SelectedStep.WizardSections.Count; i++)
				{
					if (SelectedStep.WizardSections[i].Id == id)
					{
						var response = MessageBox.Show("Delete this section?", "Remove Section", MessageBoxButton.OKCancel);
						if (response == MessageBoxResult.OK)
						{
							SelectedStep.WizardSections.RemoveAt(i);
							i--;
						}
					}
				}
			}
		}
		public void EditSection(object obj)
		{
			int id = -1;
			if (int.TryParse(obj.ToString(), out id))
			{
				for (int i = 0; i < SelectedStep.WizardSections.Count; i++)
				{
					if (SelectedStep.WizardSections[i].Id == id)
					{
						SelectedStep.WizardSections[i].IsEditing = !SelectedStep.WizardSections[i].IsEditing;
						IsEditing = SelectedStep.WizardSections[i].IsEditing;
					}
				}
			}
		}
		#endregion Singleton

		private int selectedStepIndex { get; set; }
		public int SelectedStepIndex
		{
			get { return selectedStepIndex; }
			set
			{
				selectedStepIndex = value;
				NotifyPropertyChanged();
			}
		}

		private ObservableCollection<WizardStep> wizardSteps { get; set; }
		public ObservableCollection<WizardStep> WizardSteps
		{
			get { return wizardSteps; }
			set
			{
				wizardSteps = value;
				wizardSteps.CollectionChanged += OnCollectionChanged;
				NotifyPropertyChanged();
			}
		}


		private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NotifyPropertyChanged("WizardSteps");
		}

		private UniversalTreeNode sampleTree { get; set; }
		public UniversalTreeNode SampleTree
		{
			get { return sampleTree; }
			set { sampleTree = value; }
		}

		private string sampleJson { get; set; }
		public string SampleJson
		{
			get
			{
				return sampleJson;
			}
			set
			{
				sampleJson = value;
				NotifyPropertyChanged();
			}
		}
		private bool showLibrary { get; set; }
		public bool ShowLibrary
		{
			get
			{
				return showLibrary;
			}
			set
			{
				showLibrary = value;
				NotifyPropertyChanged();
			}
		}
		private bool isEditing { get; set; }
		public bool IsEditing
		{
			get
			{
				return isEditing;
			}
			set
			{
				isEditing = value;
				NotifyPropertyChanged();
			}
		}
		private string metaJson { get; set; }
		public string MetaJson
		{
			get
			{
				return metaJson;
			}
			set
			{
				metaJson = value;
				NotifyPropertyChanged();
			}
		}


		private WizardStep selectedStep { get; set; }
		public WizardStep SelectedStep
		{
			get { return selectedStep; }
			set
			{
				selectedStep = value;
				SelectedStepIndex = wizardSteps.IndexOf(selectedStep);
				NotifyPropertyChanged();
			}
		}

		private WizardSection selectedSection { get; set; }
		public WizardSection SelectedSection
		{
			get { return selectedSection; }
			set
			{
				selectedSection = value;
				SelectedSectionIndex = SelectedStep.WizardSections.IndexOf(selectedSection);
				NotifyPropertyChanged();
			}
		}

		#region Commands
		private ICommand switchLibraryCommand;
		public ICommand SwitchLibraryCommand
		{
			get
			{
				return switchLibraryCommand;
			}
			set
			{
				switchLibraryCommand = value;
			}
		}
		private ICommand addStepCommand;
		public ICommand AddStepCommand
		{
			get
			{
				return addStepCommand;
			}
			set
			{
				addStepCommand = value;
			}
		}

		private ICommand addSectionCommand;
		public ICommand AddSectionCommand
		{
			get
			{
				return addSectionCommand;
			}
			set
			{
				addSectionCommand = value;
			}
		}
		private ICommand removeStepCommand;
		public ICommand RemoveStepCommand
		{
			get
			{
				return removeStepCommand;
			}
			set
			{
				removeStepCommand = value;
			}
		}
		private ICommand addEntryCommand;
		public ICommand AddEntryCommand
		{
			get
			{
				return addEntryCommand;
			}
			set
			{
				addEntryCommand = value;
			}
		}
		private ICommand removeEntryCommand;
		public ICommand RemoveEntryCommand
		{
			get
			{
				return removeEntryCommand;
			}
			set
			{
				removeEntryCommand = value;
			}
		}
		
		private ICommand editStepCommand;
		public ICommand EditStepCommand
		{
			get
			{
				return editStepCommand;
			}
			set
			{
				editStepCommand = value;
			}
		}
		private ICommand editSectionCommand;
		public ICommand EditSectionCommand
		{
			get
			{
				return editSectionCommand;
			}
			set
			{
				editSectionCommand = value;
			}
		}

		private ICommand removeSectionCommand;
		public ICommand RemoveSectionCommand
		{
			get
			{
				return removeSectionCommand;
			}
			set
			{
				removeSectionCommand = value;
			}
		}

		private ICommand previewsSectionCommand;
		public ICommand PreviewsSectionCommand
		{
			get
			{
				return previewsSectionCommand;
			}
			set
			{
				previewsSectionCommand = value;
			}
		}

		private ICommand previewsStepCommand;
		public ICommand PreviewsStepCommand
		{
			get
			{
				return previewsStepCommand;
			}
			set
			{
				previewsStepCommand = value;
			}
		}

		private ICommand previewsEntryCommand;
		public ICommand PreviewsEntryCommand
		{
			get
			{
				return previewsEntryCommand;
			}
			set
			{
				previewsEntryCommand = value;
			}
		}

		private ICommand nextStepCommand;
		public ICommand NextStepCommand
		{
			get
			{
				return nextStepCommand;
			}
			set
			{
				nextStepCommand = value;
			}
		}

		private ICommand nextSectionCommand;
		public ICommand NextSectionCommand
		{
			get
			{
				return nextSectionCommand;
			}
			set
			{
				nextSectionCommand = value;
			}
		}

		private ICommand nextEntryCommand;
		public ICommand NextEntryCommand
		{
			get
			{
				return nextEntryCommand;
			}
			set
			{
				nextEntryCommand = value;
			}
		}

		public int SelectedSectionIndex { get; set; }
		public int SelectedEntryIndex { get; set; }
		public EditorMeta SelectedEntry { get; set; }
		public UniversalTreeNode SelectedNode { get; set; }

		#endregion

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}