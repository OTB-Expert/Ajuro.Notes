using MemoDrops.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops.DataAccess
{

	public class ItemProvider
	{
		public List<FileItem> GetItems(string path)
		{
			var items = new List<FileItem>();

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
				var item = new FileItem
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
