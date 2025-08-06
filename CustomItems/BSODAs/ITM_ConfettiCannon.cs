using LotsOfItems.ItemPrefabStructures;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using UnityEngine;

namespace LotsOfItems.CustomItems.BSODAs;

public class ITM_ConfettiCannon : Item, IItemPrefab
{
    [SerializeField]
    private ConfettiPlane planePrefab;
    [SerializeField]
    private SoundObject audUse;

    [SerializeField]
    private float recoilForce = 40f;
    [SerializeField]
    private float planeSpeed = 50f;

    public void SetupPrefab(ItemObject itm)
    {
        // Create the confetti plane prefab
        var planeObj = ObjectCreationExtensions.CreateSpriteBillboard(this.GetSprite("ConfettiCannon_Plane.png", 35f), false);
        planeObj.name = "ConfettiPlane";
        planeObj.gameObject.layer = LayerStorage.ignoreRaycast;
        planeObj.gameObject.ConvertToPrefab(true);

        var rb = planeObj.gameObject.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.drag = 0.5f;

        var box = planeObj.gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(2f, 0.5f, 2f);
        box.isTrigger = true;

        planePrefab = planeObj.gameObject.AddComponent<ConfettiPlane>();
        planePrefab.pushForce = 30f;
        planePrefab.audMan = planeObj.gameObject.CreateAudioManager(20f, 40f);
        planePrefab.audMan.pitchModifier = 1.25f;
        planePrefab.rb = rb;
        planePrefab.hitSounds = [.. ((Baldi)NPCMetaStorage.Instance.Get(Character.Baldi).value).correctSounds.ConvertAll(x => x.selection)];

        audUse = this.GetSoundNoSub("ConfettiCannon_Shoot.wav", SoundType.Effect);
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        Transform camera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform;

        // Spawn confetti plane
        ConfettiPlane plane = Instantiate(planePrefab, camera.position + camera.forward * 2f, camera.rotation);
        plane.rb.velocity = camera.forward * planeSpeed;

        // Apply recoil
        pm.plm.Entity.AddForce(new Force(-camera.forward, recoilForce, -recoilForce / 2f));

        Singleton<CoreGameManager>.Instance.audMan.PlaySingle(audUse);
        Destroy(gameObject);
        return true;
    }
}

public class ConfettiPlane : MonoBehaviour
{
    public float pushForce;
    public AudioManager audMan;
    public Rigidbody rb;
    public SoundObject[] hitSounds;

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
        {
            Vector3 pushDir = (npc.transform.position - transform.position).normalized;
            npc.Navigator.Entity.AddForce(new Force(pushDir, pushForce, -pushForce));

            Vector3 vel = rb.velocity;
            vel.y = Mathf.Abs(vel.y); // Turns absolute to go up
            rb.velocity = vel * 1.25f; // Get a even stronger boost forwards

            vel = rb.angularVelocity;
            Vector3 random = Random.insideUnitSphere;
            vel.x *= random.x;
            vel.y *= random.y;
            vel.z *= random.z;
            rb.angularVelocity = vel;

            audMan.pitchModifier = Random.Range(0.75f, 1.35f);
            audMan.PlayRandomAudio(hitSounds);
        }
    }

    void Update()
    {
        if (transform.position.y < -10f)
            Destroy(gameObject);
    }
}