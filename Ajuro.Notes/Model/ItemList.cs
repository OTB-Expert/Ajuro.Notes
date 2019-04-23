using MemoDrops.View;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

namespace MemoDrops.Model
{
    public class ItemList
    {
        public ItemList(IList<MyItem> items)
        {
            Items = new ObservableCollection<MyItem>(items);
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

        public void orderByName()
        {
            FileItemsView = CollectionViewSource.GetDefaultView(Items);
            FileItemsView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));
            FileItemsView.SortDescriptions.Add(
               new SortDescription("Name", ListSortDirection.Ascending));
            
        }

        public ObservableCollection<MyItem> Items { get; set;  }
        public MyItem CurrentItem { get; set; }
        public ICollectionView FileItemsView { get; set; }


        internal void Add(MyItem currentItem)
        {
            Items.Add(currentItem);
        }

        internal void Remove(MyItem currentItem)
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
                CurrentItem = new MyItem()
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
