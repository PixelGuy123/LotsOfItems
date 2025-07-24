using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.Tapes;

public class ITM_BaldisMostFavoriteTape : ITM_GenericTape
{
	public Sprite[] baldiAnimation, baldiDance;
	[SerializeField]
	internal SpriteRenderer spriteRenderer;
	[SerializeField]
	internal float baldiAppearanceMaxTime = 0.35f, baldiFramerate = 16f, baldiDanceFramerate = 2.75f, baldiHeight = 3.911f, baldiInitialHeight = 4.3f;
	protected override void VirtualSetupPrefab(ItemObject itm)
	{
		audioToOverride = [this.GetSound("BaldisMostFavoriteTape_song.wav", "LtsOItems_Vfx_BaldiFavoriteSound", SoundType.Effect, Color.green)];
		baldiAnimation = this.GetSpriteSheet("BaldiMostFavoriteTape_DancingBaldi.png", 13, 13, 30f).RemoveEmptySprites();
		baldiDance = GenericExtensions.FindResourceObject<BaldiDance>().danceSprites;

		spriteRenderer = ObjectCreationExtensions.CreateSpriteBillboard(baldiAnimation[0]);
		spriteRenderer.gameObject.ConvertToPrefab(true);
		spriteRenderer.name = "BaldiMostFavoriteTape_DancingBaldi";
	}

	// Coroutine that drags Baldi to the player's position until the tape finishes playing.
	protected override IEnumerator NewCooldown(TapePlayer tapePlayer)
	{
		var baldi = tapePlayer.Ec.GetBaldi();
		EnvironmentController ec = tapePlayer.ec;
		bool baldiIsNull = false;

		if (baldi == null)
		{
			baldiIsNull = true;
			while (!baldi)
			{
				baldi = ec.GetBaldi();
				yield return null; // Waits until Baldi isn't null
			}
		}

		// Disables Baldi to appear the dancing one
		baldi.Navigator.Entity.SetFrozen(true);
		baldi.Navigator.Entity.SetInteractionState(false);
		baldi.Navigator.Entity.SetVisible(false);
		baldi.Navigator.Entity.SetBlinded(true);
		baldi.enabled = false; // Hopefully nobody touches this lol

		// Creates dancing Baldi
		var dancingBaldi = Instantiate(spriteRenderer);
		Vector3 startPos = GetSafeCellForBaldi(ec.CellFromPosition(tapePlayer.transform.position), false).FloorWorldPosition;
		startPos.y = -10f; // To slide above Baldi

		Vector3 endPos = startPos;
		endPos.y = baldiInitialHeight; // Yup, same height

		// Go to dancing baldi position
		baldi.Navigator.Entity.Teleport(endPos);

		dancingBaldi.transform.position = startPos;

		// Timing for movement
		float t = 0f, clampedT;

		// Framing for the animation
		float frame = 0f;

		if (!baldiIsNull)
		{
			while (t < baldiAppearanceMaxTime)
			{
				t += Time.deltaTime * ec.EnvironmentTimeScale;
				clampedT = t / baldiAppearanceMaxTime;

				// Position
				dancingBaldi.transform.position = Vector3.Slerp(startPos, endPos, clampedT);

				// Animation
				frame += Time.deltaTime * ec.EnvironmentTimeScale * baldiFramerate;

				if (frame >= baldiAnimation.Length)
					frame = baldiAnimation.Length - 1;

				dancingBaldi.sprite = baldiAnimation[Mathf.FloorToInt(frame)];
				yield return null;
			}
		}

		dancingBaldi.transform.position = endPos;

		while (true)
		{
			// Rest of the animation
			frame += Time.deltaTime * ec.EnvironmentTimeScale * baldiFramerate;

			if (frame >= baldiAnimation.Length)
				break;

			dancingBaldi.sprite = baldiAnimation[Mathf.FloorToInt(frame)];

			yield return null;
		}

		frame = 0f;
		endPos.y = baldiHeight;
		dancingBaldi.transform.position = endPos;

		while (tapePlayer.audMan.AnyAudioIsPlaying)
		{
			// Dance
			frame += Time.deltaTime * ec.EnvironmentTimeScale * baldiDanceFramerate;
			frame %= baldiDance.Length;
			dancingBaldi.sprite = baldiDance[Mathf.FloorToInt(frame)];

			yield return null;
		}

		Destroy(dancingBaldi.gameObject);

		if (baldi)
		{
			// Baldi here again
			baldi.Navigator.Entity.SetFrozen(false);
			baldi.Navigator.Entity.SetInteractionState(true);
			baldi.Navigator.Entity.SetVisible(true);
			baldi.Navigator.Entity.SetBlinded(false);
			baldi.enabled = true;
		}
		yield break;


		// Local Methods to help
		Cell GetSafeCellForBaldi(Cell startCell, bool ignoreSafeCells)
		{
			for (int i = 0; i < Directions.Count; i++)
			{
				IntVector2 position = startCell.position + ((Direction)i).ToIntVector2();
				if (!ec.ContainsCoordinates(position)) continue; // Must be in-bounds

				var cell = ec.CellFromPosition(position);
				// If Cell exists and it is a safe spot, Baldi can spawn in there.
				if (!cell.Null && cell.TileMatches(startCell.room) && (ignoreSafeCells || startCell.room.entitySafeCells.Contains(cell.position)))
					return cell;
			}

			if (ignoreSafeCells)
				return startCell; // Failsafe to avoid infinite loop

			// If no position found, try again with Baldo clipping lol
			return GetSafeCellForBaldi(startCell, true);
		}
	}


}
