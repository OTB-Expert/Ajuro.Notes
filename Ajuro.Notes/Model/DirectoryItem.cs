using System.Collections.Generic;

namespace Ajuro.Notes.Model
{
	public class DirectoryItem : MultiFileDocument
	{
		public List<MultiFileDocument> Items { get; set; }

		public DirectoryItem()
		{
			Items = new List<MultiFileDocument>();
		}
	}
}
