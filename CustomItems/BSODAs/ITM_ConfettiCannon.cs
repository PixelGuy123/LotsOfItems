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
    private float recoilForce = 35f, pushForce = 100f;
    [SerializeField]
    private float planeSpeed = 50f;

    public void SetupPrefab(ItemObject itm)
    {
        // Create the confetti plane prefab
        var planeObjHolder = new GameObject("ConfettiPlane")
        {
            layer = LayerStorage.ignoreRaycast
        };
        MakeQuad(false);
        MakeQuad(true);
        planeObjHolder.ConvertToPrefab(true);

        var rb = planeObjHolder.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.drag = 0.5f;
        rb.freezeRotation = false;

        // Make sure to not break collision
        var box = planeObjHolder.gameObject.AddComponent<BoxCollider>();
        box.size = Vector3.one * 7f;
        box.isTrigger = true;

        planePrefab = planeObjHolder.AddComponent<ConfettiPlane>();
        planePrefab.pushForce = pushForce;
        planePrefab.audMan = planeObjHolder.CreateAudioManager(20f, 40f);
        planePrefab.audMan.pitchModifier = 1.25f;
        planePrefab.rb = rb;
        planePrefab.hitSounds = [.. ((Baldi)NPCMetaStorage.Instance.Get(Character.Baldi).value).correctSounds.ConvertAll(x => x.selection)];

        audUse = this.GetSoundNoSub("ConfettiCannon_Shoot.wav", SoundType.Effect);

        GameObject MakeQuad(bool isBack)
        {
            var planeObj = GameObject.CreatePrimitive(PrimitiveType.Quad);
            planeObj.name = "ConfettiPlaneRenderer_" + (isBack ? "Back" : "Front");
            planeObj.transform.localScale = Vector3.one * 10f;
            planeObj.transform.SetParent(planeObjHolder.transform);
            planeObj.transform.localPosition = Vector3.zero;
            planeObj.transform.localRotation = isBack ? Quaternion.identity : Quaternion.Euler(0f, 180f, 0f);

            var planeRenderer = planeObj.GetComponent<MeshRenderer>();
            planeRenderer.material = new Material(Shader.Find(LotOfItemsPlugin.tileAlpha_shader))
            {
                mainTexture = this.GetTexture("ConfettiCannon_Plane.png"),
                name = "ConfettiMat"
            };
            planeRenderer.transform.localScale = Vector3.one * 10f;
            Destroy(planeObj.GetComponent<MeshCollider>()); // We're not using mesh collider, but box collision
            return planeObj;
        }
    }
    public void SetupPrefabPost() { }

    public override bool Use(PlayerManager pm)
    {
        Transform camera = Singleton<CoreGameManager>.Instance.GetCamera(pm.playerNumber).transform;

        // Spawn confetti plane
        ConfettiPlane plane = Instantiate(planePrefab, camera.position + camera.forward * 2f, camera.rotation);
        plane.rb.velocity = camera.forward * planeSpeed;
        plane.transform.rotation = Random.rotation;
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
    public float maxAngularVelocity = 5f;

    public void RandomizeAngularVelocity()
    {
        var vel = rb.angularVelocity;
        Vector3 random = Random.insideUnitSphere;
        vel.x += random.x * Random.Range(0f, maxAngularVelocity);
        vel.y += random.y * Random.Range(0f, maxAngularVelocity);
        vel.z += random.z * Random.Range(0f, maxAngularVelocity);
        rb.angularVelocity = vel;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger && other.CompareTag("NPC") && other.TryGetComponent<NPC>(out var npc))
        {
            Vector3 pushDir = (npc.transform.position - transform.position).normalized;
            npc.Navigator.Entity.AddForce(new Force(pushDir, pushForce, -pushForce));

            Vector3 vel = rb.velocity;
            vel.y = Mathf.Abs(vel.y * 1.085f); // Turns absolute to go up
            rb.velocity = vel * 1.1f; // Get a even stronger boost forwards

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