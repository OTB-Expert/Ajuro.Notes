using System.ComponentModel;
using System.Windows.Input;
using Ajuro.Net.User.Model;
using Ajuro.Net.User;
using MemoDrops.Commands;

namespace MemoDrops.Model
{
	public class UserPresenterViewModel : INotifyPropertyChanged
	{
		public event UserChanged UserChangedEvent;
		Ajuro.Net.User.Manager UserManager = new Manager();

		// Google identity;
		private IdentityModel googleIdentity { get; set; }

		// Google identity;
		private IdentityModel facebookIdentity { get; set; }

		/// <summary>
		/// Login with Google command
		/// </summary>
		public ICommand ConnectGoogleCommand { get; set; }

		/// <summary>
		/// Login with Facebook command
		/// </summary>
		public ICommand ConnectFacebookCommand { get; set; }

		/// <summary>
		/// Login with Facebook command
		/// </summary>
		public ICommand EditInvitationCommand { get; set; }
		public UserPresenterViewModel()
		{
			ChatInMessage = "Ckick here to login with Facebook!";
			ChatInImage = "Resources/Images/Business Objects/BOCustomer_32x32.png";

			GoogleIdentity = new IdentityModel() { Picture = "http://otb.expert/img/male.png", ButtonText = "Login with Google", Provider = KnownIdentityProvider.Google };
			FacebookIdentity = new IdentityModel() { Picture = "http://otb.expert/img/male.png", ButtonText = "Login with Facebook", Provider = KnownIdentityProvider.Facebook };

			UserManager.GoogleIdentity = GoogleIdentity;
			ConnectGoogleCommand = new ManageAccounts(ExecuteConnectGoogleMethod, canExecute, GoogleIdentity);

			UserManager.FacebookIdentity = FacebookIdentity;
			ConnectFacebookCommand = new ManageAccounts(ExecuteConnectFacebookMethod, canExecute, FacebookIdentity);

			UserManager.UserChangedEvent += UserManager_UserChangedEvent;
		}

		private void UserManager_UserChangedEvent(IdentityModel newIdentity)
		{
			if (UserChangedEvent != null)
			{

				ChatInMessage = newIdentity.FirstName;
				ChatInImage = newIdentity.Picture;
				UserChangedEvent(newIdentity);
			}
		}

		/// <summary>
		/// Google user profile.
		/// </summary>
		public IdentityModel GoogleIdentity
		{
			get
			{
				return googleIdentity;
			}
			set
			{
				googleIdentity = value;
				OnPropertyChanged("GoogleIdentity");
			}
		}

		/// <summary>
		/// Google user profile.
		/// </summary>
		public IdentityModel FacebookIdentity
		{
			get
			{
				return facebookIdentity;
			}
			set
			{
				facebookIdentity = value;
				OnPropertyChanged("FacebookIdentity");
			}
		}

		public string chatInMessage { get; set; }

		/// <summary>
		/// Message displayed before login.
		/// </summary>
		public string ChatInMessage
		{
			get
			{
				return chatInMessage;
			}
			set
			{
				chatInMessage = value;
				OnPropertyChanged("ChatInMessage");
			}
		}

		public string chatInImage { get; set; }

		/// <summary>
		/// Message displayed before login.
		/// </summary>
		public string ChatInImage
		{
			get
			{
				return chatInImage;
			}
			set
			{
				chatInImage = value;
				OnPropertyChanged("ChatInImage");
			}
		}

		private bool canExecute(object parameter)
		{
			return true;
		}

		private void ExecuteConnectGoogleMethod(IdentityModel googleIdentity)
		{
			if (googleIdentity.ProviderId == null)
			{
				UserManager.LoginWithGoogle(googleIdentity);
			}
			else
			{
				UserManager.DisconnectGoogle(googleIdentity);
			}
		}

		private void ExecuteConnectFacebookMethod(IdentityModel facebookIdentity)
		{
			if (facebookIdentity.ProviderId == null)
			{
				ChatInMessage = "Authenticationg...";
				ChatInImage = "";
				facebookIdentity.FirstName = "Loading...";
				WebLogin WebLogin = new WebLogin();
				WebLogin.UserChangedEvent += UserManager_UserChangedEvent;
				WebLogin.Show();
				WebLogin.FacebookLogin(facebookIdentity);
			}
			else
			{
				Ajuro.Net.User.Facebook.Manager.Disconnect(facebookIdentity);
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnPropertyChanged(string property)
		{
			PropertyChangedEventHandler propertyChangedEventHandler = PropertyChanged;
			if (propertyChangedEventHandler != null)
			{
				propertyChangedEventHandler(this, new PropertyChangedEventArgs(property));
			}
		}
	}
}

