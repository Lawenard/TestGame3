using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerController : MonoBehaviour
{
    #region vars
    private readonly Vector3 growVector = new Vector3(1, 0, 1);

    // setup
    [SerializeField]
    private GameObject towerPartPrefab;
    [SerializeField]
    private Material wrongMat, ghostMat, baseMat;
    [SerializeField]
    private CameraMovement cameraMovement;
    [SerializeField]
    private GameSettings gameSettings;

    // settings
    [SerializeField]
    private Vector3 partMaxScale;
    [SerializeField]
    private float
        growSpeed, errorMargin,
        initialSize, minSize,
        perfectMargin, perfectDelay,
        perfectGrow, perfectShrink,
        perfectTowerGrow, perfectTowerShrink,
        errorShowTime;

    [SerializeField] [Tooltip("Sets part parameters from prefab instead of manual config")]
    private bool autoPartParameters;
    [SerializeField] [Tooltip("Use settings file to configure the game")]
    private bool loadParameters;

    // internal stuff
    private TowerPool towerPool;
    private Transform currentPart, lastPart;

    // game mechanics
    private int partCount;
    private bool gameRunning;
    private readonly List<int> perfectIds = new List<int>();
    #endregion

    // launch initialization and configuration
    private void Awake()
    {
        if (loadParameters)
            LoadParameters();

        if (autoPartParameters)
            partMaxScale = towerPartPrefab.transform.localScale;

        if (!towerPool)
        {
            towerPool = gameObject.AddComponent<TowerPool>();
            towerPool.SetPrefab(towerPartPrefab);
            towerPool.SetController(this);
        }
        Restart();
    }

    // load game settings from object
    private void LoadParameters()
    {
        if (!gameSettings)
        {
            Debug.LogError("Game Settings are not initialized!");
            return;
        }
        partMaxScale = gameSettings.partMaxScale;
        growSpeed = gameSettings.growSpeed;
        errorMargin = gameSettings.errorMargin;
        initialSize = gameSettings.initialSize;
        minSize = gameSettings.minSize;
        perfectMargin = gameSettings.perfectMargin;
        perfectDelay = gameSettings.perfectDelay;
        perfectGrow = gameSettings.perfectGrow;
        perfectShrink = gameSettings.perfectShrink;
        perfectTowerGrow = gameSettings.perfectTowerGrow;
        perfectTowerShrink = gameSettings.perfectTowerShrink;
        errorShowTime = gameSettings.errorShowTime;
    }

    // full restart of the game
    private void Restart()
    {
        StopAllCoroutines();

        gameRunning = true;
        partCount = 0;
        perfectIds.Clear();

        towerPool.DisableAll();
        SpawnPart(true);
        cameraMovement.Reset();
    }

    // player is placing the part so it scales
    private void PartGrow()
    {
        if (currentPart)
            currentPart.localScale += growVector * growSpeed * Time.deltaTime;
    }

    // makes everything that depends on player input happen
    private void PlayerUpdate()
    {
        if (Input.touchCount > 0)
        {
            switch (Input.GetTouch(0).phase)
            {
                case TouchPhase.Began:
                    {
                        if (gameRunning)
                            SpawnPart(false);
                    } break;
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    {
                        if (gameRunning)
                            FinishPart();
                        else
                            Restart();
                    } break;
            }
        }
    }

    // player stops placing a part
    private void FinishPart()
    {
        ValidatePart();
        currentPart = null;
    }

    // makes everything that depends on part scale differences happen
    private void ValidatePart()
    {
        float diff = currentPart.localScale.x - lastPart.localScale.x;
        if (diff < errorMargin)
        {
            currentPart.GetComponent<MeshRenderer>().material = baseMat;
            lastPart = currentPart;

            if (Mathf.Abs(diff) <= perfectMargin)
                StartCoroutine(PerfectMove());

            gameRunning = true;
        }
        else
        {
            currentPart.GetComponent<MeshRenderer>().material = wrongMat;
            cameraMovement.ShowTower(currentPart.position);
            gameRunning = false;
            StartCoroutine(DelayedDisable(currentPart.gameObject));
        }
    }

    // used to remove wrong part
    private IEnumerator DelayedDisable(GameObject partObj)
    {
        yield return new WaitForSeconds(errorShowTime);
        partObj.SetActive(false);
    }

    // whole perfect move algorithm
    private IEnumerator PerfectMove()
    {

        perfectIds.Add(partCount);
        float growSize, shrinkSize;

        // scaling new perfect part differently
        growSize = currentPart.localScale.x + perfectGrow;
        shrinkSize = growSize - perfectShrink;
        shrinkSize = ClampSize(shrinkSize);

        StartCoroutine(PerfectMovePartEffect(currentPart, growSize, shrinkSize));

        // working with every other part from top to bottom
        Transform part;
        bool perfect;
        // we're jumping down 1 to turn count into id
        // and then 1 more to jump to the last part before the perfect one
        int id = partCount - 2;
        while (id >= 0)
        {
            part = towerPool.GetTowerPart(id).transform;
            yield return new WaitForSeconds(perfectDelay);

            perfect = perfectIds.Exists(x => x == id);
            growSize = part.localScale.x + perfectTowerGrow;
            shrinkSize = part.localScale.x * (perfect ? 1 : perfectTowerShrink);
            shrinkSize = ClampSize(shrinkSize);

            StartCoroutine(PerfectMovePartEffect(part, growSize, shrinkSize));
            id--;
        }
    }

    // individual part scaling animation for perfect move
    private IEnumerator PerfectMovePartEffect(Transform part, float growSize, float shrinkSize)
    {
        while (part.localScale.x < growSize)
        {
            part.localScale = ClampScaling(part.localScale, growSize);
            yield return new WaitForEndOfFrame();
        }
        while (part.localScale.x > shrinkSize)
        {
            part.localScale = ClampScaling(part.localScale, shrinkSize);
            yield return new WaitForEndOfFrame();
        }
    }

    // used to create parts either immediately or as intended by gameplay
    private void SpawnPart(bool immediately)
    {
        GameObject partObj = towerPool.GetTowerPart(partCount);
        partObj.SetActive(true);

        Material material = immediately ? baseMat : ghostMat;
        partObj.GetComponent<MeshRenderer>().material = material;

        partCount++;

        if (immediately)
        {
            lastPart = partObj.transform;
            lastPart.localScale = partMaxScale;
        }
        else
        {
            currentPart = partObj.transform;
            currentPart.localScale = GetPartScale(initialSize);
        }
        cameraMovement.targetPosition = partObj.transform.position;
    }

    // well, you know...
    private void Update()
    {
        PlayerUpdate();

        if (gameRunning)
            PartGrow();
    }

    // shortcut: position of the part by its number in the tower
    public Vector3 GetTowerPartPosition(int id)
        => id * partMaxScale.y * Vector3.up * 2;

    // shortcut: scale by a value from horizontal plane
    public Vector3 GetPartScale(float size)
       => size * growVector + Vector3.up * partMaxScale.y;

    // shortcut: clamps size to allowed value
    private float ClampSize(float size)
        => Mathf.Clamp(size, minSize, partMaxScale.x);

    // avoids going out of scaling bounds with linear scaling
    // used in perfect move animations
    private Vector3 ClampScaling(Vector3 currentScale, float desiredSize)
    {
        bool shrink = desiredSize < currentScale.x;
        if (shrink)
        {
            currentScale -= growVector * growSpeed * Time.deltaTime;
            if (currentScale.x < desiredSize)
                return GetPartScale(desiredSize);
        }
        else
        {
            currentScale += growVector * growSpeed * Time.deltaTime;
            if (currentScale.x > desiredSize)
                return GetPartScale(desiredSize);
        }
        return currentScale;
    }
}
