using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Net.Http;
using System.IO;


/// <summary>
/// Base class on top of which AUTHENTICATED SESSION requests to Cloudbeds are based
/// </summary>
class CloudbedsAuthenticatedRequestBase : CloudbedsRequestBase
{
    private readonly ICloudbedsAuthSessionId _authenticatedSesssion;
    protected CloudbedsAuthenticatedRequestBase(ICloudbedsAuthSessionId authSession, TaskStatusLogs statusLogs) : base (statusLogs)
    {
        _authenticatedSesssion = authSession;
    }

    /// <summary>
    /// Add a authorization header...
    /// </summary>
    /// <param name="httpMethod"></param>
    /// <param name="uri"></param>
    /// <returns></returns>
    protected override HttpRequestMessage Internal_CreateHttpRequest(string httpMethod, string uri)
    {
        var httpRequest = base.Internal_CreateHttpRequest(httpMethod, uri);

        httpRequest.Headers.Add(
            "Authorization",
            "Bearer " + _authenticatedSesssion.AuthSessionsId);
        return httpRequest;
    }


    /// <summary>
    /// Validates that the response contains a succes:true element
    /// </summary>
    /// <param name="jsonOut"></param>
    /// <exception cref="Exception"></exception>
    protected static void ValidateResponseForSuccessFlag(JsonDocument jsonOut)
    {
        //Check the SUCCESS node explicitly for TRUE
        bool? isSuccess = JsonParseHelpers.FindJasonAttributeValue_BoolOrNull(jsonOut.RootElement, "success");
        if (isSuccess == null)
        {
            throw new Exception("1023-600: Response does not have a 'success' property");
        }

        if (isSuccess.Value != true)
        {
            throw new Exception("1023-640: Response 'success' property is not 'true");
        }
    }
}
