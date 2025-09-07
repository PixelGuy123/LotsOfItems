using LotsOfItems.ItemPrefabStructures;
using LotsOfItems.Plugin;
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
    private float recoilForce = 65f, pushForce = 100f;
    [SerializeField]
    private float planeSpeed = 50f;

    public void SetupPrefab(ItemObject itm)
    {
        // Create the confetti plane prefab
        var planeObj = GameObject.CreatePrimitive(PrimitiveType.Plane);
        planeObj.name = "ConfettiPlane";
        planeObj.layer = LayerStorage.ignoreRaycast;
        planeObj.transform.localScale = Vector3.one * 8.5f;
        planeObj.ConvertToPrefab(true);

        var planeRenderer = planeObj.GetComponent<MeshRenderer>();
        planeRenderer.material = new Material(Shader.Find(LotOfItemsPlugin.tileAlpha_shader))
        {
            mainTexture = this.GetTexture("ConfettiCannon_Plane.png"),
            name = "ConfettiMat"
        };

        var rb = planeObj.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.drag = 0.5f;
        rb.freezeRotation = false;
        rb.isKinematic = true;

        // Make sure to not break collision
        Destroy(planeObj.GetComponent<MeshCollider>()); // We're not using mesh collider, but box collision
        var box = planeObj.gameObject.AddComponent<BoxCollider>();
        box.size = new Vector3(2f, 0.5f, 2f);
        box.isTrigger = true;

        planePrefab = planeObj.AddComponent<ConfettiPlane>();
        planePrefab.pushForce = pushForce;
        planePrefab.audMan = planeObj.CreateAudioManager(20f, 40f);
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
        plane.RandomizeAngularVelocity();

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

    public void RandomizeAngularVelocity()
    {
        var vel = rb.angularVelocity;
        Vector3 random = Random.insideUnitSphere;
        vel.x *= random.x;
        vel.y *= random.y;
        vel.z *= random.z;
        rb.angularVelocity = vel;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
        {
            Vector3 pushDir = (npc.transform.position - transform.position).normalized;
            npc.Navigator.Entity.AddForce(new Force(pushDir, pushForce, -pushForce));

            Vector3 vel = rb.velocity;
            vel.y = Mathf.Abs(vel.y); // Turns absolute to go up
            rb.velocity = vel * 1.25f; // Get a even stronger boost forwards

            RandomizeAngularVelocity();

            audMan.pitchModifier = Random.Range(0.75f, 1.35f);
            audMan.PlayRandomAudio(hitSounds);
        }
    }

    void Update()
    {
        if (transform.position.y < -10f)
        {
            Vector3 pos = transform.position;
            pos.y = -10f;
            transform.position = pos;
            if (!audMan.AnyAudioIsPlaying)
                Destroy(gameObject);
        }
    }
}