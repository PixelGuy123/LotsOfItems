using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.Patches;

[HarmonyPatch(typeof(PlayerClick))]
internal static class PlayerClickPatches
{
    [HarmonyTranspiler]
    [HarmonyPatch(nameof(PlayerClick.Update))]
    static IEnumerable<CodeInstruction> OnClickAccess(IEnumerable<CodeInstruction> i) =>
        new CodeMatcher(i)
        .MatchForward(true,
        new(OpCodes.Ldarg_0),
        new(CodeInstruction.LoadField(typeof(IClickable<int>), "click")),
        new(OpCodes.Ldarg_0),
        new(CodeInstruction.LoadField(typeof(PlayerClick), "pm")),
        new(CodeInstruction.LoadField(typeof(PlayerManager), "playerNumber")),
        new(OpCodes.Callvirt, AccessTools.Method(typeof(IClickable<int>), "Clicked"))
        )
        .InsertAndAdvance(
            new(OpCodes.Ldarg_0),
            CodeInstruction.LoadField(typeof(IClickable<int>), "click"),
            new(OpCodes.Ldarg_0),
            Transpilers.EmitDelegate((IClickable<int> clickable, PlayerClick click) => click.GetHandler().InvokeEvent((clickable as MonoBehaviour).gameObject, click))
        )
        .InstructionEnumeration();

    public static PlayerClickHandler GetHandler(this PlayerClick click)
    {
        if (click.TryGetComponent<PlayerClickHandler>(out var hand))
            return hand;
        return click.gameObject.AddComponent<PlayerClickHandler>();
    }
}

internal class PlayerClickHandler : MonoBehaviour
{
    public event System.Action<GameObject, PlayerClick> OnPlayerClick;
    internal void InvokeEvent(GameObject clickable, PlayerClick click) =>
        OnPlayerClick(clickable, click);
}