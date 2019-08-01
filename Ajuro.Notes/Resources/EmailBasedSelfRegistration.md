## SignUp
Create account if not existent.

#### Request

	{
		"RealName": "FirstName lastName",
		"DisplayName": "OTB Expert",
		"Email": "office@otb.expert",
		"Username": "weexpert",
		"Password": "Password123"
	}

#### Meaning

- RealName - Priavate, used by system to address the user in communication. Real name field is optional.
- DisplayName - Public name of the user. User can change it. Is optional, defaults to permalinkId by default.
- Email - Mandatory, used for account management and system communication.
- Username - Used by user to login. defaults to email address.
- Password - Encripted on clinet syde. No clear text passwords should trawel the network. Password confirmation as second field is only used on client side for local validation.


#### Specific Error Messages

Email address in use.

	{
		"Meta":
		{
			"Success": 0,
			"Code": "E000",
		}
	}

## Email address confirmation

New accounts are ready to use before the email is confirmed.
Without email confirmation upload is not possible.
Confirmation link will contain all information needed to confirm the email address.

> Please confirm your account to activate your cloud storage for uploads.

**MemoDrops - Please confirm your account**
> Welcome to MemoDrops!
Please confirm your email addess.

GET: https://otb.expert/account/confirme/J3UOPS-US3W-W12V-P0UY-PWHEG5/office@otb.expert

## Update Account Details
Logged in users can update their account details. 
In case of email updates make sure the user has acces to the new email.

Email to last confirmed email address:

**MemoDrops - Please confirm your new email**
> Dear user. Your account information where updated successfully except the email address.  Changing emails requires email confirmation. To ensures the continuity of your access to your own account we have to make sure the user has acces to the new email. No change will be effective before the email is confirmed. Once you confirme the new email all our communication will continue on your new email addess.

Email to the new email adress:

**MemoDrops - Please confirm your new email**
> Dear user. Your account information where updated successfully except the email address. 
Changing emails requires email confirmation. To ensures the continuity of your access to your own account we have to make sure the user has acces to the new email. No change will be effective before the email is confirmed. Once you confirme the new email all our communication will continue on your new email addess.

GET: https://otb.expert/account/confirme/J3UOPS-US3W-W12V-P0UY-PWHEG5/new_email@otb.expert  

#### Request

	{
		"RealName": "FirstName lastName",
		"DisplayName": "OTB Expert",
		"Email": "office@otb.expert"
	}

#### Response

	{
		"Meta":
		{
			"Success": 1,
		},
		"Data":
		{
			"DisplayName": "OTB Expert",
			"PermalinkId": "TIH32H",
			"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5"
		}
	}

> Your account details where updated successfully.


## LogIn
Get user info if exists.

#### Request

	{
		"Username": "weexpert",
		"Password": "Password123"
	}

#### Response

	{
		"Meta":
		{
			"Success": 1,
		},
		"Data":
		{
			"DisplayName": "OTB Expert",
			"PermalinkId": "TIH32H",
			"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5"
		}
	}

#### Meaning

- DisplayName - Public name of the user. User can change it.
- PermalinkId - Public Id to identify the user's notes and build permanent linkd.
- UserId - Userid used for by the system. The system can change this to improve security.

#### Specific Error Messages

When account is not found. IP is blocked for 1 hour if 3 login attempts fail in 5 minutes.
User is notified by email about this measure (with a link to reset the password if he wants ?).

|	Wrong username or password.

When system suggests a password change.

|	Please reset your password.

	{
		"Meta":
		{
			"Success": 0,
			"Code": "E001",
		}
	}

Alternative full version might expose too much information and might provide outdated messages making the system unsafe and difficult to maintain:

	
	{
		"Meta":
		{
			"Success": 0,
			"Code": "E001",
			"Message": "Please reset your password."
		},
		"Data":
		{
			"DisplayName": null,
			"PermalinkId": null,
			"UserId": null
		}
	}


## LogOut
Remove local data.

#### Request

	{
		"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5"
	}

#### Response

	{
		"DisplayName": "OTB Expert",
		"PermalinkId": "TIH32H",
		"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5"
	}

The client will continue the logout procedure once the request is sent. The response from the server will not affect the client in any way.

Local storage will be deleted.
By default the local notes will not be deleted. user can choose to sync and delete them at logout.

## Reset Password Request
Send email with temporaru token.

**MemoDrops - Password Recovery**
> Someone requested a password reset for your MemoDrops account. In case this is your intention please use this code to reset your password in the desktop MemoDrops application: **RET45IJD**

	{
		"Email": "office@otb.expert"
	}

A email will be sent to the address with a link/code to reset the password.
The link/code is valid 24 hours.
No more than 5 reset requests can be sent per day. Once sent, all tokens will be accepted.

The effective reset password 

#### Request

	{
		"Token": "RET45IJD",
		"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5",
		"Action": "Reset Password"
	}

#### Response

Same response as for login.

	{
		"DisplayName": "OTB Expert",
		"PermalinkId": "TIH32H",
		"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5"
	}

#### Specific Error Messages

If a old token is used a new token will be automatically sent.

**MemoDrops - Password Recovery Faild**
> Your attempt to reset your password faild because you used a token older than 24 hours. Please use the following token instead: **RET45IJD** 

Using old token for a password reset action. We just sent you a new valid token to your email. Check your email to reset the password.

	{
		"Meta":
		{
			"Success": 0,
			"Code": "E001",
		}
	}

## Delete Account Request

Soft delete for 3 days deletion revert.

**MemoDrops - Account Deletion Requested and Deletion Policy**
> You requested the deletion of your account. To protect your data we suspended your account. Other users can no more search for you or your data. However, any user that downloaded any of your public items or user information can continue to use them on their own account. They can clone and share the data they had already downloaded. All logs where your identity was used will display Anonymous User instead.

- Mark all user data as hidden for other users. 
- Soft delete the account.
- Send email to user with a link to reactivate the account in 7 days.
- Send email to user after 6 days reminding that the account will be irreversible deleted in 24 hours id it is not reactivated.
- Log out user.


## Recover Account Request
Send email with temporary token.

Recover Account email can be requested any time. Last 10 tokens will be active until the deletion. 
Effect
Cloned data and local copies of the user data used by other users will not be affected. Other users can not search for the deleted user of for it's data. 

If user recover the account all data and sharing options will recover their status.

If user does not recover the account all information he deposited in the system will be deleted.
Cloned data and local copies of the user data used by other users will not be affected.  

- Keep account suspended. 
- Sent account recovery code.
- Update deletion timeout to 30 days.

	{
		"Email": "office@otb.expert"
	}

**MemoDrops - Account Recovery Request**
> Someone requested an Account Recovert code for your MemoDrops account. In case this is your intention please use the code in this email.
.
The interval until the irevesible deletion of your account was extended with 30 days because you tried to recover your account.
.

GET: https://otb.expert/account/recovery/J3UOPS-US3W-W12V-P0UY-PWHEG5/RET45IJD

If a inactive token is used a new token is automatically sent.

**MemoDrops - URGENT - Account Recovery Failed - Please read recovery instructions**
> The attempt to recover the account failed due to the use of a old token. A new token was just sent to your email. Please open your last recovery email that we just sent and follow the instructions to recover your account. The interval until the irevesible deletion of your account was extended with 30 days because you tried to recover your account.

GET: https://otb.expert/account/recovery/J3UOPS-US3W-W12V-P0UY-PWHEG5/RET45IJD

## Syetem Information

#### Generic error messages

In case of breaking changes on the API side, for unsupported client versions:

	{
		"Meta":
		{
			"Success": 0,
			"Code": "E002",
			"Message": "Please download a new version of MemoDrops"
		}
	}

In cae of server errors. Email the IT Department.

	{
		"Meta":
		{
			"Success": 0,
			"Code": "E003",
			"Message": "Server error. Our IT department was notified is working on this."
		}
	}

#### Database Structure

	AccountEntity
	{
		"RealName": "FirstName lastName",
		"DisplayName": "OTB Expert",
		"Email": "office@otb.expert",
		"Username": "weexpert",
		"Password": "Password123",
		"PermalinkId": "TIH32H",
		"UserId": "J3UOPS-US3W-W12V-P0UY-PWHEG5",
		"IsSuspended": true,
		"DeletionScheduledTo": "20-04-2019",
		"LastAccess": "10-04-2019"
	}

	Requests
	{	
		"IP": "96.55.11.27",
		"Expiration": "96.55.11.27",
		"Token": "96.55.11.27",
		"Type": "96.55.11.27"
	}
