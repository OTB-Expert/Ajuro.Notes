using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace Ajuro.WPF.Base.Model
{
	public class VersionList : INotifyPropertyChanged
	{
		public ObservableCollection<VersionModel> Items { get; set; }
		
		public VersionModel selectedItem { get; set; }
		public VersionModel SelectedItem
		{
			get { return selectedItem; }
			set
			{
				selectedItem = value;
				NotifyPropertyChanged();
			}
		}

		public ICollectionView FileItemsView { get; set; }
		
		public VersionList(IList<VersionModel> items)
		{
			Items = new ObservableCollection<VersionModel>(items);
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

		public void Add(VersionModel currentItem)
		{
			Items.Add(currentItem);
		}

		public void Remove(VersionModel currentItem)
		{
			Items.Remove(currentItem);
		}

		public void Clear()
		{
			Items.Clear();
		}

		/// <summary>
		/// Add a new note
		/// </summary>
		public void NewItem(string BasePath, ref int FileNr, string itemName = null)
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
				SelectedItem = new VersionModel()
				{
					Name = itemName
				};
				Items.Add(SelectedItem);
				MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Add(SelectedItem);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
