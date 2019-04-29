using Markdig;
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
using System.Runtime.CompilerServices;
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
		LocalSettings localSettings = new LocalSettings();
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
		MyItem CurrentItem { get; set; }

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
		public ObservableCollection<MyItem> AllItems { get; set; }
		public ObservableCollection<MyItem> FileItems { get; set; }

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
			FileItems = new ObservableCollection<MyItem>();
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
			ObservableCollection<MyItem> items = new ObservableCollection<MyItem>();
			foreach (string filePath in files)
			{
				if (filePath.EndsWith(".meta"))
				{
					NoteEntity noteMeta = JsonConvert.DeserializeObject<NoteEntity>(File.ReadAllText(filePath));
					items.Add(new MyItem()
					{
						Key = noteMeta.RowKey,
						Name = noteMeta.Title,
						Synced = noteMeta.Synced,
						Label = string.Empty,
						Visibility = VisibilityLevels[0]
					});
				}
			}
			AllItems = items;
			FilterNotes(string.Empty); // Dows it improve performance to wait for the list to be constructed?

			// Wire-up events
			ResourceContent.PreviewKeyUp += ResourceContent_PreviewKeyUp;
			ResourceContent.PreviewKeyDown += ResourceContent_PreviewKeyDown;
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
			DataContext = this;
		}

		private void ResourceContent_PreviewKeyDown(object sender, KeyEventArgs e)
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
				NewItem();
			}
			if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
			{
				DuplicateItem("notes");
			}
			if (CurrentItem != null)
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
				CurrentItem = new MyItem()
				{
					Name = itemName,
					Key = Guid.NewGuid().ToString(),
					Label = "!!"
				};
				FileItems.Add(CurrentItem);
				AllItems.Add(CurrentItem);
				ResourceContent.Document = new FlowDocument();
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
			string queryString = new TextRange(ResourceContent.Document.ContentStart, ResourceContent.Document.ContentEnd).Text;
			var property = MainModel.Instance.SelectedProfile.Properties.Where(p => p.Key == "cs").FirstOrDefault();
			string connectionString = property == null ? string.Empty : property.Value;

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

			if (queryString.StartsWith("-- MD") || CurrentItem.Name.EndsWith(".md"))
			{
				var result = Markdown.ToHtml(queryString);
				PreviewHtml.NavigateToString(result);
			}

			if (queryString.StartsWith("-- HTML") || CurrentItem.Name.EndsWith(".html"))
			{
				PreviewHtml.NavigateToString(queryString);
			}

			if (queryString.StartsWith("-- RD") || CurrentItem.Name.EndsWith(".rd"))
			{
				TemplaterInstruction templaterInstruction = JsonConvert.DeserializeObject<TemplaterInstruction>(queryString);
				TemplateProcessor TemplateProcessor = null;
				TemplateInterpreter TemplateInterpreter = null;
				TemplateProcessor = new TemplateProcessor();
				TemplateInterpreter = new TemplateInterpreter();

				var itemVar = AllItems.Where(p => p.Name.Equals(templaterInstruction.Model)).FirstOrDefault();
				string variables = File.ReadAllText(BasePath + itemVar.Key);
				var itemTemplate = AllItems.Where(p => p.Name.Equals(templaterInstruction.Template)).FirstOrDefault();
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
			var response = MessageBox.Show("Delete file [" + CurrentItem.Name + "] ?", "Question", MessageBoxButton.YesNo);
			if (response == MessageBoxResult.Yes)
			{
				File.Delete(BasePath + CurrentItem.Name);
				FileItems.Remove(CurrentItem);
				AllItems.Remove(CurrentItem);
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
				NewItem();
			}
			if (CurrentItem != null)
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
					NewItem();
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
						CurrentItem.Name = Resource_Name.Text;
						CurrentItem.Status = 1;
					}
				}
			}
		}

		/// <summary>
		/// When user edits the content of the note.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void ResourceContent_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			KeyShortcuts.Visibility = Visibility.Collapsed;
			ExecuteWindowAction(e);

			if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
			{
				ReloadItems("notes", "list", SelectedChannel);
			}
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
						AllItems.Add(new MyItem()
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
					AllItems.Add(new MyItem()
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

			var httpWebRequest = (HttpWebRequest)WebRequest.Create(MainModel.UrlBase + entity + "/" + (CurrentItem.Synced == DateTime.MinValue ? "insert" : "update/" + CurrentItem.Key));

			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				TextRange range = new TextRange(ResourceContent.Document.ContentStart, ResourceContent.Document.ContentEnd);
				NoteEntity item = new NoteEntity()
				{
					Author = Me.RowKey,
					RowKey = CurrentItem.Key,
					Content = range.Text,
					Title = CurrentItem.Name
				};

				streamWriter.Write(JsonConvert.SerializeObject(item));
				streamWriter.Flush();
				streamWriter.Close();

				var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
				using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
				{
					var result = streamReader.ReadToEnd();
					CurrentItem.Synced = DateTime.Now;
					CurrentItem.Author = Me.RowKey;
					SaveItem(entity);
					StatusBartextBlock.Text = DateTime.Now.ToShortTimeString() + ": Uploaded [" + CurrentItem.Name + "]";
					CurrentItem.Status = 2;
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
			MyItem NewCurrentItem = new MyItem()
			{
				Name = CurrentItem.Name,
				Label = ""
			};
			FileItems.Add(NewCurrentItem);
			TextRange range = new TextRange(ResourceContent.Document.ContentStart, ResourceContent.Document.ContentEnd);
			File.WriteAllText(BasePath + CurrentItem.Name, range.Text);
			CurrentItem.Label = "";
		}

		/// <summary>
		/// Work in progress gets saved in the same file if the note name has not changes. If the name changed, a new file is created and the old file is deleted.
		/// </summary>
		private void SaveItem(string entity)
		{
			TextRange range = new TextRange(ResourceContent.Document.ContentStart, ResourceContent.Document.ContentEnd);
			File.WriteAllText(BasePath + CurrentItem.Key, range.Text);
			CurrentItem.Name = Resource_Name.Text;
			CurrentItem.LastUpdated = DateTime.UtcNow;
			File.WriteAllText(BasePath + CurrentItem.Key + ".meta", JsonConvert.SerializeObject(
			new NoteEntity()
			{
				Author = SelectedChannel,
				RowKey = CurrentItem.Key,
				Synced = CurrentItem.Synced,
				Content = string.Empty,
				Title = Resource_Name.Text
			}));
			/*CurrentItem.Label = "";
			CurrentItem.Name = Resource_Name.Text;
			if (CurrentItem.Name.ToLower() != CurrentItem.NameOriginal.ToLower())
			{
				if (File.Exists(BasePath + Resource_Name.Text))
				{
					File.Delete(BasePath + CurrentItem.NameOriginal);
				}
			}*/
			CurrentItem.Status = 0;
		}

		/// <summary>
		/// When another note is selected in the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			CurrentItem = ((MyItem)filesList.SelectedItem);
			if (CurrentItem != null)
			{
				Resource_Name.Text = CurrentItem.Name;
				FlowDocument ResourceContentDocument = new FlowDocument();
				FlowDocument LinksReaderDocument = new FlowDocument();
				Paragraph ResourceContentParagraph = new Paragraph();
				Paragraph LinksReaderParagraph = new Paragraph();
				if (!File.Exists(BasePath + CurrentItem.Key))
				{
					CurrentItem.Label = "!";
					return;
				}
				string content = File.ReadAllText(BasePath + CurrentItem.Key);
				ResourceContentParagraph.Inlines.Add(new Run(content));
				LinksReaderParagraph.Inlines.Add(new Run(content));

				ResourceContentDocument.Blocks.Add(ResourceContentParagraph);
				ResourceContent.Document = ResourceContentDocument;

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
				if (ItemsSelectedIndex >= FileItems.Count)
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
					ItemsSelectedIndex = FileItems.Count - 1;
				}
				filesList.SelectedIndex = ItemsSelectedIndex;
				return;
			}
			if (e.Key == Key.Enter && FileItems.Count == 0)
			{
				NewItem(FilterItems.Text.Trim());
			}
			string filterString = FilterItems.Text.Trim().ToLower();
			FilterNotes(filterString);
		}

		private void FilterNotes(string filterString)
		{
			if (string.IsNullOrEmpty(filterString))
			{
				FileItems.Clear();

				foreach (var item in AllItems)
				{
					FileItems.Add(item);
				}
			}
			else
			{
				FileItems.Clear();
				var items = AllItems.Where(p => p.Name.ToLower().Contains(filterString));
				foreach (var item in items)
				{
					FileItems.Add(item);
				}
			}
			if (FileItems.Count > 0)
			{
				ItemsSelectedIndex = 0;
				filesList.SelectedIndex = ItemsSelectedIndex;
			}
		}

		private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			// Process.Start("notepad.exe", BasePath + CurrentItem.Key + ".meta");
			NoteMetaEditorWindow editorWindow = new NoteMetaEditorWindow();
			editorWindow.LinkData(CurrentItem);
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
			PreviewItems("notes", "list", SelectedChannel);
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
			var start = ResourceContent.Document.ContentStart;
			var here = ResourceContent.CaretPosition;
			var range = new TextRange(start, here);
			int indexInText = range.Text.Length;

			string allText = new TextRange(ResourceContent.Document.ContentStart, ResourceContent.Document.ContentEnd).Text;
			allText = allText.Substring(0, (allText.IndexOf("\n", indexInText)));
			int fromIndex = allText.LastIndexOf("\n") + 2;
			int toIndex = allText.Length + 1;
			allText = allText.Substring(allText.LastIndexOf("\n") + 1).Trim();
			TextPointer fromPointer = ResourceContent.Document.ContentStart.GetPositionAtOffset(fromIndex);
			TextPointer toPointer = ResourceContent.Document.ContentStart.GetPositionAtOffset(toIndex);
			ResourceContent.Selection.Select(fromPointer, toPointer);
			TextRange newRange = new TextRange(fromPointer, toPointer);

			if (allText.StartsWith("> "))
			{
				try
				{
					Process.Start(allText.Substring(2));
				}
				catch (Exception ex)
				{

				}
			}
		}
		private void LinksReader_TextChanged(object sender, TextChangedEventArgs e)
		{

		}

		private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
		{
			SettingsWindow settingsWindow = new SettingsWindow();
			settingsWindow.Show();
		}

		private void ResourceContent_KeyUp(object sender, KeyEventArgs e)
		{
			if (Keyboard.Modifiers == ModifierKeys.None)
			{
				// CurrentItem.Status = 1;
			}
		}

		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			MyItem item = (MyItem)((Image)sender).Tag;
			ShareItem(item);
		}

		private void ShareItem(MyItem item)
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
			MyItem item = (MyItem)((Button)sender).Tag;
			ShareItem(item);
		}

		private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
	}
	
	public class NoteEntity
	{
		public string RowKey { get; set; }
		public DateTime Synced { get; set; }
		public string Author { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
	}
}
