using LotsOfItems.Components;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Extensions;
using System.Collections;
using UnityEngine;

namespace LotsOfItems.CustomItems.AlarmClock;
public class ITM_AncientClock : ITM_GenericAlarmClock
{
	[SerializeField]
	internal Sprite pieces;

	[SerializeField]
	internal float shatterSpeed = 9f;

	[SerializeField]
	internal SoundObject audShatter;

	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		base.VirtualSetupPrefab(itm);
		audMan.soundOnStart = [];
		audShatter = GenericExtensions.FindResourceObjectByName<SoundObject>("GlassBreak");
		audRing = this.GetSound("AncientClock_ring.ogg", audRing.soundKey, audRing.soundType, audRing.color);
		var sprs = this.GetSpriteSheet("AncientClock_world.png", 2, 1, spriteRenderer.sprite.pixelsPerUnit);
		spriteRenderer.sprite = sprs[0];
		pieces = sprs[1];
	}

	public override bool AllowClickable() => false;

	protected override void OnClockRing()
	{
		foreach (var npc in ec.Npcs)
		{
			if (npc.Navigator.isActiveAndEnabled)
				npc.navigationStateMachine.ChangeState(new NavigationState_ForceTargetPosition(npc, 24, transform.position));
		}
	}

	protected override void Destroy() =>
		StartCoroutine(TurnIntoPieces());

	IEnumerator TurnIntoPieces()
	{
		audMan.PlaySingle(audShatter);
		spriteRenderer.sprite = pieces;
		var rendTransform = spriteRenderer.transform;
		var pos = rendTransform.localPosition;
		while (true)
		{
			pos.y -= ec.EnvironmentTimeScale * Time.deltaTime * shatterSpeed;
			rendTransform.localPosition = pos;
			if (pos.y < -10f && !audMan.AnyAudioIsPlaying)
			{
				base.Destroy();
				yield break;
			}

			yield return null;
		}
	}
}
