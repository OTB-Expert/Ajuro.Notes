using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops.Model
{
	public class DirectoryItem : FileItem
	{
		public List<FileItem> Items { get; set; }

		public DirectoryItem()
		{
			Items = new List<FileItem>();
		}
	}
}
