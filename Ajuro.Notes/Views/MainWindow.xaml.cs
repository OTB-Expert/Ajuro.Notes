using Markdig;
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
using System.Text;
using Ajuro.WPF.Base.Model;
using Ajuro.WPF.Base;
using Ajuro.WPF.Base.DataAccess;
using Ajuro.Net.Template.Processor;

namespace Ajuro.Notes.View
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		Ajuro.WPF.Base.Views.MainWindowLogic logic = new WPF.Base.Views.MainWindowLogic();
		/// <summary>
		/// Default public constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
			logic.CustomInitialize();

			LoginWithFacebookButton.DataContext = logic.view;
			this.Closing += Window_Closing;
			ResourceContentTextBox.PreviewKeyUp += ResourceContentTextBox_PreviewKeyUp;
			ResourceContentTextBox.PreviewKeyDown += ResourceContentTextBox_PreviewKeyDown;
			Resource_Name.PreviewKeyUp += Resource_Name_PreviewKeyUp;
			MultyFileDocumentsList.PreviewKeyUp += FilesList_PreviewKeyUp;


			MainModel.Instance.CommandRequestedEvent += Instance_CommandRequestedEvent;

			// Provide me as model
			DataContext = MainModel.Instance;
		}

		private string Instance_CommandRequestedEvent(KnownCommands str)
		{
			logic.ExecuteQuery(Key.F5, false);
			return string.Empty;
		}

		private void ResourceContentTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			logic.ResourceContentTextBox_PreviewKeyDown();
		}

		/// <summary>
		/// When selected note is changed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			logic.FilesList_PreviewKeyUp(e.Key);
		}

		/// <summary>
		/// User is editing the note name. From this point the user can either save, creating a new file and deleting the old one or he can duplicate, creating a new file.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void Resource_Name_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			logic.Resource_Name_PreviewKeyUp(e.Key);
		}

		/// <summary>
		/// When user edits the content of the note.
		/// </summary>
		/// <param name="sender">Control</param>
		/// <param name="e">Event</param>
		private void ResourceContentTextBox_PreviewKeyUp(object sender, KeyEventArgs e)
		{
			logic.ResourceContentTextBox_PreviewKeyUp(e.Key);
		}

		/// <summary>
		/// When another note is selected in the list.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void FilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			logic.FilesList_SelectionChanged();
		}
		
		private void HelpLink_MouseUp(object sender, MouseButtonEventArgs e)
		{
			logic.HelpLink_MouseUp();
		}


		private void FilterTemplateItems_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FilterTemplateItems_KeyUp(e.Key);
		}

		private void FilterVersionItems_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FilterVersionItems_KeyUp(e.Key);
		}

		private void FilterProjectItems_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FilterProjectItems_KeyUp(e.Key);
		}

		private void FilesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.FilesList_MouseDoubleClick();
		}
		
		private void ChannelSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			logic.ChannelSelector_SelectionChanged();
			// PreviewItems("notes", "list", SelectedChannel);
		}

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{
			logic.MenuItem_Click();
		}

		private void AccountWindow_Closing(object sender, CancelEventArgs e)
		{
			UserAccount me = ((AccountWindow)sender).Me;
			logic.AccountWindow_Closing(me);
		}

		private void LinksReader_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.LinksReader_MouseDoubleClick();
		}

		private void MenuItemSettings_Click(object sender, RoutedEventArgs e)
		{
			logic.MenuItemSettings_Click();
		}
		
		private void Image_MouseUp(object sender, MouseButtonEventArgs e)
		{
			MultiFileDocument item = (MultiFileDocument)((Image)sender).Tag;
			logic.Image_MouseUp(item);
		}

		private void InputBox_Answered(InputBox sender, string answer)
		{
			sender.Close();
		}


		private void Window_Closing(object sender, CancelEventArgs e)
		{
			logic.Window_Closing();
		}

		private void ShareButton_Click(object sender, RoutedEventArgs e)
		{
			MultiFileDocument item = (MultiFileDocument)((Button)sender).Tag;
			logic.ShareButton_Click(item);
		}

		private void RepositorySourceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			logic.RepositorySourceComboBox_SelectionChanged();
		}

		private void orderCriteria_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			MainModel.Instance.TemplateItems.OrderByName(OrderCriteriaComboBox.Text);
		}

		private void ItemResourcesButton_Click(object sender, RoutedEventArgs e)
		{
			logic.ItemResourcesButton_Click();
		}

		private void TagEditor_KeyUp(object sender, KeyEventArgs e)
		{
			logic.TagEditor_KeyUp(e.Key);
		}

		private void FileEditor_KeyUp(object sender, KeyEventArgs e)
		{
			logic.FileEditor_KeyUp(e.Key);
		}

		private void SortByNameLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			logic.SortByNameLabel_MouseUp();
		}

		private void DocumenPartLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			var item = (string)((Label)sender).Tag;
			logic.DocumenPartLabel_MouseDoubleClick(item);
		}

		private void DocumenPartLabel_MouseUp(object sender, MouseButtonEventArgs e)
		{
			var item = (string)((Label)sender).Tag;
			logic.DocumenPartLabel_MouseUp(item);
		}


		private void MenuItemHelp_Click(object sender, RoutedEventArgs e)
		{
			logic.MenuItemHelp_Click();
		}

		private void MenuItemHelp_DoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.MenuItemHelp_DoubleClick();
		}

		private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			logic.Label_MouseDoubleClick();
		}

		private void Label_MouseUp(object sender, MouseButtonEventArgs e)
		{
			logic.Label_MouseUp();
		}

		private void Browser_OnLoadCompleted(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			var browser = sender as WebBrowser;
			logic.Browser_OnLoadCompleted(browser);
		}

		private void Button_Click_BrowseLog(object sender, RoutedEventArgs e)
		{
			logic.Button_Click_BrowseLog();
		}

		private void Button_Click_Compile(object sender, RoutedEventArgs e)
		{
			logic.Button_Click_Compile();
		}

		private void Button_Click_Project(object sender, RoutedEventArgs e)
		{
			logic.Button_Click_Project();
		}

		private void Button_Click_Template(object sender, RoutedEventArgs e)
		{
			logic.Button_Click_Template();
		}

		private void Button_Click_Data(object sender, RoutedEventArgs e)
		{
			logic.Button_Click_Data();
		}
	}

}