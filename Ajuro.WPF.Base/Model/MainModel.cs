using Ajuro.WPF.Base.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace Ajuro.WPF.Base.Model
{
	public enum KnownCommands { ExecuteTemplate, EditTemplate }
	public delegate string CommandRequestedDelegate(KnownCommands str);
	public class MainModel : INotifyPropertyChanged
	{
		public event CommandRequestedDelegate CommandRequestedEvent;


		#region Singleton
		private static MainModel instance { get; set; }
		public static MainModel Instance
		{
			get
			{
				if (instance == null)
				{
					instance = new MainModel();
					instance.ShowTemplateEditor = false;
				}
				return instance;
			}
		}

		public bool showFileContent { get; set; }
		public bool ShowFileContent
		{
			get { return showFileContent; }
			set
			{
				showFileContent = value;
				NotifyPropertyChanged();
			}
		}

		public bool showRemoteContent { get; set; }
		public bool ShowRemoteContent
		{
			get { return showRemoteContent; }
			set
			{
				showRemoteContent = value;
				NotifyPropertyChanged();
			}
		}
		
		public bool showGeneratedCode { get; set; }
		public bool ShowGeneratedCode
		{
			get { return showGeneratedCode; }
			set
			{
				showGeneratedCode = value;
				NotifyPropertyChanged();
			}
		}

		public bool showTemplateEditor { get; set; }
		public bool ShowTemplateEditor
		{
			get { return showTemplateEditor; }
			set
			{
				showTemplateEditor = value;
				NotifyPropertyChanged();
			}
		}

		public bool showAffectedFiles { get; set; }
		public bool ShowAffectedFiles
		{
			get { return showAffectedFiles; }
			set
			{
				showAffectedFiles = value;
				NotifyPropertyChanged();
			}
		}

		public bool showTemplates { get; set; }
		public bool ShowTemplates
		{
			get { return showTemplates; }
			set
			{
				showTemplates = value;
				NotifyPropertyChanged();
			}
		}

		public string templateFilter { get; set; }
		public string TemplateFilter
		{
			get { return templateFilter; }
			set
			{
				templateFilter = value;
				NotifyPropertyChanged();
			}
		}

		public string versionFilter { get; set; }
		public string VersionFilter
		{
			get { return versionFilter; }
			set
			{
				versionFilter = value;
				NotifyPropertyChanged();
			}
		}

		public string projectFilter { get; set; }
		public string ProjectFilter
		{
			get { return projectFilter; }
			set
			{
				projectFilter = value;
				NotifyPropertyChanged();
			}
		}

		private static void CustomInitialize()
		{

		}

		private ICommand switchGeneratedCodeCommand;
		public ICommand SwitchGeneratedCodeCommand
		{
			get
			{
				return switchGeneratedCodeCommand;
			}
			set
			{
				switchGeneratedCodeCommand = value;
			}
		}

		private ICommand switchTemplateEditorCommand;
		public ICommand SwitchTemplateEditorCommand
		{
			get
			{
				return switchTemplateEditorCommand;
			}
			set
			{
				switchTemplateEditorCommand = value;
			}
		}

		private ICommand switchFileContentCommand;
		public ICommand SwitchFileContentCommand
		{
			get
			{
				return switchFileContentCommand;
			}
			set
			{
				switchFileContentCommand = value;
			}
		}

		private ICommand processTemplateCommand;
		public ICommand ProcessTemplateCommand
		{
			get
			{
				return processTemplateCommand;
			}
			set
			{
				processTemplateCommand = value;
			}
		}

		private ICommand editTemplateCommand;
		public ICommand EditTemplateCommand
		{
			get
			{
				return editTemplateCommand;
			}
			set
			{
				editTemplateCommand = value;
			}
		}


		private ICommand switchTemplatesCommand;
		public ICommand SwitchTemplatesCommand
		{
			get
			{
				return switchTemplatesCommand;
			}
			set
			{
				switchTemplatesCommand = value;
			}
		}

		private ICommand switchAffectedFilesCommand;
		public ICommand SwitchAffectedFilesCommand
		{
			get
			{
				return switchAffectedFilesCommand;
			}
			set
			{
				switchAffectedFilesCommand = value;
			}
		}

		private ICommand switchRemoteContentCommand;
		public ICommand SwitchRemoteContentCommand
		{
			get
			{
				return switchRemoteContentCommand;
			}
			set
			{
				switchRemoteContentCommand = value;
			}
		}

		private MainModel()
		{
			LastDocumentNames = new ObservableCollection<string>();
			SwitchTemplatesCommand = new RelayCommand(SwitchTemplates, param => true);
			SwitchFileContentCommand = new RelayCommand(SwitchFileContent, param => true);
			ProcessTemplateCommand = new RelayCommand(ProcessTemplate, param => true);
			SwitchTemplateEditorCommand = new RelayCommand(SwitchTemplateEditor, param => true);
			SwitchGeneratedCodeCommand = new RelayCommand(SwitchGeneratedCode, param => true);
			EditTemplateCommand = new RelayCommand(EditTemplate, param => true);
			SwitchAffectedFilesCommand = new RelayCommand(SwitchAffectedFiles, param => true);
			SwitchRemoteContentCommand = new RelayCommand(SwitchRemoteContent, param => true);
		}

		public void SwitchGeneratedCode(object obj)
		{
			ShowGeneratedCode = !ShowGeneratedCode;
		}

		public void SwitchTemplateEditor(object obj)
		{
			ShowTemplateEditor = !ShowTemplateEditor;
		}

		public void SwitchRemoteContent(object obj)
		{
			ShowRemoteContent = !ShowRemoteContent;
		}

		public void SwitchAffectedFiles(object obj)
		{
			ShowAffectedFiles = !ShowAffectedFiles;
		}

		public void ProcessTemplate(object obj)
		{
			if (CommandRequestedEvent != null)
			{
				CommandRequestedEvent(KnownCommands.ExecuteTemplate);
			}
		}

		public void EditTemplate(object obj)
		{
			if (CommandRequestedEvent != null)
			{
				CommandRequestedEvent(KnownCommands.EditTemplate);
			}
		}

		public void SwitchTemplates(object obj)
		{
			ShowTemplates = !ShowTemplates;
		}

		public void SwitchFileContent(object obj)
		{
			ShowFileContent = !ShowFileContent;
		}

		#endregion Singleton

		#region MultifileDocuments

		/// <summary>
		/// Unfiltred collection
		/// </summary>
		public TemplateList AllTemplateItems { get; set; }

		/// <summary>
		/// Filtred collection
		/// </summary>
		public TemplateList TemplateItems { get; set; }

		#endregion MultifileDocuments

		private ObservableCollection<string> lastDocumentNames { get; set; }
		public ObservableCollection<string> LastDocumentNames
		{
			get { return lastDocumentNames; }
			set
			{
				lastDocumentNames = value;
				NotifyPropertyChanged();
			}
		}
		
		private string selectedAffectedFileText { get; set; }
		public string SelectedAffectedFileText
		{
			get { return selectedAffectedFileText; }
			set
			{
				selectedAffectedFileText = value;
				NotifyPropertyChanged();
			}
		}

		private string relatedFileFilter { get; set; }
		public string RelatedFileFilter
		{
			get { return relatedFileFilter; }
			set
			{
				relatedFileFilter = value;
				NotifyPropertyChanged();
			}
		}

		private string previewHtmlText { get; set; }
		public string PreviewHtmlText
		{
			get { return previewHtmlText; }
			set
			{
				previewHtmlText = value;
				NotifyPropertyChanged();
			}
		}

		private AffectedFile selectedAffectedFile { get; set; }
		public AffectedFile SelectedAffectedFile
		{
			get { return selectedAffectedFile; }
			set
			{
				if (value != null && File.Exists(value.Path))
				{
					SelectedAffectedFileText = File.ReadAllText(value.Path);
				}
				else
				{
					SelectedAffectedFileText = string.Empty;
				}
				selectedAffectedFile = value;
				NotifyPropertyChanged();
			}
		}

		public Visibility LoginWithFacebookButton_Visibility { get; internal set; }
		private SettingsProfile selectedProfile { get; set; }
		public List<ResourceFolder> RepositorySourceComboBox_ItemsSource { get; internal set; }

		public SettingsProfile SelectedProfile
		{
			get { return selectedProfile; }
			set
			{
				selectedProfile = value;
				NotifyPropertyChanged();
			}
		}
		public int ChannelSelector_SelectedIndex { get; set; }

		private string jsonRepresentation { get; set; }
		public string JsonRepresentation
		{
			get { return jsonRepresentation; }
			set
			{
				jsonRepresentation = value;
				NotifyPropertyChanged();
			}
		}

		private string remoteContentDocument { get; set; }
		public string RemoteContentDocument
		{
			get { return remoteContentDocument; }
			set
			{
				remoteContentDocument = value;
				NotifyPropertyChanged();
			}
		}

		private string resourceContentDocument { get; set; }
		public string ResourceContentDocument
		{
			get { return resourceContentDocument; }
			set
			{
				resourceContentDocument = value;
				NotifyPropertyChanged();
			}
		}

		public ObservableCollection<SettingsProfile> SettingProfiles { get; set; }

		// Curent user info
		public static UserAccount Me { get; set; }
		public object SelectedAffectedFileTextTab { get; internal set; }
		public bool SelectedAffectedFileTextTab_IsSelected { get; internal set; }
		public Visibility KeyShortcuts_Visibility { get; internal set; }
		public Visibility ButtonShortcuts_Visibility { get; internal set; }
		public Visibility Button_Data_Visibility { get; internal set; }
		public Visibility Button_Template_Visibility { get; internal set; }
		public Visibility Button_Project_Visibility { get; internal set; }
		public Visibility Button_Compile_Visibility { get; internal set; }
		public Visibility Button_BrowseLog_Visibility { get; internal set; }
		public string Resource_Name_Text { get; internal set; }
		public Visibility FilesContainer_Visibility { get; internal set; }
		public string StatusBartextBlock_Text { get; internal set; }
		public int MultyFileDocumentsList_SelectedIndex { get; internal set; }
		public string ChannelSelector_Text { get; internal set; }
		public object ChannelSelector_SelectedValue { get; internal set; }
		public ObservableCollection<string> channelSelector_Items { get; set; }
		public ObservableCollection<string> ChannelSelector_Items
		{
			get { return channelSelector_Items; }
			set
			{
				channelSelector_Items = value;
				NotifyPropertyChanged();
			}
		}
		public object TagsStackPanel_Children { get; internal set; }
		public object FilesStackPanel_Children { get; internal set; }
		public Orientation TagsStackPanel_Orientation { get; internal set; }
		public static bool InstanceResourceContentTextBox_AcceptsTab { get; internal set; }
		public int RepositorySourceComboBox_SelectedIndex { get; internal set; }
		public MultiFileDocument MultyFileDocumentsList_SelectedItem { get; internal set; }
		public FlowDocument LinksReader_Document { get; internal set; }
		public int DataTabControl_SelectedIndex { get; internal set; }
		public static Visibility InstanckeyShortcuts_Visibility { get; internal set; }
		public string ResourceContent { get; internal set; }
		public bool HtmlTab_IsSelected { get; set; }

		public static string UrlBase = "https://ajuro.azurewebsites.net/api/";
		// public static string UrlBase = "https://localhost:44351/api/";



		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}