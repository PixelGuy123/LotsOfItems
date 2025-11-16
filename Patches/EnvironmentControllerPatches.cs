using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace LotsOfItems.Patches
{
    [HarmonyPatch(typeof(EnvironmentController))]
    static class AmplifiedCellsPatch
    {
        // A set to store all cells that are currently amplifying sound.
        public static readonly HashSet<Cell> amplifiedCells = [];

        [HarmonyPatch(nameof(EnvironmentController.MakeNoise), typeof(GameObject), typeof(Vector3), typeof(int), typeof(bool))]
        [HarmonyPrefix]
        private static void AmplifySoundInCell(EnvironmentController __instance, Vector3 position, ref int value)
        {
            // Check if the cell where the noise is made is in our amplified set.
            if (__instance.ContainsCoordinates(position))
            {
                Cell cell = __instance.CellFromPosition(position);
                if (amplifiedCells.Contains(cell))
                {
                    // Double the noise value if it's an amplified cell.
                    value *= 2;
                }
            }

            // Lil' limit to make the noise value not go through Baldi's hearing limits
            value = Mathf.Min(value, 127); // Limit of its interaction array
        }
    }
}