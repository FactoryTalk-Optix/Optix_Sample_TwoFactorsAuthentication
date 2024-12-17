# Two Factor Authentication

How to add a second authentication step to Optix users. This project was tested with both Google Authenticator and Microsoft Authenticator.

## Dependencies

1. NuGet package `GoogleAuthenticator` by [Brandon Potter](https://github.com/BrandonPotter/GoogleAuthenticator) (tested version 3.2.0)

## Usage

1. In the `UsersEditor` page, users can be manipulated and a 2FA pin can be added
2. Per each user, click the `QR` button and setup the account on the mobile device by scanning the QR code with an authenticator app
3. Validate the generated pin in the `Test MFA` field, a green light should come up
4. Move to `UserLogin` page
5. Login with the selected account, the one time pin will be validated in addition to the password set on the user (by default, `User1` has no password)

### Disclaimer

Rockwell Automation maintains these repositories as a convenience to you and other users. Although Rockwell Automation reserves the right at any time and for any reason to refuse access to edit or remove content from this Repository, you acknowledge and agree to accept sole responsibility and liability for any Repository content posted, transmitted, downloaded, or used by you. Rockwell Automation has no obligation to monitor or update Repository content

The examples provided are to be used as a reference for building your own application and should not be used in production as-is. It is recommended to adapt the example for the purpose, observing the highest safety standards.
