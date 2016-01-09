using System;
using System.Net;
using System.Text;
using Microsoft.Build.Utilities;

public class UpdateVersionTask : Task
{
    public override bool Execute()
    {
        try
        {
            InnerExecute();
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e);
        }

        return !Log.HasLoggedErrors;
    }

    private void InnerExecute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR")))
            return;

        var buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");

        var restBase = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

        var request = (HttpWebRequest)WebRequest.Create(restBase + "api/build");
        request.Method = "PUT";

        Log.LogMessage("AppVeyor PUT {0}api/build", restBase);

        var data = string.Format("{{ \"version\": \"1.0+build{0}\" }}", buildNumber);
        Log.LogMessage("AppVeyor Content: {0}", data);

        var bytes = Encoding.UTF8.GetBytes(data);
        request.ContentLength = bytes.Length;
        request.ContentType = "application/json";

        using (var writeStream = request.GetRequestStream())
        {
            writeStream.Write(bytes, 0, bytes.Length);
        }

        using (var response = (HttpWebResponse)request.GetResponse())
        {
            if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
            {
                Log.LogError("Respose Error {0}", response.StatusCode);
            }
        }
    }
}