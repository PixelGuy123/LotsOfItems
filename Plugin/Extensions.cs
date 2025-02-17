using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using MTM101BaldAPI;
using PixelInternalAPI.Extensions;
using LotsOfItems.Components;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;

namespace LotsOfItems.Plugin
{
	public static class Extensions
	{
		static bool IsInheritFromType(Type t1, Type t2) =>
			t1.IsSubclassOf(t2) || t1 == t2;

		public static T GetACopyFromFields<T, C>(this T original, C toCopyFrom) where T : MonoBehaviour where C : MonoBehaviour
		{
			var type = toCopyFrom.GetType();

			if (!IsInheritFromType(typeof(T), type))
			{
				throw new ArgumentException($"Type T ({typeof(T).FullName}) does not inherit from type C ({type.FullName})");
			}

			List<Type> typesToFollow = [type];
			Type t = type;

			while (true)
			{
				t = t.BaseType;

				if (t == null || t == typeof(MonoBehaviour))
				{
					break;
				}

				typesToFollow.Add(t);
			}

			foreach (var ty in typesToFollow)
			{
				foreach (FieldInfo fieldInfo in AccessTools.GetDeclaredFields(ty))
				{
					fieldInfo.SetValue(original, fieldInfo.GetValue(toCopyFrom));
					//Debug.Log("fieldname: " + fieldInfo.Name + " ToCopyFrom value: " + fieldInfo.GetValue(toCopyFrom));
				}
			}

			return original;
		}

		public static Texture2D ActualResize(this Texture2D original, int newWidth, int newHeight) // Apparently you work an average of WxH grid, not linear lol
		{
			int originalWidth = original.width;
			int originalHeight = original.height;

			// Calculate scaling factors.
			int scaleX = originalWidth / newWidth;
			int scaleY = originalHeight / newHeight;

			Texture2D newTex = new(newWidth, newHeight, original.format, false)
			{
				filterMode = original.filterMode
			};

			// Get the original pixel data.
			Color[] originalColors = original.GetPixels();
			Color[] newColors = new Color[newWidth * newHeight];

			// Loop over every pixel in the new texture.
			for (int newY = 0; newY < newHeight; newY++)
			{
				for (int newX = 0; newX < newWidth; newX++)
				{
					Color colorSum = Color.black;
					bool invalidAlpha = false;

					// For each new pixel, average over the corresponding block in the original texture.
					for (int offsetY = 0; offsetY < scaleY; offsetY++)
					{
						for (int offsetX = 0; offsetX < scaleX; offsetX++)
						{
							int origX = newX * scaleX + offsetX;
							int origY = newY * scaleY + offsetY;
							int index = origY * originalWidth + origX;

							Color current = originalColors[index];
							if (current.a < 1f)
							{
								invalidAlpha = true;
							}
							colorSum += current;
						}
					}

					int pixelCount = scaleX * scaleY;
					Color avgColor = colorSum / pixelCount;
					avgColor.a = invalidAlpha ? 0f : 1f;

					newColors[newY * newWidth + newX] = invalidAlpha ? Color.clear : avgColor;
				}
			}

			// Apply the new colors and update the texture.
			newTex.SetPixels(newColors);
			newTex.Apply();

			return newTex;
		}

		public static void Explode(this Item item, float explosionRadius, LayerMask collisionLayer, float explosionForce, float explosionAcceleration)
		{
			Vector3 position = item.transform.position;

			Collider[] colliders = Physics.OverlapSphere(position, explosionRadius, collisionLayer, QueryTriggerInteraction.Ignore);
			Ray ray = new();
			for (int i = 0; i < colliders.Length; i++)
			{
				if (colliders[i].transform == item.transform)
					continue;

				Entity entity = colliders[i].GetComponent<Entity>();
				if (entity == null || !entity.InBounds)
					continue;

				Vector3 entityDirection = (entity.transform.position - position).normalized;
				ray.origin = position;
				ray.direction = entityDirection;

				var allCasts = Physics.RaycastAll(ray, 9999f, LayerStorage.principalLookerMask, QueryTriggerInteraction.Ignore);
				for (int z = 0; z < allCasts.Length; z++)
				{
					var hit = allCasts[z];
					if (hit.transform == colliders[i].transform)
					{
						entity.AddForce(new Force(entityDirection, explosionForce, explosionAcceleration));
						break;
					}
				}
			}
		}

		public static RaycastBlocker GetRawChalkParticleGenerator(bool visualOnly = false)
		{
			var chalk = GenericExtensions.FindResourceObject<ChalkEraser>();
			chalk.gameObject.SetActive(false);
			var chalkClone = UnityEngine.Object.Instantiate(chalk);
			var chalkTransform = chalkClone.transform;

			UnityEngine.Object.Destroy(chalkClone.GetComponent<RendererContainer>());
			if (visualOnly)
			{
				foreach (var collider in chalkTransform.GetComponentsInChildren<Collider>())
					UnityEngine.Object.Destroy(collider);
			}
			UnityEngine.Object.Destroy(chalkClone);

			var blocker = chalkTransform.gameObject.AddComponent<RaycastBlocker>(); // Should make this work good
			blocker.system = chalkTransform.GetComponent<ParticleSystem>();
			blocker.colliders = visualOnly ? [] : chalkTransform.GetComponentsInChildren<Collider>();

			chalk.gameObject.SetActive(true);
			chalkTransform.gameObject.ConvertToPrefab(true);
			chalkTransform.name = "RawChalkGenerator";

			return blocker;
		}

		// Cell Extensions
		public static void UncoverSoftWall(this Cell cell, Direction dir)
		{
			CellCoverage mask = (CellCoverage)(1 << (int)dir);
			if ((cell.softCoverage & mask) != CellCoverage.None)
			{
				cell.softCoverage &= ~mask;
			}
		}

		public static void BlockAll(this Cell cell, EnvironmentController ec, bool block)
		{
			for (int i = 0; i < Directions.Count; i++)
			{
				Direction dir = (Direction)i;
				if (!cell.HasWallInDirection(dir))
				{
					cell.Block(dir, block);
					ec.CellFromPosition(cell.position + dir.ToIntVector2()).Block(dir.GetOpposite(), block);
				}
			}
		}
	}
	public static class ReusableExtensions
	{
		public static ItemObject[] CreateNewReusableInstances<T>(this T ogItem, ItemObject ogItmObj, string nameKey, int count) where T : Item
		{
			var instances = new ItemObject[count + 1];
			instances[0] = ogItmObj;

			var meta = ogItmObj.GetMeta();
			meta.itemObjects = new ItemObject[count + 1];
			meta.itemObjects[0] = ogItmObj;

			for (; count > 0; count--)
			{
				ogItem = ogItem.Internal_CreateNewReusableInstance(ogItmObj, nameKey, count, out var itemObject);
				meta.itemObjects[meta.itemObjects.Length - count] = itemObject;
				instances[instances.Length - count] = itemObject;
			}
			return instances;
		}
		static T Internal_CreateNewReusableInstance<T>(this T ogItem, ItemObject ogItmObj, string newNameKey, int count, out ItemObject newItmObj) where T : Item
		{
			newItmObj = UnityEngine.Object.Instantiate(ogItmObj);
			newItmObj.nameKey = $"{newNameKey}_{count}";
			newItmObj.name = $"ItmOb_{newItmObj.nameKey}";

			newItmObj.item.gameObject.SetActive(false); // To make sure the prefab is disabled and no Awake() is called
			var newItm = UnityEngine.Object.Instantiate(newItmObj.item as T);
			newItmObj.item.gameObject.SetActive(true);

			newItmObj.item = newItm;
			newItm.name = $"ObjItmOb_{newItmObj.nameKey}";
			newItm.gameObject.ConvertToPrefab(true);

			var fldInfo = AccessTools.Field(typeof(T), "nextItem");
			fldInfo.SetValue(ogItem, newItmObj); // Expects the field to have this name by default for every class that supports this

			if (count <= 1)
				fldInfo.SetValue(newItm, null);

			return newItm;
		}
	}
}
