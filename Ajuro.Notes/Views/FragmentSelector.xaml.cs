using Ajuro.Net.Template.Processor;
using Ajuro.Notes.View;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Ajuro.Notes.Views
{
	/// <summary>
	/// Interaktionslogik für FragmentSelector.xaml
	/// </summary>
	public partial class FragmentSelector : Window
	{
		TemplaterInstruction TemplaterInstruction { get; set; }

		Random random = new Random();
		bool SuspendedSelectionTrigger = false;
		FragmentSelectorViewModel vm { get; set; }
		CodeFragment lastFragment { get; set; }
		CodeFragment parentFragment { get; set; }
		public string BasePath { get; internal set; }

		public FragmentSelector()
		{
			InitializeComponent();
			CustomInitialize();
			DataContext = vm;
		}

		private void CustomInitialize()
		{
			this.WindowState = WindowState.Maximized;
			vm = new FragmentSelectorViewModel();
			vm.OriginalCode = File.ReadAllText("Resources\\Template\\InputExample.txt");
			Closing += FragmentSelector_Closing;
			ResetRoot();
		}

		public void SelectOrCreateJson(CodeFragment currentFragment)
		{
			List<CodeFragment> breadcrumbs = new List<CodeFragment>();
			while (currentFragment.Parent != null)
			{
				breadcrumbs.Insert(0, currentFragment);
				currentFragment = currentFragment.Parent;
			}

			object jsonFragment = vm.JsonSample;
			if (jsonFragment == null)
			{
				jsonFragment = new JObject();
				vm.JsonSample = jsonFragment;
			}

			foreach (CodeFragment fragment in breadcrumbs)
			{
				switch (fragment.Type)
				{
					case 0:
						if (!((JObject)jsonFragment).ContainsKey(fragment.Name))
						{
							((JObject)jsonFragment).Add(new JProperty(fragment.Name, new JArray()));
						}
						jsonFragment = ((JObject)jsonFragment)[fragment.Name];
						break;
					case 1:
						if (fragment.Parent != null && fragment.Parent.Type == 0)
						{
							if (((JArray)jsonFragment).Count == 0)
							{
								((JArray)jsonFragment).Add(new JObject());
							}
							var i = 0;
							foreach (var item in ((JArray)jsonFragment).Children())
							{
								i++;
								if (!((JObject)item).ContainsKey(fragment.Name))
								{
									((JObject)item).Add(new JProperty(fragment.Name, fragment.Name + "_" + i));
								}
							}
						}
						jsonFragment = ((JArray)jsonFragment)[0];
						jsonFragment = ((JObject)jsonFragment)[fragment.Name];
						break;
				}
			}
		}


		private void FragmentSelector_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{

			if (!File.Exists(BasePath + TemplaterInstruction.TempData))
			{
				var h = File.Create(BasePath + TemplaterInstruction.TempData);
				h.Close();
			}

			// JsonConvert.SerializeObject(vm.JsonSample, Formatting.Indented)

			System.Windows.Documents.TextPointer myTextPointer1 = RichTextBoxCodeFragment.Document.ContentStart;
			System.Windows.Documents.TextPointer myTextPointer2 = RichTextBoxCodeFragment.Document.ContentEnd;
			TextRange range = new TextRange(myTextPointer1, myTextPointer2);
			vm.JsonSampleString = range.Text;
			File.WriteAllText(BasePath + TemplaterInstruction.TempData, vm.JsonSampleString);

			if (!File.Exists(BasePath + TemplaterInstruction.TempCode))
			{
				var h = File.Create(BasePath + TemplaterInstruction.TempCode);
				h.Close();
			}

			SuspendedSelectionTrigger = true;
				OriginalCodeRichTextBox.SelectAll();
				OriginalCodeRichTextBox.Selection.ClearAllProperties();
				var txt = OriginalCodeRichTextBox.Selection.Text;
			if(txt.Length < 10)
			{
				return;
			}
				File.WriteAllText(BasePath + TemplaterInstruction.TempCode, txt);
			
			if (!File.Exists(BasePath + TemplaterInstruction.TempJson))
			{
				var h = File.Create(BasePath + TemplaterInstruction.TempJson);
				h.Close();
			}
			File.WriteAllText(BasePath + TemplaterInstruction.TempJson, JsonConvert.SerializeObject(vm.RootCodeFragment, Formatting.Indented));

		}

		private void ResetRoot()
		{
			vm.RootCodeFragment = new CodeFragment() { Name = "Root", SelectionStart = 0, SelectionLength = vm.OriginalCode.Length, Fragments = new ObservableCollection<CodeFragment>() };
			vm.CurrentFragment = vm.RootCodeFragment;
			System.Windows.Documents.TextPointer myTextPointer1 = OriginalCodeRichTextBox.Document.ContentStart;
			System.Windows.Documents.TextPointer myTextPointer2 = OriginalCodeRichTextBox.Document.ContentEnd;
			TextRange range = new TextRange(myTextPointer1, myTextPointer2);
			vm.RootCodeFragment.Content = range.Text;
			parentFragment = vm.RootCodeFragment;
			vm.StructureJson = JsonConvert.SerializeObject(vm.RootCodeFragment, Formatting.Indented);
		}

		int i = 0;
		private void OrigonalRichTextBox_SelectionChanged(object sender, RoutedEventArgs e)
		{
			if (SuspendedSelectionTrigger)
			{
				return;
			}
			int SelectionStart = OriginalCodeRichTextBox.Document.ContentStart.GetOffsetToPosition(OriginalCodeRichTextBox.Selection.Start);
			int SelectionLength = OriginalCodeRichTextBox.Selection.Text.Length;
			if (SelectionLength > 0)
			{
				if (SelectionStart - 1 != vm.CurrentFragment.SelectionStart && SelectionStart + 1 != vm.CurrentFragment.SelectionStart && SelectionStart != vm.CurrentFragment.SelectionStart && SelectionStart + SelectionLength != vm.CurrentFragment.SelectionStart + vm.CurrentFragment.SelectionLength)
				{
					vm.CurrentFragment = new CodeFragment() { Name = "TN_" + i, SelectionStart = SelectionStart, SelectionLength = SelectionLength, Parent = GetInnermostFragment(vm.RootCodeFragment, SelectionStart, SelectionLength), Fragments = new ObservableCollection<CodeFragment>() };
					vm.CurrentFragment.Parent.Fragments.Add(vm.CurrentFragment);
					i++;
					vm.CurrentFragment.Color = new int[] { random.Next(55) + 200, random.Next(55) + 200, random.Next(55) + 200 };
					SelectionTolerance = true;
				}
				else
				{
					vm.CurrentFragment.SelectionStart = SelectionStart;
					vm.CurrentFragment.SelectionLength = SelectionLength;
				}
				vm.CurrentFragment.Content = OriginalCodeRichTextBox.Selection.Text;
			}
			UpdateFromSelection(vm.CurrentFragment, lastFragment);
			foreach (var item in FragmentsTreeView.Items)
			{
				TreeViewItem treeItem = FragmentsTreeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
				// ExpandAll(treeItem, false);
			}


			foreach (object o in FragmentsTreeView.ItemsSource)
			{
				object container = FragmentsTreeView.ItemContainerGenerator.ContainerFromItem(o);
				TreeViewItem item = container as TreeViewItem;
				if (item != null)
				{
					item.IsExpanded = true;
					OpenMulticolumnItems(item);
				}
			}

			vm.StructureJson = JsonConvert.SerializeObject(vm.RootCodeFragment, Formatting.Indented);
			ColorizeText(vm.RootCodeFragment);
			OrderFragments(vm.RootCodeFragment);
		}

		public void OrderFragments(CodeFragment root)
		{
			if (root.Fragments != null)
			{
				for (int i = 0; i < root.Fragments.Count - 2; i++)
				{
					for (int j = i + 1; j < root.Fragments.Count - 1; j++)
					{
						if (root.Fragments[i].SelectionStart > root.Fragments[j].SelectionStart)
						{
							var transport = root.Fragments[i].SelectionStart;
							root.Fragments[i].SelectionStart = root.Fragments[j].SelectionStart;
							root.Fragments[j].SelectionStart = transport;
						}
					}
				}
			}
		}

		private void OpenMulticolumnItems(TreeViewItem item)
		{
			foreach (object o in item.ItemsSource)
			{
				object container = item.ItemContainerGenerator.ContainerFromItem(o);
				TreeViewItem child = container as TreeViewItem;
				if (child != null)
				{
					child.IsExpanded = true;
					OpenMulticolumnItems(child);
				}
			}
		}

		public void ExpandAll(TreeViewItem treeViewItem, bool isExpanded = true)
		{
			try
			{
				if(treeViewItem != null && treeViewItem.Items != null)
				foreach (var child in treeViewItem.Items)
				{
					var childContainer = child as TreeViewItem;
					if (childContainer == null)
					{
						childContainer = treeViewItem.ItemContainerGenerator.ContainerFromItem(child) as TreeViewItem;
					}
					if (childContainer != null)
					{
						ExpandAll(childContainer, true);
					}
				}
				treeViewItem.IsExpanded = isExpanded;
			}
			catch (Exception) { }
		}


		public void UpdateFromSelection(CodeFragment CurrentFragment, CodeFragment LastFragment)
		{
			// Detect when children are uncovered
			var ChildrenCount = CurrentFragment.Fragments != null ? CurrentFragment.Fragments.Count : 0; // Not sure if number of children is reevaluated in a for loop, just to make sure I can force the for loop to be aware that the number of children might change.
			for (int i = 0; i < ChildrenCount; i++) // I am not using foreach because of a foreach iterration can not be changed inside the foreach.
			{
				var Child = CurrentFragment.Fragments[i]; // Consider each child...
				var Sel = CurrentFragment; // A alias for CurrentFragment just to simplify notations.
										   // ChildrenCount--; // Because there is one less child;

				if (Sel.SelectionStart > Child.SelectionStart || Sel.SelectionStart + Sel.SelectionLength < Child.SelectionStart + Child.SelectionLength) // Detect if the child escaped from under the parent. We might need to also check if the child is partially covered, this check is not needed in case of progressive selection adjustment as the child is uncovered ine character at a time.
				{
					i--;
					ChildrenCount--;
					Sel.Fragments.Remove(Child); // Make the child a orphan. Not needed in this case but a good practice when a child can not belong to multiple collections (treeView? not sure)
					Child.Parent = Sel.Parent; // Re-asign the orphane to the structure as a sibling to the curent selection.
					Sel.Parent.Fragments.Add(Child);
					if (Sel.SelectionStart > Child.SelectionStart) // In case of uncovering the left interval margin, the left margin of CurrentFragment is not allowed to partially overlap it's sibling (former child).
					{
						Sel.SelectionStart = Child.SelectionStart + Child.SelectionLength; // If selection is complete, this will be be sufficient to make sure the new interval does not partially cover it's sibling
																						   // If selection continues to be adjusted. A check against each siblings has to be reevaluated because with each adjustment the SelectionStart will be set to richTextBoxParentCode.SelectionStart;
					}
					if (Sel.SelectionStart + Sel.SelectionLength < Child.SelectionStart + Child.SelectionLength) // In case of uncovering the right interval margin, the right margin of CurrentFragment is not allowed to partially overlap it's sibling (former child).
					{
						Sel.SelectionStart = Child.SelectionStart + Child.SelectionLength; // If selection is complete, this will be be sufficient to make sure the new interval does not partially cover it's sibling
																						   // If selection continues to be adjusted. A check against each siblings has to be reevaluated because with each adjustment the SelectionLength will be set to richTextBoxParentCode.SelectionLength;
					}
					// Above algorithm should be tested for selections that progress to the left and to the right of their starting point or adjustment that jmps over their starting point.

					// Check for duplicates
					if (Sel.SelectionStart > Child.SelectionStart && Sel.SelectionStart + Sel.SelectionLength < Child.SelectionStart + Child.SelectionLength)
					{
						// TODO: Only drop this child if detected after selection completed, so can not be dropped here, should be  before CurrentFragment.Fragments.Add(newFragment);
					}
				}
			}

			// Detect if siblings are fully covered
			var SiblingsCount = CurrentFragment.Parent != null && CurrentFragment.Parent.Fragments != null ? CurrentFragment.Parent.Fragments.Count : 0;
			for (int i = 0; i < SiblingsCount; i++)
			{
				var Sib = CurrentFragment.Parent.Fragments[i]; // Consider each child...
				var Sel = CurrentFragment; // A alias for CurrentFragment just to simplify notations.		
				if (Sel == Sib)
				{
					continue; // Make sure we don't test the current interval against itself
				}

				if (Sel.SelectionStart <= Sib.SelectionStart && Sel.SelectionStart + Sel.SelectionLength >= Sib.SelectionStart + Sib.SelectionLength) // Detect if a sibling gets fully covered
				{
					// While adjusting the selection, there might be cases when for a short time the selection has exact same limits as a existing sibling. In this case the sibling become a child. But it will be dropped if the selection is completed because no duplicates are allowed.

					CurrentFragment.Parent.Fragments.Remove(Sib); // Remove from siblings
					SiblingsCount--;
					Sib.Parent = Sel; // Re-asign the orphane to the structure as a sibling to the curent selection.
					if (Sel.Fragments == null)
					{
						Sel.Fragments = new System.Collections.ObjectModel.ObservableCollection<CodeFragment>();
					}
					Sel.Fragments.Add(Sib); // Re-asign the orphane to the structure as a child to the curent selection.
				}
			}
		}

		private void StructureTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{

		}

		private CodeFragment GetInnermostFragment(CodeFragment root, int selectionStart, int SelectionLength)
		{
			foreach (var fragment in root.Fragments)
			{
				if (fragment.Fragments != null)
				{
					if (fragment.SelectionStart <= selectionStart && fragment.SelectionStart + fragment.SelectionLength >= selectionStart + SelectionLength)
					{
						return GetInnermostFragment(fragment, selectionStart, SelectionLength);
					}
				}
			}
			return root;
		}

		public void RestoreParents(CodeFragment codeFragment)
		{
			if (codeFragment.Fragments != null)
			{
				foreach (var fragment in codeFragment.Fragments)
				{
					fragment.Parent = codeFragment;
					RestoreParents(fragment);
				}
			}
		}

		private void ColorizeText(CodeFragment currentFragment)
		{
			if (currentFragment.Color != null)
			{
				SuspendedSelectionTrigger = true;
				// OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)currentFragment.Color[0], (byte)currentFragment.Color[1], (byte)currentFragment.Color[2])));
				SuspendedSelectionTrigger = false;
				// OriginalCodeRichTextBox
			}
			if (currentFragment.Fragments != null)
			{
				foreach (var fragment in currentFragment.Fragments)
				{
					ColorizeText(fragment);
				}
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			ResetRoot();
		}

		bool IsMouseDown = false;
		bool SelectionTolerance = false;

		private void OrigonalRichTextBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			IsMouseDown = true;
			SelectionTolerance = false;
		}

		private void OrigonalRichTextBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			IsMouseDown = false;
		}

		private void OrigonalRichTextBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			IsMouseDown = true;
			SelectionTolerance = false;
		}

		private void OrigonalRichTextBox_TouchDown(object sender, System.Windows.Input.TouchEventArgs e)
		{
			IsMouseDown = true;
			SelectionTolerance = false;
		}

		private void OrigonalRichTextBox_TouchUp(object sender, System.Windows.Input.TouchEventArgs e)
		{
			IsMouseDown = false;
		}

		private void OrigonalRichTextBox_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
		{

		}

		private void OrigonalRichTextBox_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
		{

		}

		int lastSelectionIndex = 0;

		private void FragmentsTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
		{
			try
			{
				// CurrentFragment = (CodeFragment)FragmentsTreeView.SelectedItem;
				if (FragmentsTreeView.SelectedItem != null)
				{
					vm.CurrentFragment = (CodeFragment)FragmentsTreeView.SelectedItem;
					UpdateFragmentType();
					// vm.Selection = vm.OriginalCode.Substring(vm.CurrentFragment.SelectionStart, vm.CurrentFragment.SelectionLength);


					//if (vm.LastFragment != null && vm.LastFragment != vm.CurrentFragment)
					{
						//ColorizeText(vm.LastFragment);
					}
					if (lastFragment == null) lastFragment = vm.CurrentFragment;
					int offset = 0;
					int offsetEnd = 0;
					// if()
					{

					}
					if (lastFragment.SelectionStart < vm.CurrentFragment.SelectionStart) offset += 2;
					if (lastFragment.SelectionStart < vm.CurrentFragment.SelectionStart && lastFragment.SelectionStart + lastFragment.SelectionLength < vm.CurrentFragment.SelectionStart + vm.CurrentFragment.SelectionLength) offset += 2;
					if (lastFragment.SelectionStart > vm.CurrentFragment.SelectionStart && lastFragment.SelectionStart + lastFragment.SelectionLength < vm.CurrentFragment.SelectionStart + vm.CurrentFragment.SelectionLength) offsetEnd += 4;

					System.Windows.Documents.TextPointer myTextPointer1 = OriginalCodeRichTextBox.Document.ContentStart.GetPositionAtOffset(vm.CurrentFragment.SelectionStart + offset);

					System.Windows.Documents.TextPointer myTextPointer2 = OriginalCodeRichTextBox.Document.ContentStart.GetPositionAtOffset(vm.CurrentFragment.SelectionStart + vm.CurrentFragment.SelectionLength + offset + offsetEnd);
					TextRange range = new TextRange(myTextPointer1, myTextPointer2);
					vm.Selection = range.Text;

					lastSelectionIndex = vm.CurrentFragment.SelectionStart;

					SuspendedSelectionTrigger = true;
					try
					{
						OriginalCodeRichTextBox.SelectAll();
						OriginalCodeRichTextBox.Selection.ClearAllProperties();
						// vm.CodeFragment = vm.CurrentFragment.Content;
						OriginalCodeRichTextBox.SelectionBrush = Brushes.Red;
						// OriginalCodeRichTextBox.Selection.ApplyPropertyValue();
						OriginalCodeRichTextBox.Selection.Select(myTextPointer1, myTextPointer2);
						OriginalCodeRichTextBox.Selection.ApplyPropertyValue(System.Windows.Documents.TextElement.BackgroundProperty, new SolidColorBrush(Color.FromRgb((byte)(vm.CurrentFragment.Color[0] - 50), (byte)(vm.CurrentFragment.Color[1] - 50), (byte)(vm.CurrentFragment.Color[2] - 50))));
						OriginalCodeRichTextBox.SelectionBrush = Brushes.Green;
					}
					catch (Exception)
					{
					}
					SuspendedSelectionTrigger = false;
					lastFragment = vm.CurrentFragment;
					/*	vm.FragmentName = vm.CurrentFragment.Name;
						vm.SelectionStart = vm.CurrentFragment.SelectionStart;
						vm.SelectionLength = vm.CurrentFragment.SelectionLength;*/
					try
					{
						SelectOrCreateJson(vm.CurrentFragment);
					}
					catch (Exception) { }
					// vm.JsonString = JsonConvert.SerializeObject(Marker.JsonRoot, Formatting.Indented);
				}
				OrderFragments(vm.RootCodeFragment);
			}
			catch { }
		}

		internal void LoadTemplateFromPath(string executionPlan)
		{
			TemplaterInstruction = JsonConvert.DeserializeObject<TemplaterInstruction>(executionPlan.Replace("\\", "\\\\"));
			if (File.Exists(BasePath + TemplaterInstruction.TempCode))
			{
				vm.OriginalCode = File.ReadAllText(BasePath + TemplaterInstruction.TempCode);
			}
			if (File.Exists(BasePath + TemplaterInstruction.TempData))
			{
				vm.JsonSampleString = File.ReadAllText(BasePath + TemplaterInstruction.TempData);
			}

			if (File.Exists(BasePath + TemplaterInstruction.TempJson))
			{
				vm.StructureJson = File.ReadAllText(BasePath + TemplaterInstruction.TempJson);
				vm.RootCodeFragment = JsonConvert.DeserializeObject<CodeFragment>(vm.StructureJson);
				RestoreParents(vm.RootCodeFragment);
			}
			foreach (var item in FragmentsTreeView.Items)
			{
				TreeViewItem treeItem = FragmentsTreeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
				ExpandAll(treeItem, false);
			}
		}

		private void FragmentContentTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			vm.CurrentFragment.Content = FragmentContentTextBox.Text;
			vm.StructureJson = JsonConvert.SerializeObject(vm.RootCodeFragment, Formatting.Indented);
		}

		private void FragmentNameTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			vm.CurrentFragment.Name = FragmentNameTextBox.Text.Trim();
		}

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
			ColorizeText(vm.RootCodeFragment);
		}

		private void SelectionFromTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			// Cover/Uncover
			UpdateFromSelection(vm.CurrentFragment, lastFragment);
		}

		private void SelectionToTextBox_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			// Cover/Uncover
			UpdateFromSelection(vm.CurrentFragment, lastFragment);
		}

		private void RichTextBoxCodeFragment_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if(e.Key != System.Windows.Input.Key.F5)
			{
				return;
			}

			System.Windows.Documents.TextPointer myTextPointerS = OriginalCodeRichTextBox.Document.ContentStart;
			System.Windows.Documents.TextPointer myTextPointerE = OriginalCodeRichTextBox.Document.ContentEnd;
			TextRange rangeR = new TextRange(myTextPointerS, myTextPointerE);
			vm.RootCodeFragment.Content = rangeR.Text;

			string inflated = Inflate(vm.RootCodeFragment, JsonConvert.DeserializeObject(vm.JsonSampleString));

			try
			{
				webBrowserPreview.Refresh();
			}
			catch (Exception) { }

			webBrowserPreview.NavigateToString("<html><style>.fragment_marker { color: green; display: none; } .repeat_fragment { border: 1 solid magenta; margin: 2px; } .replace_fragment { border: 1 solid blue; margin: 2px; }</style>" + inflated + "</html>");
			return;

			Ajuro.Net.Template.Processor.TemplateMarker Marker = new Ajuro.Net.Template.Processor.TemplateMarker();

			System.Windows.Documents.TextPointer myTextPointer1 = OriginalCodeRichTextBox.Document.ContentStart;
			System.Windows.Documents.TextPointer myTextPointer2 = OriginalCodeRichTextBox.Document.ContentEnd;
			TextRange range = new TextRange(myTextPointer1, myTextPointer2);
			vm.RootCodeFragment.Content = range.Text;

			string[] data = Marker.ProcessTemplate(vm.RootCodeFragment);
			TemplateProcessor TemplateProcessor = new TemplateProcessor();
			var result0 = TemplateProcessor.UpdateTemplate(vm.JsonSampleString, data[0]);
			// NewItem(templaterInstruction.Ready, result);
			// TemplateInterpreter.InterpretProcessedTemplate(templaterInstruction.Project, MainModel.Instance.SelectedProfile.Properties, result0);
			// richTextBoxInflatedText = "<pre>" + result0 + "</pre>";
			
			webBrowserPreview.Refresh();
			webBrowserPreview.NavigateToString("<html><style>.fragment_marker { color: green; display: none; } .repeat_fragment { border: 1 solid magenta; margin: 2px; } .replace_fragment { border: 1 solid blue; margin: 2px; }</style>" + data[1] + "</html>");
			// richTextBox1Text = data[0];
		}

		private string Inflate(CodeFragment rootCodeFragment, object v)
		{
			string inflated = rootCodeFragment.Content;
			List<List<object>> Fragments = new List<List<object>>();
			if (v is JObject)
			{
				var currentObject = v as JObject;
				for (int i = rootCodeFragment.Fragments.Count - 1; i > -1; i--)
				{
					var childFragment = rootCodeFragment.Fragments[i];
					foreach (var property in currentObject.Children())
					{
						var currentProperty = property as JProperty;
						List<List<string>> stringValues = new List<List<string>>();
						if (childFragment.Name.Equals(currentProperty.Name))
						{
							var currentValue = currentObject.GetValue(currentProperty.Name);
							string text = Inflate(childFragment, currentValue);
							rootCodeFragment.FormattedContent = rootCodeFragment.Content.Substring(0, childFragment.SelectionStart - rootCodeFragment.SelectionStart) + text + rootCodeFragment.Content.Substring(childFragment.SelectionStart - rootCodeFragment.SelectionStart + childFragment.SelectionLength - 2);
						}
					}
					// Fragments.Add(new List<object>() {  });
				}
			}
			else
			if (v is JArray)
			{
				var currentArray = v as JArray;
				string itemsString = string.Empty;
				foreach (var item in currentArray)
				{
					if (item is JObject)
					{
						var currentObjectItem = item as JObject;
						string text = Inflate(rootCodeFragment, currentObjectItem);
					}
				}
			}
			else
			if (v is JProperty)
			{
				foreach (var items in ((JProperty)v).Children())
				{

				}
			}
			if(v is JValue)
			{
				var currentValue = v as JValue;
				return currentValue.ToString();
			}
			return inflated;
		}

		private void RadioButtonRepeat_Checked(object sender, RoutedEventArgs e)
		{
			vm.CurrentFragment.Type = 0;
			UpdateFragmentType();
		}

		private void UpdateFragmentType()
		{
			RadioButtonRepeat.IsChecked = vm.CurrentFragment.Type == 0;
			RadioButtonReplace.IsChecked = vm.CurrentFragment.Type == 1;
		}

		private void RadioButtonReplace_Checked(object sender, RoutedEventArgs e)
		{
			vm.CurrentFragment.Type = 1;
			UpdateFragmentType();
		}
	}
}