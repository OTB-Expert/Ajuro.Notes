using Newtonsoft.Json;
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

namespace MemoDrops
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
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
			if (Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account"))
			{
				string myIdentity = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/OTB/MemoDrops/Account/me.json");
				Me = JsonConvert.DeserializeObject<UserAccount>(myIdentity);
			}
			else
			{
				Me = new UserAccount()
				{
					RowKey = Guid.NewGuid().ToString(),
					DisplayName = "Log In",
					Email = string.Empty
				};
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
						Label = string.Empty
					});
				}
			}
			AllItems = items;
			FilterNotes(string.Empty); // Dows it improve performance to wait for the list to be constructed?

			// Wire-up events
			ResourceContent.PreviewKeyUp += ResourceContent_PreviewKeyUp;
			Resource_Name.PreviewKeyUp += Resource_Name_PreviewKeyUp;
			filesList.PreviewKeyUp += FilesList_PreviewKeyUp;

			// Provide me as model
			DataContext = this;
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

		/// <summary>
		/// Add a new note
		/// </summary>
		private void NewItem(string itemName = null)
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
			if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
			{
				NewItem();
			}
			if (CurrentItem != null)
			{
				if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
				{
					SaveItem("notes");
				}
				else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
				{
					DuplicateItem("notes");
				}
				else if (e.Key != Key.LeftCtrl && e.Key != Key.LeftAlt && e.Key != Key.LeftShift)
				{
					CurrentItem.Name = Resource_Name.Text;
					if (CurrentItem.Label.IndexOf("?") < 0)
					{
						CurrentItem.Label = CurrentItem.Label += "?";
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
			if (e.Key == Key.N && Keyboard.Modifiers == ModifierKeys.Control)
			{
				NewItem();
			}
			if (e.Key == Key.M && Keyboard.Modifiers == ModifierKeys.Control)
			{
				ReloadItems("notes", "list", SelectedChannel);
			}
			if (CurrentItem != null)
			{
				if (e.Key == Key.U && Keyboard.Modifiers == ModifierKeys.Control)
				{
					UploadItem("notes");
				}
				else if (e.Key == Key.S && Keyboard.Modifiers == ModifierKeys.Control)
				{
					SaveItem("notes");
				}
				else if (e.Key == Key.D && Keyboard.Modifiers == ModifierKeys.Control)
				{
					DuplicateItem("notes");
				}
				else if (e.Key != Key.LeftCtrl && e.Key != Key.LeftAlt && e.Key != Key.LeftShift)
				{
					CurrentItem.Label = "*";
				}
			}
		}

		private void ReloadItems(string entity, string action, string channel)
		{
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(DataAccess.UrlBase + entity + "/" + action + "/" + channel);
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
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(DataAccess.UrlBase + entity + "/" + action + "/" + channel);
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
			var httpWebRequest = (HttpWebRequest)WebRequest.Create(DataAccess.UrlBase + "/" + (CurrentItem.Synced == DateTime.MinValue ? "insert" : "update/" + CurrentItem.Key));
			httpWebRequest.ContentType = "application/json";
			httpWebRequest.Method = "POST";

			using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
			{
				TextRange range = new TextRange(ResourceContent.Document.ContentStart, ResourceContent.Document.ContentEnd);
				NoteEntity item = new NoteEntity()
				{
					Author = SelectedChannel,
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
					CurrentItem.Author = SelectedChannel;
					SaveItem(entity);
					StatusBartextBlock.Text = DateTime.Now.ToShortTimeString() + ": Uploaded [" + CurrentItem.Name + "]";
				}
			}
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
				FlowDocument mcFlowDoc = new FlowDocument();
				Paragraph para = new Paragraph();
				if (!File.Exists(BasePath + CurrentItem.Key))
				{
					CurrentItem.Label = "!";
					return;
				}
				para.Inlines.Add(new Run(File.ReadAllText(BasePath + CurrentItem.Key)));
				mcFlowDoc.Blocks.Add(para);
				ResourceContent.Document = mcFlowDoc;
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
			Process.Start("notepad.exe", BasePath + CurrentItem.Key + ".meta");
		}

		private void ChannelSelector_KeyUp(object sender, KeyEventArgs e)
		{

		}

		private void ChannelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (ChannelSelector.SelectedValue != null)
			{
				SelectedChannel = ((ComboBoxItem)ChannelSelector.SelectedValue).Content.ToString();
			}
			else
			{
				SelectedChannel = ChannelSelector.Text;
			}
			PreviewItems("notes", "list", SelectedChannel);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			AccountWindow accountWindow = new AccountWindow();

			accountWindow.WireUpData(Me);
			accountWindow.Closing += AccountWindow_Closing;
			accountWindow.Show();
		}

		private void AccountWindow_Closing(object sender, CancelEventArgs e)
		{
			UserAccount me = ((AccountWindow)sender).Me;
			if (string.IsNullOrEmpty(me.DisplayName))
			{
				Me.DisplayName = "Log In";
			}
			else
			{
				Me.DisplayName = me.DisplayName;
			}
			Me.Email = me.Email;
			Me.PartitionKey = me.PartitionKey;
			Me.Password = me.Password;
			Me.PermalinkId = me.PermalinkId;
			Me.RealName = me.RealName;
			Me.RowKey = me.RowKey;
			Me.Username = me.Username;
		}
	}

	/// <summary>
	/// The structure of a note
	/// </summary>
	public class MyItem : INotifyPropertyChanged
	{
		public string Key { get; set; }
		public string Author { get; set; }
		public DateTime Synced { get; set; }
		private string name { get; set; }
		/// <summary>
		/// Name of the note, is also the name of the file storing the content of the note.
		/// </summary>
		public string Name
		{
			get { return name; }
			set
			{
				name = value;
				NotifyPropertyChanged();
			}
		}

		private string label { get; set; }
		/// <summary>
		/// Used as dirty flag for the note's name or content. Is emptied on save.
		/// </summary>
		public string Label
		{
			get { return label; }
			set
			{
				label = value;
				NotifyPropertyChanged();
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
