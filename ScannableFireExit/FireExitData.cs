using System.Collections.Generic;
using LethalModDataLib.Attributes;
using LethalModDataLib.Enums;

namespace ScannableFireExit;

public static class FireExitData {
    [ModData(SaveWhen.OnSave, ResetWhen = ResetWhen.OnGameOver)]
    internal static readonly Dictionary<string, List<int>> LevelFireExitDictionary = [
    ];
}