using Ajuro.Net.Template.Processor;
using Ajuro.WPF.Base.DataAccess;
using Ajuro.WPF.Base.Logic;
using Ajuro.WPF.Base.Model;
using Markdig;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Ajuro.WPF.Base.Views
{
	public class MainWindowLogic
	{
		public string ScriptErrorSuppressor = "<script>window.onerror = function(msg,url,line){return true;}</script>";
		private MultyFileDocumentManager documentManager = new MultyFileDocumentManager();
		public List<LogEntry> LogEntries = new List<LogEntry>();
		//AccountWindow accountWindow = new AccountWindow();
		//NoteMetaEditorWindow editorWindow = new NoteMetaEditorWindow();
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

		public string MarkdownToHtml(string content)
		{
			return Markdown.ToHtml(content);
		}

		public readonly AppViewModel view = new AppViewModel();

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
		// string BasePath = @"C:\\Work\\Resources\\"; // test

		/// <summary>
		/// Last selected note item.
		/// </summary>
		//FileItem SelectedItem { get; set; }

		/// <summary>
		/// Last selected note item.
		/// </summary>
		public ObservableCollection<VisibilityLevel> VisibilityLevels { get; set; }

		/// <summary>
		/// Use a increment to generate unique file names. Check file existence for while available.
		/// </summary>
		static int FileNr = 0;

		public bool CompileRT { get; private set; }


		public void Button_Click_Data()
		{
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				if (!string.IsNullOrEmpty(MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Model))
				{
					var resource = MainModel.Instance.TemplateItems.Items.Where(p => p.Name == MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Model).FirstOrDefault();
					if (resource != null)
					{
						Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + resource.Key);
					}
				}
			}
		}

		public void Button_Click_Template()
		{
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				if (!string.IsNullOrEmpty(MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Template))
				{
					var resource = MainModel.Instance.TemplateItems.Items.Where(p => p.Name == MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Template).FirstOrDefault();
					if (resource != null)
					{
						Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + resource.Key);
					}
				}
			}
		}

		public void Button_Click_Project()
		{
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				Process.Start(MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Project);
			}
		}

		public void EditItem(VersionModel item)
		{
		}

		public void EditItem(ProjectModel item)
		{
		}

		public void EditItem(MultiFileDocument item)
		{
		}

		public void DeleteItem(VersionModel item)
		{
			documentManager.DeleteVersion(item);
			FilterVersions(null);
		}

		public void DeleteItem(ProjectModel item)
		{
			documentManager.DeleteProject(item);
			FilterProjects(MainModel.Instance.ProjectFilter);
		}

		public void DeleteItem(MultiFileDocument item)
		{
			documentManager.DeleteMultyFile(item);
		}

		public void Button_Click_Compile()
		{
			ExecuteQuery(Key.F5, false);
		}

		public void Button_Click_BrowseLog()
		{
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Project + "\\LastLog.html");
			}
			else
			{
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", "LastLog.html");
			}
		}

		public void Browser_OnLoadCompleted(WebBrowser browser)
		{
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

		public void Label_MouseUp()
		{
			MainModel.Instance.SelectedAffectedFileTextTab_IsSelected = true;
		}

		public void Label_MouseDoubleClick()
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

		public void ResourceContentTextBox_PreviewKeyDown()
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				MainModel.InstanckeyShortcuts_Visibility = Visibility.Visible;
				MainModel.Instance.ButtonShortcuts_Visibility = Visibility.Collapsed;
			}
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
							if (numberOfTabs < identationLevel + 2)
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
			//// jsonObject.Remove("Tables");
			//// jsonObject.Add("Tables", jsonPointer);
			jsonPointer.RemoveAt(0);
			jsonObject["Tables"] = jsonPointer;
			string data = jsonObject.ToString();
			return data;
		}

		public void AffectedFilesList_SelectionChanged()
		{
			MainModel.Instance.ProcessTemplateCommand.CanExecuteChanged += ProcessTemplateCommand_CanExecuteChanged;
			if (MainModel.Instance.SelectedAffectedFile == null)
			{
				return;
			}
			if(File.Exists(MainModel.Instance.SelectedAffectedFile.Path))
			{
				MainModel.Instance.RemoteContentDocument = File.ReadAllText(MainModel.Instance.SelectedAffectedFile.Path);
			}
			else
			{
				MainModel.Instance.RemoteContentDocument = string.Empty;
			}
		}

		private void ProcessTemplateCommand_CanExecuteChanged(object sender, EventArgs e)
		{
			// ExecuteQuery(Key.F5, false);
		}

		public void FilesList_PreviewKeyUp(Key key)
		{
			if (key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
			{/*
				MainModel.Instance.TemplateItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);*/
			}
			if (key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
			{
				DuplicateItem("notes");
			}
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				if (key == Key.Delete)
				{
					DeleteItem("notes");
				}
			}
		}

		public void Resource_Name_PreviewKeyUp(Key key)
		{
			ExecuteWindowAction(key);
		}

		public void ResourceContentTextBox_PreviewKeyUp(Key key)
		{
			MainModel.InstanckeyShortcuts_Visibility = Visibility.Collapsed;
			MainModel.Instance.ButtonShortcuts_Visibility = Visibility.Visible;
			ExecuteWindowAction(key);

			if (key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
			{
				ReloadItems("notes", "list", SelectedChannel);
			}
		}

		public void RelatedFiles_SelectionChanged()
		{
			var document = GeDocumentByName(MainModel.Instance.SelectedRelatedFile);
			if (document != null)
			{
				/*if (File.Exists(BasePath + document.Key))
				{
					var content = File.ReadAllText(BasePath + document.Key);
					MainModel.Instance.ResourceContentDocument = content;
				}
				else
				{
					MainModel.Instance.ResourceContentDocument = string.Empty;
				}*/
				MainModel.Instance.TemplateItems.SelectedItem = document;
			}
			else
			{
				// MainModel.Instance.ResourceContentDocument = string.Empty;
			}
		}

		public void Browser_OnLoadCompleted()
		{
			// MainModel.Instance.PreviewHtmlText = "ok";
		}

		public void FilesList_SelectionChanged()
		{
			bool isCompilable = false;/*
			MainModel.Instance.TemplateItems.SelectedItem = ((MultiFileDocument)MultyFileDocumentsList.SelectedItem);*/
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				MultiFileDocument document = null;
				if (MainModel.Instance.TemplateItems.SelectedItem.Name.EndsWith(".rd"))
				{
					document = MainModel.Instance.TemplateItems.SelectedItem;
				}
				else
				if (MainModel.Instance.TemplateItems.SelectedItem.Files != null && MainModel.Instance.TemplateItems.SelectedItem.Files.Count > 0)
				{
					if (MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name.EndsWith(".rd"))
					{
						document = GeDocumentByName(MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name);
					}
					else
					{
						var file = MainModel.Instance.TemplateItems.SelectedItem.Files.Where(p => p.Name.EndsWith(".rd")).FirstOrDefault();
						if (!string.IsNullOrEmpty(file.Name))
						{
							document = GeDocumentByName(file.Name);
						}
					}
				}
				else
				{
				}

				if (document != null)
				{
					isCompilable = true;
					if(isCompilable)
					{
						if (File.Exists(documentManager.GetPathByKey(document.Key + ".snap")))
						{
							MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles = JsonConvert.DeserializeObject<ObservableCollection<AffectedFile>>(File.ReadAllText(documentManager.GetPathByKey(document.Key) + ".snap"));
						}
					}


					MainModel.Instance.Button_BrowseLog_Visibility = Visibility.Visible;
					MainModel.Instance.Button_Compile_Visibility = Visibility.Visible;
					MainModel.Instance.Button_Project_Visibility = Visibility.Visible;
					MainModel.Instance.Button_Template_Visibility = Visibility.Visible;
					MainModel.Instance.Button_Data_Visibility = Visibility.Visible;

					var executionPlan = File.ReadAllText(documentManager.GetPathByKey(document.Key));
					MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction = JsonConvert.DeserializeObject<TemplaterInstruction>(executionPlan.Replace("\\", "\\\\"));
				}
				
				//// MainModel.Instance..AffectedFilesList_ItemsSource = MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles;
				MainModel.Instance.LastDocumentNames.Add(MainModel.Instance.TemplateItems.SelectedItem.Name);
				while (MainModel.Instance.LastDocumentNames.Count > 10)
				{
					MainModel.Instance.LastDocumentNames.RemoveAt(MainModel.Instance.LastDocumentNames.Count - 1);
				}

				//// MainModel.Instance.SelectedItem = MainModel.Instance.TemplateItems.SelectedItem;
				MainModel.Instance.Resource_Name_Text = MainModel.Instance.TemplateItems.SelectedItem.Name;
				FlowDocument ResourceContentDocument = new FlowDocument();
				FlowDocument LinksReaderDocument = new FlowDocument();
				Paragraph ResourceContentParagraph = new Paragraph();
				Paragraph LinksReaderParagraph = new Paragraph();
				/*
				if (!File.Exists(BasePath + MainModel.Instance.TemplateItems.SelectedItem.Key))
				{
					MainModel.Instance.TagsStackPanel_Children.Clear();
					MainModel.Instance.FilesStackPanel_Children.Clear();
				}
				MainModel.Instance.TagsStackPanel_Orientation = Orientation.Horizontal;

				MainModel.Instance.FilesStackPanel_Children.Clear();
				if (MainModel.Instance.TemplateItems.SelectedItem.Files != null)
				{
					MainModel.Instance.FilesContainer_Visibility = Visibility.Visible;
					foreach (string file in MainModel.Instance.TemplateItems.SelectedItem.Files)
					{
						Label label = new Label();
						label.BorderBrush = Brushes.Gray;
						label.Tag = file.Substring(file.LastIndexOf("\\") + 1);
						label.MouseUp += DocumenPartLabel_MouseUp;
						label.MouseDoubleClick += DocumenPartLabel_MouseDoubleClick;
						label.BorderThickness = new Thickness(2);
						label.Content = file.Substring(file.LastIndexOf("\\") + 1);
						MainModel.Instance.FilesStackPanel_Children.Add(label);
					}
				}

				MainModel.Instance.TagsStackPanel_Children.Clear();
				if (MainModel.Instance.TemplateItems.SelectedItem.Tags != null)
				{
					foreach (string tag in MainModel.Instance.TemplateItems.SelectedItem.Tags)
					{
						Label label = new Label();
						label.Content = tag;
						MainModel.Instance.TagsStackPanel_Children.Add(label);
					}
				}
				*/
				var filePath = documentManager.GetPathByKey(MainModel.Instance.TemplateItems.SelectedItem.Key);
				if (!File.Exists(filePath))
				{
					MainModel.Instance.TemplateItems.SelectedItem.Label = "!";
					MainModel.Instance.ResourceContentDocument = string.Empty;
					MainModel.Instance.DataTabControl_SelectedIndex = 1;

					if (MainModel.Instance.TemplateItems.SelectedItem.Files != null && File.Exists(MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name))
					{
						string resourceContent = File.ReadAllText(MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name);
						LinksReaderParagraph.Inlines.Add(new Run(resourceContent));
						LinksReaderDocument.Blocks.Add(LinksReaderParagraph);
						MainModel.Instance.LinksReader_Document = LinksReaderDocument;
					}
					return;
				}
				string content = File.ReadAllText(documentManager.GetPathByKey(MainModel.Instance.TemplateItems.SelectedItem.Key)); ;
				
				// ResourceContentParagraph.Inlines.Add(new Run(content));
				LinksReaderParagraph.Inlines.Add(new Run(content));

				// ResourceContentDocument.Blocks.Add(ResourceContentParagraph);
				MainModel.Instance.ResourceContentDocument = content;
				if (MainModel.Instance.TemplateItems.SelectedItem.Name.ToLower().EndsWith(".rd"))
				{
					FilterVersions(string.Empty);
					FilterProjects(string.Empty);
					var docFile = GeDocumentByName(MainModel.Instance.TemplateItems.SelectedItem.Name.ToLower().Replace(".rd", ".md"));

					var docFilePath = documentManager.GetPathByKey(docFile.Key);
					if (docFile != null && File.Exists(docFilePath))
					{
						var docContent = File.ReadAllText(docFilePath);
						MainModel.Instance.PreviewHtmlText = docContent;
					}
					else
					{
						MainModel.Instance.PreviewHtmlText = "-";
					}
				}
				else
				{
					MainModel.Instance.PreviewHtmlText = "-";
				}

				LinksReaderDocument.Blocks.Add(LinksReaderParagraph);
				MainModel.Instance.LinksReader_Document = LinksReaderDocument;
			}
			if (!isCompilable)
			{

				MainModel.Instance.Button_BrowseLog_Visibility = Visibility.Collapsed;
				MainModel.Instance.Button_Compile_Visibility = Visibility.Collapsed;
				MainModel.Instance.Button_Project_Visibility = Visibility.Collapsed;
				MainModel.Instance.Button_Template_Visibility = Visibility.Collapsed;
				MainModel.Instance.Button_Data_Visibility = Visibility.Collapsed;
			}
		}

		private JArray FindVariable(JObject jsonObject, string v)
		{
			JArray result = null;
			if (jsonObject.ContainsKey(v))
			{
				result = (JArray)jsonObject[v];
			}
			return result;
		}

		/// <summary>
		/// Delete existent note
		/// </summary>
		private void DeleteItem(string entity)
		{
			var response = MessageBox.Show("Delete file [" + MainModel.Instance.TemplateItems.SelectedItem.Name + "] ?", "Question", MessageBoxButton.YesNo);
			if (response == MessageBoxResult.Yes)
			{
				File.Delete(documentManager.GetPathByKey(MainModel.Instance.TemplateItems.SelectedItem.Name));
				MainModel.Instance.TemplateItems.Remove(MainModel.Instance.TemplateItems.SelectedItem);
				MainModel.Instance.AllTemplateItems.Remove(MainModel.Instance.TemplateItems.SelectedItem);
			}
		}


		private void ReloadItems(ResourceFolder rootFolder)
		{
			var rawItemsList = documentManager.GetItems(rootFolder.Path);
			bool hasMetadata = rootFolder.Type == "notes";

			MainModel.Instance.AllTemplateItems.Clear();
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
						var meta = JsonConvert.DeserializeObject<NoteEntity>(File.ReadAllText(documentManager.GetPathByKey(item.Name)));
						MainModel.Instance.AllTemplateItems.Add(new MultiFileDocument()
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
					ObservableCollection<VersionedFile> files = new ObservableCollection<VersionedFile>();
					if (File.Exists(item.Path))
					{
						files.Add(new VersionedFile() { Name = item.Path });
					}
					else
					{
						// Is a folder
						//// files.AddRange(Directory.GetFiles(item.Path));
					}

					MainModel.Instance.AllTemplateItems.Add(new MultiFileDocument()
					{
						Name = item.Name,
						Synced = DateTime.Now,
						Label = string.Empty,
						Files = files
					});
				}
			}
			FilterTemplates(string.Empty);
			MainModel.Instance.StatusBartextBlock_Text = "Found " + MainModel.Instance.TemplateItems.Items.Count + " / " + MainModel.Instance.AllTemplateItems.Items.Count + " items;";
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
					if (!File.Exists(documentManager.GetPathByKey(item.RowKey)))
					{
						//// File.WriteAllText(BasePath + item.Title, item.Content);
						File.WriteAllText(documentManager.GetPathByKey(item.RowKey), item.Content);
						item.Content = string.Empty;
						File.WriteAllText(documentManager.GetPathByKey(item.RowKey + ".meta"), JsonConvert.SerializeObject(item));
						MainModel.Instance.AllTemplateItems.Add(new MultiFileDocument()
						{
							Name = item.RowKey,
							Synced = DateTime.Now,
							Label = string.Empty
						});

						added++;
					}
				}
			}
			FilterTemplates(string.Empty);
			MainModel.Instance.StatusBartextBlock_Text = "Found " + added + " new items from " + found;
		}

		private void PreviewItems(string entity, string action, string channel)
		{
			if (MainModel.Instance.AllTemplateItems == null)
			{
				return;
			}
			MainModel.Instance.AllTemplateItems.Clear();
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
					if (!File.Exists(documentManager.GetPathByKey(item.RowKey)))
					{
						//// File.WriteAllText(BasePath + item.Title, item.Content);
						File.WriteAllText(documentManager.GetPathByKey(item.RowKey), item.Content);

						added++;
					}
					item.Content = string.Empty;
					//// File.WriteAllText(BasePath + item.RowKey + ".meta", JsonConvert.SerializeObject(item));
					MainModel.Instance.AllTemplateItems.Add(new MultiFileDocument()
					{
						Author = item.Author,
						Name = item.Title,
						Key = item.RowKey,
						Synced = DateTime.Now,
						Label = string.Empty
					});
				}
			}
			FilterTemplates(string.Empty);
			MainModel.Instance.StatusBartextBlock_Text = "Found " + added + " new items from " + found;
		}

		public void HelpLink_MouseUp()
		{
			System.Diagnostics.Process.Start("http://otb.expert/Ajuro.Notes/mail_template_invitation.html");
		}

		public void TemplatesFilterKeyUp(Key key)
		{
			if (key == Key.Down)
			{
				if(MainModel.Instance.TemplateItems.SelectedItem == null)
				{
					return;
				}
				ItemsSelectedIndex += 1;
				if (ItemsSelectedIndex >= MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items.Count)
				{
					ItemsSelectedIndex = 0;
				}
				MainModel.Instance.MultyFileDocumentsList_SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (key == Key.Up)
			{
				ItemsSelectedIndex -= 1;
				if (ItemsSelectedIndex < 0)
				{
					ItemsSelectedIndex = MainModel.Instance.TemplateItems.Items.Count - 1;
				}
				MainModel.Instance.MultyFileDocumentsList_SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (key == Key.Enter && MainModel.Instance.TemplateItems.Items.Count == 0)
			{
				documentManager.Create(MainModel.Instance.TemplateFilter);
			}

			FilterTemplates(MainModel.Instance.TemplateFilter);
		}

		public void FilterVersionItems_KeyUp(Key key)
		{
			if (key == Key.Down)
			{
				ItemsSelectedIndex += 1;
				if (ItemsSelectedIndex >= MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Items.Count)
				{
					ItemsSelectedIndex = 0;
				}
				MainModel.Instance.MultyFileDocumentsList_SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (key == Key.Up)
			{
				ItemsSelectedIndex -= 1;
				if (ItemsSelectedIndex < 0)
				{
					ItemsSelectedIndex = MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Items.Count - 1;
				}
				MainModel.Instance.MultyFileDocumentsList_SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (key == Key.Delete && MainModel.Instance.TemplateItems.SelectedItem.VersionItems.SelectedItem != null)
			{
				MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Remove(MainModel.Instance.TemplateItems.SelectedItem.VersionItems.SelectedItem);
			}
			if (key == Key.Enter && MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Items.Count == 0)
			{
				documentManager.CreateVersion(MainModel.Instance.VersionFilter);
				MainModel.Instance.ResourceContentDocument = JsonConvert.SerializeObject(MainModel.Instance.TemplateItems.SelectedItem);

				documentManager.SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
			}

			FilterVersions(MainModel.Instance.VersionFilter);
		}

		public void FilterProjectItems_KeyUp(Key key)
		{
			if (key == Key.Down)
			{
				ItemsSelectedIndex += 1;
				if (ItemsSelectedIndex >= MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Items.Count)
				{
					ItemsSelectedIndex = 0;
				}
				MainModel.Instance.MultyFileDocumentsList_SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (key == Key.Up)
			{
				ItemsSelectedIndex -= 1;
				if (ItemsSelectedIndex < 0)
				{
					ItemsSelectedIndex = MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Items.Count - 1;
				}
				MainModel.Instance.MultyFileDocumentsList_SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (key == Key.Enter && MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Items.Count == 0)
			{
				documentManager.CreateFile(MainModel.Instance.ProjectFilter);
				MainModel.Instance.ResourceContentDocument = JsonConvert.SerializeObject(MainModel.Instance.TemplateItems.SelectedItem);
				documentManager.SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
			}

			FilterProjects(MainModel.Instance.ProjectFilter);
		}

		private void UploadItem(string entity)
		{
			if (string.IsNullOrEmpty(Me.RowKey))
			{
				Login();
			}

			var httpWebRequest = (HttpWebRequest)WebRequest.Create(MainModel.UrlBase + entity + "/" + (MainModel.Instance.TemplateItems.SelectedItem.Synced == DateTime.MinValue ? "insert" : "update/" + MainModel.Instance.TemplateItems.SelectedItem.Key));

			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{/*
				TextRange range = new TextRange(MainModel.Instance.ResourceContentTextBox_Document.ContentStart, MainModel.Instance.ResourceContentTextBox_Document.ContentEnd);
				NoteEntity item = new NoteEntity()
				{
					Author = Me.RowKey,
					RowKey = MainModel.Instance.TemplateItems.SelectedItem.Key,
					Content = range.Text,
					Title = MainModel.Instance.TemplateItems.SelectedItem.Name
				};

				streamWriter.Write(JsonConvert.SerializeObject(item));*/
				streamWriter.Flush();
				streamWriter.Close();

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					MainModel.Instance.TemplateItems.SelectedItem.Synced = DateTime.Now;
					MainModel.Instance.TemplateItems.SelectedItem.Author = Me.RowKey;
					documentManager.SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
					MainModel.Instance.StatusBartextBlock_Text = DateTime.Now.ToShortTimeString() + ": Uploaded [" + MainModel.Instance.TemplateItems.SelectedItem.Name + "]";
					MainModel.Instance.TemplateItems.SelectedItem.Status = 2;
				}
			}
		}

		public void FilesList_MouseDoubleClick()
		{
			//// Process.Start("notepad.exe", BasePath + SelectedItem.Key + ".meta");
			/*
			editorWindow.LinkData(MainModel.Instance.TemplateItems.SelectedItem, SelectedResourceFolder);
			editorWindow.Show();*/
		}

		public void ChannelSelector_SelectionChanged()
		{
			if (MainModel.Instance.ChannelSelector_SelectedValue != null)
			{
				//// SelectedChannel = ((ComboBoxItem)ChannelSelector.SelectedValue).Content.ToString();
				SelectedChannel = MainModel.Instance.ChannelSelector_SelectedValue.ToString();
			}
			else
			{
				SelectedChannel = MainModel.Instance.ChannelSelector_Text;
			}
		}

		public void MenuItem_Click()
		{
			Login();
		}

		public void AccountWindow_Closing(UserAccount me)
		{
			if (string.IsNullOrEmpty(me.DisplayName))
			{
				Me.DisplayName = "Log In";
				SelectedChannel = null;
				/*
				MainModel.Instance.ChannelSelector_Items.Clear();*/
			}
			else
			{
				Me.DisplayName = me.DisplayName;
				/*
				MainModel.Instance.ChannelSelector_Items.Insert(0, Me.RowKey);*/
				MainModel.Instance.ChannelSelector_SelectedIndex = 0;
				SelectedChannel = MainModel.Instance.ChannelSelector_Text;
			}
			Me.Email = me.Email;
			Me.PartitionKey = me.PartitionKey;
			Me.Password = me.Password;
			Me.PermalinkId = me.PermalinkId;
			Me.RealName = me.RealName;
			Me.RowKey = me.RowKey;
			Me.Username = me.Username;
		}

		private void Login()
		{/*
			accountWindow.WireUpData(Me);
			accountWindow.Closing += AccountWindow_Closing;
			accountWindow.Show();*/
		}

		/// <summary>
		/// Creating a duplicate is actually saving the work in progress as a new note.
		/// </summary>
		private void DuplicateItem(string entity)
		{
			MultiFileDocument NewSelectedItem = new MultiFileDocument()
			{
				Name = MainModel.Instance.TemplateItems.SelectedItem.Name,
				Label = ""
			};
			MainModel.Instance.TemplateItems.Add(NewSelectedItem);
			/*
			TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
			File.WriteAllText(BasePath + MainModel.Instance.TemplateItems.SelectedItem.Name, range.Text);
			MainModel.Instance.TemplateItems.SelectedItem.Label = "";*/
		}

		public void LinksReader_MouseDoubleClick()
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

		public void MenuItemSettings_Click()
		{
			/*SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();*/
		}

		/// <summary>
		/// Work in progress gets saved in the same file if the note name has not changes. If the name changed, a new file is created and the old file is deleted.
		/// </summary>

		public void Window_Closing()
		{
			SaveLocalSettings();
		}

		public void ShareButton_Click(MultiFileDocument item)
		{
			ShareItem(item);
		}

		public void RepositorySourceComboBox_SelectionChanged()
		{/*
			if (RepositorySourceComboBox.SelectedItem != null)
			{
				SelectedResourceFolder = (ResourceFolder)RepositorySourceComboBox.SelectedItem;
				ReloadItems(SelectedResourceFolder);
			}*/
		}

		private void FilterTemplates(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				MainModel.Instance.TemplateItems.Clear();

				foreach (var item in MainModel.Instance.AllTemplateItems.Items)
				{
					MainModel.Instance.TemplateItems.Add(item);
				}
			}
			else
			{
				filterString = filterString.Trim().ToLower();
				MainModel.Instance.TemplateItems.Clear();
				var items = MainModel.Instance.AllTemplateItems.Items.Where(p => p.Name.ToLower().Contains(filterString));
				foreach (var item in items)
				{
					MainModel.Instance.TemplateItems.Add(item);
				}
			}
			if (MainModel.Instance.TemplateItems.Items.Count > 0)
			{
				ItemsSelectedIndex = 0;/*
				MultyFileDocumentsList.SelectedIndex = ItemsSelectedIndex;*/
			}
		}

		private void FilterVersions(string filterString)
		{
			if (MainModel.Instance.TemplateItems.SelectedItem == null)
			{
				return;
			}
			if(MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items == null)
			{
				MainModel.Instance.TemplateItems.SelectedItem.Versions = new ObservableCollection<VersionModel>();
				var collectionItems = new ObservableCollection<BaseModel>();
				foreach (var collectionItem in MainModel.Instance.TemplateItems.SelectedItem.Versions)
				{
					collectionItems.Add((BaseModel)collectionItem);
				}
				MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items = collectionItems;
			}
			if (string.IsNullOrEmpty(filterString))
			{
				MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Clear();

				foreach (var item in MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items)
				{
					MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Add(item);
				}
			}
			else
			{
				filterString = filterString.Trim().ToLower();
				if(MainModel.Instance.TemplateItems.SelectedItem.VersionItems == null)
				{
					MainModel.Instance.TemplateItems.SelectedItem.VersionItems = new VersionList(new ObservableCollection<VersionModel>());
				}
				MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Clear();
				var items = MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items.Where(p => p.Name.ToLower().Contains(filterString));
				foreach (var item in items)
				{
					MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Add(item);
				}
			}
			if (MainModel.Instance.TemplateItems.SelectedItem.VersionItems.Items.Count > 0)
			{
				ItemsSelectedIndex = 0;/*
				MultyFileDocumentsList.SelectedIndex = ItemsSelectedIndex;*/
			}
		}

		private void FilterProjects(string filterString)
		{
			if (MainModel.Instance.TemplateItems.SelectedItem.AllProjectItems.Items == null)
			{
				MainModel.Instance.TemplateItems.SelectedItem.Projects = new ObservableCollection<ProjectModel>();
				var collectionItems = new ObservableCollection<BaseModel>();
				foreach (var collectionItem in MainModel.Instance.TemplateItems.SelectedItem.Projects)
				{
					collectionItems.Add((BaseModel)collectionItem);
				}
				MainModel.Instance.TemplateItems.SelectedItem.AllProjectItems.Items = collectionItems;
			}
			if (string.IsNullOrEmpty(filterString))
			{
				MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Clear();

				foreach (var item in MainModel.Instance.TemplateItems.SelectedItem.AllProjectItems.Items)
				{
					MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Add(item);
				}
			}
			else
			{
				filterString = filterString.Trim().ToLower();
				if (MainModel.Instance.TemplateItems.SelectedItem.ProjectItems == null)
				{
					MainModel.Instance.TemplateItems.SelectedItem.ProjectItems = new ProjectList(new ObservableCollection<ProjectModel>());
				}
				MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Clear();
				var items = MainModel.Instance.TemplateItems.SelectedItem.AllProjectItems.Items.Where(p => p.Name.ToLower().Contains(filterString));
				foreach (var item in items)
				{
					MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Add(item);
				}
			}
			if (MainModel.Instance.TemplateItems.SelectedItem.ProjectItems.Items.Count > 0)
			{
				ItemsSelectedIndex = 0;/*
				MultyFileDocumentsList.SelectedIndex = ItemsSelectedIndex;*/
			}
		}

		public void ItemResourcesButton_Click()
		{
			if (SelectedResourceFolder != null)
			{
				if (Directory.Exists(SelectedResourceFolder.Path + "\\" + MainModel.Instance.TemplateItems.SelectedItem.Name))
				{
					Process.Start(SelectedResourceFolder.Path + "\\" + MainModel.Instance.TemplateItems.SelectedItem.Name);
				}
			}
		}

		public void TagEditor_KeyUp(Key key)
		{/*
			if (key == Key.Tab || key == Key.Enter)
			{
				Label label = new Label();
				label.Content = TagEditor.Text.Trim().ToLower();
				TagsStackPanel.Children.Add(label);
				if (MainModel.Instance.TemplateItems.SelectedItem.Tags == null)
				{
					MainModel.Instance.TemplateItems.SelectedItem.Tags = new ObservableCollection<string>();
				}
				MainModel.Instance.TemplateItems.SelectedItem.Tags.Add(TagEditor.Text.Trim().ToLower());
				TagEditor.Text = string.Empty;
				FileDataAccess.SaveItemMeta(BasePath, MainModel.Instance.TemplateItems.SelectedItem);
			}

			if (key == Key.Delete || key == Key.Back)
			{
				if (TagEditor.Text == "")
				{
					readyForDelete = !readyForDelete;
					if (readyForDelete)
					{
						if (TagsStackPanel.Children.Count > 0)
						{
							MainModel.Instance.TemplateItems.SelectedItem.Tags.RemoveAt(MainModel.Instance.TemplateItems.SelectedItem.Tags.Count - 1);
							TagsStackPanel.Children.RemoveAt(TagsStackPanel.Children.Count - 1);
						}
					}
				}
			}
			else
			{
				readyForDelete = false;
			}*/
		}

		public void SelectText(bool isSpecial)
		{/*
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
						var visiblePage = MainModel.Instance.TemplateItems.Items.Where(p => p.Name.Equals(fileName)).FirstOrDefault();
						if (visiblePage == null)
						{
							MainModel.Instance.TemplateItems.Add(existentPage);
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
			*/
		}

		public void FileEditor_KeyUp(Key key)
		{
			if (key == Key.Tab || key == Key.Enter)
			{
				Label label = new Label();
				/*label.Content = FileEditorTextBox.Text.Trim();
				label.Tag = FileEditorTextBox.Text;
				label.MouseUp += DocumenPartLabel_MouseUp;
				FilesStackPanel.Children.Add(label);
				*/
				if (MainModel.Instance.TemplateItems.SelectedItem.Files == null)
				{
					MainModel.Instance.TemplateItems.SelectedItem.Files = new ObservableCollection<VersionedFile>();
				}
				MainModel.Instance.TemplateItems.SelectedItem.Files.Add(new VersionedFile() { Name = MainModel.Instance.RelatedFileFilter.Trim() });
				MainModel.Instance.RelatedFileFilter = string.Empty;
				documentManager.SaveItemMeta(MainModel.Instance.TemplateItems.SelectedItem);
			}
		}

		public void SortByNameLabel_MouseUp()
		{
			bool isAsccending = MainModel.Instance.TemplateItems.OrderByName("Name");/*
			SortByNameLabel.Foreground = isAsccending ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Magenta;*/
		}

		private void ShareItem(MultiFileDocument item)
		{/*
			InputBox inputBox = new InputBox("Send to email:");
			inputBox.Answered += InputBox_Answered;
			inputBox.Show();*/
		}

		private void RealTimeCompileCheckBox_Checked(object sender, RoutedEventArgs e)
		{/*
			CompileInRealTime(RealTimeCompileCheckBox.IsChecked);*/
		}

		private void CompileInRealTime(bool? isChecked)
		{
			CompileRT = true;
		}

		bool readyForDelete = false;


		private void ExecuteWindowAction(Key key)
		{
			if (key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
			{
				documentManager.Create();
			}
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				bool consumed = false;
				if (key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					documentManager.SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
				}
				if (key == Key.F6 && Keyboard.Modifiers == ModifierKeys.Control)
				{
					if (MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name.EndsWith(".rd"))
					{
						MultiFileDocument document = GeDocumentByName(MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name);
						/*FragmentSelector templateEditorWindow = new FragmentSelector();
						templateEditorWindow.BasePath = BasePath;
						templateEditorWindow.LoadTemplateFromPath(File.ReadAllText(BasePath + document.Key));
						templateEditorWindow.Show();*/
					}
					else if (true)
					{

					}
				}
				if (key == Key.F5 || CompileRT)
				{
					consumed = true;
					ExecuteQuery(key, Keyboard.Modifiers == ModifierKeys.Control);
				}
				if (key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					MainModel.Instance.TemplateItems.NewItem(documentManager.BasePath, ref FileNr);
				}
				if (key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					DuplicateItem("notes");
				}
				if (key == Key.U && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					UploadItem("notes");
				}
				if (!consumed)
				{
					if (key != Key.PageUp && key != Key.PageDown && key != Key.Home && key != Key.End && key != Key.Tab && key != Key.Right && key != Key.Left && key != Key.Up && key != Key.Down && key != Key.System && key != Key.LeftCtrl && key != Key.LeftAlt && key != Key.LeftShift && key != Key.RightCtrl && key != Key.RightAlt && key != Key.RightShift)
					{
						MainModel.Instance.TemplateItems.SelectedItem.Name = MainModel.Instance.Resource_Name_Text;
						MainModel.Instance.TemplateItems.SelectedItem.Status = 1;
					}
				}
			}
		}

		private void SaveLocalSettings()
		{
			LocalSettings localSettings = new LocalSettings();
			localSettings.ProfileName = MainModel.Instance.SelectedProfile.Name;
			if (MainModel.Instance.TemplateItems.SelectedItem != null)
			{
				localSettings.LastDocumentName = MainModel.Instance.TemplateItems.SelectedItem.Name;
			}
			localSettings.LastDocumentNames = MainModel.Instance.LastDocumentNames.ToList();/*
			localSettings.LastFilterValue = FilterItems.Text.Trim().ToLower();*/
			File.WriteAllText("LocalSettings.json", JsonConvert.SerializeObject(localSettings));
		}

		/// <summary>
		/// Find document by name. If is not visible, add it to the visible list.
		/// </summary>
		/// <param name="documentName"></param>
		/// <returns></returns>
		private MultiFileDocument GeDocumentByName(string documentName)
		{
			MultiFileDocument foundDocument = null;
			var existentPage = MainModel.Instance.AllTemplateItems.Items.Where(p => p.Name.Equals(documentName)).FirstOrDefault();
			if (existentPage != null)
			{
				foundDocument = MainModel.Instance.TemplateItems.Items.Where(p => p.Name.Equals(documentName)).FirstOrDefault();
				if (foundDocument == null)
				{
					MainModel.Instance.TemplateItems.Add(existentPage);
					foundDocument = existentPage;
				}
			}
			return foundDocument;
		}


		private void CreateDocument(string name)
		{
			File.Create(documentManager.GetPathByKey(name));
			var multiFileDocument = new MultiFileDocument()
			{
				Key = Guid.NewGuid().ToString(),
				Name = name,
				Label = name,
				Visibility = VisibilityLevels[0]
			};
			MainModel.Instance.TemplateItems.Add(multiFileDocument);
		}

		public void MenuItemHelp_DoubleClick()
		{
			Process.Start("Resources\\Docs");
		}

		public void MenuItemHelp_Click()
		{
			string helpContent = File.ReadAllText("Resources\\Docs\\index.md");
			var result = Markdown.ToHtml(helpContent);
			MainModel.Instance.PreviewHtmlText = ScriptErrorSuppressor + result;
			MainModel.Instance.HtmlTab_IsSelected = true;
		}

		public void DocumenPartLabel_MouseUp(string item)
		{
			if (item != null)
			{
				MultiFileDocument document = GeDocumentByName(item);
				if (document != null)
				{
					/*
					MultyFileDocumentsList.SelectedItem = document;
					Adopt(document);
					*/
				}
				else
				{
					if (Keyboard.Modifiers == ModifierKeys.Control)
					{/*
						MainModel.Instance.TemplateItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox, item);*/
					}
				}
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
		/// Add a new note
		/// </summary>
		private void NewItem(string itemName = null, string content = null)
		{
			if (string.IsNullOrEmpty(itemName))
			{
				while (File.Exists(documentManager.BasePath + "NewItem_" + FileNr + ".txt"))
				{
					FileNr++;
				}
				itemName = "NewItem_" + FileNr + ".txt"; // Microsoft.VisualBasic.Interaction.InputBox("Question?", "Title", "Default Text");
			}
			FileNr++;
			//// if (response == MessageBoxResult.Yes)
			{
				MainModel.Instance.TemplateItems.SelectedItem = new MultiFileDocument()
				{
					Name = itemName,
					Key = Guid.NewGuid().ToString(),
					Label = "!!"
				};
				MainModel.Instance.TemplateItems.Add(MainModel.Instance.TemplateItems.SelectedItem);
				MainModel.Instance.AllTemplateItems.Add(MainModel.Instance.TemplateItems.SelectedItem);
				/*
				ResourceContentTextBox.Document = new FlowDocument();
				*/
			}
		}

		public string ExecuteSQL(string connectionString, string queryString)
		{
			/*
			DataAccess dataAccess = new DataAccess();
			string result = dataAccess.ExecuteQuery(connectionString, queryString);
			return result;
			*/
			return null;
		}

		public string EditConfigurationJson(KnownCommands str)
		{
			switch (str)
			{
				case KnownCommands.EditTemplate:
					EditConfigurationJson("C:\\OTB\\templates\\profile.json", "Resources\\meta_profile.json");
					break;
				case KnownCommands.EditConfiguration:
					if (Ajuro.WPF.Base.Model.MainModel.Instance.TemplateItems.SelectedItem == null)
					{
						return null;
					}
					EditConfigurationJson(documentManager.GetPathByKey(Ajuro.WPF.Base.Model.MainModel.Instance.TemplateItems.SelectedItem.Key), "Resources\\meta_config.json");
					break;
			}
			return string.Empty;
		}

		private void EditConfigurationJson(string dataJsonPath, string metaJsonPath)
		{
			JObject SampleJson = null;

			// InitializeComponent();
			var MainViewModel = WizardModel.Instance;
			WizardModel.DataJsonPath = dataJsonPath;
			WizardModel.MetaJsonPath = metaJsonPath;
			if (!File.Exists(WizardModel.DataJsonPath))
			{
				var file = File.Create(WizardModel.DataJsonPath);
				file.Close();
				File.WriteAllText(WizardModel.DataJsonPath, "{}");
			}
			if (!File.Exists(WizardModel.MetaJsonPath))
			{
				var file = File.Create(WizardModel.MetaJsonPath);
				file.Close();
				File.WriteAllText(WizardModel.MetaJsonPath, "{}");
			}


			if (File.Exists(WizardModel.DataJsonPath))
			{
				MainViewModel.SampleJson = File.ReadAllText(WizardModel.DataJsonPath);
				SampleJson = JObject.Parse(MainViewModel.SampleJson);
				MainViewModel.SampleTree = UniversalTreeNode.CreateTree(SampleJson);
				MainViewModel.SampleJson = JsonConvert.SerializeObject(SampleJson, Formatting.Indented);
				SampleJson = JObject.Parse(MainViewModel.SampleJson);
			}

			if (!File.Exists(WizardModel.MetaJsonPath))
			{
				var stream = File.Create(WizardModel.MetaJsonPath);
				stream.Close();
				File.WriteAllText(WizardModel.MetaJsonPath, "{}");
			}

			if (File.Exists(WizardModel.MetaJsonPath))
			{
				MainViewModel.MetaJson = File.ReadAllText(WizardModel.MetaJsonPath);
				var metaJson = JObject.Parse(MainViewModel.MetaJson);
				MainViewModel.MetaJson = JsonConvert.SerializeObject(metaJson, Formatting.Indented);
			}

			var steps = Newtonsoft.Json.JsonConvert.DeserializeObject<WizardStep>(MainViewModel.MetaJson).Children;
			if (steps == null)
			{
				steps = new ObservableCollection<WizardStep>();
			}
			WizardModel.Instance.WizardSteps.Clear();
			foreach (var step in steps)
			{
				WizardModel.Instance.WizardSteps.Add(step);
			}
		}


		public void ExecuteQuery(Key key, bool openWindow)
		{
			Base.DataAccess.DataAccess dataAccess = new Base.DataAccess.DataAccess();
			string queryString = MainModel.Instance.ResourceContentDocument;// new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd).Text;
			var property = MainModel.Instance.SelectedProfile.Properties.Where(p => p.Key == "cs").FirstOrDefault();
			string connectionString = property.Value == null ? string.Empty : property.Value.ToString();


			// Edit in external editor!
			if (Keyboard.Modifiers == ModifierKeys.Control && key == Key.F5)
			{
				Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + MainModel.Instance.TemplateItems.SelectedItem.Key);
				return;
			}
			else if (queryString.StartsWith("-- MD") || MainModel.Instance.TemplateItems.SelectedItem.Name.EndsWith(".md"))
			{
				var result = Markdown.ToHtml(queryString);
				//// PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<div>You can not execute a MD file. Change to another file if you need to execute.</div>" + result);
				return;
			}

			if (MainModel.Instance.TemplateItems.SelectedItem.Files != null && MainModel.Instance.TemplateItems.SelectedItem.Files.Count > 0 && MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name.EndsWith(".rd"))
			{
				documentManager.SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
				MultiFileDocument document = GeDocumentByName(MainModel.Instance.TemplateItems.SelectedItem.Files[0].Name);
				if (document != null)
				{
					ExecuteTemplate(File.ReadAllText(documentManager.GetPathByKey(document.Key)));
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
				/*PreviewHtml.NavigateToString(ScriptErrorSuppressor + view);
				if (string.IsNullOrEmpty(MainModel.Instance.JsonRepresentation))
				{
					PreviewJson.NavigateToString("Parse Error");
				}
				else
				{
					PreviewJson.NavigateToString(MainModel.Instance.JsonRepresentation);
				}
				PreviewHtml.NavigateToString(ScriptErrorSuppressor + result);*/
			}

			if (queryString.StartsWith("-- HTML") || MainModel.Instance.TemplateItems.SelectedItem.Name.EndsWith(".html"))
			{
				File.WriteAllText("Results.html", queryString);
				if (!CompileRT)
				{
					//// PreviewHtml.NavigateToString(ScriptErrorSuppressor + queryString);
				}
			}

			if (queryString.StartsWith("-- JSON") || MainModel.Instance.TemplateItems.SelectedItem.Name.EndsWith(".json"))
			{
				var result = Markdown.ToHtml(queryString);
				//// PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<pre>" + result + "</pre>");
			}
			if (Keyboard.Modifiers == ModifierKeys.Shift && key == Key.F5)
			{
				File.WriteAllText("Results.html", File.ReadAllText("C:\\Work\\Resources\\" + MainModel.Instance.TemplateItems.SelectedItem.Key));
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", "Results.html");
			}

			if (queryString.StartsWith("-- RD") || MainModel.Instance.TemplateItems.SelectedItem.Name.EndsWith(".rd"))
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
				var itemCSV = MainModel.Instance.AllTemplateItems.Items.Where(p => p.Name.Equals(templaterInstruction.CSV)).FirstOrDefault();
				string variablesCSV = File.ReadAllText(documentManager.GetPathByKey(itemCSV.Key));
				var itemVar = MainModel.Instance.AllTemplateItems.Items.Where(p => p.Name.Equals(templaterInstruction.Model)).FirstOrDefault();
				variables = File.ReadAllText(documentManager.GetPathByKey(itemVar.Key));
				variables = RecreateVariables(variables, variablesCSV);
			}
			else
			{
				var itemVar = MainModel.Instance.AllTemplateItems.Items.Where(p => p.Name.Equals(templaterInstruction.Model)).FirstOrDefault();
				if (itemVar == null)
				{
					return;
				}
				variables = File.ReadAllText(documentManager.GetPathByKey(itemVar.Key));
			}

			if (!File.Exists(templaterInstruction.Project + "\\data.json"))
			{
				var streamReader = File.Create(templaterInstruction.Project + "\\data.json");
				streamReader.Close();
				Log(new LogEntry() { Message = "Saving DATA json.", Name = "data.json", Path = templaterInstruction.Project + "\\data.json", Size = variables.Length });
			}
			File.WriteAllText(templaterInstruction.Project + "\\data.json", variables);

			var itemTemplate = MainModel.Instance.AllTemplateItems.Items.Where(p => p.Name.Equals(templaterInstruction.Template)).FirstOrDefault();
			
			if (itemTemplate != null && File.Exists(documentManager.GetPathByKey(itemTemplate.Key)))
			{
				LogEntries.Clear();
				Log(new LogEntry() { Message = "Processing template: " + itemTemplate.Name });

				logLevel++;
				var result0 = ParseTemplate(templaterInstruction.Project, itemTemplate.Name, documentManager.GetPathByKey(itemTemplate.Key), variables);
				//// AffectedFilesList.ItemsSource = MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles;

				logLevel--;

				//// PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<pre>" + result0 + "</pre>");

				string logHtml = "<style>.message{color: green} .type{color: red} .link{color: blue}</style>" + LogEntriesToHtml();

				File.WriteAllText("LastLog.html", logHtml);
				File.WriteAllText(MainModel.Instance.TemplateItems.SelectedItem.TemplaterInstruction.Project + "\\LastLog.html", logHtml);
				//// PreviewJson.NavigateToString("<pre>" + TemplateInterpreter.Trace + "</pre><br /><pre>" + TemplateProcessor.Trace + "</pre>");
				//// PreviewJson.NavigateToString(logHtml);
			}
			/*else
			{
				PreviewHtml.NavigateToString(ScriptErrorSuppressor + "<pre>Missing template file!</pre>");
			}
			AffectedFilesTab.IsSelected = true;
			LogsTab.IsSelected = true;*/
			File.WriteAllText(documentManager.GetPathByKey(MainModel.Instance.TemplateItems.SelectedItem.Key + ".snap"), JsonConvert.SerializeObject(MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles));
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

			//// LogEntries.AddRange(templateProcessor.LogEntries);

			if (string.IsNullOrEmpty(projectPath))
			{
				Log(new LogEntry() { Message = "No project path was defined, in case we need to create files they will be created in the 'none' folder of the working folder. For: " + templatePath });
			}
			//// NewItem(templaterInstruction.Ready, result);
			templateInterpreter.InterpretProcessedTemplate(projectPath, MainModel.Instance.SelectedProfile.Properties, templateReady);

			LogEntries.AddRange(templateInterpreter.LogEntries);
			//// File.WriteAllText("Resources\\" + TemplateInterpreter + ".logs.html", log);

			MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles.Clear();
			foreach (var entry in LogEntries)
			{
				if (entry.Name != null)
				{
					if (MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles.Where(p => p.Name.Equals(entry.Name)).FirstOrDefault() == null)
					{
						if (File.Exists(entry.Path))
						{
							entry.Size = new FileInfo(entry.Path).Length;
							if (entry.Size > 0)
							{
								entry.Size = entry.Size / 8;
							}
						}
						MainModel.Instance.TemplateItems.SelectedItem.AffectedFiles.Add(new AffectedFile() { Name = entry.Name, Path = entry.Path, Size = entry.Size });
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
				var data = variables;
				if (!string.IsNullOrEmpty(item.Data))
				{
					Log(new LogEntry() { Message = "Collecting data from: " + projectPath + "\\" + item.Data });
					data = File.ReadAllText(projectPath + "\\" + item.Data);
				}
				else
				{
					Log(new LogEntry() { Message = "No data provided for this fork. Use initial data." });
				}
				var childTemplateReady = ParseTemplate(projectPath, item.TemplatePath.Substring(item.TemplatePath.LastIndexOf("\\") + 1), item.TemplatePath, data);
				logLevel--;
			}
			return templateReady;
		}

		public void DocumenPartLabel_MouseDoubleClick(string item)
		{
			if (item != null)
			{
				MultiFileDocument document = GeDocumentByName(item);
				if (document == null)
				{
					//// CreateDocument(item);
					/*
					MainModel.Instance.TemplateItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox, item);
					*/
				}
			}
		}

		// private FileDataAccess FileDataAccess { get; set; }


		/// <summary>
		/// All custom window initialization maintained by developers.
		/// </summary>
		public void CustomInitialize()
		{
			MainModel.Instance.ChannelSelector_Items = new ObservableCollection<string>();
			// FileDataAccess = new FileDataAccess();
			MainModel.Instance.LoginWithFacebookButton_Visibility = Visibility.Visible;


			// Accept Tab key in RichTextBox
			MainModel.InstanceResourceContentTextBox_AcceptsTab = true;
			MainModel.Instance.TemplateItems = new TemplateList(
				new List<MultiFileDocument>
				{ }
				);
			MainModel.Instance.AllTemplateItems = new TemplateList(
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
				MainModel.Instance.RepositorySourceComboBox_ItemsSource = businessConfiguration.ResourceFolders;
				if (businessConfiguration.ResourceFolders.Count > 0)
				{
					MainModel.Instance.RepositorySourceComboBox_SelectedIndex = 0;
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

				MainModel.Instance.ChannelSelector_Items.Insert(0, Me.RowKey);
				MainModel.Instance.ChannelSelector_SelectedIndex = 0;
				SelectedChannel = MainModel.Instance.ChannelSelector_Text;
			}
			else
			{
				Me = new UserAccount()
				{
					RowKey = Guid.NewGuid().ToString(),
					DisplayName = "Log In",
					Email = string.Empty
				};

				MainModel.Instance.ChannelSelector_Items.Clear();
				SelectedChannel = null;
			}

			MainModel.Instance.ChannelSelector_SelectedIndex = 0;
			MainModel.Instance.TemplateItems.Items = new ObservableCollection<MultiFileDocument>();
			// Don't be invasive, ask user for permission to ctreate stuffs on his disk if is not in your app folder.
			if (!Directory.Exists(documentManager.BasePath))
			{
				var s = MessageBox.Show("Folder is expected. Create [" + documentManager.BasePath + "] ?", "Startup", MessageBoxButton.YesNo);
				if (s == MessageBoxResult.Yes)
				{
					Directory.CreateDirectory(documentManager.BasePath);
				}
				else
				{
					return;
				}
			}

			// Colect notes from disk
			var files = Directory.GetFiles(documentManager.BasePath).ToList();
			ObservableCollection<MultiFileDocument> items = new ObservableCollection<MultiFileDocument>();
			foreach (string filePath in files)
			{
				if (filePath.EndsWith(".meta"))
				{
					NoteEntity noteMeta = JsonConvert.DeserializeObject<NoteEntity>(File.ReadAllText(filePath));
					if (noteMeta.Title.EndsWith("profile.rd"))
					{
						// continue;
					}
					if(noteMeta.Tags != null && noteMeta.Tags.Count > 0)
					{

					}
					var item = new MultiFileDocument()
					{
						Tags = noteMeta.Tags,
						Files = noteMeta.Files,
						Key = noteMeta.RowKey,
						Name = noteMeta.Title,
						Synced = noteMeta.Synced,
						Label = string.Empty,
						Versions = noteMeta.Versions,
						Projects = noteMeta.Projects,
						Visibility = VisibilityLevels[0]
					};
					items.Add(item);

					if (noteMeta.Versions!= null)
					{

					}

					var versions = new ObservableCollection<BaseModel>();
					foreach (var collectionItem in noteMeta.Versions)
					{
						versions.Add((BaseModel)collectionItem);
					}
					var projects = new ObservableCollection<BaseModel>();
					foreach (var collectionItem in noteMeta.Projects)
					{
						projects.Add((BaseModel)collectionItem);
					}

					item.AllVersionItems.Items = versions;
					item.AllProjectItems.Items = projects;
				}
			}
			MainModel.Instance.AllTemplateItems.Items = items;
			FilterTemplates(string.Empty); // Dows it improve performance to wait for the list to be constructed?

			// Wire-up events

			InisializeSettings();
			
			if (!string.IsNullOrEmpty(localSettings.ProfileName) && MainModel.Instance.SettingProfiles != null)
			{
				MainModel.Instance.SelectedProfile = MainModel.Instance.SettingProfiles.Where(p => p.Name.Equals(localSettings.ProfileName)).FirstOrDefault();
			}

			if (MainModel.Instance.SelectedProfile == null)
			{
				MainModel.Instance.SelectedProfile = MainModel.Instance.SettingProfiles[0];
			}
			
			string filterString = localSettings.LastFilterValue;
			MainModel.Instance.TemplateFilter = filterString;
			FilterTemplates(filterString);

			if (!string.IsNullOrEmpty(localSettings.LastDocumentName))
			{
				MultiFileDocument lastDocument = GeDocumentByName(localSettings.LastDocumentName);
				MainModel.Instance.MultyFileDocumentsList_SelectedItem = lastDocument;
				//// Adopt(lastDocument);
			}
		}
	}
}
