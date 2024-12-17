#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.HMIProject;
using FTOptix.UI;
using FTOptix.Retentivity;
using FTOptix.NativeUI;
using FTOptix.Core;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using Google.Authenticator;
using System.IO;
using static System.Net.Mime.MediaTypeNames;
using System.Web;
using FTOptix.OPCUAServer;
using static System.Runtime.CompilerServices.RuntimeHelpers;
#endregion

public class OTP_GenerateAndValidate : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
    }

    [ExportMethod]
    public void ValidateCodeToLed(NodeId selectedUser, string code, NodeId resultLed) {
        // Get the selected user
        var user = GetUserFromNodeId(selectedUser);
        // Validate input code ad return result as bool
        ValidateCode(user.AccountSecretKey, code, out bool result);
        // Set result to LED
        var led = InformationModel.Get<FTOptix.UI.Led>(resultLed);
        if (result)
        {
            Log.Info($"ValidateCode", "Code is valid");
            user.OneTimeKeyValidated = true;
            led.Color = Colors.Lime;
        }
        else
        {
            Log.Warning($"ValidateCode", "Code is invalid");
            led.Color = Colors.Red;
        }
    }

    [ExportMethod]
    public void ValidateCode(string accountSecretKey, string code, out bool result)
    {
        // Validate input code ad return result as bool
        result = tfa.ValidateTwoFactorPIN(accountSecretKey, code, new TimeSpan(0, 1, 0));
    }

    [ExportMethod]
    public static void ResetMfaToUser(NodeId selectedUser)
    {
        // Get the user we are interacting with
        var user = GetUserFromNodeId(selectedUser);
        // Reset user properties
        user.AccountSecretKey = "";
        user.SetupKeyImage = "";
        user.SetupKeyValue = "";
        user.OneTimeKeyValidated = false;
    }

    [ExportMethod]
    public void AddMfaToUser(NodeId selectedUser)
    {
        // Get the user we are interacting with
        var user = GetUserFromNodeId(selectedUser);
        // Generate QR image
        user.AccountSecretKey = GenerateSecureRandomKey(20);
        SetupCode setupInfo = tfa.GenerateSetupCode("FTOptix MFA", user.BrowseName, user.AccountSecretKey, false);
        // Store image and manual setup script
        user.SetupKeyImage = setupInfo.QrCodeSetupImageUrl;
        user.SetupKeyValue = setupInfo.ManualEntryKey;
        // Log path of the output images
        Log.Info("AddMfaToUser", "User properties were generated");
    }

    private static string GenerateSecureRandomKey(int keySize)
    {
        byte[] key = new byte[keySize];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(key);
        }
        return Convert.ToBase64String(key);
    }

    private static UserWithMFA GetUserFromNodeId(NodeId userNodeId)
    {
        var user = InformationModel.Get<UserWithMFA>(userNodeId);
        if (user == null)
        {
            Log.Error("AddMfaToUser", "User not found");
            throw new ArgumentException("Cannot find such user");
        }
        return user;
    }

    private readonly TwoFactorAuthenticator tfa = new TwoFactorAuthenticator(HashType.SHA256);
}
