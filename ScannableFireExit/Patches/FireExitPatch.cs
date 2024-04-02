using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace ScannableFireExit.Patches;

[HarmonyPatch(typeof(EntranceTeleport))]
public static class FireExitPatch {
    [HarmonyPatch(nameof(EntranceTeleport.Awake))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterAwake(EntranceTeleport __instance) {
        if (!__instance.isEntranceToBuilding || __instance.entranceId == 0)
            return;

        HandleFireExit(__instance);
    }

    [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
    [HarmonyPostfix]
    // ReSharper disable once InconsistentNaming
    public static void AfterEnterOrExit(EntranceTeleport __instance) {
        FindFireExitEntrance(__instance, out var fireExit);

        if (fireExit is null)
            return;

        MarkFireExitAsFound(fireExit);
        HandleFireExit(fireExit);
    }

    private static void MarkFireExitAsFound(EntranceTeleport fireExit) {
        var levelName = GetCurrentLevelName();
        var fireExits = GetOrCreateFireExitsForLevel(levelName);

        if (fireExits.Contains(fireExit.entranceId))
            return;

        fireExits.Add(fireExit.entranceId);
    }

    private static void HandleFireExit(EntranceTeleport entranceTeleport) {
        if (entranceTeleport.GetComponent<ScanNodeContainer>() is not null)
            return;

        var levelName = GetCurrentLevelName();
        var fireExits = GetOrCreateFireExitsForLevel(levelName);

        var found = fireExits.Any(otherFireExit => entranceTeleport.entranceId == otherFireExit);

        if (!found && OnlyWhenSeen())
            return;

        CreateScanNodeOnObject(entranceTeleport.gameObject, "Fire Exit",
                               $"Emergency Exit #{entranceTeleport.entranceId}");
    }

    private static void FindFireExitEntrance(EntranceTeleport entranceTeleport, out EntranceTeleport? entrance) {
        entrance = null;

        if (entranceTeleport.entranceId == 0)
            return;

        if (entranceTeleport.isEntranceToBuilding) {
            entrance = entranceTeleport;
            return;
        }

        var allEntranceTeleports = Object.FindObjectsOfType<EntranceTeleport>();

        entrance = allEntranceTeleports.FirstOrDefault(allEntranceTeleport =>
                                                           allEntranceTeleport.isEntranceToBuilding
                                                        && allEntranceTeleport.entranceId
                                                        == entranceTeleport.entranceId);
    }

    private static bool OnlyWhenSeen() =>
        ScannableFireExit.onlySeenFireExits.Value;

    private static string GetCurrentLevelName() =>
        RoundManager.Instance.currentLevel.PlanetName;

    private static List<int> GetOrCreateFireExitsForLevel(string levelName) {
        if (FireExitData.LevelFireExitDictionary.TryGetValue(levelName, out var fireExits))
            return fireExits;

        fireExits = [
        ];

        FireExitData.LevelFireExitDictionary[levelName] = fireExits;

        return fireExits;
    }

    private static void CreateScanNodeOnObject(GameObject gameObject, string headerText, string? subText) {
        const int nodeType = 0;
        const int minRange = 12;
        const int maxRange = 52;
        const int size = 1;

        var scanNodeObject = new GameObject("ScanNode", typeof(ScanNodeProperties), typeof(BoxCollider)) {
            layer = LayerMask.NameToLayer("ScanNode"),
            transform = {
                localScale = Vector3.one * size,
                parent = gameObject.transform,
            },
        };

        scanNodeObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        var scanNode = scanNodeObject.GetComponent<ScanNodeProperties>();

        scanNode.scrapValue = 0;
        scanNode.creatureScanID = -1;
        scanNode.nodeType = nodeType;
        scanNode.minRange = minRange;
        scanNode.maxRange = maxRange;
        scanNode.requiresLineOfSight = false;
        scanNode.headerText = headerText;

        if (subText != null)
            scanNode.subText = subText;

        var scanNodeContainer = gameObject.AddComponent<ScanNodeContainer>();

        scanNodeContainer.scanNodeGameObject = scanNodeObject;
        scanNodeContainer.scanNode = scanNode;
    }
}