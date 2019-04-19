using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MemoDrops
{
	public class DataAccess
	{
		// Curent user info
		public static UserAccount Me { get; set; }

		// public static string UrlBase = "https://ajuro.azurewebsites.net/api/";
		public static string UrlBase = "https://localhost:44351/api/";
	}
}
