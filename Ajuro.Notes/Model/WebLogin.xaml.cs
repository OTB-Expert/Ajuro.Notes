using Ajuro.Net.User.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ajuro.Net.Util.Api;

namespace Ajuro.Notes.Model
{
	public delegate void UserChanged(IdentityModel newIdentity);
	///<summary>
	/// Interaction logic for WebLogin.xaml
	///</summary>
	public partial class WebLogin : Window
	{

		public event UserChanged UserChangedEvent;
		IdentityModel FacebookIdentity { get; set; }
		public WebLogin()
		{
			InitializeComponent();
		}

		public void FacebookLogin(IdentityModel facebookIdentity)
		{
			// FBClient.AppId = "1806629292701133";
			// FBClient.AppSecret = "0af6b63b5fdcf055b2d77b646ab76e2a";

			FacebookIdentity = facebookIdentity;
			// string link = "https://www.facebook.com/v2.11/dialog/oauth?&response_type=token&display=popup&client_id=1806629292701133&display=popup&redirect_uri=http://jobit.io/back_to_jobit.html&scope=email";
			// LoginWebBrowser.Navigate("https://graph.facebook.com/oauth/authorize?client_id=370849350146422&redirect_uri=https://www.facebook.com/connect/login_success.html&type=user_agent&display=popup");
			LoginWebBrowser.Navigate("https://graph.facebook.com/oauth/authorize?client_id=1806629292701133&redirect_uri=https://www.facebook.com/connect/login_success.html&type=user_agent&display=popup");
		}

		private void LoginWebBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
		{
			string response = e.Uri.ToString();
			if (response.Contains("www.facebook.com/connect/login_success.html"))
			{
				Secure secureApi = new Secure();
				response = response.Split('&')[0];
				string token = response.Substring(response.IndexOf("access_token=") + "access_token=".Length);
				var jwt = secureApi.GetJwt(token);
				if (jwt.Id == 2)
				{
					jwt.Id = 7;
				}
				FacebookIdentity.Id = jwt.Id;
				Ajuro.Net.User.Facebook.Manager.SetIdentity(FacebookIdentity, token);
				if (UserChangedEvent != null)
				{
					UserChangedEvent(FacebookIdentity);
				}
				this.Close();
			}
			else
			{
				var o = 0;
			}
		}
	}
}