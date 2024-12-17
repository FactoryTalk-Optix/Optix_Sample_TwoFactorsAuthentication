#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.NetLogic;
using FTOptix.Retentivity;
using FTOptix.Core;
using FTOptix.OPCUAServer;
#endregion

public class LoadAuthKeyLogic : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        selectedUserVariable = LogicObject.GetVariable("SelectedUser");
        selectedUserVariable.VariableChange += SelectedUserVariable_VariableChange;
        LoadMfaKey();
    }

    private void SelectedUserVariable_VariableChange(object sender, VariableChangeEventArgs e)
    {
        LoadMfaKey();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        generateKeyForUser?.Dispose();
        selectedUserVariable.VariableChange -= SelectedUserVariable_VariableChange;
    }

    private void LoadMfaKey()
    {
        selectedUser = InformationModel.Get<UserWithMFA>(LogicObject.GetVariable("SelectedUser").Value);
        Log.Info("LoadAuthKeyLogic", $"Selected user: {selectedUser?.BrowseName}");
        if (selectedUser == null)
        {
            Log.Error("LoadAuthKeyLogic", "User not found");
            return;
        }

        if (string.IsNullOrEmpty(selectedUser.SetupKeyValue))
        {
            Log.Info("LoadAuthKeyLogic", $"User {selectedUser.BrowseName} does not have an auth key yet, generating new key");
            generateKeyForUser = new LongRunningTask(GenerateKeyMethod, LogicObject);
            generateKeyForUser.Start();
        }

        Owner.Get<TextBox>("UniqueKey/ShowAuthKey").Text = selectedUser.SetupKeyValue;
    }

    private void GenerateKeyMethod()
    {
        // Get the NetLogic containing the method to execute
        var myScript = Project.Current.GetObject("NetLogic/OTP_GenerateAndValidate");
        try
        {
            // Launch the validation method
            object[] inputArgs = [selectedUser.NodeId];
            myScript.ExecuteMethod("AddMfaToUser", inputArgs, out object[] _);
            LoadMfaKey();
        }
        catch (Exception e)
        {
            Log.Error("GenerateKeyMethod", e.Message);
        }
    }

    private LongRunningTask generateKeyForUser;
    private UserWithMFA selectedUser;
    private IUAVariable selectedUserVariable;
}
