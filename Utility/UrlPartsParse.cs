using System;
using System.Text;
//using System.Web;
using System.Collections.Generic;

/// <summary>
/// Parse a URL into its query parameters
/// </summary>
class UrlPartsParse
{
    private readonly List<SingleParameterParse> _parsedParameters;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="urlIn"></param>
    public UrlPartsParse(string urlIn)
    {

        //Example:https://{{SOME_SERVER}}/{{SOME_PAGE}}?code={{SOME-CODE}}&state={{SOME-STATE}}

        var parsedParameters = new List<SingleParameterParse>();

        //Break the URL at the '?'
        var queryParts = urlIn.Split('?');
        if(queryParts.Length < 2)
        {
            IwsDiagnostics.Assert(false, "722-259: No query string on URL: " + urlIn);
            throw new Exception("722-259: No query string on URL: " + urlIn);
        }

        var parameterSegments = queryParts[1].Split('&');

        //
        foreach(var thisParamText in parameterSegments)
        {
            parsedParameters.Add(new SingleParameterParse(thisParamText));
        }

        _parsedParameters = parsedParameters;
    }


    /// <summary>
    /// Gets the parameter value out
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public string GetParameterValue(string name)
    {
        foreach(var thisParam in _parsedParameters)
        {
            if(thisParam.ParameterName == name)
            {
                return thisParam.ParameterValue;
            }
        }

        return null; //Not found
    }


    /// <summary>
    /// Parses a parameter 'xxxxx=yyyyyyy'
    /// </summary>
    private class SingleParameterParse
    {
        public readonly string ParameterName;
        public readonly string ParameterValue;

        public SingleParameterParse(string parameterSegment)
        {
            var paramParts = parameterSegment.Split('=');
            this.ParameterName = paramParts[0];

            if(paramParts.Length > 1)
            {
                this.ParameterValue = paramParts[1];
                IwsDiagnostics.Assert(paramParts.Length == 2, "722-254: Parameter has multiple '=': " + parameterSegment);
            }
            else
            {
                IwsDiagnostics.Assert(false, "722-254: Parameter has no '=': " + parameterSegment);
            }
        }
    }

}
