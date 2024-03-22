using System;


/// <summary>
/// Abstraction to strongly type the bootstrap "code" that is returned during a
/// UI log-in
/// </summary>
class OAuth_BootstrapCode
{
    public readonly string TokenValue;
    public override string ToString()
    {
        return TokenValue;
    }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="token"></param>
    public OAuth_BootstrapCode(string token)
    {
        this.TokenValue = token;
    }

}
