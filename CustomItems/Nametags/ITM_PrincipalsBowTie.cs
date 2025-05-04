using System.Collections;
using LotsOfItems.ItemPrefabStructures;
using PixelInternalAPI.Classes;
using UnityEngine;

namespace LotsOfItems.CustomItems.Nametags
{
    public class ITM_PrincipalsBowTie : Item, IItemPrefab
    {
        public HudGauge gauge;

        [SerializeField]
        Sprite gaugeSprite;

        [SerializeField]
        float setTime = 60f;
        [SerializeField]
        private CapsuleCollider ignoreRaycastCollider;
        EnvironmentController ec;

        public void SetupPrefab(ItemObject itm)
        {
            gaugeSprite = itm.itemSpriteLarge;

            ignoreRaycastCollider = gameObject.AddComponent<CapsuleCollider>();
            ignoreRaycastCollider.isTrigger = true;
            ignoreRaycastCollider.gameObject.layer = LayerStorage.ignoreRaycast;
            ignoreRaycastCollider.radius = 3f;
        }


        public void SetupPrefabPost() { }

        public override bool Use(PlayerManager pm)
        {
            base.pm = pm;
            ec = pm.ec;
            gauge = Singleton<CoreGameManager>.Instance.GetHud(pm.playerNumber).gaugeManager.ActivateNewGauge(gaugeSprite, setTime);
            StartCoroutine(Timer(setTime));
            return true;
        }

        public IEnumerator Timer(float time)
        {
            while (time > 0f)
            {
                time -= Time.deltaTime * pm.PlayerTimeScale;
                gauge.SetValue(setTime, time);
                yield return null;
            }

            gauge.Deactivate();
            Deactivate();
        }

        void Deactivate()
        {
            Destroy(gameObject);
            pm.ClearGuilt();
        }

        void Update()
        {
            pm.RuleBreak("Bullying", 2f);
            transform.position = pm.transform.position;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
            {
                if (IsPrincipal(npc) || npc is Baldi) // If it is Baldi or inherits Baldi, it should always be ignored
                {
                    Deactivate();
                    return;
                }

                int num = Random.Range(0, ec.offices.Count);
                npc.Navigator.Entity.Teleport(ec.RealRoomMid(ec.offices[num]));
                npc.ClearGuilt();
                npc.SentToDetention();
            }
        }

        protected virtual bool IsPrincipal(NPC npc) =>
            npc.Character == Character.Principal;
    }
}
