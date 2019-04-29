using MemoDrops.Model;
using MemoDrops.View;
using System.Windows;

namespace MemoDrops.Views
{
	/// <summary>
	/// Interaction logic for NoteMetaEditorWindow.xaml
	/// </summary>
	public partial class NoteMetaEditorWindow : Window
	{
		FileItem Item { get; set; }

		public NoteMetaEditorWindow()
		{
			InitializeComponent();
		}

		internal void LinkData(FileItem item)
		{
			Item = item;
		}
	}
}
