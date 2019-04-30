using MemoDrops.Model;
using MemoDrops.View;
using System.Diagnostics;
using System.Windows;

namespace MemoDrops.Views
{
	/// <summary>
	/// Interaction logic for NoteMetaEditorWindow.xaml
	/// </summary>
	public partial class NoteMetaEditorWindow : Window
	{
		FileItem Item { get; set; }
		ResourceFolder SelectedResourceFolder{get;set ;}

		public NoteMetaEditorWindow()
		{
			InitializeComponent();
		}

		internal void LinkData(FileItem item, ResourceFolder selectedResourceFolder)
		{
			Item = item;
			SelectedResourceFolder = selectedResourceFolder;
			RepoNameTextBox.Text = selectedResourceFolder.Name;
			FileNameTextBox.Text = item.Key;
			MetaTextBox.Text = item.Key + ".meta";
		}

		private void RepoName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Process.Start(SelectedResourceFolder.Path);
		}

		private void FileName_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Process.Start(SelectedResourceFolder.Path + "\\" + Item.Key);
		}

		private void Meta_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			Process.Start(SelectedResourceFolder.Path + "\\" + Item.Key + ".meta");
		}
	}
}
