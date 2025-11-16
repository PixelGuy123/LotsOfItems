using System.Collections;
using HarmonyLib;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Teleporters
{
	public class ITM_CalibratedTeleporter : Item, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm)
		{
			audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
			var sheet = this.GetSpriteSheet("CalibratedTeleporter_MarkerIcon.png", 2, 1, 40f);
			markerSprite = sheet[0];
			markerNotAllowedSprite = sheet[1];
		}

		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			if (activeTps || Singleton<CoreGameManager>.Instance.MapOpen || Singleton<GlobalCam>.Instance.TransitionActive)
			{
				Destroy(gameObject);
				return false;
			}

			if (!Singleton<CoreGameManager>.Instance.sceneObject.usesMap)
			{
				pm.Teleport(pm.ec.RandomCell(false, false, true).FloorWorldPosition);
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
				Destroy(gameObject);
				return true;
			}

			activeTps = this;
			this.pm = pm;

			zoomItUsedToBe = pm.ec.map.zoom;
			zoomSpeedItUsedToBe = pm.ec.map.zoomSpeed;
			scrollSpeedUsedToBe = pm.ec.map.scrollSpeed;

			pm.ec.map.zoom = defaultZoom;
			pm.ec.map.zoomSpeed = 0f;
			pm.ec.map.scrollSpeed = 0f;

			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).mapCam.enabled = false;
			Singleton<CoreGameManager>.Instance.Pause(false);
			Singleton<CoreGameManager>.Instance.OpenMap(true);


			StartCoroutine(WaitUntilMapClose());

			return false;
		}

		public void MarkAsUsed() =>
			setPoint = true;

		IEnumerator WaitUntilMapClose()
		{
			while (Singleton<GlobalCam>.Instance.TransitionActive)
				yield return null;

			pm.ec.map.ForcedActivateMarkerMode(markerSprite);

			while (Singleton<CoreGameManager>.Instance.MapOpen)
				yield return null;
			if (setPoint)
				pm.itm.RemoveItem(pm.itm.selectedItem);

			yield return null;

			if (setPoint)
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
			else
				pm.ec.map.EndMarkerPlacing();

			pm.ec.map.zoom = zoomItUsedToBe;
			pm.ec.map.zoomSpeed = zoomSpeedItUsedToBe;
			pm.ec.map.scrollSpeed = scrollSpeedUsedToBe;

			activeTps = null;

			Destroy(gameObject);
		}

		void OnDestroy() =>
			activeTps = null;

		bool setPoint = false;
		float zoomItUsedToBe, zoomSpeedItUsedToBe, scrollSpeedUsedToBe;


		[SerializeField]
		internal float defaultZoom = 20f;

		[SerializeField]
		internal Sprite markerSprite, markerNotAllowedSprite;

		[SerializeField]
		internal SoundObject audTeleport;

		internal static ITM_CalibratedTeleporter activeTps = null;
	}

	[HarmonyPatch(typeof(Map))]
	internal static class CalibratedTeleporter_Patches
	{
		internal static void ForcedActivateMarkerMode(this Map map, Sprite markerSprite)
		{
			map.placingMarker = true;
			map.currentMarkerId = -1;
			map.markerCursor.sprite = markerSprite;
			map.markerCursor.gameObject.SetActive(true);
			map.UpdateMarkerCursorPosition();
			map.markerIndicator.SetActive(true);
			map.fullIndicator.SetActive(false);
		}

		[HarmonyPatch("ActivateMarkerMode")]
		[HarmonyPrefix]
		static bool AvoidThisWhenCalibratedTpActivated(ref bool ___placingMarker, int ___currentMarkerId) =>
			!(___placingMarker && ___currentMarkerId == -1 && ITM_CalibratedTeleporter.activeTps); // If this returns "true", then don't called this method

		[HarmonyPatch("TouchScreen")]
		[HarmonyPrefix]
		static bool AvoidMarkerAddition(Map __instance, ref bool ___placingMarker, int ___currentMarkerId)
		{
			if (___placingMarker && ___currentMarkerId == -1 && ITM_CalibratedTeleporter.activeTps)
			{
				var vector = __instance.cams[0].ScreenToWorldPoint(Singleton<GlobalCam>.Instance.CursorToRealScreenPosition());

				var realPos = new Vector3(vector.x * 10f + 5f, 0f, vector.y * 10f + 5f);
				var cell = __instance.Ec.CellFromPosition(realPos);

				if (__instance.CheckCell(cell))
					return false;

				Singleton<InputManager>.Instance.StopFrame();

				ITM_CalibratedTeleporter.activeTps.MarkAsUsed();

				Singleton<CoreGameManager>.Instance.GetPlayer(0).Teleport(cell.FloorWorldPosition);
				Singleton<CoreGameManager>.Instance.Pause(false);
				Singleton<CoreGameManager>.Instance.CloseMap();
				__instance.EndMarkerPlacing();

				___placingMarker = false;
				return false;
			}

			return true;
		}

		[HarmonyPatch("UpdateMarkerCursorPosition")]
		[HarmonyPostfix]
		static void MakeSureMarkerIsInRightSpot(Map __instance, ref SpriteRenderer ___markerCursor, ref bool ___placingMarker, int ___currentMarkerId)
		{
			if (___placingMarker && ___currentMarkerId == -1 && ITM_CalibratedTeleporter.activeTps)
			{
				var vector = __instance.cams[0].ScreenToWorldPoint(Singleton<GlobalCam>.Instance.CursorToRealScreenPosition());

				var realPos = new Vector3(vector.x * 10f + 5f, 0f, vector.y * 10f + 5f);
				var cell = __instance.Ec.CellFromPosition(realPos);

				___markerCursor.sprite = !__instance.CheckCell(cell) ? ITM_CalibratedTeleporter.activeTps.markerSprite : ITM_CalibratedTeleporter.activeTps.markerNotAllowedSprite;
			}
		}

		static bool CheckCell(this Map map, Cell cell) =>
			cell.offLimits || cell.Null || cell.hideFromMap || !map.foundTiles[cell.position.x, cell.position.z] || (cell.room.type == RoomType.Hall ? cell.HasAnyHardCoverage : !cell.room.eventSafeCells.Contains(cell.position));
	}
}
