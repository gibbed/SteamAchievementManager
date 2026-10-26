using SAM.App.Helpers;
using SAM.App.Lists;

namespace SAM.App.Library;

/// <summary>State of a game from Steam's local files: section, progress, medal, sets and profile visibility.</summary>
internal sealed record CompletionInfo(ProgressState State, double Ratio, int Unlocked, int Total, MedalKind Medal, SetSummary? Sets, bool? Vetted);
