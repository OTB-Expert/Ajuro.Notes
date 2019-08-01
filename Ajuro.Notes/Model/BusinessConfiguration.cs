using System.Collections.Generic;

namespace Ajuro.Notes.Model
{
	public class BusinessConfiguration
	{
		public List<ResourceFolder> ResourceFolders { get; set; }
	}

	public class ResourceFolder
	{
		public string Name { get; set; }
		public string Path { get; set; }
		public string Type { get; set; }
	}
}
