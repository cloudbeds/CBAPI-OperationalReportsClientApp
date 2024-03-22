using System;

/// <summary>
/// Abstraction to strongly type AccessTokens secret
/// </summary>
class OAuth_AccessToken
{
    public readonly string TokenValue;
    public override string ToString()
    {
        return TokenValue;
    }

    public OAuth_AccessToken(string token)
    {
        this.TokenValue = token;
    }

}
