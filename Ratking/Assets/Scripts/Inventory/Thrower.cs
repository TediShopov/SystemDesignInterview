using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;


public struct PredictedSoundInfo
{
    public Vector3 WorldPosition;
    public Vector3 LocalScale;
    public bool IsBreaking;
}

public class Thrower : MonoBehaviour
{
    [SerializeField] private readonly int _maxPhysicsFrameIterations = 100;

    //Dynamic object need to be updated every single simulation so to be synced with th
    // other physics scene
    private Dictionary<GameObject, GameObject> _dynamicObjects;
    private PhysicsScene2D _physicsScene;
    private Scene _simulationScene;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] public GameObject ThrowItemPrefab;

    [Header("Throw Power Calculation")]
    [Range(0, 3)] public float MaxDistance;
    [Range(0, 50)] public float MaxPower;
    [Range(0, 3)] public float MinDistance;
    [Range(0, 10)] public float MinPower;
    [Range(0, 50)] public float StartDistanceFromPlayer;

    [Header("Predicted Throw Effects")]
    [HideInInspector] public List<PredictedSoundInfo> PredictedSoundPositions;
    [HideInInspector] public List<GameObject> PredictedSoundPrefabs;
    [SerializeField] public GameObject PredictedSoundPrefab;
    [SerializeField] public GameObject PredictedItemBreakObject;
    [SerializeField][Range(1, 5)] public int MaxPredictedSounds;
    public delegate void ThrowEvent(GameObject itemThrown, Vector2 impulse);

    public event ThrowEvent OnItemThow;

    private RatAudioPlayer _ratAudioPlayer;
    // Start is called before the first frame update
    private void Start()
    {
        _ratAudioPlayer = GetComponent<RatAudioPlayer>();
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        _dynamicObjects = new Dictionary<GameObject, GameObject>();
        this.PredictedSoundPrefabs = new List<GameObject>();
        for (int i = 0; i < MaxPredictedSounds; i++)
        {
            PredictedSoundPrefabs.Add(Instantiate(PredictedSoundPrefab));
        }
        this.PredictedSoundPositions = new List<PredictedSoundInfo>();
        this.PredictedItemBreakObject = Instantiate(PredictedItemBreakObject, new Vector3(-999, -999, -999), Quaternion.identity);

        //Used to set all prefabs to disable as there are currently no predicted sounds
        VisualizePRedictedSounds();

        CreatePhysicsScene();
    }

    // Update is called once per frame

    private void Update()
    {
        //Item to be thrown should be assigned in a method from other higher systems
       //UpdateThrowItem();
        ResetPredictedSound();
        if (ThrowItemPrefab == null)
        {

            _lineRenderer.positionCount = 0;
            return;
        }
        if (LevelData.InventoryView == null || LevelData.InventoryView.gameObject.activeSelf==true)
            return;


        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        SynchronizeDynamicObjects();
        SimulateTrajectory(worldPos);
        VisualizePRedictedSounds();
      //  if () Throw(worldPos);
      //  if (Input.GetMouseButtonDown(1)) LevelData.InventoryView.gameObject.SetActive(true);
    }

    public void throwOnInput(InputAction.CallbackContext context)
    {
        if (LevelData.InventoryView == null || LevelData.InventoryView.gameObject.activeSelf == true)
            return;
        if (!context.performed)
            return;
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        Throw(worldPos);
    }

    public void OnDrawGizmosSelected()
    {
        DrawDebugZones();
    }

    public void returnToInventoryScreen(InputAction.CallbackContext context)
    {
        if (LevelData.InventoryView == null || LevelData.InventoryView.gameObject.activeSelf == true)
            return;
        if (!(context.performed))
            return;

        var isCollectible = ThrowItemPrefab.GetComponent<Collectible>() != null;
        if (isCollectible && LevelData.InventoryView.CanEnable())
        {
            LevelData.InventoryView.gameObject.SetActive(true);
        }
        else if (isCollectible != null)
        {
            this.ThrowItemPrefab = null;
        }
       
    }

    //public void UpdateThrowItem()
    //{
    //    if (ThrowItemPrefab == null)
    //    {
    //        //Select Inventory Unplaced Item as throw item
    //        if (LevelData.InventoryView == null)
    //            return;

    //        if (LevelData.InventoryView.ItemToPlace != null)
    //            ThrowItemPrefab = LevelData.InventoryView.ItemToPlace.ItemData.PrefabReference;
    //        else
    //            ThrowItemPrefab = null;
    //    }

    //}

    public void Throw(Vector2 mousePos)
    {
       
        if (ThrowItemPrefab == null)
            return;

        ////TODO check new throwing criteria
        //if (!CanSpawnItemAtThrowPosition(GetStartThrowPostion(mousePos)))
        //    return;


        GameObject spawnedObj = Instantiate(ThrowItemPrefab, GetStartThrowPostion(mousePos), Quaternion.identity);

        var collectible = spawnedObj.GetComponent<Collectible>();
        if (collectible != null)
        {
            if (LevelData.InventoryView.ItemToPlace)
            {
                collectible.UniquenessHash = LevelData.InventoryView.ItemToPlace.UniqueCollectibleHash;
            }

        }
        AddObjectToPhysicsSimulation(spawnedObj);
        _ratAudioPlayer.PlayAudio(_ratAudioPlayer.throwClip);

        var impulse = ApplyThrowForce(spawnedObj, mousePos);
        if (OnItemThow != null)
            OnItemThow.Invoke(spawnedObj, impulse);

        ThrowItemPrefab = null;
        //LevelData.InventoryView.DestroyItemToPlace();

        //LevelData.InventoryView.ItemToPlace = null;
        //LevelData.InventoryView.RemoveItemToPlace();
    }

    private Vector2 ApplyThrowForce(GameObject obj, Vector2 mousePos)
    {
        Rigidbody2D rigidbody = obj.GetComponent<Rigidbody2D>();

        Vector2 pos = transform.position;
        Vector2 direction = GetStartThrowPostion(mousePos) - pos;
        direction.Normalize();
        direction *= CalculateImpulseMagnitude(mousePos);
        rigidbody.AddForce(direction, ForceMode2D.Impulse);
        return direction;
    }

    private void SynchronizeDynamicObjects()
    {
        var toRemoveFromPhysicsSimulation = new List<GameObject>();
        foreach (var dynamicObject in _dynamicObjects)
            if (dynamicObject.Key == null)
                toRemoveFromPhysicsSimulation.Add(dynamicObject.Value);

        foreach (GameObject objToRemove in toRemoveFromPhysicsSimulation)
        {
            GameObject physicsObject = _dynamicObjects[objToRemove];
            Destroy(physicsObject);
            _dynamicObjects.Remove(objToRemove);
        }


        foreach (var dynamicObject in _dynamicObjects)
        {
            Rigidbody2D rbPhysicsObject = dynamicObject.Value.GetComponent<Rigidbody2D>();
            Rigidbody2D rbActualObject = dynamicObject.Key.GetComponent<Rigidbody2D>();
            rbPhysicsObject.position = rbActualObject.position;
            rbPhysicsObject.rotation = rbActualObject.rotation;
            rbPhysicsObject.velocity = rbActualObject.velocity;
            rbPhysicsObject.angularDrag = rbActualObject.angularDrag;
            rbPhysicsObject.angularVelocity = rbActualObject.angularVelocity;
        }
    }

    private void AddObjectToPhysicsSimulation(GameObject gameObjectToCopy)
    {
        if (gameObjectToCopy.layer == LayerMask.NameToLayer("Player"))
        {
            return;
        }

        GameObject ghostObj = Instantiate(gameObjectToCopy, gameObjectToCopy.transform.position,
            gameObjectToCopy.transform.rotation);
        ghostObj.transform.localScale = gameObjectToCopy.transform.lossyScale;
        Rigidbody2D rb = ghostObj.GetComponent<Rigidbody2D>();

        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic) _dynamicObjects.Add(gameObjectToCopy, ghostObj);

        DestroyImmediate(ghostObj.GetComponent<SoundSpawner>());
        DestroyImmediate(ghostObj.GetComponent<CraftingResource>());

        DestroyImmediate(ghostObj.GetComponent<Collectible>());

        //DestroyImmediate(ghostObj.GetComponent<Properties>());
        Helpers.DeleteAllChildren(ghostObj.transform);


        Renderer renderer = ghostObj.GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
        SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
    }

    public void SetThrowItem(GameObject throwItemPrefab)
    {
        ThrowItemPrefab = throwItemPrefab;
    }

    private void CreatePhysicsScene()
    {
        _simulationScene =
            SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics2D));
        _physicsScene = _simulationScene.GetPhysicsScene2D();
        var allObjwithColliders = FindObjectsOfType<Collider2D>();

        foreach (Collider2D obj in allObjwithColliders)
        {
            //if ( obj.gameObject.layer == LayerMask.GetMask("Gathering")) continue;
            //AddObjectToPhysicsSimulation(obj.gameObject);
            if (Physics2D.GetIgnoreLayerCollision(LayerMask.NameToLayer("Gathering"), obj.gameObject.layer) || obj.isTrigger) continue;
            AddObjectToPhysicsSimulation(obj.gameObject);
        }

    }

    private void SimulateTrajectory(Vector2 mousePos)
    {

        GameObject ghostObj = Instantiate(ThrowItemPrefab, GetStartThrowPostion(mousePos), Quaternion.identity);
        var collectibleOriginal = ghostObj.GetComponent<Collectible>();
        if (collectibleOriginal!=null)
        {
            var collectibleGhost = ghostObj.AddComponent<CollectibleGhost>();
            collectibleGhost.ThrowerScript = this;
            collectibleGhost.DamageThreshold = collectibleOriginal.DamageThreshold;
        }
        //collectibleGhost.SoundProperties = ThrowItemPrefab.GetComponent<Properties>();
        DestroyImmediate(ghostObj.GetComponent<CraftingResource>());
        DestroyImmediate(collectibleOriginal);



        Renderer renderer = ghostObj.GetComponent<Renderer>();
        if (renderer != null) renderer.enabled = false;
        SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);

        ApplyThrowForce(ghostObj, mousePos);
        _lineRenderer.positionCount = _maxPhysicsFrameIterations;


        Vector3 lastPosition = new Vector3();
        for (var i = 0; i < _maxPhysicsFrameIterations; i++)
        {
            _physicsScene.Simulate(Time.fixedDeltaTime);
            bool isBroken = this.PredictedSoundPositions.Any(x => x.IsBreaking);
            if (!isBroken)
            {
                lastPosition = ghostObj.transform.position;
                _lineRenderer.SetPosition(i, ghostObj.transform.position);

            }
            else
            {
                _lineRenderer.SetPosition(i, lastPosition);

            }
        }

        Destroy(ghostObj);
    }

    public bool CanSpawnItemAtThrowPosition(Vector2 spawnPos)
    {
        if (ThrowItemPrefab == null)
            return false;
        Vector2 pos = transform.position;
        Vector2 dir = (pos - spawnPos).normalized;
        var minDistance = Vector2.Distance(pos, spawnPos);
        var layerMask = ~LayerMask.GetMask("Player", "Default", "Smell");
        RaycastHit2D hits = Physics2D.Raycast(spawnPos, dir, minDistance, layerMask);
        Debug.DrawRay(spawnPos, dir, Color.red, 2.0f);
        if (hits.collider == null) return true;

        return false;
    }

    public Vector2 GetStartThrowPostion(Vector2 mousePos)
    {
        Vector2 pos = gameObject.transform.position;
        Vector2 direction = (mousePos - pos).normalized;
        Vector2 toReturn = pos + direction * StartDistanceFromPlayer;
        return toReturn;
    }

    public void DrawDebugZones()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, MinDistance);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, MaxDistance);
    }

    public float CalculateImpulseMagnitude(Vector2 mousePos)
    {
        var distance = Vector2.Distance(mousePos, transform.position);
        return Helpers.ConvertToNewRangeClamped(distance, MinDistance, MaxDistance, MinPower, MaxPower);
    }




    public void ResetPredictedSound()
    {
        this.PredictedSoundPositions.Clear();
        this.PredictedItemBreakObject.SetActive(false);
        foreach (var predictedSoundPrefab in this.PredictedSoundPrefabs)
        {
            predictedSoundPrefab.SetActive(false);
        }
    }

    public void VisualizePRedictedSounds()
    {

        PredictedSoundInfo? brokenPredictedSound = null;
        for (int i = 0; i < this.PredictedSoundPrefabs.Count; i++)
        {
            if (i < this.PredictedSoundPositions.Count)
            {
                PredictedSoundPrefabs[i].SetActive(true);
                PredictedSoundPrefabs[i].transform.position = PredictedSoundPositions[i].WorldPosition;
                PredictedSoundPrefabs[i].transform.localScale = PredictedSoundPositions[i].LocalScale;

                if (PredictedSoundPositions[i].IsBreaking)
                {
                    brokenPredictedSound = PredictedSoundPositions[i];
                }

            }
            else
            {
                PredictedSoundPrefabs[i].SetActive(false);
            }
        }


        if (brokenPredictedSound.HasValue)
        {
            this.PredictedItemBreakObject.SetActive(true);
            this.PredictedItemBreakObject.transform.position = brokenPredictedSound.Value.WorldPosition;
        }

    }
}