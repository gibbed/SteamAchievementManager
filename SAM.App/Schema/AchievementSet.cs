namespace SAM.App.Schema;

/// <summary>
/// An achievement set (Steam beta since October 2026) from the schema: groups/&lt;id&gt; with name, optional
/// dlcappid (set of a DLC) and order. Achievements without groupid belong to the "base game" set (Id 0).
/// </summary>
internal sealed class AchievementSet
{
    public const int BaseGameId = 0;

    public int Id { get; init; }

    /// <summary>Name in the Steam client's language; null for the base game (the app uses its own text).</summary>
    public string? Name { get; init; }

    /// <summary>App ID of the DLC; 0 for base game sets.</summary>
    public uint DlcAppId { get; init; }

    public int Order { get; init; }

    public bool IsDlc => DlcAppId != 0;
}
