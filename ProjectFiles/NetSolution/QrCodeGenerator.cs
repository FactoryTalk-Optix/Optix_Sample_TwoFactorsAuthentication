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
using System.ComponentModel;
using System.Web;
using System.IO;
using FTOptix.OPCUAServer;
#endregion

public class QrCodeGenerator : BaseNetLogic
{
    public override void Start()
    {
        // Insert code to be executed when the user-defined logic is started
        renderImage = new LongRunningTask(RenderImageMethod, LogicObject);
        renderImage.Start();
    }

    public override void Stop()
    {
        // Insert code to be executed when the user-defined logic is stopped
        renderImage?.Dispose();
    }

    private void RenderImageMethod()
    {
        // Get the user we are interacting with
        var user = InformationModel.Get<UserWithMFA>(Owner.GetVariable("QrCodeText").Value);
        if (user == null)
        {
            Log.Error("RenderImageMethod", "User not found");
            return;
        }
        // Get path to PNG image
        var prjUri = new Uri(new Uri(System.IO.Directory.GetCurrentDirectory()), Project.Current.ProjectDirectory);
        var imgUri = new Uri(prjUri, $"{Guid.NewGuid().ToString().Replace("-", "").AsSpan(0, 10)}.png");
        string filePath = imgUri.AbsolutePath;
        filePath = HttpUtility.UrlDecode(filePath);
        Log.Debug("File path: " + filePath);
        // Write BASE64 image to file
        string base64Image = user.SetupKeyImage.Split(",")[user.SetupKeyImage.Split(",").Length - 1];
        File.WriteAllBytes(filePath, Convert.FromBase64String(base64Image));
        // Update image file
        Owner.Get<FTOptix.UI.Image>("QrCodeImage").Path = filePath;
    }

    private LongRunningTask renderImage;
}
