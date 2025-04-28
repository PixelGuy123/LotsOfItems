// Unused since nametags don't contain an animator anymore
// using UnityEngine;
// using UnityEngine.UI;

// namespace LotsOfItems.Components
// {
// 	public class NametagAnimationOverrider : MonoBehaviour
// 	{
// 		[SerializeField]
// 		internal ITM_Nametag nameTag;

// 		[SerializeField]
// 		internal Sprite[] animation;

// 		[SerializeField]
// 		internal float frameRate = 16.5f;

// 		[SerializeField]
// 		internal Image image;

// 		float frame = 0f;

// 		public static NametagAnimationOverrider InstallOverrider(ITM_Nametag tag, params Sprite[] sprites)
// 		{
// 			var ov = tag.gameObject.AddComponent<NametagAnimationOverrider>();
// 			ov.nameTag = tag;
// 			ov.image = tag.GetComponentInChildren<Image>();

// 			var anim = tag.GetComponentInChildren<Animator>();
// 			if (anim)
// 				Destroy(anim); // No animator anymore

// 			ov.animation = sprites;
// 			ov.image.sprite = ov.animation[0];

// 			return ov;
// 		}
// 		void Update()
// 		{
// 			frame += Time.deltaTime * frameRate;
// 			frame %= animation.Length;

// 			image.sprite = animation[Mathf.FloorToInt(frame)];
// 		}
// 	}
// }
