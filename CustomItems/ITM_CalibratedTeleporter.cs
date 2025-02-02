using LotsOfItems.ItemPrefabStructures;
using System.Collections;
using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using PixelInternalAPI.Extensions;

namespace LotsOfItems.CustomItems
{
	public class ITM_CalibratedTeleporter : Item, IItemPrefab
	{
		public void SetupPrefab(ItemObject itm)
		{
			audTeleport = GenericExtensions.FindResourceObjectByName<SoundObject>("Teleport");
			markerSprite = this.GetSprite("CalibratedTeleporter_MarkerIcon.png", 40f);
		}

		public void SetupPrefabPost() { }

		public override bool Use(PlayerManager pm)
		{
			if (activeTps.Count != 0 || Singleton<CoreGameManager>.Instance.MapOpen || Singleton<GlobalCam>.Instance.TransitionActive || !Singleton<CoreGameManager>.Instance.sceneObject.usesMap)
			{
				Destroy(gameObject);
				return false;
			}

			activeTps.Add(this);
			this.pm = pm;

			zoomItUsedToBe = pm.ec.map.zoom;
			zoomSpeedItUsedToBe = pm.ec.map.zoomSpeed;
			scrollSpeedUsedToBe = pm.ec.map.scrollSpeed;

			pm.ec.map.zoom = defaultZoom;
			pm.ec.map.zoomSpeed = 0f;
			pm.ec.map.scrollSpeed = 0f;

			Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).mapCam.enabled = false;
			Singleton<CoreGameManager>.Instance.Pause(false);
			Singleton<CoreGameManager>.Instance.OpenMap();
			

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
			{
				transform.position = pm.transform.position;
				Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audTeleport);
			}
			else
				pm.ec.map.EndMarkerPlacing();

			pm.ec.map.zoom = zoomItUsedToBe;
			pm.ec.map.zoomSpeed = zoomSpeedItUsedToBe;
			pm.ec.map.scrollSpeed = scrollSpeedUsedToBe;

			activeTps.Remove(this);

			while (Singleton<CoreGameManager>.Instance.audMan.AnyAudioIsPlaying)
				yield return null;

			Destroy(gameObject);
		}

		void OnDestroy() =>
			activeTps.Remove(this);

		bool setPoint = false;
		float zoomItUsedToBe, zoomSpeedItUsedToBe, scrollSpeedUsedToBe;


		[SerializeField]
		internal float defaultZoom = 20f;

		[SerializeField]
		internal Sprite markerSprite;

		[SerializeField]
		internal SoundObject audTeleport;

		internal readonly static List<ITM_CalibratedTeleporter> activeTps = [];
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
			!(___placingMarker && ___currentMarkerId == -1 && ITM_CalibratedTeleporter.activeTps.Count != 0); // If this returns "true", then don't called this method

		[HarmonyPatch("TouchScreen")]
		[HarmonyPrefix]
		static bool AvoidMarkerAddition(Map __instance, ref bool ___placingMarker, int ___currentMarkerId)
		{
			if (___placingMarker && ___currentMarkerId == -1 && ITM_CalibratedTeleporter.activeTps.Count != 0)
			{
				var vector = __instance.cams[0].ScreenToWorldPoint(Singleton<GlobalCam>.Instance.CursorToRealScreenPosition());

				var realPos = new Vector3(vector.x * 10f + 5f, 0f, vector.y * 10f + 5f);
				var cell = __instance.Ec.CellFromPosition(realPos);

				if (cell.offLimits || cell.Null || cell.hideFromMap || !__instance.foundTiles[cell.position.x, cell.position.z])
					return false;

				Singleton<InputManager>.Instance.StopFrame();

				for (int i = 0; i < ITM_CalibratedTeleporter.activeTps.Count; i++)
					ITM_CalibratedTeleporter.activeTps[i].MarkAsUsed();

				Singleton<CoreGameManager>.Instance.GetPlayer(0).Teleport(cell.FloorWorldPosition);
				Singleton<CoreGameManager>.Instance.Pause(false);
				Singleton<CoreGameManager>.Instance.CloseMap();
				__instance.EndMarkerPlacing();

				___placingMarker = false;
				return false;
			}

			return true;
		}
	}
}
