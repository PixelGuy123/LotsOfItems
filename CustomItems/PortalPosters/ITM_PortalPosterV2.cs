using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using PixelInternalAPI.Components;
using UnityEngine;

namespace LotsOfItems.CustomItems.PortalPosters;

public class ITM_PortalPosterV2 : ITM_PortalPoster, IItemPrefab
{
    public void SetupPrefab(ItemObject itm)
    {
        windowObject = Instantiate(windowObject); // Copies it
        windowObject.name = "PortalPosterV2";
        Texture2D texture = this.GetTexture("PortalPosterV2_world.png");
        for (int i = 0; i < windowObject.open.Length; i++)
        {
            windowObject.open[i] = new(windowObject.open[i])
            {
                name = "PortalPosterV2Open",
                mainTexture = texture
            }; // Clone Material
        }

        windowObject.windowPre = Instantiate(windowObject.windowPre);
        windowObject.windowPre.name = "PortalPosterV2Window"; // Get a custom instance of the prefab
        windowObject.windowPre.gameObject.ConvertToPrefab(true);
    }

    public void SetupPrefabPost()
    {
        var anim = windowObject.windowPre.GetComponent<TextureAnimator>();
        if (anim) // Casual BB+ Animations support (might actually have a real animation in the future lol)
            Destroy(anim);
    }
    public override bool Use(PlayerManager pm)
    {
        if (Physics.Raycast(pm.transform.position, Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform.forward, out hit, pm.pc.reach, pm.pc.ClickLayers, QueryTriggerInteraction.Ignore))
        {
            if (hit.transform.CompareTag("Wall"))
            {
                Direction direction = Directions.DirFromVector3(hit.transform.forward, 5f);
                Cell cell = pm.ec.CellFromPosition(IntVector2.GetGridPosition(hit.transform.position + hit.transform.forward * 5f));
                Cell cell2 = pm.ec.CellFromPosition(IntVector2.GetGridPosition(hit.transform.position + hit.transform.forward * -5f));
                if (pm.ec.ContainsCoordinates(IntVector2.GetGridPosition(hit.transform.position + hit.transform.forward * 5f)) && !cell.Null && cell.HasWallInDirection(direction.GetOpposite()) && !cell.WallHardCovered(direction.GetOpposite()) && pm.ec.ContainsCoordinates(IntVector2.GetGridPosition(hit.transform.position + hit.transform.forward * -5f)) && !cell2.Null && !cell2.WallHardCovered(direction))
                {
                    pm.ec.BuildWindow(cell2, direction, windowObject, editorMode: false);
                    BlockPortalForNPCs(pm.ec, cell2, direction);
                    Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audYes);
                    return true;
                }
                Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audNo);
                return false;
            }
            Destroy(gameObject);
            return false;
        }
        Destroy(gameObject);
        return false;
    }

    // Block the portal for NPCs by marking the cells as impassable for them
    private void BlockPortalForNPCs(EnvironmentController ec, Cell cell, Direction dir)
    {
        cell.Block(dir, true);
        ec.CellFromPosition(cell.position + dir.ToIntVector2()).Block(dir.GetOpposite(), true); // Opposite
    }
}
