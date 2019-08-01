using Markdig;
using Ajuro.Notes.DataAccess;
using Ajuro.Notes.Model;
using Ajuro.Notes.Views;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Ajuro.Net.Template.Processor;
using System.Text;

namespace Ajuro.Notes.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public string ScriptErrorSuppressor = "<script>window.onerror = function(msg,url,line){return true;}</script>";
		public List<LogEntry> LogEntries = new List<LogEntry>();
		public int logLevel = 0;
		public void Log(LogEntry logEntry)
		{
			logEntry.LogLevel = logLevel;
			LogEntries.Add(logEntry);
			Console.WriteLine(logEntry.Message);
		}
		public string LogEntriesToHtml()
		{
			StringBuilder sb = new StringBuilder();
			foreach (LogEntry logEntry in LogEntries)
			{
				sb.AppendLine("<div>" + new String('.', logEntry.LogLevel) + "<span class='type'>" + (logEntry.Type == LogType.Normal ? "" : logEntry.Type.ToString()) + "</span> <span class='message'>" + logEntry.Message + "</span>" + (string.IsNullOrEmpty(logEntry.Link) ? "" : " <span class='link'>" + logEntry.Link + "</span>") + "</div>");
			}
			return sb.ToString();
		}
		public string LogEntriesToText()
		{
			StringBuilder sb = new StringBuilder();
			foreach (LogEntry logEntry in LogEntries)
			{
				sb.AppendLine(new String('\t', logEntry.LogLevel) + logEntry.Message);
			}
			return sb.ToString();
		}


		private readonly AppViewModel view = new AppViewModel();
		private FileDataAccess FileDataAccess { get; set; }
		private TemplateInterpreter templateInterpreter = null;
		private TemplateProcessor templateProcessor = null;

		// Business configuration used to define this instance as a customized product.
		BusinessConfiguration businessConfiguration = new BusinessConfiguration();

		// Local settings related to environment.
		LocalSettings localSettings = new LocalSettings();

		public ResourceFolder SelectedResourceFolder { get; set; }

		public string Mem { get; set; }

		public UserAccount Me { get; set; }

		int ItemsSelectedIndex = 0;
		string SelectedChannel { get; set; }


		/// <summary>
		/// Where all notes are stored?
		/// </summary>
		string BasePath = @"C:\\Work\\Resources\\"; // test

		/// <summary>
		/// Last selected note item.
		/// </summary>
		//FileItem CurrentItem { get; set; }

		/// <summary>
		/// Last selected note item.
		/// </summary>
		public ObservableCollection<VisibilityLevel> VisibilityLevels { get; set; }

		/// <summary>
		/// Use a increment to generate unique file names. Check file existence for while available.
		/// </summary>
		static int FileNr = 0;

		public bool CompileRT { get; private set; }

		/// <summary>
		/// Default public constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			CustomInitialize();
		}

		/// <summary>
		/// All custom window initialization maintained by developers.
		/// </summary>
		private void CustomInitialize()
		{
			FileDataAccess = new FileDataAccess();
			LoginWithFacebookButton.DataContext = view;
			LoginWithFacebookButton.Visibility = Visibility.Visible;


			// Accept Tab key in RichTextBox
			ResourceContentTextBox.AcceptsTab = true;
			this.Closing += Window_Closing;
			MainModel.Instance.FileItems = new ItemList(
				new List<MultiFileDocument>
				{ }
				);
			MainModel.Instance.AllItems = new ItemList(
				new List<MultiFileDocument>
				{ }
				);

			VisibilityLevels = new ObservableCollection<VisibilityLevel>()
			{
				new VisibilityLevel()
				{
					Key = 0,
					Name = "Only Me",
					Image = "https://justice.org.au/wp-content/uploads/2017/08/avatar-icon-300x300.png"
				},
				new VisibilityLevel()
				{
					Key = 1,
					Name = "With Link",
					Image = "http://icon-park.com/imagefiles/link_icon_black.png"
				},
				new VisibilityLevel()
				{
					Key = 2,
					Name = "Everyone",
					Image = "https://static.thenounproject.com/png/17086-200.png"
				}
			};

			if (File.Exists("Resources/Config/BusinessConfiguration.json"))
			{
				businessConfiguration = JsonConvert.DeserializeObject<BusinessConfiguration>(File.ReadAllText("Resources/Config/BusinessConfiguration.json"));
				RepositorySourceComboBox.ItemsSource = businessConfiguration.ResourceFolders;
				if(businessConfiguration.ResourceFolders.Count > 0)
				{
					RepositorySourceComboBox.SelectedIndex = 0;
				}
			}
			if (File.Exists("LocalSettings.json"))
			{
				localSettings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText("LocalSettings.json"));
			}
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account"))
			{
				string myIdentity = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account/me.json");
				Me = JsonConvert.DeserializeObject<UserAccount>(myIdentity);

				ChannelSelector.Items.Insert(0, Me.RowKey);
				ChannelSelector.SelectedIndex = 0;
				SelectedChannel = ChannelSelector.Text;
			}
			else
			{
				Me = new UserAccount()
				{
					RowKey = Guid.NewGuid().ToString(),
					DisplayName = "Log In",
					Email = string.Empty
				};

				ChannelSelector.Items.Clear();
				SelectedChannel = null;
			}

			ChannelSelector.SelectedIndex = 0;
			MainModel.Instance.FileItems.Items = new ObservableCollection<MultiFileDocument>();
			// Don't be invasive, ask user for permission to ctreate stuffs on his disk if is not in your app folder.
			if (!Directory.Exists(BasePath))
			{
				var s = MessageBox.Show("Folder is expected. Create [" + BasePath + "] ?", "Startup", MessageBoxButton.YesNo);
				if (s == MessageBoxResult.Yes)
				{
					Directory.CreateDirectory(BasePath);
				}
				else
				{
					return;
				}
			}

			// Colect notes from disk
			var files = Directory.GetFiles(BasePath).ToList();
			ObservableCollection<MultiFileDocument> items = new ObservableCollection<MultiFileDocument>();
			foreach (string filePath in files)
			{
				if (filePath.EndsWith(".meta"))
				{
					NoteEntity noteMeta = JsonConvert.DeserializeObject<NoteEntity>(File.ReadAllText(filePath));
					items.Add(new MultiFileDocument()
					{
						Tags = noteMeta.Tags,
						Files = noteMeta.Files,
						Key = noteMeta.RowKey,
						Name = noteMeta.Title,
						Synced = noteMeta.Synced,
						Label = string.Empty,
						Visibility = VisibilityLevels[0]
					});
				}
			}
			MainModel.Instance.AllItems.Items = items;
			FilterNotes(string.Empty); // Dows it improve performance to wait for the list to be constructed?

			// Wire-up events
			ResourceContentTextBox.PreviewKeyUp += ResourceContentTextBox_PreviewKeyUp;
			ResourceContentTextBox.PreviewKeyDown += ResourceContentTextBox_PreviewKeyDown;
			Resource_Name.PreviewKeyUp += Resource_Name_PreviewKeyUp;
			MultyFileDocumentsList.PreviewKeyUp += FilesList_PreviewKeyUp;

			InisializeSettings();

			// SelectedProfileIndicator.DataContext = MainModel.Instance;
			// EnvironmentComboBox.DataContext = MainModel.Instance;
			// CurentItemLabel.DataContext = MainModel.Instance;

			if (!string.IsNullOrEmpty(localSettings.ProfileName) && MainModel.Instance.SettingProfiles != null)
			{
				MainModel.Instance.SelectedProfile = MainModel.Instance.SettingProfiles.Where(p => p.Name.Equals(localSettings.ProfileName)).FirstOrDefault();
			}

			if (MainModel.Instance.SelectedProfile == null)
			{
				MainModel.Instance.SelectedProfile = MainModel.Instance.SettingProfiles[0];
			}

			// Provide me as model
			DataContext = MainModel.Instance;

			string filterString = localSettings.LastFilterValue;
			FilterItems.Text = filterString;
			FilterNotes(filterString);

			if (!string.IsNullOrEmpty(localSettings.LastDocumentName))
			{
				MultiFileDocument lastDocument = GeDocumentByName(localSettings.LastDocumentName);
				MultyFileDocumentsList.SelectedItem = lastDocument;
				Adopt(lastDocument);
			}
		}

		private void ResourceContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				KeyShortcuts.Visibility = Visibility.Visible;
				ButtonShortcuts.Visibility = Visibility.Collapsed;
			}
		}

		private void InisializeSettings()
		{
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account"))
			{
				if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account/settings.json"))
				{
					string mySettings = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/Ajuro.Notes/Account/settings.json");
					var profiles = JsonConvert.DeserializeObject<List<SettingsProfile>>(mySettings);
					MainModel.Instance.SettingProfiles = new ObservableCollection<SettingsProfile>();
					foreach (var profile in profiles)
					{
						MainModel.Instance.SettingProfiles.Add(profile);
					}
				}
			}

			if (MainModel.Instance.SettingProfiles == null)
			{
				MainModel.Instance.SettingProfiles = new ObservableCollection<SettingsProfile>();

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Pre-Alfa",
					Properties = new List<KeyValuePair<string, string>>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Alfa",
					Properties = new List<KeyValuePair<string, string>>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Beta",
					Properties = new List<KeyValuePair<string, string>>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "RC",
					Properties = new List<KeyValuePair<string, string>>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "RTM",
					Properties = new List<KeyValuePair<string, string>>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "GA",
					Properties = new List<KeyValuePair<string, string>>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Live",
					Properties = new List<KeyValuePair<string, string>>()
				});
			}
		}

		/// <summary>
		/// When selected note is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
			{
				MainModel.Instance.FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);
			}
			if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
			{
				DuplicateItem("notes");
			}
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				if (e.Key == Key.Delete)
				{
					DeleteItem("notes");
				}
			}
		}

		public void ExecuteWindowAction()
		{

		}

		/// <summary>
		/// Add a new note
		/// </summary>
		private void NewItem(string itemName = null, string content = null)
		{
			if (string.IsNullOrEmpty(itemName))
			{
				while (File.Exists(BasePath + "NewItem_" + FileNr + ".txt"))
				{
					FileNr++;
				}
				itemName = "NewItem_" + FileNr + ".txt"; // Microsoft.VisualBasic.Interaction.InputBox("Question?", "Title", "Default Text");
			}
			FileNr++;
			// if (response == MessageBoxResult.Yes)
			{
				MainModel.Instance.FileItems.CurrentItem = new MultiFileDocument()
				{
					Name = itemName,
					Key = Guid.NewGuid().ToString(),
					Label = "!!"
				};
				MainModel.Instance.FileItems.Add(MainModel.Instance.FileItems.CurrentItem);
				MainModel.Instance.AllItems.Add(MainModel.Instance.FileItems.CurrentItem);
				ResourceContentTextBox.Document = new FlowDocument();
			}
		}

		public string ExecuteSQL(string connectionString, string queryString)
		{
			SQL.DataAccess dataAccess = new SQL.DataAccess();
			string result = dataAccess.ExecuteQuery(connectionString, queryString);
			return result;
		}

		private void ExecuteQuery(Key key, bool openWindow)
		{
			SQL.DataAccess dataAccess = new SQL.DataAccess();
			string queryString = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd).Text;
			var property = MainModel.Instance.SelectedProfile.Properties.Where(p => p.Key == "cs").FirstOrDefault();
			string connectionString = property.Value == null ? string.Empty : property.Value.ToString();


			// Edit in external editor!
			if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.F5)
			{
				Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + MainModel.Instance.FileItems.CurrentItem.Key);
				return;
			}
			else if (queryString.StartsWith("-- MD") || MainModel.Instance.FileItems.CurrentItem.Name.EndsWith(".md"))
			{
				var result = Markdown.ToHtml(queryString);
				PreviewHtml.NavigateToString(ScriptErrorSuppressor  + "<div>You can not execute a MD file. Change to another file if you need to execute.</div>" + result);
				return;
			}

			if (MainModel.Instance.FileItems.CurrentItem.Files!= null && MainModel.Instance.FileItems.CurrentItem.Files.Count > 0 && MainModel.Instance.FileItems.CurrentItem.Files[0].EndsWith(".rd"))
			{
				SaveItem("notes");
				MultiFileDocument document = GeDocumentByName(MainModel.Instance.FileItems.CurrentItem.Files[0]);
				if (document != null)
				{
					ExecuteTemplate(File.ReadAllText(BasePath + document.Key));
				}
				return;
			}

			if (queryString.StartsWith("-- SQL"))
			{
				if (string.IsNullOrEmpty(connectionString))
				{
					MessageBox.Show("Please define a property named cs with the value of your connectionString in your current environment or change the environment from " + MainModel.Instance.SelectedProfile.Name, "No connection string defined!");
					return;
				}
				string result = ExecuteSQL(connectionString, queryString);
				string theme = File.ReadAllText("Resources/Theme/Default/style.css");
				string view = "<style>" + theme + "</style>" + result;
				File.WriteAllText("Results.html", view);
				if (openWindow)
				{
					Process.Start("Results.html");
				}
				PreviewHtml.NavigateToString(ScriptErrorSuppressor + view);
				if (string.IsNullOrEmpty(MainModel.Instance.JsonRepresentation))
				{
					PreviewJson.NavigateToString("Parse Error");
				}
				else
				{
					PreviewJson.NavigateToString(MainModel.Instance.JsonRepresentation);
				}
				PreviewHtml.NavigateToString(ScriptErrorSuppressor + result);
			}

			if (queryString.StartsWith("-- HTML") || MainModel.Instance.FileItems.CurrentItem.Name.EndsWith(".html"))
			{
				File.WriteAllText("Results.html", queryString);
				if (!CompileRT)
				{
					PreviewHtml.NavigateToString(ScriptErrorSuppressor + queryString);
				}
			}

			if (queryString.StartsWith("-- JSON") || MainModel.Instance.FileItems.CurrentItem.Name.EndsWith(".json"))
			{
				var result = Markdown.ToHtml(queryString);
				PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<pre>" + result + "</pre>");
			}
			if (Keyboard.Modifiers == ModifierKeys.Shift && key == Key.F5)
			{
				File.WriteAllText("Results.html", File.ReadAllText("C:\\Work\\Resources\\" + MainModel.Instance.FileItems.CurrentItem.Key));
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", "Results.html");
			}

			if (queryString.StartsWith("-- RD") || MainModel.Instance.FileItems.CurrentItem.Name.EndsWith(".rd"))
			{
				ExecuteTemplate(queryString);
			}
		}

		private void ExecuteTemplate(string executionPlan)
		{
			TemplaterInstruction templaterInstruction = JsonConvert.DeserializeObject<TemplaterInstruction>(executionPlan.Replace("\\", "\\\\"));
			templateProcessor = new TemplateProcessor();
			templateInterpreter = new TemplateInterpreter();

			string variables = string.Empty;
			if (!string.IsNullOrEmpty(templaterInstruction.CSV))
			{
				var itemCSV = MainModel.Instance.AllItems.Items.Where(p => p.Name.Equals(templaterInstruction.CSV)).FirstOrDefault();
				string variablesCSV = File.ReadAllText(BasePath + itemCSV.Key);
				var itemVar = MainModel.Instance.AllItems.Items.Where(p => p.Name.Equals(templaterInstruction.Model)).FirstOrDefault();
				variables = File.ReadAllText(BasePath + itemVar.Key);
				variables = RecreateVariables(variables, variablesCSV);
			}
			else
			{
				var itemVar = MainModel.Instance.AllItems.Items.Where(p => p.Name.Equals(templaterInstruction.Model)).FirstOrDefault();
				if(itemVar == null)
				{
					return;
				}
				variables = File.ReadAllText(BasePath + itemVar.Key);
			}
			var itemTemplate = MainModel.Instance.AllItems.Items.Where(p => p.Name.Equals(templaterInstruction.Template)).FirstOrDefault();
			if (itemTemplate != null && File.Exists(BasePath + itemTemplate.Key))
			{
				LogEntries.Clear();
				Log(new LogEntry() { Message = "Processing template: " + itemTemplate.Name });

				logLevel++;
				var result0 = ParseTemplate(templaterInstruction.Project, itemTemplate.Name, BasePath + itemTemplate.Key, variables);
				AffectedFilesList.ItemsSource = MainModel.Instance.FileItems.CurrentItem.AffectedFiles;

				logLevel--;

				PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<pre>" + result0 + "</pre>");

				string logHtml = "<style>.message{color: green} .type{color: red} .link{color: blue}</style>" + LogEntriesToHtml();

				File.WriteAllText("LastLog.html", logHtml);
				File.WriteAllText(MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Project + "\\LastLog.html", logHtml);
				// PreviewJson.NavigateToString("<pre>" + TemplateInterpreter.Trace + "</pre><br /><pre>" + TemplateProcessor.Trace + "</pre>");
				PreviewJson.NavigateToString(logHtml);
			}
			else
			{
				PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<pre>Missing template file!</pre>");
			}
			AffectedFilesTab.IsSelected = true;
			LogsTab.IsSelected = true;
		}

		private string ParseTemplate(string projectPath, string templatename, string templatePath, string variables)
		{
			string template = File.ReadAllText(templatePath);
			if (template.IndexOf("============== Vars end ==============") > -1)
			{
				template = template.Substring(template.IndexOf("============== Vars end ==============") + "============== Vars end ==============".Length);
			}
			var vars = new List<AjuVarset>() { templateProcessor.ToAjuVarset(JObject.Parse(variables)) };
			TemplateProcessor.processingSteps = "";

			var result = templateProcessor.TestCase(template, vars[0].Varset);



			var templateReady = templateProcessor.UpdateTemplate(variables, template);

			File.WriteAllText(projectPath + "\\" + templatename + ".ready", templateReady);
			Log(new LogEntry() { Message = "Ajuro Ready file created: " + templatePath, Name = templatename + ".ready", Path = projectPath + "\\" + templatename + ".ready", Size = templateReady.Length });

			LogEntries.AddRange(templateProcessor.LogEntries);

			if (string.IsNullOrEmpty(projectPath))
			{
				Log(new LogEntry() { Message = "No project path was defined, in case we need to create files they will be created in the 'none' folder of the working folder. For: " + templatePath });
			}
			// NewItem(templaterInstruction.Ready, result);
			templateInterpreter.InterpretProcessedTemplate(projectPath, MainModel.Instance.SelectedProfile.Properties, templateReady);

			LogEntries.AddRange(templateInterpreter.LogEntries);
			// File.WriteAllText("Resources\\" + TemplateInterpreter + ".logs.html", log);

			MainModel.Instance.FileItems.CurrentItem.AffectedFiles.Clear();
			foreach (var entry in LogEntries)
			{
				if (entry.Name != null)
				{
					if (MainModel.Instance.FileItems.CurrentItem.AffectedFiles.Where(p => p.Name.Equals(entry.Name)).FirstOrDefault() == null)
					{
						if (File.Exists(entry.Path))
						{
							entry.Size = new FileInfo(entry.Path).Length;
							if (entry.Size > 0)
							{
								entry.Size = entry.Size / 8;
							}
						}
						MainModel.Instance.FileItems.CurrentItem.AffectedFiles.Add(new AffectedFile() { Name = entry.Name, Path = entry.Path, Size = entry.Size });
					}
				}
			}
			var ScheduledProcesses = new List<ScheduledProcess>();
			foreach (var item in templateInterpreter.ScheduledProcesses)
			{
				ScheduledProcesses.Add(item);
			}
			templateInterpreter.ScheduledProcesses.Clear();
			foreach (var item in ScheduledProcesses)
			{
				Log(new LogEntry() { Message = "Processing fork template: " + item.TemplatePath });
				logLevel++;
				variables = File.ReadAllText(projectPath + "\\" + item.Data);
				var childTemplateReady = ParseTemplate(projectPath, item.TemplatePath.Substring(item.TemplatePath.LastIndexOf("\\") + 1), item.TemplatePath, variables);
				logLevel--;
			}
			return templateReady;
		}

		private string RecreateVariables(string variables, string itemCSV)
		{
			var jsonObject = (JObject)JsonConvert.DeserializeObject(variables);
			var lines = itemCSV.Split(new string[] { "\n" }, StringSplitOptions.None);
			// for (int i = 0; i < lines.Length; i++)
			int i = 0;
			{
				if (lines[i].Length > 0)
				{
					if (lines[i][0] != '\t')
					{
						return MapVariables(jsonObject, lines, i, 0);
					}
				}
			}
			return string.Empty;
		}

		private string MapVariables(JObject jsonObject, string[] lines, int i, int identationLevel)
		{
			JArray jsonPointer = FindVariable(jsonObject, lines[i].Trim());

			// Find the property to be mapped. And determine the items structure in case of arrays.
			if (jsonPointer is JArray)
			{
				List<string> keys = new List<string>();
				// Parse all array items
				foreach (var item in ((JArray)jsonPointer).Children())
				{
					// To collect all possible properties
					foreach (var property in item.Children())
					{
						var propertyName = property.Path;
						if (propertyName.IndexOf('.') > 0)
						{
							propertyName = propertyName.Substring(propertyName.LastIndexOf('.') + 1);
						}
						if (!keys.Contains(propertyName))
						{
							keys.Add(propertyName);
						}
					}
				}

				i++;

				while (i < lines.Length)
				{
					if (string.IsNullOrWhiteSpace(lines[i]))
					{
						i++;
						continue;
					}
					int numberOfTabs = lines[i].Length - lines[i].Trim().Length;
					var pointer = ((JArray)jsonPointer);
					if (numberOfTabs == identationLevel + 1)
					{
						JObject newItem = new JObject();
						var CsvProperties = lines[i].Trim().Split('\t');
						for (int k = 0; k < CsvProperties.Length && k < keys.Count; k++)
						{
							newItem.Add(keys[k], CsvProperties[k]);
						}
						i++;

						// Adding columns

						JArray columns = new JArray();
						newItem.Add("Columns", columns);

						List<string> keys2 = new List<string>();
						foreach (var item in ((JArray)jsonPointer)[0]["Columns"].Children())
						{
							// To collect all possible properties
							foreach (var property in item.Children())
							{
								var propertyName = property.Path;
								if (propertyName.IndexOf('.') > 0)
								{
									propertyName = propertyName.Substring(propertyName.LastIndexOf('.') + 1);
								}
								if (!keys2.Contains(propertyName))
								{
									keys2.Add(propertyName);
								}
							}
						}

						while (i < lines.Length)
						{
							numberOfTabs = lines[i].Length - lines[i].TrimStart().Length;
							if(numberOfTabs < identationLevel + 2)
							{
								i--;
								break;
							}
							JObject newItem2 = new JObject();
							var CsvProperties2 = lines[i].Trim().Split('\t');
							for (int k = 0; k < CsvProperties2.Length && k < keys2.Count; k++)
							{
								newItem2.Add(keys2[k], CsvProperties2[k]);
							}
							columns.Add(newItem2);
							i++;
						}

						pointer.Add(newItem);
					}
					i++;
				}
			}
			// jsonObject.Remove("Tables");
			// jsonObject.Add("Tables", jsonPointer);
			jsonPointer.RemoveAt(0);
			jsonObject["Tables"] = jsonPointer;
			string data = jsonObject.ToString();
			return data;
		}

		private JArray FindVariable(JObject jsonObject, string v)
		{
			JArray result = null;
			if(jsonObject.ContainsKey(v))
			{
				result = (JArray)jsonObject[v];
				if(result is JObject)
				{

				}
				else if(result is JArray)
				{

				}
			}
			return result;
		}

		/// <summary>
		/// Delete existent note
		/// </summary>
		private void DeleteItem(string entity)
		{
			var response = MessageBox.Show("Delete file [" + MainModel.Instance.FileItems.CurrentItem.Name + "] ?", "Question", MessageBoxButton.YesNo);
			if (response == MessageBoxResult.Yes)
			{
				File.Delete(BasePath + MainModel.Instance.FileItems.CurrentItem.Name);
				MainModel.Instance.FileItems.Remove(MainModel.Instance.FileItems.CurrentItem);
				MainModel.Instance.AllItems.Remove(MainModel.Instance.FileItems.CurrentItem);
			}
		}

		/// <summary>
		/// User is editing the note name. From this point the user can either save, creating a new file and deleting the old one or he can duplicate, creating a new file.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void Resource_Name_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			ExecuteWindowAction(e);
		}

		private void ExecuteWindowAction(KeyEventArgs e)
		{
			if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
			{
				MainModel.Instance.FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);
			}
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				bool consumed = false;
				if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					SaveItem("notes");
				}
				if (e.Key == Key.F6 && Keyboard.Modifiers == ModifierKeys.Control)
				{
					if (MainModel.Instance.FileItems.CurrentItem.Files[0].EndsWith(".rd"))
					{
						MultiFileDocument document = GeDocumentByName(MainModel.Instance.FileItems.CurrentItem.Files[0]);
						FragmentSelector templateEditorWindow = new FragmentSelector();
						templateEditorWindow.BasePath = BasePath;
						templateEditorWindow.LoadTemplateFromPath(File.ReadAllText(BasePath + document.Key));
						templateEditorWindow.Show();
					}
					else if(true)
					{

					}
				}
				if (e.Key == Key.F5 || CompileRT)
				{
					consumed = true;
					ExecuteQuery(e.Key, Keyboard.Modifiers == ModifierKeys.Control);
				}
				if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					MainModel.Instance.FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);
				}
				if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					DuplicateItem("notes");
				}
				if (e.Key == Key.U && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					UploadItem("notes");
				}
				if (!consumed)
				{
					if (e.Key != Key.PageUp && e.Key != Key.PageDown && e.Key != Key.Home && e.Key != Key.End && e.Key != Key.Tab && e.Key != Key.Right && e.Key != Key.Left && e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.System && e.Key != Key.LeftCtrl && e.Key != Key.LeftAlt && e.Key != Key.LeftShift && e.Key != Key.RightCtrl && e.Key != Key.RightAlt && e.Key != Key.RightShift)
					{
						MainModel.Instance.FileItems.CurrentItem.Name = Resource_Name.Text;
						MainModel.Instance.FileItems.CurrentItem.Status = 1;
					}
				}
			}
		}

		/// <summary>
		/// When user edits the content of the note.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void ResourceContentTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			KeyShortcuts.Visibility = Visibility.Collapsed;
			ButtonShortcuts.Visibility = Visibility.Visible;
			ExecuteWindowAction(e);

			if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
			{
				ReloadItems("notes", "list", SelectedChannel);
			}
		}

		private void ReloadItems(ResourceFolder rootFolder)
		{
			var rawItemsList = FileDataAccess.GetItems(rootFolder.Path);
			bool hasMetadata = rootFolder.Type == "notes";

			MainModel.Instance.AllItems.Clear();
			foreach (var item in rawItemsList)
			{
				if (hasMetadata)
				{
					if (!item.Name.EndsWith(".meta"))
					{
						continue;
					}
					else
					{
						var meta = JsonConvert.DeserializeObject<NoteEntity>(File.ReadAllText(BasePath + item.Name));
						MainModel.Instance.AllItems.Add(new MultiFileDocument()
						{
							Tags = meta.Tags,
							Files = meta.Files,
							Author = meta.Author,
							Name = meta.Title,
							Key = meta.RowKey,
							Synced = DateTime.Now
						});
					}
				}
				else
				{
					ObservableCollection<string> files = new ObservableCollection<string>();
					if (File.Exists(item.Path))
					{
						files.Add(item.Path);
					}
					else
					{
						// Is a folder
						// files.AddRange(Directory.GetFiles(item.Path));
					}

					MainModel.Instance.AllItems.Add(new MultiFileDocument()
					{
						Name = item.Name,
						Synced = DateTime.Now,
						Label = string.Empty,
						Files = files
					});
				}
			}
			FilterNotes(string.Empty);
			StatusBartextBlock.Text = "Found " + MainModel.Instance.FileItems.Items.Count + " / " + MainModel.Instance.AllItems.Items.Count + " items;";
		}
		private void ReloadItems(string entity, string action, string channel)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(MainModel.UrlBase + entity + "/" + action + "/" + channel);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "GET";

			var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			int found = 0;
			int added = 0;
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var data = streamReader.ReadToEnd();
				var items = JsonConvert.DeserializeObject<List<NoteEntity>>(data);
				foreach (var item in items)
				{
					found++;
					if (!File.Exists(BasePath + item.RowKey))
					{
						// File.WriteAllText(BasePath + item.Title, item.Content);
						File.WriteAllText(BasePath + item.RowKey, item.Content);
						item.Content = string.Empty;
						File.WriteAllText(BasePath + item.RowKey + ".meta", JsonConvert.SerializeObject(item));
						MainModel.Instance.AllItems.Add(new MultiFileDocument()
						{
							Name = item.RowKey,
							Synced = DateTime.Now,
							Label = string.Empty
						});

						added++;
					}
				}
			}
			FilterNotes(string.Empty);
			StatusBartextBlock.Text = "Found " + added + " new items from " + found;
		}

		private void PreviewItems(string entity, string action, string channel)
		{
			if (MainModel.Instance.AllItems == null)
			{
				return;
			}
			MainModel.Instance.AllItems.Clear();
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(MainModel.UrlBase + entity + "/" + action + "/" + channel);
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "GET";

			var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
			int found = 0;
			int added = 0;
			using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
			{
				var data = streamReader.ReadToEnd();
				var items = JsonConvert.DeserializeObject<List<NoteEntity>>(data);
				foreach (var item in items)
				{
					found++;
					if (!File.Exists(BasePath + item.RowKey))
					{
						// File.WriteAllText(BasePath + item.Title, item.Content);
						File.WriteAllText(BasePath + item.RowKey, item.Content);

						added++;
					}
					item.Content = string.Empty;
					// File.WriteAllText(BasePath + item.RowKey + ".meta", JsonConvert.SerializeObject(item));
					MainModel.Instance.AllItems.Add(new MultiFileDocument()
					{
						Author = item.Author,
						Name = item.Title,
						Key = item.RowKey,
						Synced = DateTime.Now,
						Label = string.Empty
					});
				}
			}
			FilterNotes(string.Empty);
			StatusBartextBlock.Text = "Found " + added + " new items from " + found;
		}

		private void UploadItem(string entity)
		{
			if (string.IsNullOrEmpty(Me.RowKey))
			{
				Login();
			}

			var httpWebRequest = (HttpWebRequest)WebRequest.Create(MainModel.UrlBase + entity + "/" + (MainModel.Instance.FileItems.CurrentItem.Synced == DateTime.MinValue ? "insert" : "update/" + MainModel.Instance.FileItems.CurrentItem.Key));

			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
				NoteEntity item = new NoteEntity()
				{
					Author = Me.RowKey,
					RowKey = MainModel.Instance.FileItems.CurrentItem.Key,
					Content = range.Text,
					Title = MainModel.Instance.FileItems.CurrentItem.Name
				};

				streamWriter.Write(JsonConvert.SerializeObject(item));
				streamWriter.Flush();
				streamWriter.Close();

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					MainModel.Instance.FileItems.CurrentItem.Synced = DateTime.Now;
					MainModel.Instance.FileItems.CurrentItem.Author = Me.RowKey;
					SaveItem(entity);
					StatusBartextBlock.Text = DateTime.Now.ToShortTimeString() + ": Uploaded [" + MainModel.Instance.FileItems.CurrentItem.Name + "]";
					MainModel.Instance.FileItems.CurrentItem.Status = 2;
				}
			}
		}

		private void Login()
		{
			AccountWindow accountWindow = new AccountWindow();

			accountWindow.WireUpData(Me);
			accountWindow.Closing += AccountWindow_Closing;
			accountWindow.Show();
		}

		/// <summary>
		/// Creating a duplicate is actually saving the work in progress as a new note.
		/// </summary>
		private void DuplicateItem(string entity)
		{
			MultiFileDocument NewCurrentItem = new MultiFileDocument()
			{
				Name = MainModel.Instance.FileItems.CurrentItem.Name,
				Label = ""
			};
			MainModel.Instance.FileItems.Add(NewCurrentItem);
			TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
			File.WriteAllText(BasePath + MainModel.Instance.FileItems.CurrentItem.Name, range.Text);
			MainModel.Instance.FileItems.CurrentItem.Label = "";
		}

		/// <summary>
		/// Work in progress gets saved in the same file if the note name has not changes. If the name changed, a new file is created and the old file is deleted.
		/// </summary>
		private void SaveItem(string entity)
		{
			TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
			if (MainModel.Instance.FileItems.CurrentItem.Key == null)
			{
				MainModel.Instance.FileItems.CurrentItem.Key = Guid.NewGuid().ToString();
			}
			File.WriteAllText(BasePath + MainModel.Instance.FileItems.CurrentItem.Key, range.Text);
			MainModel.Instance.FileItems.CurrentItem.Name = Resource_Name.Text;
			MainModel.Instance.FileItems.CurrentItem.LastUpdated = DateTime.UtcNow;
			FileDataAccess.SaveItemMeta(BasePath, MainModel.Instance.FileItems.CurrentItem);
			/*CurrentItem.Label = "";
			CurrentItem.Name = Resource_Name.Text;
			if (CurrentItem.Name.ToLower() != CurrentItem.NameOriginal.ToLower())
			{
				if (File.Exists(BasePath + Resource_Name.Text))
				{
					File.Delete(BasePath + CurrentItem.NameOriginal);
				}
			}*/
			MainModel.Instance.FileItems.CurrentItem.Status = 0;
		}

		/// <summary>
		/// When another note is selected in the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			bool isCompilable = false;
			MainModel.Instance.FileItems.CurrentItem = ((MultiFileDocument)MultyFileDocumentsList.SelectedItem);
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				MultiFileDocument document = null;
				if(MainModel.Instance.FileItems.CurrentItem.Name.EndsWith(".rd"))
				{
					document = MainModel.Instance.FileItems.CurrentItem;
				}
				else
				if (MainModel.Instance.FileItems.CurrentItem.Files.Count > 0)
				{
					if(MainModel.Instance.FileItems.CurrentItem.Files[0].EndsWith(".rd"))
					{
						document = GeDocumentByName(MainModel.Instance.FileItems.CurrentItem.Files[0]);
					}
					else
					{
						var fName = MainModel.Instance.FileItems.CurrentItem.Files.Where(p => p.EndsWith(".rd")).FirstOrDefault();
						if(!string.IsNullOrEmpty(fName))
						{
							document = GeDocumentByName(fName);
						}
					}
				}
				else
				{
				}

				if (document != null)
				{
					isCompilable = true;
					Button_BrowseLog.Visibility = Visibility.Visible;
					Button_Compile.Visibility = Visibility.Visible;
					Button_Project.Visibility = Visibility.Visible;
					Button_Template.Visibility = Visibility.Visible;
					Button_Data.Visibility = Visibility.Visible;

					var executionPlan = File.ReadAllText(BasePath + document.Key);
					MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction = JsonConvert.DeserializeObject<TemplaterInstruction>(executionPlan.Replace("\\", "\\\\"));
				}

				AffectedFilesList.ItemsSource = MainModel.Instance.FileItems.CurrentItem.AffectedFiles;
				MainModel.Instance.LastDocumentNames.Add(MainModel.Instance.FileItems.CurrentItem.Name);
				while(MainModel.Instance.LastDocumentNames.Count > 10)
				{
					MainModel.Instance.LastDocumentNames.RemoveAt(MainModel.Instance.LastDocumentNames.Count-1);
				}

				// MainModel.Instance.CurrentItem = MainModel.Instance.FileItems.CurrentItem;
				Resource_Name.Text = MainModel.Instance.FileItems.CurrentItem.Name;
				FlowDocument ResourceContentDocument = new FlowDocument();
				FlowDocument LinksReaderDocument = new FlowDocument();
				Paragraph ResourceContentParagraph = new Paragraph();
				Paragraph LinksReaderParagraph = new Paragraph();
				if (!File.Exists(BasePath + MainModel.Instance.FileItems.CurrentItem.Key))
				{
					TagsStackPanel.Children.Clear();
					FilesStackPanel.Children.Clear();
				}
				TagsStackPanel.Orientation = Orientation.Horizontal;
			
				FilesStackPanel.Children.Clear();
				if (MainModel.Instance.FileItems.CurrentItem.Files != null)
				{
					FilesContainer.Visibility = Visibility.Visible;
					foreach (string file in MainModel.Instance.FileItems.CurrentItem.Files)
					{
						Label label = new Label();
						label.BorderBrush = Brushes.Gray;
						label.Tag = file.Substring(file.LastIndexOf("\\") + 1);
						label.MouseUp += DocumenPartLabel_MouseUp;
						label.MouseDoubleClick += DocumenPartLabel_MouseDoubleClick;
						label.BorderThickness = new Thickness(2);
						label.Content = file.Substring(file.LastIndexOf("\\") + 1);
						FilesStackPanel.Children.Add(label);
					}
				}

				TagsStackPanel.Children.Clear();
				if (MainModel.Instance.FileItems.CurrentItem.Tags != null)
				{
					foreach (string tag in MainModel.Instance.FileItems.CurrentItem.Tags)
					{
						Label label = new Label();
						label.Content = tag;
						TagsStackPanel.Children.Add(label);
					}
				}

				if (!File.Exists(BasePath + MainModel.Instance.FileItems.CurrentItem.Key))
				{
					MainModel.Instance.FileItems.CurrentItem.Label = "!";
					ResourceContentTextBox.Document = new FlowDocument();
					DataTabControl.SelectedIndex = 1;

					if (MainModel.Instance.FileItems.CurrentItem.Files != null && File.Exists(MainModel.Instance.FileItems.CurrentItem.Files[0]))
					{
						string resourceContent = File.ReadAllText(MainModel.Instance.FileItems.CurrentItem.Files[0]);
						LinksReaderParagraph.Inlines.Add(new Run(resourceContent));
						LinksReaderDocument.Blocks.Add(LinksReaderParagraph);
						LinksReader.Document = LinksReaderDocument;
					}
					return;
				}
				string content = File.ReadAllText(BasePath + MainModel.Instance.FileItems.CurrentItem.Key);
				ResourceContentParagraph.Inlines.Add(new Run(content));
				LinksReaderParagraph.Inlines.Add(new Run(content));

				ResourceContentDocument.Blocks.Add(ResourceContentParagraph);
				ResourceContentTextBox.Document = ResourceContentDocument;

				LinksReaderDocument.Blocks.Add(LinksReaderParagraph);
				LinksReader.Document = LinksReaderDocument;
			}
			if (!isCompilable)
			{

				Button_BrowseLog.Visibility = Visibility.Collapsed;
				Button_Compile.Visibility = Visibility.Collapsed;
				Button_Project.Visibility = Visibility.Collapsed;
				Button_Template.Visibility = Visibility.Collapsed;
				Button_Data.Visibility = Visibility.Collapsed;
			}
		}
		
		private void HelpLink_MouseUp(object sender, MouseButtonEventArgs e)
		{
			System.Diagnostics.Process.Start("http://otb.expert/Ajuro.Notes/mail_template_invitation.html");
		}

		private void FilterItems_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				ItemsSelectedIndex += 1;
				if (ItemsSelectedIndex >= MainModel.Instance.FileItems.Items.Count)
				{
					ItemsSelectedIndex = 0;
				}
				MultyFileDocumentsList.SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (e.Key == Key.Up)
			{
				ItemsSelectedIndex -= 1;
				if (ItemsSelectedIndex < 0)
				{
					ItemsSelectedIndex = MainModel.Instance.FileItems.Items.Count - 1;
				}
				MultyFileDocumentsList.SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (e.Key == Key.Enter && MainModel.Instance.FileItems.Items.Count == 0)
			{
				MainModel.Instance.FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox, FilterItems.Text.Trim());

			}
			string filterString = FilterItems.Text.Trim().ToLower();
			FilterNotes(filterString);
		}

		private void FilterNotes(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				MainModel.Instance.FileItems.Clear();

				foreach (var item in MainModel.Instance.AllItems.Items)
				{
					MainModel.Instance.FileItems.Add(item);
				}
			}
			else
			{
				MainModel.Instance.FileItems.Clear();
				var items = MainModel.Instance.AllItems.Items.Where(p => p.Name.ToLower().Contains(filterString));
				foreach (var item in items)
				{
					MainModel.Instance.FileItems.Add(item);
				}
			}
			if (MainModel.Instance.FileItems.Items.Count > 0)
			{
				ItemsSelectedIndex = 0;
				MultyFileDocumentsList.SelectedIndex = ItemsSelectedIndex;
			}
		}

		private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			// Process.Start("notepad.exe", BasePath + CurrentItem.Key + ".meta");
			NoteMetaEditorWindow editorWindow = new NoteMetaEditorWindow();
			editorWindow.LinkData(MainModel.Instance.FileItems.CurrentItem, SelectedResourceFolder);
			editorWindow.Show();
		}

		private void ChannelSelector_KeyUp(object sender, KeyEventArgs e)
		{

		}

		private void ChannelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ChannelSelector.SelectedValue != null)
			{
				// SelectedChannel = ((ComboBoxItem)ChannelSelector.SelectedValue).Content.ToString();
				SelectedChannel = ChannelSelector.SelectedValue.ToString();
			}
			else
			{
				SelectedChannel = ChannelSelector.Text;
			}
			// PreviewItems("notes", "list", SelectedChannel);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			Login();
		}

		private void AccountWindow_Closing(object sender, CancelEventArgs e)
		{
			UserAccount me = ((AccountWindow)sender).Me;
			if (string.IsNullOrEmpty(me.DisplayName))
			{
				Me.DisplayName = "Log In";
				SelectedChannel = null;

				ChannelSelector.Items.Clear();
			}
			else
			{
				Me.DisplayName = me.DisplayName;

				ChannelSelector.Items.Insert(0, Me.RowKey);
				ChannelSelector.SelectedIndex = 0;
				SelectedChannel = ChannelSelector.Text;
			}
			Me.Email = me.Email;
			Me.PartitionKey = me.PartitionKey;
			Me.Password = me.Password;
			Me.PermalinkId = me.PermalinkId;
			Me.RealName = me.RealName;
			Me.RowKey = me.RowKey;
			Me.Username = me.Username;
		}

		private void LinksReader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				SelectText(true);
			}

			if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				SelectText(false);
			}
		}

		public void SelectText(bool isSpecial)
		{
			var start = ResourceContentTextBox.Document.ContentStart;
			var here = ResourceContentTextBox.CaretPosition;
			var range = new TextRange(start, here);
			int indexInText = range.Text.Length;

			string allText = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd).Text;
			allText = allText.Substring(0, (allText.IndexOf("\n", indexInText)));
			int fromIndex = allText.LastIndexOf("\n") + 2;
			int toIndex = allText.Length + 1;
			allText = allText.Substring(allText.LastIndexOf("\n") + 1).Trim();
			TextPointer fromPointer = ResourceContentTextBox.Document.ContentStart.GetPositionAtOffset(fromIndex);
			TextPointer toPointer = ResourceContentTextBox.Document.ContentStart.GetPositionAtOffset(toIndex);
			ResourceContentTextBox.Selection.Select(fromPointer, toPointer);
			TextRange newRange = new TextRange(fromPointer, toPointer);

			if (allText.StartsWith("> "))
			{
				try
				{
					var fileName = string.Empty;
					if (!allText.ToUpper().Contains(" C:"))
					{
						fileName = allText.Substring(2);
					}
					else
					{
						fileName = allText.Substring(20);
					}
					var existentPage = MainModel.Instance.AllItems.Items.Where(p => p.Name.Equals(fileName)).FirstOrDefault();
					if (existentPage != null)
					{
						var visiblePage = MainModel.Instance.FileItems.Items.Where(p => p.Name.Equals(fileName)).FirstOrDefault();
						if (visiblePage == null)
						{
							MainModel.Instance.FileItems.Add(existentPage);
							visiblePage = existentPage;
						}
						MultyFileDocumentsList.SelectedItem = visiblePage;
						Adopt(existentPage);
					}
					else
					{
						Process.Start(allText.Substring(2));
					}
				}
				catch (Exception ex)
				{

				}
			}
		}

		private void Adopt(MultiFileDocument item)
		{
			// throw new NotImplementedException();
		}

		private void LinksReader_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();
		}

		private void ResourceContentTextBox_KeyUp(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.None)
			{
				// CurrentItem.Status = 1;
			}
		}

		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			MultiFileDocument item = (MultiFileDocument)((Image)sender).Tag;
			ShareItem(item);
		}

		private void ShareItem(MultiFileDocument item)
		{
			InputBox inputBox = new InputBox("Send to email:");
			inputBox.Answered += InputBox_Answered;
			inputBox.Show();
		}

		private void InputBox_Answered(InputBox sender, string answer)
		{
			sender.Close();
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{

		}

		private void Window_Closing(object sender, CancelEventArgs e)
		{
			SaveLocalSettings();
		}

		private void SaveLocalSettings()
		{
			LocalSettings localSettings = new LocalSettings();
			localSettings.ProfileName = MainModel.Instance.SelectedProfile.Name;
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				localSettings.LastDocumentName = MainModel.Instance.FileItems.CurrentItem.Name;
			}
			localSettings.LastDocumentNames = MainModel.Instance.LastDocumentNames.ToList();
			localSettings.LastFilterValue = FilterItems.Text.Trim().ToLower();
			File.WriteAllText("LocalSettings.json", JsonConvert.SerializeObject(localSettings));
		}

		private void ShareButton_Click(object sender, RoutedEventArgs e)
		{
			MultiFileDocument item = (MultiFileDocument)((Button)sender).Tag;
			ShareItem(item);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Process.Start(CurrentItem.Name);
		}

		private void RepositorySourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (RepositorySourceComboBox.SelectedItem != null)
			{
				SelectedResourceFolder = (ResourceFolder)RepositorySourceComboBox.SelectedItem;
				ReloadItems(SelectedResourceFolder);
			}
		}
		bool readyForDelete = false;

		private void orderCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			MainModel.Instance.FileItems.OrderByName(OrderCriteriaComboBox.Text);
		}

		private void ItemResourcesButton_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedResourceFolder != null)
			{
				if (Directory.Exists(SelectedResourceFolder.Path + "\\" + MainModel.Instance.FileItems.CurrentItem.Name))
				{
					Process.Start(SelectedResourceFolder.Path + "\\" + MainModel.Instance.FileItems.CurrentItem.Name);
				}
			}
		}

		private void TagsButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void TagEditor_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Tab || e.Key == Key.Enter)
			{
				Label label = new Label();
				label.Content = TagEditor.Text.Trim().ToLower();
				TagsStackPanel.Children.Add(label);
				if(MainModel.Instance.FileItems.CurrentItem.Tags == null)
				{
					MainModel.Instance.FileItems.CurrentItem.Tags = new ObservableCollection<string>();
				}
				MainModel.Instance.FileItems.CurrentItem.Tags.Add(TagEditor.Text.Trim().ToLower());
				TagEditor.Text = string.Empty;
				FileDataAccess.SaveItemMeta(BasePath, MainModel.Instance.FileItems.CurrentItem);
			}

			if (e.Key == Key.Delete || e.Key == Key.Back)
			{
				if (TagEditor.Text == "")
				{
					readyForDelete = !readyForDelete;
					if (readyForDelete)
					{
						if (TagsStackPanel.Children.Count > 0)
						{
							MainModel.Instance.FileItems.CurrentItem.Tags.RemoveAt(MainModel.Instance.FileItems.CurrentItem.Tags.Count - 1);
							TagsStackPanel.Children.RemoveAt(TagsStackPanel.Children.Count - 1);
						}
					}
				}
			}
			else
			{
				readyForDelete = false;
			}
		}

		private void RealTimeCompileCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			CompileInRealTime(RealTimeCompileCheckBox.IsChecked);
		}

		private void CompileInRealTime(bool? isChecked)
		{
			CompileRT = true;
		}

		private void FileEditor_KeyUp(object sender, KeyEventArgs e)
		{

			if (e.Key == Key.Tab || e.Key == Key.Enter)
			{
				Label label = new Label();
				label.Content = FileEditorTextBox.Text.Trim();
				label.Tag = FileEditorTextBox.Text;
				label.MouseUp += DocumenPartLabel_MouseUp;
				FilesStackPanel.Children.Add(label);
				if (MainModel.Instance.FileItems.CurrentItem.Files == null)
				{
					MainModel.Instance.FileItems.CurrentItem.Files = new ObservableCollection<string>();
				}
				MainModel.Instance.FileItems.CurrentItem.Files.Add(FileEditorTextBox.Text.Trim());
				FileEditorTextBox.Text = string.Empty;
				FileDataAccess.SaveItemMeta(BasePath, MainModel.Instance.FileItems.CurrentItem);
			}
		}

		private void SortByNameLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			bool isAsccending = MainModel.Instance.FileItems.OrderByName("Name");
			SortByNameLabel.Foreground = isAsccending ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Magenta;
		}

		private void DocumenPartLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = (string)((Label)sender).Tag;
			if (item != null)
			{
				MultiFileDocument document = GeDocumentByName(item);
				if(document == null)
				{
					// CreateDocument(item);
					MainModel.Instance.FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox, item);
				}
			}
		}

		private void CreateDocument(string name)
		{
			File.Create(BasePath + name);
			var multiFileDocument = new MultiFileDocument()
			{
				/*Tags = noteMeta.Tags,
				Files = noteMeta.Files,
				Synced = noteMeta.Synced,,*/
				Key = Guid.NewGuid().ToString(),
				Name = name,
				Label = name,
				Visibility = VisibilityLevels[0]
			};
			MainModel.Instance.FileItems.Add(multiFileDocument);
		}

		private void DocumenPartLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var item = (string)((Label)sender).Tag;
			if(item!= null)
			{
				MultiFileDocument document = GeDocumentByName(item);
				if(document != null)
				{
					MultyFileDocumentsList.SelectedItem = document;
					Adopt(document);
				}
				else
				{
					if (Keyboard.Modifiers == ModifierKeys.Control)
					{
						MainModel.Instance.FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox, item);
					}
				}
			}
		}

		/// <summary>
		/// Find document by name. If is not visible, add it to the visible list.
		/// </summary>
		/// <param name="documentName"></param>
		/// <returns></returns>
		private MultiFileDocument GeDocumentByName(string documentName)
		{
			MultiFileDocument foundDocument = null;
			var existentPage = MainModel.Instance.AllItems.Items.Where(p => p.Name.Equals(documentName)).FirstOrDefault();
			if (existentPage != null)
			{
				foundDocument = MainModel.Instance.FileItems.Items.Where(p => p.Name.Equals(documentName)).FirstOrDefault();
				if (foundDocument == null)
				{
					MainModel.Instance.FileItems.Add(existentPage);
					foundDocument = existentPage;
				}
			}
			return foundDocument;
		}

		private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
		{
			string helpContent = File.ReadAllText("Resources\\Docs\\index.md");
			var result = Markdown.ToHtml(helpContent);
			PreviewHtml.NavigateToString(ScriptErrorSuppressor + result);
			HtmlTab.IsSelected = true;
		}

		private void MenuItemHelp_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			Process.Start("Resources\\Docs");
		}

		private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (File.Exists(MainModel.Instance.SelectedAffectedFile.Path))
			{
				if (Keyboard.Modifiers == ModifierKeys.Control)
				{
					Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", MainModel.Instance.SelectedAffectedFile.Path);
					return;
				}

				if (MainModel.Instance.SelectedAffectedFile.Path.ToUpper().StartsWith("C:\\PRO\\PHP\\"))
				{
					Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", "http://localhost:86/" + MainModel.Instance.SelectedAffectedFile.Path.Substring(11).Replace("\\", "/"));
				}
				else
				{
					Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", MainModel.Instance.SelectedAffectedFile.Path);
				}
			}
		}

		private void Label_MouseUp(object sender, MouseButtonEventArgs e)
		{
			SelectedAffectedFileTextTab.IsSelected = true;
		}

		private void Browser_OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			var browser = sender as WebBrowser;

			if (browser == null || browser.Document == null)
				return;

			dynamic document = browser.Document;

			if (document.readyState != "complete")
				return;

			dynamic script = document.createElement("script");
			script.type = @"text/javascript";
			script.text = @"window.onerror = function(msg,url,line){return true;}";
			document.head.appendChild(script);
		}

		private void Button_Click_BrowseLog(object sender, RoutedEventArgs e)
		{
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Project + "\\LastLog.html");
			}
			else
			{
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", "LastLog.html");
			}
		}

		private void Button_Click_Compile(object sender, RoutedEventArgs e)
		{
			ExecuteQuery(Key.F5, false);
		}

		private void Button_Click_Project(object sender, RoutedEventArgs e)
		{
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				Process.Start(MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Project);
			}
		}

		private void Button_Click_Template(object sender, RoutedEventArgs e)
		{
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				if (!string.IsNullOrEmpty(MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Template))
				{
					var resource = MainModel.Instance.FileItems.Items.Where(p=>p.Name == MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Template).FirstOrDefault();
					if (resource != null)
					{
						Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + resource.Key);
					}
				}
			}
		}

		private void Button_Click_Data(object sender, RoutedEventArgs e)
		{
			if (MainModel.Instance.FileItems.CurrentItem != null)
			{
				if (!string.IsNullOrEmpty(MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Model))
				{
					var resource = MainModel.Instance.FileItems.Items.Where(p => p.Name == MainModel.Instance.FileItems.CurrentItem.TemplaterInstruction.Model).FirstOrDefault();
					if (resource != null)
					{
						Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + resource.Key);
					}
				}
			}
		}

		private void Button_Click_DB(object sender, RoutedEventArgs e)
		{

		}

		private void Button_Click_API(object sender, RoutedEventArgs e)
		{

		}

		private void Button_Click_WEB(object sender, RoutedEventArgs e)
		{

		}
	}

	public class NoteEntity
	{
		public string RowKey { get; set; }
		public ObservableCollection<string> Tags { get; set; }
		public ObservableCollection<string> Files { get; set; }
		public DateTime Synced { get; set; }
		public string Author { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
	}
}