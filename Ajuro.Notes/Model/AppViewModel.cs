﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Ajuro.Net.Util.Helpers;
using Ajuro.Net.Util.Models;
using Ajuro.Net.Util.Resource;
using Ajuro.Notes.Commands;

namespace Ajuro.Notes.Model
{
	public class AppViewModel
	{
		// public static string Host = "http://localhost:5000/"; // "http://alfa.drops.jobit.io/
		public static string Host = "http://alfa.drops.jobit.io/";

		public AppCommands AppCommands { get; set; }

		public UserPresenterViewModel UserPresenterViewModel { get; set; }

		public AjuroTemplate SelectedTemplate = null;

		public AppViewModel()
		{
			Ajuro.Net.Util.Managers.FileSystemManager.Initialize();
			AppCommands = new AppCommands();

			UserPresenterViewModel = new UserPresenterViewModel();

			UserPresenterViewModel.UserChangedEvent += UserPresenterViewModel_UserChangedEvent;


			var uri = (Host + "template/list");
			return;
		}

		private void UserPresenterViewModel_UserChangedEvent(Ajuro.Net.User.Model.IdentityModel newIdentity)
		{
			if (newIdentity == null)
			{
			}
			else
			{
				UserPresenterViewModel.FacebookIdentity = newIdentity;
			}
		}	               

	}
}