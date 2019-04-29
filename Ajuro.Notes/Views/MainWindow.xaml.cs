using Markdig;
using MemoDrops.DataAccess;
using MemoDrops.Model;
using MemoDrops.Views;
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
using Ajuro.Template.Processor;

namespace MemoDrops.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
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

		/// <summary>
		/// All notes as a list fo binding.
		/// </summary>
		//public ObservableCollection<FileItem> AllItems { get; set; }
		//public ObservableCollection<FileItem> FileItems { get; set; }
        public ItemList FileItems { get; set; }
        public static  ItemList AllItems { get; set; }

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
            FileItems = new ItemList(
                new List<FileItem>
                    {}
                );
            AllItems = new ItemList(
                new List<FileItem>
                    {}
                );

            this.Closing += Window_Closing;
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
			}
			if (File.Exists("LocalSettings.json"))
			{
				localSettings = JsonConvert.DeserializeObject<LocalSettings>(File.ReadAllText("LocalSettings.json"));
			}
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account"))
			{
				string myIdentity = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/me.json");
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
			//FileItems = new ObservableCollection<FileItem>();
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
            ObservableCollection<FileItem> items = new ObservableCollection<FileItem>();
            
            foreach (string filePath in files)
			{
				if (filePath.EndsWith(".meta"))
				{
					NoteEntity noteMeta = JsonConvert.DeserializeObject<NoteEntity>(File.ReadAllText(filePath));
					items.Add(new FileItem()
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
            AllItems.Items = items;
			FilterNotes(string.Empty); // Dows it improve performance to wait for the list to be constructed?

			// Wire-up events
			ResourceContentTextBox.PreviewKeyUp += ResourceContentTextBox_PreviewKeyUp;
			ResourceContentTextBox.PreviewKeyDown += ResourceContentTextBox_PreviewKeyDown;
			Resource_Name.PreviewKeyUp += Resource_Name_PreviewKeyUp;
			filesList.PreviewKeyUp += FilesList_PreviewKeyUp;

			InisializeSettings();

			SelectedProfileIndicator.DataContext = MainModel.Instance;
			EnvironmentComboBox.DataContext = MainModel.Instance;

			if (!string.IsNullOrEmpty(localSettings.ProfileName) && MainModel.Instance.SettingProfiles != null)
			{
				MainModel.Instance.SelectedProfile = MainModel.Instance.SettingProfiles.Where(p => p.Name.Equals(localSettings.ProfileName)).FirstOrDefault();
			}

			if (MainModel.Instance.SelectedProfile == null)
			{
				MainModel.Instance.SelectedProfile = MainModel.Instance.SettingProfiles[0];
			}

			// Provide me as model
			DataContext = FileItems;
		}

		private void ResourceContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				KeyShortcuts.Visibility = Visibility.Visible;
			}
		}

		private void InisializeSettings()
		{
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account"))
			{
				if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/settings.json"))
				{
					string mySettings = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/settings.json");
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
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Alfa",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Beta",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "RC",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "RTM",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "GA",
					Properties = new List<KeyValue>()
				});

				MainModel.Instance.SettingProfiles.Add(new SettingsProfile()
				{
					Name = "Live",
					Properties = new List<KeyValue>()
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
                FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);
			}
			if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
			{
				DuplicateItem("notes");
			}
			if (FileItems.CurrentItem != null)
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
				FileItems.CurrentItem = new FileItem()
				{
					Name = itemName,
					Key = Guid.NewGuid().ToString(),
					Label = "!!"
				};
				FileItems.Add(FileItems.CurrentItem);
				AllItems.Add(FileItems.CurrentItem);
				ResourceContentTextBox.Document = new FlowDocument();
			}
		}

		public string ExecuteSQL(string connectionString, string queryString)
		{
			SQL.DataAccess dataAccess = new SQL.DataAccess();
			string result = dataAccess.ExecuteQuery(connectionString, queryString);
			return result;
		}

		private void ExecuteQuery(bool openWindow)
		{
			string queryString = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd).Text;
			var property = MainModel.Instance.SelectedProfile.Properties.Where(p => p.Key == "cs").FirstOrDefault();
			string connectionString = property == null ? string.Empty : property.Value;

			if (queryString.StartsWith("-- SQL") || FileItems.CurrentItem.Name.EndsWith(".sql"))
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
				PreviewHtml.NavigateToString(view);
				if (string.IsNullOrEmpty(MainModel.Instance.JsonRepresentation))
				{
					PreviewJson.NavigateToString("Parse Error");
				}
				else
				{
					PreviewJson.NavigateToString(MainModel.Instance.JsonRepresentation);
				}
				PreviewHtml.NavigateToString(result);
			}

			if (queryString.StartsWith("-- MD") || FileItems.CurrentItem.Name.EndsWith(".md"))
			{
				var result = Markdown.ToHtml(queryString);
				PreviewHtml.NavigateToString(result);
			}

			if (queryString.StartsWith("-- HTML") || FileItems.CurrentItem.Name.EndsWith(".html"))
			{
				var result = Markdown.ToHtml(queryString);
				PreviewHtml.NavigateToString(result);
			}

			if (queryString.StartsWith("-- JSON") || FileItems.CurrentItem.Name.EndsWith(".json"))
			{
				var result = Markdown.ToHtml(queryString);
				PreviewHtml.NavigateToString("<pre>" + result + "</pre>");
			}

			if (Keyboard.Modifiers == ModifierKeys.Control)
			{
				Process.Start(@"C:\Program Files (x86)\Notepad++\notepad++.exe", "C:\\Work\\Resources\\" + FileItems.CurrentItem.Key);
			}
			if (Keyboard.Modifiers == ModifierKeys.Shift)
			{
				File.WriteAllText("Results.html", File.ReadAllText("C:\\Work\\Resources\\" + FileItems.CurrentItem.Key));
				Process.Start(@"C:\Program Files (x86)\Google\Chrome Dev\Application\chrome.exe", "Results.html");
			}


			if (queryString.StartsWith("-- HTML") || FileItems.CurrentItem.Name.EndsWith(".html"))
			{
				PreviewHtml.NavigateToString(queryString);
			}

			if (queryString.StartsWith("-- RD") || FileItems.CurrentItem.Name.EndsWith(".rd"))
			{
				TemplaterInstruction templaterInstruction = JsonConvert.DeserializeObject<TemplaterInstruction>(queryString);
				TemplateProcessor TemplateProcessor = null;
				TemplateInterpreter TemplateInterpreter = null;
				TemplateProcessor = new TemplateProcessor();
				TemplateInterpreter = new TemplateInterpreter();

				var itemVar = AllItems.Items.Where(p => p.Name.Equals(templaterInstruction.Model)).FirstOrDefault();
				string variables = File.ReadAllText(BasePath + itemVar.Key);
				var itemTemplate = AllItems.Items.Where(p => p.Name.Equals(templaterInstruction.Template)).FirstOrDefault();
				string template = File.ReadAllText(BasePath + itemTemplate.Key);
				if(template.IndexOf("============== Vars end ==============") > -1)
				{
					template = template.Substring(template.IndexOf("============== Vars end ==============") + "============== Vars end ==============".Length);
				}
				var vars = new List<AjuVarset>() { TemplateProcessor.ToAjuVarset(JObject.Parse(variables)) };
				var result = TemplateProcessor.TestCase(template, vars[0].Varset);
				var result0 = TemplateProcessor.UpdateTemplate(variables, template);
				// NewItem(templaterInstruction.Ready, result);
				TemplateInterpreter.InterpretProcessedTemplate(result0);
				PreviewHtml.NavigateToString("<pre>" + result + "</pre>");
			}
		}

		/// <summary>
		/// Delete existent note
		/// </summary>
		private void DeleteItem(string entity)
		{
			var response = MessageBox.Show("Delete file [" + FileItems.CurrentItem.Name + "] ?", "Question", MessageBoxButton.YesNo);
			if (response == MessageBoxResult.Yes)
			{
				File.Delete(BasePath + FileItems.CurrentItem.Name);
				FileItems.Remove(FileItems.CurrentItem);
				AllItems.Remove(FileItems.CurrentItem);
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
                FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);
            }
			if (FileItems.CurrentItem != null)
			{
				bool consumed = false;
				if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
					SaveItem("notes");
				}
				if (e.Key == Key.F5)
				{
					consumed = true;
					ExecuteQuery(Keyboard.Modifiers == ModifierKeys.Control);
				}
				if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
				{
					consumed = true;
                    FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox);
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
                        FileItems.CurrentItem.Name = Resource_Name.Text;
                        FileItems.CurrentItem.Status = 1;
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
			ExecuteWindowAction(e);

			if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
			{
				ReloadItems("notes", "list", SelectedChannel);
			}
		}

		private void ReloadItems(ResourceFolder rootFolder)
		{
			var itemProvider = new ItemProvider();

			var rawItemsList = itemProvider.GetItems(rootFolder.Path);
			bool hasMetadata = rootFolder.Type=="notes";

			AllItems.Clear();
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
						AllItems.Add(new FileItem()
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
					List<string> files = new List<string>();
					if(File.Exists(item.Path))
					{
						files.Add(item.Path);
					}
					else
					{
						// Is a folder
						files.AddRange(Directory.GetFiles(item.Path));
					}

					AllItems.Add(new FileItem()
					{
						Name = item.Name,
						Synced = DateTime.Now,
						Label = string.Empty,
						Files = files

					});
				}
			}
			FilterNotes(string.Empty);
			StatusBartextBlock.Text = "Found " + AllItems.Items.Count + " items;";
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
						AllItems.Add(new FileItem()
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
			if (AllItems == null)
			{
				return;
			}
			AllItems.Clear();
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
					AllItems.Add(new FileItem()
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

			var httpWebRequest = (HttpWebRequest)WebRequest.Create(MainModel.UrlBase + entity + "/" + (FileItems.CurrentItem.Synced == DateTime.MinValue ? "insert" : "update/" + FileItems.CurrentItem.Key));

			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
				NoteEntity item = new NoteEntity()
				{
					Author = Me.RowKey,
					RowKey = FileItems.CurrentItem.Key,
					Content = range.Text,
					Title = FileItems.CurrentItem.Name
				};

				streamWriter.Write(JsonConvert.SerializeObject(item));
				streamWriter.Flush();
				streamWriter.Close();

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
                    FileItems.CurrentItem.Synced = DateTime.Now;
                    FileItems.CurrentItem.Author = Me.RowKey;
					SaveItem(entity);
					StatusBartextBlock.Text = DateTime.Now.ToShortTimeString() + ": Uploaded [" + FileItems.CurrentItem.Name + "]";
                    FileItems.CurrentItem.Status = 2;
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
			FileItem NewCurrentItem = new FileItem()
			{
				Name = FileItems.CurrentItem.Name,
				Label = ""
			};
			FileItems.Add(NewCurrentItem);
			TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
			File.WriteAllText(BasePath + FileItems.CurrentItem.Name, range.Text);
            FileItems.CurrentItem.Label = "";
		}

		/// <summary>
		/// Work in progress gets saved in the same file if the note name has not changes. If the name changed, a new file is created and the old file is deleted.
		/// </summary>
		private void SaveItem(string entity)
		{
			TextRange range = new TextRange(ResourceContentTextBox.Document.ContentStart, ResourceContentTextBox.Document.ContentEnd);
			if(FileItems.CurrentItem.Key == null)
			{
				FileItems.CurrentItem.Key = Guid.NewGuid().ToString(); 
			}
			File.WriteAllText(BasePath + FileItems.CurrentItem.Key, range.Text);
			FileItems.CurrentItem.Name = Resource_Name.Text;
			FileItems.CurrentItem.LastUpdated = DateTime.UtcNow;
			File.WriteAllText(BasePath + FileItems.CurrentItem.Key + ".meta", JsonConvert.SerializeObject(
			new NoteEntity()
			{
				Tags = FileItems.CurrentItem.Tags,
				Files = FileItems.CurrentItem.Files,
				Author = SelectedChannel,
				RowKey = FileItems.CurrentItem.Key,
				Synced = FileItems.CurrentItem.Synced,
				Content = string.Empty,
				Title = Resource_Name.Text
			})) ;
            /*CurrentItem.Label = "";
			CurrentItem.Name = Resource_Name.Text;
			if (CurrentItem.Name.ToLower() != CurrentItem.NameOriginal.ToLower())
			{
				if (File.Exists(BasePath + Resource_Name.Text))
				{
					File.Delete(BasePath + CurrentItem.NameOriginal);
				}
			}*/
            FileItems.CurrentItem.Status = 0;
		}

		/// <summary>
		/// When another note is selected in the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
            FileItems.CurrentItem = ((FileItem)filesList.SelectedItem);
			if (FileItems.CurrentItem != null)
			{
				Resource_Name.Text = FileItems.CurrentItem.Name;
				FlowDocument ResourceContentDocument = new FlowDocument();
				FlowDocument LinksReaderDocument = new FlowDocument();
				Paragraph ResourceContentParagraph = new Paragraph();
				Paragraph LinksReaderParagraph = new Paragraph();
				if (!File.Exists(BasePath + FileItems.CurrentItem.Key))
				AdditionalFiles.Children.Clear();
				AdditionalFiles.Orientation = Orientation.Horizontal;

				if (FileItems.CurrentItem.Files != null)
				{
					AdditionalFilesContainer.Visibility = Visibility.Visible;
					foreach (string file in FileItems.CurrentItem.Files)
					{
						Label label = new Label();
						label.Content = file.Substring(file.LastIndexOf("\\") + 1);
						AdditionalFiles.Children.Add(label);
						// label
					}
				}

				TagsStackPanel.Children.Clear();
				if (FileItems.CurrentItem.Tags != null)
				{
					foreach (string tag in FileItems.CurrentItem.Tags)
					{
						Label label = new Label();
						label.Content = tag;
						TagsStackPanel.Children.Add(label);
					}
				}

				if (!File.Exists(BasePath + FileItems.CurrentItem.Key))
				{
					FileItems.CurrentItem.Label = "!";
					ResourceContentTextBox.Document = new FlowDocument();
					DataTabControl.SelectedIndex = 1;

					if (FileItems.CurrentItem.Files != null)
					{
						string resourceContent = File.ReadAllText(FileItems.CurrentItem.Files[0]);
						LinksReaderParagraph.Inlines.Add(new Run(resourceContent));
						LinksReaderDocument.Blocks.Add(LinksReaderParagraph);
						LinksReader.Document = LinksReaderDocument;
					}
					return;
				}
				string content = File.ReadAllText(BasePath + FileItems.CurrentItem.Key);
				ResourceContentParagraph.Inlines.Add(new Run(content));
				LinksReaderParagraph.Inlines.Add(new Run(content));

				ResourceContentDocument.Blocks.Add(ResourceContentParagraph);
				ResourceContentTextBox.Document = ResourceContentDocument;

				LinksReaderDocument.Blocks.Add(LinksReaderParagraph);
				LinksReader.Document = LinksReaderDocument;
			}
		}

		private void HelpLink_MouseUp(object sender, MouseButtonEventArgs e)
		{
			System.Diagnostics.Process.Start("http://otb.expert/MemoDrops/mail_template_invitation.html");
		}

		private void FilterItems_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Down)
			{
				ItemsSelectedIndex += 1;
				if (ItemsSelectedIndex >= FileItems.Items.Count)
				{
					ItemsSelectedIndex = 0;
				}
				filesList.SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (e.Key == Key.Up)
			{
				ItemsSelectedIndex -= 1;
				if (ItemsSelectedIndex < 0)
				{
					ItemsSelectedIndex = FileItems.Items.Count - 1;
				}
				filesList.SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (e.Key == Key.Enter && FileItems.Items.Count == 0)
			{
                FileItems.NewItem(BasePath, ref FileNr, ref ResourceContentTextBox, FilterItems.Text.Trim());
                
			}
			string filterString = FilterItems.Text.Trim().ToLower();
			FilterNotes(filterString);
		}

		private void FilterNotes(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				FileItems.Clear();

				foreach (var item in AllItems.Items)
				{
					FileItems.Add(item);
				}
			}
			else
			{
				FileItems.Clear();
				var items = AllItems.Items.Where(p => p.Name.ToLower().Contains(filterString));
				foreach (var item in items)
				{
					FileItems.Add(item);
				}
			}
			if (FileItems.Items.Count > 0)
			{
				ItemsSelectedIndex = 0;
				filesList.SelectedIndex = ItemsSelectedIndex;
			}
		}

		private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			// Process.Start("notepad.exe", BasePath + CurrentItem.Key + ".meta");
			NoteMetaEditorWindow editorWindow = new NoteMetaEditorWindow();
			editorWindow.LinkData(FileItems.CurrentItem);
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
					if(!allText.ToUpper().Contains(" C:"))
					{
						fileName = allText.Substring(2);
					}
					else
					{
						fileName = allText.Substring(20);
					}
					var existentPage = AllItems.Items.Where(p => p.Name.Equals(fileName)).FirstOrDefault();
					if (existentPage != null)
					{
						var visiblePage = FileItems.Items.Where(p => p.Name.Equals(fileName)).FirstOrDefault();
						if(visiblePage==null)
						{
							FileItems.Add(existentPage);
							visiblePage = existentPage;
						}
						filesList.SelectedItem = visiblePage;
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

		private void Adopt(FileItem item)
		{
			throw new NotImplementedException();
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
			FileItem item = (FileItem)((Image)sender).Tag;
			ShareItem(item);
		}

		private void ShareItem(FileItem item)
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
			File.WriteAllText("LocalSettings.json", JsonConvert.SerializeObject(localSettings));
		}

		private void ShareButton_Click(object sender, RoutedEventArgs e)
		{
			FileItem item = (FileItem)((Button)sender).Tag;
			ShareItem(item);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Process.Start(CurrentItem.Name);
		}

		private void RepositorySourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if(RepositorySourceComboBox.SelectedItem != null)
			{
				SelectedResourceFolder = (ResourceFolder)RepositorySourceComboBox.SelectedItem;
				ReloadItems(SelectedResourceFolder);
			}
		}

        private void orderCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FileItems.OrderByName(OrderCriteriaComboBox.Text);
        }

		private void ItemResourcesButton_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedResourceFolder != null)
			{
				Process.Start(SelectedResourceFolder.Path + "\\" + FileItems.CurrentItem.Name);
			}
		}

		private void TagsButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void TagEditor_KeyUp(object sender, KeyEventArgs e)
		{
			if(e.Key == Key.Tab || e.Key == Key.Enter)
			{
				Label label = new Label();
				label.Content = TagEditor.Text.Trim().ToLower();
				TagsStackPanel.Children.Add(label);
			FileItems.CurrentItem.Tags.Add(TagEditor.Text.Trim().ToLower());
				TagEditor.Text = string.Empty;
			}

			if(e.Key == Key.Delete || e.Key == Key.Back)
			{
				if (TagEditor.Text == "")
				{
					readyForDelete = !readyForDelete;
					if (readyForDelete)
					{
						if (TagsStackPanel.Children.Count > 0)
						{
						FileItems.CurrentItem.Tags.RemoveAt(FileItems.CurrentItem.Tags.Count-1);
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
		bool readyForDelete = false;

		private void Label_MouseLeave(object sender, MouseEventArgs e)
		{

		}

		private void SortByNameLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			bool isAsccending = FileItems.OrderByName("Name");
			SortByNameLabel.Foreground = isAsccending ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Magenta;
		}
	}

	public class NoteEntity
	{
		public string RowKey { get; set; }
		public List<string> Tags { get; set; }
		public List<string> Files { get; set; }
		public DateTime Synced { get; set; }
		public string Author { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
	}
}
