#region Using directives
using System;
using FTOptix.CoreBase;
using FTOptix.HMIProject;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.NetLogic;
using FTOptix.Core;
using FTOptix.UI;
using FTOptix.OPCUAServer;
using System.Linq;
#endregion

public class LoginButtonLogic : BaseNetLogic
{
    public override void Start()
    {
        ComboBox comboBox = Owner.Owner.Get<ComboBox>("Username");
        if (Project.Current.Authentication.AuthenticationMode == AuthenticationMode.ModelOnly)
        {
            comboBox.Mode = ComboBoxMode.Normal;
        }
        else
        {
            comboBox.Mode = ComboBoxMode.Editable;
        }
    }

    public override void Stop()
    {

    }

    [ExportMethod]
    public void PerformLogin(string username, string password, string code)
    {
        var usersAlias = LogicObject.GetAlias("Users");
        if (usersAlias == null || usersAlias.NodeId == NodeId.Empty)
        {
            Log.Error("LoginButtonLogic", "Missing Users alias");
            return;
        }

        if (LogicObject.GetAlias("PasswordExpiredDialogType") is not DialogType passwordExpiredDialogType)
        {
            Log.Error("LoginButtonLogic", "Missing PasswordExpiredDialogType alias");
            return;
        }

        Button loginButton = (Button)Owner;
        loginButton.Enabled = false;

        try
        {
            var outputMessageLabel = Owner.Owner.GetObject("LoginFormOutputMessage");
            var outputMessageLogic = outputMessageLabel.GetObject("LoginFormOutputMessageLogic");

            // Get the NetLogic containing the method to execute
            var myScript = Project.Current.GetObject("NetLogic/OTP_GenerateAndValidate");
            try
            {
                // Get the user to validate
                var userToValidate = usersAlias.Children.OfType<UserWithMFA>().FirstOrDefault(x => x.BrowseName == username);
                if (userToValidate == null)
                {
                    Log.Error("LoginButtonLogic", "User not found");
                    return;
                }
                // Launch the validation method
                object[] inputArgs = [userToValidate.AccountSecretKey, code];
                myScript.ExecuteMethod("ValidateCode", inputArgs, out object[] outputArguments);
                if (outputArguments.Length == 0 || !(bool) outputArguments[0])
                {
                    throw new ArgumentException("Invalid OTP code");
                }
            }
            catch (Exception e)
            {
                outputMessageLogic.ExecuteMethod("SetOutputMessage", [ChangeUserResultCode.WrongPassword]);
                Log.Error("LoginButtonLogic", e.Message);
                loginButton.Enabled = true;
                return;
            }

            // Call the login method to the session
            var loginResult = Session.Login(username, password);
            if (loginResult.ResultCode == ChangeUserResultCode.PasswordExpired)
            {
                loginButton.Enabled = true;
                var user = usersAlias.Get<User>(username);
                var ownerButton = (Button)Owner;
                ownerButton.OpenDialog(passwordExpiredDialogType, user.NodeId);
                return;
            }
            else if (loginResult.ResultCode != ChangeUserResultCode.Success)
            {
                loginButton.Enabled = true;
                Log.Error("LoginButtonLogic", "Authentication failed");
            }

            if (loginResult.ResultCode != ChangeUserResultCode.Success)
            {
                outputMessageLogic.ExecuteMethod("SetOutputMessage", [(int)loginResult.ResultCode]);
            }
        }
        catch (Exception e)
        {
            Log.Error("LoginButtonLogic", e.Message);
        }

        loginButton.Enabled = true;
    }
}
