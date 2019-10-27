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
	public class VersionList : BaseList, INotifyPropertyChanged
	{
		public VersionList(ObservableCollection<VersionModel> items)
		{
			var collectionItems = new ObservableCollection<BaseModel>();
			foreach (var collectionItem in items)
			{
				collectionItems.Add((BaseModel)collectionItem);
			}
			Add(collectionItems);
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
				// Items.Add(SelectedItem);
				MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Add(SelectedItem);
			}
		}
	}
}
