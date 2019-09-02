using Ajuro.WPF.Base.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ajuro.WPF.Base.DataAccess
{
	public class FileDataAccess
	{
		/// <summary>
		/// Saving a simplified structure
		/// </summary>
		/// <param name="basePath">The path to phisical file. This is defined by the current repository.</param>
		/// <param name="item">The file item to save. It is the current item of the main model.</param>
		public void SaveItemMeta(string basePath, MultiFileDocument item)
		{
			File.WriteAllText(basePath + item.Key + ".meta", JsonConvert.SerializeObject(
			   new NoteEntity()
			   {
				   Tags = item.Tags,
				   Files = item.Files,
				   // Author = SelectedChannel,
				   RowKey = item.Key,
				   Synced = item.Synced,
				   Content = string.Empty,
				   Title = item.Name
			   }));
		}

		/// <summary>
		/// Collect file items
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public List<MultiFileDocument> GetItems(string path)
		{
			var items = new List<MultiFileDocument>();

			var dirInfo = new DirectoryInfo(path);

			foreach (var directory in dirInfo.GetDirectories())
			{
				var item = new DirectoryItem
				{
					Name = directory.Name,
					Path = directory.FullName,
					Items = GetItems(directory.FullName)
				};

				items.Add(item);
			}

			foreach (var file in dirInfo.GetFiles())
			{
				var item = new MultiFileDocument
				{
					Name = file.Name,
					Path = file.FullName
				};

				items.Add(item);
			}

			return items;
		}
	}
}
