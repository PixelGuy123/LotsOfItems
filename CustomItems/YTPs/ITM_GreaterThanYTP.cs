using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using UnityEngine;

namespace LotsOfItems.CustomItems.YTPs
{
    public class ITM_GreaterThanYTP : ITM_YTPs, IItemPrefab
    {
        public void SetupPrefab(ItemObject itm) => gaugeSprite = itm.itemSpriteLarge;
        public void SetupPrefabPost() { }

        [SerializeField]
        internal float rotationDuration = 15f;

        [SerializeField]
        internal float initialRotationSpeed = 35f;

        [SerializeField]
        internal Sprite gaugeSprite;

        HudGauge gauge;



        public override bool Use(PlayerManager pm)
        {
            Debug.Log("I\'m alive!");
            this.pm = pm;
            gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, rotationDuration);
            StartCoroutine(SpinEnumerator());
            return base.Use(pm);
        }

        IEnumerator SpinEnumerator()
        {
            Transform playerTransform = pm.transform;
            float elapsedTime = 0f;

            while (elapsedTime < rotationDuration)
            {
                Debug.Log("I\'m running this properly!");
                // Calculate current speed (linear decrease)
                float currentSpeed = Mathf.Lerp(initialRotationSpeed, 0f, elapsedTime / rotationDuration);

                // Apply rotation (clockwise around Y-axis)
                playerTransform.Rotate(Vector3.up, currentSpeed * Time.deltaTime * pm.ec.EnvironmentTimeScale, Space.World);

                elapsedTime += Time.deltaTime * pm.ec.EnvironmentTimeScale;
                gauge.SetValue(rotationDuration, rotationDuration - elapsedTime);
                yield return null; // Wait for the next frame
            }
            gauge.Deactivate();
            Destroy(gameObject);
        }
    }
}
