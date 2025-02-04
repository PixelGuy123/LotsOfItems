using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System;
using UnityEngine;
using MTM101BaldAPI;

namespace LotsOfItems.Plugin
{
	public static class Extensions
	{
		static bool IsInheritFromType<T, T2>() =>
			typeof(T).IsSubclassOf(typeof(T2)) || typeof(T) == typeof(T2);

		public static T GetACopyFromFields<T, C>(this T original, C toCopyFrom) where T : MonoBehaviour where C : MonoBehaviour
		{
			if (!IsInheritFromType<T, C>())
			{
				throw new ArgumentException($"Type T ({typeof(T).FullName}) does not inherit from type C ({typeof(C).FullName})");
			}

			List<Type> typesToFollow = [typeof(C)];
			Type t = typeof(C);

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
				}
			}

			return original;
		}

		public static T ReplaceComponent<T, C>(this C toReplace) where T : MonoBehaviour where C : MonoBehaviour
		{
			var toExist = toReplace.gameObject.AddComponent<T>();
			toExist.GetACopyFromFields(toReplace);
			UnityEngine.Object.Destroy(toReplace);
			return toExist;
		}
	}
	public static class ReusableExtensions
	{
		public static T CreateNewReusableInstance<T>(this T ogItem, ItemObject ogItmObj, string newNameKey, int count) where T : Item
		{
			var newItmObj = UnityEngine.Object.Instantiate(ogItmObj);
			newItmObj.nameKey = $"{newNameKey}_{count}";
			newItmObj.name = $"ItmOb_{newItmObj.nameKey}";

			newItmObj.item.gameObject.SetActive(false); // To make sure the prefab is disabled and no Awake() is called
			var newItm = UnityEngine.Object.Instantiate(newItmObj.item as T);
			newItmObj.item.gameObject.SetActive(true);

			newItmObj.item = newItm;
			newItm.name = $"ObjItmOb_{newItmObj.nameKey}";
			newItm.gameObject.ConvertToPrefab(true);

			if (count > 1)
				AccessTools.Field(typeof(T), "nextItem").SetValue(ogItem, newItmObj); // Expects the field to have this name by default for every class that supports this
			return newItm;
		}
	}
}
