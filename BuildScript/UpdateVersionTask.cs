using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

public class UpdateVersionTask : Microsoft.Build.Utilities.Task
{
    public override bool Execute()
    {
        try
        {
            AsyncPump.Run(InnerExecute);
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e);
        }

        return !Log.HasLoggedErrors;
    }

    private async Task InnerExecute()
    {
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPVEYOR")))
            return;

        var buildNumber = Environment.GetEnvironmentVariable("APPVEYOR_BUILD_NUMBER");

        var restBase = Environment.GetEnvironmentVariable("APPVEYOR_API_URL");

        using (var client = new HttpClient())
        {
            Log.LogMessage("AppVeyor PUT {0}api/build", restBase);

            var data = string.Format("{{ \"version\": \"1.0+build{0}\" }}", buildNumber);
            Log.LogMessage("AppVeyor Content: {0}", data);

            using (var response = await client.PutAsync(restBase + "api/build", new StringContent(data)).ConfigureAwait(false))
            {
                if (response.StatusCode != HttpStatusCode.OK && response.StatusCode != HttpStatusCode.NoContent)
                {
                    Log.LogError("Respose Error {0}", response.StatusCode);
                }
            }
        }
    }
}