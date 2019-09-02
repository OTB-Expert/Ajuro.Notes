using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Ajuro.Net.Template.Processor;
using Ajuro.WPF.Base.Model;

namespace Ajuro.Notes.View
{
	/// <summary>
	/// Interaction logic for AccountWindow.xaml
	/// </summary>
	public partial class TemplateEditorWindow : Window
	{
		TemplateEditorModel VM { get; set; }
		
		string RichTextBoxCodeStructurePreviewText { get; set; }
		string richTextBox1Text { get; set; }
		string richTextBoxInflatedText { get; set; }

		// Holds the controls of each property
		List<PropertyControl> PropertyControls = new List<PropertyControl>();


		// Save selection starting point
		private int SelectionStart { get; set; }

		// Save selection length
		private int SelectionLength { get; set; }
		bool SuspendedSelectionTrigger = false;
		CodeFragment CurrentNode { get; set; }

		Ajuro.Net.Template.Processor.TemplateMarker Marker = new Ajuro.Net.Template.Processor.TemplateMarker();

		public TemplateEditorWindow()
		{
			VM = new TemplateEditorModel();
			VM.RootCodeFragment = new CodeFragment();
			VM.RootCodeFragment.Name = "Root";
			VM.RootCodeFragment.Fragments = new System.Collections.ObjectModel.ObservableCollection<CodeFragment>();
			VM.CurrentFragment = VM.RootCodeFragment;
			InitializeComponent();
			WireUpEvents();
			CustomInitialize();
			DataContext = VM;
		}

		private void WireUpEvents()
		{
			Closing += TemplateEditorWindow_Closing;
			JsonEditorRichTextBox.KeyUp += JsonEditorRichTextBox_KeyUp;
			JsonEditorRichTextBox.TextChanged += JsonEditorRichTextBox_TextChanged;
			FragmentsTreeView.KeyUp += FragmentsTreeView_KeyUp;
			FragmentNameTextBox.KeyUp += FragmentNameTextBox_KeyUp;
			OriginalCodeRichTextBox.MouseUp += RichTextBoxParentCode_MouseUp;
			FragmentsTreeView.SelectedItemChanged += FragmentsTreeView_SelectedItemChanged;
			RichTextBoxCodeFragment.KeyUp += RichTextBoxChildCode_KeyUp;
			// ButtonReset.Click += ButtonReset_Click;
			// buttonBack.Click += buttonBack_Click;
			// buttonSave.Click += buttonSave_Click;
			OriginalCodeRichTextBox.SelectionChanged += RichTextBoxOriginalCode_SelectionChanged;
			ValueExampleTextBox.TextChanged += NewItemNameTextBox_TextChanged;
			ValueExampleTextBox.TextChanged += NewItemValueTextBox_TextChanged;
		}

		public void LoadTemplateFromPath(string templatePath, string jsonPath = null)
		{
			if (File.Exists(templatePath))
			{
				VM.OriginalCode = File.ReadAllText(templatePath);
			}
			if (File.Exists(jsonPath))
			{
				VM.JsonString = File.ReadAllText(jsonPath);
			}
		}

		
		private void NewItemValueTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			KeyValuePair<string, string> keyValue = (KeyValuePair<string, string>)textBox.Tag;
			keyValue = new KeyValuePair<string, string>(keyValue.Key, textBox.Text);
		}

		private void NewItemNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			TextBox textBox = (TextBox)sender;
			KeyValuePair<string, string> keyValue = (KeyValuePair<string, string>)textBox.Tag;
			keyValue = new KeyValuePair<string, string>(textBox.Text, keyValue.Value);
		}
		
		private void CustomInitialize()
		{
			VM.RealTimeUpdates = true;
			if (File.Exists("Resources\\Template\\BackupStructure.txt"))
			{
				VM.RootCodeFragment = JsonConvert.DeserializeObject<CodeFragment>(File.ReadAllText("Resources\\Template\\BackupStructure.txt"));
				Marker.OrderFragments(VM.RootCodeFragment);
			}
			if (File.Exists("Resources\\Template\\JsonObject.json"))
			{
				Marker.JsonRoot = JsonConvert.DeserializeObject(File.ReadAllText("Resources\\Template\\JsonObject.json"));
				VM.JsonString = JsonConvert.SerializeObject(Marker.JsonRoot, Formatting.Indented);
			}

			Marker.RestoreParents(VM.RootCodeFragment);

			VM.OriginalCode = File.ReadAllText("Resources\\Template\\InputExample.txt");
			VM.OriginalCode = VM.OriginalCode.Replace("\r\n", "\n");
			RichTextBoxCodeStructurePreviewText = JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented);
			VM.CurrentFragment = VM.RootCodeFragment;
			VM.CurrentFragment.Content = VM.OriginalCode;
			PreviewHTML();
			// UpdateTree(VM.RootCodeFragment.Fragments, FragmentsTreeView.Items);

			foreach (var item in FragmentsTreeView.Items)
			{
				var tvi = item as TreeViewItem;
				if (tvi != null)
					tvi.ExpandSubtree();
			}
			
			ColorizeText(VM.RootCodeFragment);
			//// richTextBoxParentCode.Select(0, 0);
			this.Closing += TemplateEditorWindow_Closing; ;
		}

		private void ColorizeText(CodeFragment currentFragment)
		{
			if (currentFragment.Color != null)
			{
				SuspendedSelectionTrigger = true;
				//// richTextBoxParentCode.SelectionStart = currentFragment.SelectionStart;
				//// richTextBoxParentCode.SelectionLength = currentFragment.SelectionLength;
				OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)currentFragment.Color[0], (byte)currentFragment.Color[1], (byte)currentFragment.Color[2])));
				SuspendedSelectionTrigger = false;
			}
			if (currentFragment.Fragments != null)
			{
				foreach (var fragment in currentFragment.Fragments)
				{
					ColorizeText(fragment);
				}
			}
		}
		
		//
		private void TemplateEditorWindow_Closing(object sender, CancelEventArgs e)
		{
			File.WriteAllText("Resources\\Template\\BackupStructure.txt", JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented));
			if (VM.JsonString != null)
			{
				Marker.JsonRoot = JsonConvert.DeserializeObject(VM.JsonString);
				File.WriteAllText("Resources\\Template\\JsonObject.json", JsonConvert.SerializeObject(Marker.JsonRoot, Formatting.Indented));
			}
		}
		
		private string Escape(string content)
		{
			return content.Replace("<", "&lt;").Replace("\n", "<br>").Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;").Replace(" ", "&nbsp;");
		}

		//
		private void RichTextBoxOriginalCode_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (SuspendedSelectionTrigger)
			{
				return;
			}
			// User selected a fragment
			VM.CodeFragment = OriginalCodeRichTextBox.Selection.Text;
			SelectionStart = OriginalCodeRichTextBox.Document.ContentStart.GetOffsetToPosition(OriginalCodeRichTextBox.Selection.Start);
			VM.SelectionStart = SelectionStart;
			SelectionLength = OriginalCodeRichTextBox.Selection.Text.Length;
			VM.SelectionLength = SelectionLength;

			//// richTextBox1Text = OriginalCodeRichTextBox.Substring(0, SelectionLength) + "\r\n----------------------------------------\r\n" + richTextBoxParentCode.con.Substring(SelectionLength);
			//// richTextBox1Text = richTextBox1Text.Replace("\r\n", "\n");



			if (SelectionLength > 0 && Marker.LastFragmentStart != SelectionStart && Marker.LastFragmentEnd != SelectionStart + SelectionLength)
			{
				if (OriginalCodeRichTextBox.Selection.Text.Trim().Length > 0)
				{
					CodeFragment cf = VM.LastFragment;
					Marker.UpdateFromSelection(VM.RootCodeFragment, VM.CurrentFragment, ref cf, SelectionStart, SelectionLength);
					VM.LastFragment = cf;
					if (VM.LastFragment != null)
					{
						OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)VM.LastFragment.Color[0], (byte)VM.LastFragment.Color[1], (byte)VM.LastFragment.Color[2])));
					}
					RichTextBoxCodeStructurePreviewText = JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented);
				}

				Marker.Sequence[(int)Marker.LastFragmentType]++;
			}
				

			if (VM.CurrentFragment != VM.RootCodeFragment && (OriginalCodeRichTextBox.Selection.Text.Length) > 0) // Makes no sense to compute no selection and in case of shrinking, a minimum duplicate selection would be 1 in lenght, sa for length 0 there is nothing to consider.
			{
				Marker.UpdateFromSelection2(VM.CurrentFragment, VM.LastFragment);
			}

			if (SelectionLength > 0)
			{
				Marker.LastFragmentStart = SelectionStart;
				Marker.LastFragmentEnd = SelectionStart + SelectionLength;
			}

			// It there are spaces in selection, most probably you want a repeat fragment.
			if (OriginalCodeRichTextBox.Selection.Text.Trim().IndexOf(' ') < 0 && OriginalCodeRichTextBox.Selection.Text.Trim().IndexOf('\t') < 0 && OriginalCodeRichTextBox.Selection.Text.Trim().IndexOf('\n') < 0)
			{
				if (SelectionLength > 0)
				{
					Marker.LastFragmentType = TemplateMarker.MarkerType.Replace;
				}
				RadioButtonReplace.IsChecked = true;
				VM.FragmentName = "Property_" + Marker.Sequence[(int)TemplateMarker.MarkerType.Replace];
			}
			else
			{
				if (SelectionLength > 0)
				{
					Marker.LastFragmentType = TemplateMarker.MarkerType.Repeat;
				}
				RadioButtonRepeat.IsChecked = true;
				VM.FragmentName = "ItemsArray_" + Marker.Sequence[(int)TemplateMarker.MarkerType.Repeat];
			}
			if (VM.RealTimeUpdates && OriginalCodeRichTextBox.Selection.Text.Trim().Length > 0)
			{
				string fragmentName = VM.FragmentName.Trim();
				int fragmentType = (bool)RadioButtonRepeat.IsChecked ? 0 : 1; // This is not nice. I should not use UI here.

				// OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)222, (byte)100, (byte)250)));

				VM.CurrentFragment.Name = fragmentName;
				VM.CurrentFragment.Type = fragmentType;
				VM.CurrentFragment.SelectionStart = SelectionStart;
				VM.CurrentFragment.SelectionLength = SelectionLength;
				VM.CurrentFragment.Content = VM.CodeFragment;
				RichTextBoxCodeStructurePreviewText = JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented);
				PreviewHTML();
			}
			// FragmentsTreeView.Items.Clear();
			if (VM.RootCodeFragment.Fragments != null)
			{
				// UpdateTree(VM.RootCodeFragment.Fragments, FragmentsTreeView.Items);

				foreach (var item in FragmentsTreeView.Items)
				{
					var tvi = item as TreeViewItem;
					if (tvi != null)
						tvi.ExpandSubtree();
				}
			}
		}

		private void traverse_tree_inside(CodeFragment current, ref CodeFragment newFragment)
		{
			return;
			bool found_a_fit = false;
			if (current.Fragments != null)
			{
				//loop through childrem
				for (var i = 0; i < current.Fragments.Count; i++)
				{
					//if the selection fits, check if the child has more children where it could fit as a child
					if (current.Fragments[i].SelectionStart <= SelectionStart && current.Fragments[i].SelectionStart + current.Fragments[i].SelectionLength >= SelectionStart + SelectionLength && !(current.Fragments[i].SelectionStart == SelectionStart && current.Fragments[i].SelectionStart + current.Fragments[i].SelectionLength == SelectionStart))
					{
						found_a_fit = true;
						CodeFragment newCurrent = current.Fragments[i];
						traverse_tree_inside(newCurrent, ref newFragment);
					}

				}
				if (found_a_fit == false)
				//if it didn't find a child that could take the new selection as a child
				{
					current.Fragments.Add(newFragment);
					newFragment.Parent = current;
				}
			}
			else //if the child doesn't have children
			{
				current.Fragments = new System.Collections.ObjectModel.ObservableCollection<CodeFragment>();
				current.Fragments.Add(newFragment);
				newFragment.Parent = current;
			}
		}

		private void UpdateTree(List<CodeFragment> fragments, ItemCollection nodes)
		{
			foreach (var fragment in fragments)
			{
				var treeNodeItem = new TreeViewItem();
				treeNodeItem.Header = fragment.Name;
				treeNodeItem.Tag = fragment;
				nodes.Add(treeNodeItem);
				if (fragment.Fragments != null)
				{
					// UpdateTree(fragment.Fragments, treeNodeItem.Items);
				}
			}

			//me code, should change it:
			//TreeNode node = new TreeNode(CurrentFragment.Name);
			//node.Tag = CurrentFragment;
			//node.Name = CurrentFragment.Name;

			//Boolean added = false;

			//TreeNodeCollection nodes = treeView1.Nodes;
			//foreach (TreeNode n in nodes)
			//{
			//    var selectionStart = ((CodeFragment)node.Tag).SelectionStart;
			//    var selectionEnd = ((CodeFragment)node.Tag).SelectionStart + ((CodeFragment)node.Tag).SelectionLength;
			//    if ( (selectionStart < ((CodeFragment)n.Tag).SelectionStart) && (selectionEnd < (((CodeFragment)n.Tag).SelectionStart + ((CodeFragment)n.Tag).SelectionLength)) )
			//    {
			//        n.Nodes.Add(CurrentFragment.Content);
			//        added = true;
			//    }

			//}

			//if (added == false)
			//{
			//    treeView1.Nodes.Add(node);
			//    node.Nodes.Add(((CodeFragment)node.Tag).Content);
			//}


		}

		//
		private void buttonSave_Click(object sender, EventArgs e)
		{
			string fragmentName = VM.FragmentName.Trim();
			int fragmentType = (bool)RadioButtonRepeat.IsChecked ? 0 : 1;
			if (string.IsNullOrEmpty(fragmentName))
			{
				MessageBox.Show("Please define both name and type!");
			}
			CodeFragment newFragment = new CodeFragment()
			{
				Name = fragmentName,
				Type = fragmentType,
				SelectionStart = SelectionStart,
				SelectionLength = SelectionLength,
				Content = VM.CodeFragment
			};
			if (VM.CurrentFragment.Fragments == null)
			{
				VM.CurrentFragment.Fragments = new System.Collections.ObjectModel.ObservableCollection<CodeFragment>();
			}
			VM.CurrentFragment.Fragments.Add(newFragment);

			File.WriteAllText("Resources\\Template\\BackupStructure.txt", JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented));

			VM.OriginalCode = VM.CodeFragment;
			VM.CodeFragment = string.Empty;
			RichTextBoxCodeStructurePreviewText = JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented);
			PreviewHTML();
			newFragment.Parent = VM.CurrentFragment;
			VM.CurrentFragment = newFragment;
		}

		private void PreviewHTML()
		{
			//// webBrowserPreview.Refresh();
			//// webBrowserPreview.DocumentText = "<html><style>.fragment_marker { color: green; display: none; } .repeat_fragment { border: 1 solid magenta; margin: 2px; } .replace_fragment { border: 1 solid blue; margin: 2px; }</style>" + Marker.ToHtmlString(VM.RootCodeFragment) + "</html>";
		}

		//
		private void buttonBack_Click(object sender, EventArgs e)
		{
			if (VM.CurrentFragment.Parent != null)
			{
				VM.CurrentFragment = VM.CurrentFragment.Parent;
				AdoptFragment();
			}
		}

		private void AdoptFragment()
		{
			VM.OriginalCode = VM.CurrentFragment.Content;
		}

		//
		private void ButtonReset_Click(object sender, EventArgs e)
		{
			VM.RootCodeFragment = new CodeFragment();
			VM.OriginalCode = File.ReadAllText("Resources\\Template\\InputExample.txt");
			VM.OriginalCode = VM.OriginalCode.Replace("\r\n", "\n");
			VM.CurrentFragment = VM.RootCodeFragment;
			VM.CurrentFragment.Content = VM.OriginalCode;
			RichTextBoxCodeStructurePreviewText = JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented);
			PreviewHTML();
		}

		//
		private void RichTextBoxChildCode_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (VM.RealTimeUpdates)
			{
				VM.CurrentFragment.Content = VM.CodeFragment;
				RichTextBoxCodeStructurePreviewText = JsonConvert.SerializeObject(VM.RootCodeFragment, Formatting.Indented);
				PreviewHTML();
			}
		}

		//
		private void FragmentsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			try
			{
				CurrentNode = (CodeFragment)FragmentsTreeView.SelectedItem;
				if (FragmentsTreeView.SelectedItem != null)
				{
					VM.CurrentFragment = (CodeFragment)FragmentsTreeView.SelectedItem;
					if (VM.LastFragment != null && VM.LastFragment != VM.CurrentFragment)
					{
						ColorizeText(VM.LastFragment);
					}

					VM.CodeFragment = VM.CurrentFragment.Content;
					SuspendedSelectionTrigger = true;

					System.Windows.Documents.TextPointer myTextPointer1 = OriginalCodeRichTextBox.Document.ContentStart.GetPositionAtOffset(VM.CurrentFragment.SelectionStart);
					System.Windows.Documents.TextPointer myTextPointer2 = OriginalCodeRichTextBox.Document.ContentStart.GetPositionAtOffset(VM.CurrentFragment.SelectionStart + VM.CurrentFragment.SelectionLength);
					OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)(VM.CurrentFragment.Color[0] - 50), (byte)(VM.CurrentFragment.Color[1] - 50), (byte)(VM.CurrentFragment.Color[2] - 50))));
					OriginalCodeRichTextBox.Selection.Select(myTextPointer1, myTextPointer2);
					SuspendedSelectionTrigger = false;
					VM.LastFragment = VM.CurrentFragment;
					VM.FragmentName = VM.CurrentFragment.Name;
					VM.SelectionStart = VM.CurrentFragment.SelectionStart;
					VM.SelectionLength = VM.CurrentFragment.SelectionLength;
					try
					{
						Marker.SelectOrCreateJson(VM.CurrentFragment);
					}
					catch (Exception) { }
					VM.JsonString = JsonConvert.SerializeObject(Marker.JsonRoot, Formatting.Indented);
				}
			}
			catch { }
		}

		//
		private void RichTextBoxParentCode_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (VM.LastFragment != null)
			{
				OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)VM.LastFragment.Color[0], (byte)VM.LastFragment.Color[1], (byte)VM.LastFragment.Color[2])));
			}
		}

		//
		private void FragmentNameTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (CurrentNode != null)
			{
				// CurrentNode.Header = VM.FragmentName .Trim();
				VM.CurrentFragment.Name = VM.FragmentName.Trim();
			}
		}

		//
		private void FragmentsTreeView_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == System.Windows.Input.Key.Delete)
			{
				if (FragmentsTreeView.SelectedItem != null)
				{
					var codeFragment = (CodeFragment)FragmentsTreeView.SelectedItem;
					DeleteFragment(codeFragment);
				}
			}
		}

		private void DeleteFragment(CodeFragment codeFragment)
		{
			if (codeFragment.Fragments != null)
			{
				foreach (var fragment in codeFragment.Fragments)
				{
					fragment.Parent = codeFragment.Parent;
					codeFragment.Parent.Fragments.Add(fragment);
				}
			}
			codeFragment.Parent.Fragments.Remove(codeFragment);

			// FragmentsTreeView.Items.Clear();
			// UpdateTree(VM.RootCodeFragment.Fragments, FragmentsTreeView.Items);
			foreach (var item in FragmentsTreeView.Items)
			{
				var tvi = item as TreeViewItem;
				if (tvi != null)
					tvi.ExpandSubtree();
			}
			ColorizeText(VM.RootCodeFragment);
		}

		//
		private void JsonEditorRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			string[] data = Marker.ProcessTemplate(VM.RootCodeFragment);
			TemplateProcessor TemplateProcessor = new TemplateProcessor();
			var result0 = TemplateProcessor.UpdateTemplate(VM.JsonString, data[0]);
			// NewItem(templaterInstruction.Ready, result);
			// TemplateInterpreter.InterpretProcessedTemplate(templaterInstruction.Project, MainModel.Instance.SelectedProfile.Properties, result0);
			richTextBoxInflatedText = "<pre>" + result0 + "</pre>";

			return;//
			webBrowserPreview.Refresh();
			webBrowserPreview.NavigateToString("<html><style>.fragment_marker { color: green; display: none; } .repeat_fragment { border: 1 solid magenta; margin: 2px; } .replace_fragment { border: 1 solid blue; margin: 2px; }</style>" + data[1] + "</html>");
			richTextBox1Text = data[0];
		}

		//
		private void JsonEditorRichTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			try
			{
				Marker.JsonRoot = JsonConvert.DeserializeObject(VM.JsonString);
				File.WriteAllText("Resources\\Template\\JsonObject.json", JsonConvert.SerializeObject(Marker.JsonRoot, Formatting.Indented));
				Marker.JsonRoot.ToString();
			}
			catch (Exception ex)
			{

			}
		}
	}
}
