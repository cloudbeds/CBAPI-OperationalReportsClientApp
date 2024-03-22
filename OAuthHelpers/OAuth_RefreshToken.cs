using System;


/// <summary>
/// Abstraction to strongly type RefreshTokens secret
/// </summary>
class OAuth_RefreshToken
{
    public readonly string TokenValue;
    public override string ToString()
    {
        return TokenValue;
    }

    public OAuth_RefreshToken(string token)
    {
        this.TokenValue = token;
    }

}
