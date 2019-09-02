using Ajuro.WPF.Base.Model;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace Ajuro.Notes.Views
{
	/// <summary>
	/// Interaction logic for NoteMetaEditorWindow.xaml
	/// </summary>
	public partial class NoteMetaEditorWindow : Window
	{
		MultiFileDocument Item { get; set; }
		ResourceFolder SelectedResourceFolder{get;set ;}

		public NoteMetaEditorWindow()
		{
			InitializeComponent();
		}

		internal void LinkData(MultiFileDocument item, ResourceFolder selectedResourceFolder)
		{
			if (item != null)
			{
				Item = item;
				SelectedResourceFolder = selectedResourceFolder;
				RepoNameTextBox.Text = selectedResourceFolder.Name;
				FileNameTextBox.Text = item.Key;
				MetaTextBox.Text = item.Key + ".meta";
			}
		}

		private void RepoName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (File.Exists(SelectedResourceFolder.Path + "\\" + Item.Key))
			{
				Process.Start(SelectedResourceFolder.Path);
			}
		}

		private void FileName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (File.Exists(SelectedResourceFolder.Path + "\\" + Item.Key))
			{
				Process.Start(SelectedResourceFolder.Path + "\\" + Item.Key);
			}
		}

		private void Meta_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Process.Start(SelectedResourceFolder.Path + "\\" + Item.Key + ".meta");
		}
	}
}
