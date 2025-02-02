using LotsOfItems.Plugin;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using PixelInternalAPI.Extensions;
using System.IO;
using UnityEngine;

namespace LotsOfItems.ItemPrefabStructures
{
	public interface IItemPrefab
	{
		void SetupPrefab(ItemObject itm);
		void SetupPrefabPost();
	}

	public static class ItemPrefabExtensions
	{
		public static SoundObject GetSound(this IItemPrefab prefab, string audioName, string subtitle, SoundType soundType, Color color) =>
			ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromFile(Path.Combine(LotOfItemsPlugin.ModPath, audioName)), subtitle, soundType, color);
		public static SoundObject GetSoundNoSub(this IItemPrefab prefab, string audioName, SoundType type)
		{
			var sd = prefab.GetSound(audioName, string.Empty, type, Color.white);
			sd.subtitle = false;
			return sd;
		}
		public static Sprite GetSprite(this IItemPrefab prefab, string spriteName, Vector2 center, float pixelsPerUnit) =>
			AssetLoader.SpriteFromFile(Path.Combine(LotOfItemsPlugin.ModPath, spriteName), center, pixelsPerUnit);
		public static Sprite GetSprite(this IItemPrefab prefab, string spriteName, float pixelsPerUnit) =>
			prefab.GetSprite(spriteName, Vector2.one * 0.5f, pixelsPerUnit);
		public static Sprite[] GetSpriteSheet(this IItemPrefab prefab, string spriteName, int rowX, int rowY, float pixelsPerUnit, Vector2 center) =>
			TextureExtensions.LoadSpriteSheet(rowX, rowY, pixelsPerUnit, center, LotOfItemsPlugin.ModPath, spriteName);
		public static Sprite[] GetSpriteSheet(this IItemPrefab prefab, string spriteName, int rowX, int rowY, float pixelsPerUnit) =>
			prefab.GetSpriteSheet(spriteName, rowX, rowY, pixelsPerUnit, Vector2.one * 0.5f);
		public static Texture2D GetTexture(this IItemPrefab prefab, string textureName) =>
			AssetLoader.TextureFromFile(Path.Combine(LotOfItemsPlugin.ModPath, textureName));
	}
}
