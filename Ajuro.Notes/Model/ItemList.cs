using MemoDrops.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace MemoDrops.Model
{
	public class ItemList
	{
		public ObservableCollection<FileItem> Items { get; set; }
		public FileItem CurrentItem { get; set; }
		public ICollectionView FileItemsView { get; set; }

		public ItemList(IList<FileItem> items)
		{
			Items = new ObservableCollection<FileItem>(items);
			InitialiseViews();
		}

		private void InitialiseViews()
		{
			InitialiseFileItemsView();
		}

		private void InitialiseFileItemsView()
		{
			FileItemsView = CollectionViewSource.GetDefaultView(Items);
			FileItemsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
		}

		public bool OrderByName(string criteria)
		{
			FileItemsView = CollectionViewSource.GetDefaultView(Items);
			FileItemsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

			SortDescription oldCriteria = new SortDescription();
			foreach(var existentCriteria in FileItemsView.SortDescriptions)
			{
				if(existentCriteria.PropertyName == criteria)
				{
					oldCriteria = existentCriteria;
				}
			}
			if (!string.IsNullOrEmpty(oldCriteria.PropertyName))
			{
				// Revers sorting
				oldCriteria = new SortDescription(criteria, oldCriteria.Direction == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending);
				FileItemsView.SortDescriptions.Clear();
				FileItemsView.SortDescriptions.Add(oldCriteria);
			}
			else
			{
				// Ascending by default
				FileItemsView.SortDescriptions.Clear();
				oldCriteria = new SortDescription(criteria, ListSortDirection.Ascending);
				FileItemsView.SortDescriptions.Add(oldCriteria);
			}
			return oldCriteria.Direction == ListSortDirection.Ascending;
		}

		internal void Add(FileItem currentItem)
		{
			Items.Add(currentItem);
		}

		internal void Remove(FileItem currentItem)
		{
			Items.Remove(currentItem);
		}

		internal void Clear()
		{
			Items.Clear();
		}

		/// <summary>
		/// Add a new note
		/// </summary>
		public void NewItem(string BasePath, ref int FileNr, ref RichTextBox ResourceContent, string itemName = null)
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
				CurrentItem = new FileItem()
				{
					Name = itemName,
					Key = Guid.NewGuid().ToString(),
					Label = "!!"
				};
				Items.Add(CurrentItem);
				MainWindow.AllItems.Items.Add(CurrentItem);
				ResourceContent.Document = new FlowDocument();
			}
		}
	}
}
