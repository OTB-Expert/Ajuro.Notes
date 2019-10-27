using Ajuro.WPF.Base.DataAccess;
using Ajuro.WPF.Base.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.WPF.Base.Logic
{
	public class MultyFileDocumentManager
	{
		public MultyFileDocumentManager()
		{
			FileDataAccess = new FileDataAccess();
		}

		private FileDataAccess FileDataAccess { get; set; }

		/// <summary>
		/// Where all notes are stored?
		/// </summary>
		public string BasePath = @"C:\\Work\\Resources\\";

		/// <summary>
		/// Use a increment to generate unique file names. Check file existence for while available.
		/// </summary>
		static int FileNr = 0;
		
		/// <summary>
		/// Create a multy-file-document
		/// </summary>
		/// <param name="fileName"></param>
		internal void Create(string fileName = null)
		{
			if(string.IsNullOrEmpty(fileName))
			{
				MainModel.Instance.TemplateItems.NewItem(BasePath, ref FileNr);
			}
			else
			{
				MainModel.Instance.TemplateItems.NewItem(BasePath, ref FileNr, fileName);
			}
		}

		internal string GetPathByKey(string key)
		{
			return BasePath + key;
		}

		internal void CreateVersion(string versionName)
		{
			MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.NewItem(BasePath, ref FileNr, versionName);

		}

		internal void CreateFile(string fileName)
		{
			MainModel.Instance.TemplateItems.SelectedItem.AllProjectItems.NewItem(BasePath, ref FileNr, fileName);
		}


		internal void DeleteVersion(VersionModel item)
		{
			MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Remove(item);
			SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
			return;

			var itemsCount = MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items.Count;
			for (var i = 0; i < itemsCount; i++)
			{
				if (MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items[i].Name == item.Name)
				{
					MainModel.Instance.TemplateItems.SelectedItem.AllVersionItems.Items.RemoveAt(i);
					itemsCount--;
					i--;
				}
			}
		}

		internal void DeleteProject(ProjectModel item)
		{
			MainModel.Instance.TemplateItems.SelectedItem.AllProjectItems.Items.Remove(item);
			SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
		}

		internal void DeleteMultyFile(MultiFileDocument item)
		{
			MainModel.Instance.TemplateItems.Remove(item);
			SaveItem(MainModel.Instance.TemplateItems.SelectedItem);
		}

		internal void SaveItem(MultiFileDocument item)
		{
			if (MainModel.Instance.TemplateItems.SelectedItem.Key == null)
			{
				MainModel.Instance.TemplateItems.SelectedItem.Key = Guid.NewGuid().ToString();
			}
			File.WriteAllText(GetPathByKey(MainModel.Instance.TemplateItems.SelectedItem.Key), MainModel.Instance.ResourceContentDocument);
			MainModel.Instance.TemplateItems.SelectedItem.Name = MainModel.Instance.Resource_Name_Text;
			MainModel.Instance.TemplateItems.SelectedItem.LastUpdated = DateTime.UtcNow;
			FileDataAccess.SaveItemMeta(BasePath, MainModel.Instance.TemplateItems.SelectedItem);
			/*SelectedItem.Label = "";
			SelectedItem.Name = Resource_Name.Text;
			if (SelectedItem.Name.ToLower() != SelectedItem.NameOriginal.ToLower())
			{
				if (File.Exists(BasePath + Resource_Name.Text))
				{
					File.Delete(BasePath + SelectedItem.NameOriginal);
				}
			}*/
			MainModel.Instance.TemplateItems.SelectedItem.Status = 0;
		}

		internal List<MultiFileDocument> GetItems(string path)
		{
			return FileDataAccess.GetItems(path);
		}

		internal void SaveItemMeta(MultiFileDocument selectedItem)
		{
			FileDataAccess.SaveItemMeta(BasePath, MainModel.Instance.TemplateItems.SelectedItem);
		}
	}
}
