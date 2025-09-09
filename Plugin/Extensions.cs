using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.Plugin
{
    // General extensions for Plus
    public static class GameExtensions
    {
        public static bool IsPrincipal(this NPC npc) =>
            npc.Character == Character.Principal; // A self-patch will be applied here to provide support for Replacement Characters.


        public static void CallOutPrincipals(this EnvironmentController ec, Vector3 position) // Should be patched by the self-patch to handle replacement principals (I should make a method...)
        {
            for (int i = 0; i < ec.Npcs.Count; i++)
            {
                if (IsPrincipal(ec.Npcs[i]) && ec.Npcs[i] is Principal pr)
                {
                    pr.WhistleReact(position);
                }
            }
        }
    }

    // Sprite Extensions
    public static class SpriteExtensions
    {
        public static Sprite DuplicateItself(this Sprite ogSpr, float newPixelsPerUnit)
        {
            var spr = AssetLoader.SpriteFromTexture2D(ogSpr.texture, newPixelsPerUnit);
            spr.name = $"{ogSpr.name}_Duplicate";
            return spr;
        }
    }

    // General MonoBehaviour extensions
    public static class MonoBehaviourExtensions
    {
        public static T SafeDuplicatePrefab<T>(this T obj, bool setActive) where T : Component
        {
            obj.gameObject.SetActive(false);

            var inst = obj.DuplicatePrefab();
            inst.gameObject.SetActive(setActive);

            obj.gameObject.SetActive(true);

            return inst;
        }

        static bool IsInheritFromType(Type t1, Type t2) => t1.IsSubclassOf(t2) || t1 == t2;

        public static T GetACopyFromFields<T, C>(this T original, C toCopyFrom)
            where T : MonoBehaviour
            where C : MonoBehaviour
        {
            var type = toCopyFrom.GetType();

            if (!IsInheritFromType(typeof(T), type))
            {
                throw new ArgumentException(
                    $"Type T ({typeof(T).FullName}) does not inherit from type C ({type.FullName})"
                );
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

        public static ClickableLink CreateClickableLink<T>(this MonoBehaviour clickable, Vector3 clickableLocalPos)
        {
            if (clickable.GetComponent<IClickable<T>>() == null)
                throw new ArgumentException($"Given clickable ({clickable.name}) doesn\'t have any IClickable<{typeof(T).Name}>");

            var obj = new GameObject($"{clickable.name}_Clickable");
            obj.transform.SetParent(clickable.transform);
            obj.transform.localPosition = clickableLocalPos;

            obj.gameObject.layer = LayerStorage.iClickableLayer;
            var gm = obj.AddComponent<ClickableLink>();
            gm.link = clickable;

            return gm;
        }
        public static ClickableLink CreateClickableLink<T>(this MonoBehaviour clickable) =>
            clickable.CreateClickableLink<T>(Vector3.zero);
    }

    // Collider/ClickableLink extensions
    public static class ColliderExtensions
    {
        public static ClickableLink CopyColliderAttributes(this ClickableLink link, CapsuleCollider myCol)
        {
            var col = link.gameObject.AddComponent<CapsuleCollider>();
            col.isTrigger = true;
            col.height = myCol.height;
            col.direction = myCol.direction;
            col.radius = myCol.radius;
            return link;
        }
    }

    // ItemObject/Shopping extensions
    public static class ItemObjectExtensions
    {
        public static List<ItemObject> GetAllShoppingItems()
        {
            List<ItemObject> itmObjs = [];
            foreach (var s in GenericExtensions.FindResourceObjects<SceneObject>())
            {
                s.shopItems.Do(x =>
                {
                    var meta = x.selection.GetMeta();
                    if (meta != null && !itmObjs.Contains(meta.value))
                        itmObjs.Add(meta.value);
                });
            }
            return itmObjs;
        }
    }

    // Texture2D extensions
    public static class Texture2DExtensions
    {
        public static Texture2D Mask(this Texture2D original, Texture2D texRef)
        {
            if (original.width != texRef.width || original.height != texRef.height)
                throw new ArgumentException($"Original texture has different dimension of texRef. Original ({original.width}x{original.height}) | TexRef ({texRef.width}x{texRef.height})");

            var colorRef = texRef.GetPixels();
            var pixels = original.GetPixels();

            for (int i = 0; i < pixels.Length; i++)
            {
                if (colorRef[i].a == 0f) // If alpha is 0, it's not inside the mask
                    pixels[i] = Color.clear;
            }
            original.SetPixels(pixels);
            original.Apply();
            return original;
        }

        public static Texture2D ApplyColorLevel(this Texture2D original, Color tint)
        {
            var pixels = original.GetPixels();
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i].r *= tint.r;
                pixels[i].g *= tint.g;
                pixels[i].b *= tint.b;
            }
            original.SetPixels(pixels);
            original.Apply();
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
                filterMode = original.filterMode,
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
    }

    // Item/Explosion extensions
    public static class ItemExtensions
    {
        // For Items, in World space
        static Ray ray = new();

        public static void Explode(
            this Item item,
            float explosionRadius,
            LayerMask collisionLayer,
            float explosionForce,
            float explosionAcceleration
        )
        {
            Vector3 position = item.transform.position;

            var colliders = Physics.OverlapSphere(
                position,
                explosionRadius,
                collisionLayer,
                QueryTriggerInteraction.Ignore
            );

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

                var allResults = Physics.RaycastAll(
                    ray,
                    9999f,
                    LayerStorage.principalLookerMask,
                    QueryTriggerInteraction.Ignore
                );
                for (int z = 0; z < allResults.Length; z++)
                {
                    var hit = allResults[z];
                    if (hit.transform == colliders[i].transform)
                    {
                        entity.AddForce(
                            new Force(entityDirection, explosionForce, explosionAcceleration)
                        );
                        break;
                    }
                }
            }
        }

        public static void SpawnInstantly(this ITM_NanaPeel nanaPeel, EnvironmentController ec, Vector3 position, Vector3 forward, bool splatSound = true)
        {
            nanaPeel.Spawn(ec, position, forward, 0f);

            nanaPeel.ready = true;
            nanaPeel.height = nanaPeel.endHeight;
            nanaPeel.entity.SetGrounded(true);
            nanaPeel.entity.SetHeight(nanaPeel.height);
            nanaPeel.audioManager.PlaySingle(nanaPeel.audSplat);
            nanaPeel.time = nanaPeel.maxTime;
        }

        // Generic Extensions

        public static T GetVariantInstance<T>(Items item) where T : Item
        {
            var ogItem = ItemMetaStorage.Instance.FindByEnum(item).value.item;
            // Debug.Log($"Instantiating item: {ogItem} object of type {ogItem.GetType().Name} for new type ({typeof(T)})");

            ogItem.gameObject.SetActive(false); // To make sure the prefab is disabled and no Awake() is called
            var itmGO = UnityEngine.Object.Instantiate(ogItem).gameObject;
            itmGO.name = typeof(T).Name;
            itmGO.ConvertToPrefab(true);

            var genItemComp = itmGO.GetComponent<Item>();

            T newItm = itmGO.AddComponent<T>();

            if (genItemComp.GetType().IsAssignableFrom(newItm.GetType())) // basically if T inherits itm
                newItm = newItm.GetACopyFromFields(genItemComp);

            ogItem.gameObject.SetActive(true); // Forgot about this lol

            UnityEngine.Object.Destroy(genItemComp);

            return newItm;
        }

        // ITM_BSODA Extensions
        public static void DestroyParticleIfItHasOne(this ITM_BSODA bsoda)
        {
            var parts = bsoda.GetComponentInChildren<ParticleSystem>(true);
            if (parts)
            {
                if (parts.GetComponent<ITM_BSODA>()) // Immediate destruction to avoid stuff like ReusableInstance not helping out
                    UnityEngine.Object.DestroyImmediate(parts);
                else
                    UnityEngine.Object.DestroyImmediate(parts.gameObject);
            }
        }

        public static void IndividuallySpawn(this ITM_BSODA soda, EnvironmentController ec, Vector3 position, Vector3 direction, bool randomizeRotation)
        {
            soda.ec = ec;
            soda.transform.position = position;
            soda.transform.forward = direction;
            soda.entity.Initialize(ec, position);
            if (randomizeRotation)
                soda.spriteRenderer.SetSpriteRotation(UnityEngine.Random.Range(0f, 360f));
            soda.moveMod.priority = 1;
        }
        public static void IndividuallySpawn(this ITM_BSODA soda, EnvironmentController ec, Vector3 position, Vector3 direction) =>
            soda.IndividuallySpawn(ec, position, direction, true);
    }

    // Particle extensions
    public static class ParticleExtensions
    {
        public static CoverCloud GetRawChalkParticleGenerator(bool visualOnly = false)
        {
            var chalk =
                ItemMetaStorage.Instance.FindByEnum(Items.ChalkEraser).value.item as ChalkEraser;
            chalk.gameObject.SetActive(false);
            var chalkClone = UnityEngine.Object.Instantiate(chalk);
            var chalkTransform = chalkClone.transform;

            UnityEngine.Object.Destroy(chalkClone.GetComponent<RendererContainer>());
            if (visualOnly)
            {
                foreach (var collider in chalkTransform.GetComponentsInChildren<Collider>())
                {
                    collider.isTrigger = true;
                    collider.enabled = false;
                    collider.gameObject.layer = LayerStorage.ignoreRaycast; // Basically make the colliders useless lol
                }
            }
            UnityEngine.Object.Destroy(chalkClone);

            var blocker = chalkClone.cloud; // Should make this work good
            blocker.coverage = visualOnly ? CellCoverage.None : blocker.coverage;

            chalk.gameObject.SetActive(true);
            chalkTransform.gameObject.ConvertToPrefab(true);
            chalkTransform.name = "RawChalkGenerator";

            return blocker;
        }

        public static ParticleSystem GetNewParticleSystem(this ParticleSystem original, out ParticleSystemRenderer particleSystemRenderer)
        {
            var particleObject = original.gameObject;
            UnityEngine.Object.DestroyImmediate(original); // Destroys original Particle instance
            particleSystemRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
            return particleObject.AddComponent<ParticleSystem>();
        }
    }

    // Cell/Level extensions
    public static class LevelExtensions
    {
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
                    ec.CellFromPosition(cell.position + dir.ToIntVector2())
                        .Block(dir.GetOpposite(), block);
                }
            }
        }
        public static LevelType GetCustomLevelType(string lvlTypeName) // For the future, when mods start adding their LevelTypes, so I have this one function that should make it possible to support
                                                                       // all of them, even if they are not in the base game
        {
            try
            {
                return EnumExtensions.GetFromExtendedName<LevelType>(lvlTypeName);
            }
            catch
            {
                // If it doesn't exist, just return the default one
                return LevelType.Schoolhouse;
            }
        }
    }

    // Array extensions
    public static class ArrayExtensions
    {
        public static T[] Take<T>(this T[] ar, int count) =>
            ar.Take(0, count);
        public static T[] Take<T>(this T[] ar, int index, int count)
        {
            var newAr = new T[count];
            for (int z = 0; z < count; z++)
                newAr[z] = ar[index++];
            return newAr;
        }
        public static Sprite[] RemoveEmptySprites(this Sprite[] sprites)
        {
            int countFromEnd = 0;
            for (int i = sprites.Length - 1; i > 0; i--)
            {
                bool hasAnyAlpha = false;
                var spr = sprites[i];

                // Check all the area indicated by the sprite to tell whether it is transparent or not
                int xMin = Mathf.RoundToInt(spr.textureRect.x);
                int xMax = Mathf.RoundToInt(spr.textureRect.x + spr.textureRect.width);
                int yMax = Mathf.RoundToInt(spr.textureRect.y + spr.textureRect.height);
                int yMin = Mathf.RoundToInt(spr.textureRect.y);
                for (int x = xMin;
                x < xMax && // Normal loop
                !hasAnyAlpha; // The loop can break if alpha is set to true
                x++)
                {
                    for (int y = yMin; y < yMax; y++)
                    {
                        if (spr.texture.GetPixel(x, y).a != 0f) // Checks if it has any alpha
                        {
                            hasAnyAlpha = true;
                            break;
                        }
                    }
                }

                if (!hasAnyAlpha)
                    countFromEnd++; // If it is all clear, it should be removed from the array
            }

            // Build the new sprites
            if (countFromEnd >= sprites.Length)
                return Array.Empty<Sprite>();
            Sprite[] newSprs = new Sprite[sprites.Length - countFromEnd];
            for (int i = 0; i < newSprs.Length; i++)
                newSprs[i] = sprites[i];

            return newSprs;
        }
    }

    // SpriteRenderer extensions
    public static class SpriteRendererExtensions
    {
        static MaterialPropertyBlock _propertyBlock = new();
        public static void RotateBy(this SpriteRenderer renderer, float angle)
        {
            renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat("_SpriteRotation", _propertyBlock.GetFloat("_SpriteRotation") + angle);
            renderer.SetPropertyBlock(_propertyBlock);
        }
    }

    // Reusable item extensions
    public static class ReusableExtensions
    {
        public static ItemObject[] CreateNewReusableInstances<T>(
            this T ogItem,
            ItemObject ogItmObj,
            string nameKey,
            int count
        )
            where T : Item
        {
            var instances = new ItemObject[count + 1];
            instances[0] = ogItmObj;

            var meta = ogItmObj.GetMeta();
            meta.itemObjects = new ItemObject[count + 1];
            meta.itemObjects[0] = ogItmObj;

            for (; count > 0; count--)
            {
                ogItem = ogItem.Internal_CreateNewReusableInstance(
                    ogItmObj,
                    nameKey,
                    count,
                    out var itemObject
                );
                meta.itemObjects[meta.itemObjects.Length - count] = itemObject;
                instances[instances.Length - count] = itemObject;
                itemObject.AddMeta(meta);
            }
            return instances;
        }

        static T Internal_CreateNewReusableInstance<T>(
            this T ogItem,
            ItemObject ogItmObj,
            string newNameKey,
            int count,
            out ItemObject newItmObj
        )
            where T : Item
        {
            newItmObj = UnityEngine.Object.Instantiate(ogItmObj);
            newItmObj.nameKey = $"{newNameKey}_{count}";
            newItmObj.name = $"ItmOb_{newItmObj.nameKey}";

            ogItmObj.item.gameObject.SetActive(false); // To make sure the prefab is disabled and no Awake() is called
            var newItm = UnityEngine.Object.Instantiate(ogItmObj.item as T);
            ogItmObj.item.gameObject.SetActive(true);

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
