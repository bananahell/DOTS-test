using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class TestingTriangleGrid : MonoBehaviour {

  [SerializeField] private Transform trianglePrefab;
  [SerializeField] private Material materialUp;
  [SerializeField] private Material materialDown;
  [SerializeField] private Material materialSelected;
  [SerializeField] private Material materialUnwalkable;
  [SerializeField] private Transform monkey;
  [SerializeField] private Transform absoluteHorror;
  [SerializeField] private UnitSO monkeyUnitSO;

  public Material closedTriangle;
  public Material openTriangle;
  public Material pathTriangle;

  private PathNodeTriangleXZ lastGridObject;
  private float triangleSide;
  private float triangleHeight;
  private float triangleHeightTwoThirds;
  private Unit monkeyUnit;

  public static TestingTriangleGrid Instance { get; private set; }

  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
  private static void Init() {
    Instance = null;
  }

  private void Awake() {
    if (Instance != null) {
      Debug.LogError("Instance of PathfindingTriangleXZ already exists!");
    }
    Instance = this;
    int width = 40;
    int height = 20;
    this.triangleSide = 0.375f;
    this.triangleHeight = (float)(Math.Sqrt(3) * this.triangleSide / 2);
    this.triangleHeightTwoThirds = this.triangleHeight * 2 / 3;
    _ = new GridTriangleXZ<PathNodeTriangleXZ>(width,
                                               height,
                                               this.triangleSide,
                                               Vector3.zero,
                                               (int x, int y) => new PathNodeTriangleXZ(x, y));
    Quaternion rotation;
    for (int x = 0; x < width; x++) {
      for (int z = 0; z < height; z++) {
        bool rotationDir = x % 2 == z % 2;
        rotation = rotationDir ? new Quaternion(0, 0, 0, 0) : new Quaternion(0, 180, 0, 0);
        Transform visualTransform = Instantiate(this.trianglePrefab,
                                                GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWorldPosition(x, z),
                                                rotation);
        Material material = rotationDir ? this.materialUp : this.materialDown;
        PathNodeTriangleXZ node = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(x, z);
        node.visualTransform = visualTransform;
        node.AttachUnselectedOriginalMaterial(material);
        node.visualTransform.Find(Globals.SELECTED_STRING).gameObject.GetComponent<Renderer>().material = this.materialSelected;
        node.Hide();
      }
    }
  }

  private void Start() {
    this.monkeyUnit = new Unit(this.monkey, 5, 3, this.monkeyUnitSO);
  }

  private void Update() {
    this.lastGridObject?.Hide();
    this.lastGridObject = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(Mouse3D.GetMouseWorldPosition());
    this.lastGridObject?.Show();
    if (Input.GetMouseButtonDown(1)) {
      Vector3 mousePosition = Mouse3D.GetMouseWorldPosition();
      GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetXZ(mousePosition, out int endX, out int endZ);
      NativeList<int2> resultPath = new(Allocator.TempJob);
      FindPathJob findPathJob = new() {
        startPosition = new int2(this.monkeyUnit.x, this.monkeyUnit.z),
        endPosition = new int2(endX, endZ),
        gridSize = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetMapSize(),
        triangleSide = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetTriangleSide(),
        triangleHeight = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetTriangleHeight(),
        path = resultPath
      };
      JobHandle jobHandle = findPathJob.Schedule();
      jobHandle.Complete();
      if (!resultPath.IsEmpty) {
        int2 lastNode = resultPath.ElementAt(0);
        Vector3 lastPosition = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWorldPosition(lastNode.x, lastNode.y);
        this.monkeyUnit.MoveUnit(lastPosition);
        Debug.Log("Path created!");
        Debug.Log("----------------------------------------------------------------");
        for (int i = 0; i < resultPath.Length; ++i) {
          Debug.Log("--");
          Debug.Log(resultPath.ElementAt(i).x.ToString());
          Debug.Log(resultPath.ElementAt(i).y.ToString());
        }
        Debug.Log("----------------------------------------------------------------");
      }
      resultPath.Dispose();
    }
    if (Input.GetKeyDown(KeyCode.F)) {
      PathNodeTriangleXZ node = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetGridObject(Mouse3D.GetMouseWorldPosition());
      node.SetIsWalkable(false);
      node.AttachUnselectedOriginalMaterial(this.materialUnwalkable);
    }
  }

  public float GetTriangleHeightTwoThirds() {
    return this.triangleHeightTwoThirds;
  }

}

public class Unit {

  public Transform transformInstance;
  public int x;
  public int z;
  public Vector3 position;
  public float maxWalkDistance;
  public UnitSizeSO unitSize;

  private enum UnitSizes {
    Smallest,
    Small,
    Medium,
    Big
  }

  public Unit(Transform transform, int x, int z, UnitSO unitSO) {
    this.position = GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetWorldPosition(x, z);
    this.transformInstance = UnityEngine.Object.Instantiate(transform, this.position, Quaternion.identity);
    this.x = x;
    this.z = z;
    this.maxWalkDistance = unitSO.defaultMaxWalkDistance * TestingTriangleGrid.Instance.GetTriangleHeightTwoThirds();
    this.unitSize = unitSO.unitSize;
    switch (unitSO.unitSize.sizeName) {
      case Globals.UNITSIZE_SMALLEST:
        // Transform always starts out as smallest
        break;
      case Globals.UNITSIZE_SMALL:
        this.transformInstance.localScale *= 2;
        break;
      case Globals.UNITSIZE_MEDIUM:
        this.transformInstance.localScale *= 3.4f;
        break;
      case Globals.UNITSIZE_BIG:
        this.transformInstance.localScale *= 4.1f;
        break;
      default:
        // Leave transform as smallest
        break;
    }
  }

  public void MoveUnit(Vector3 position) {
    GridTriangleXZ<PathNodeTriangleXZ>.Instance.GetXZ(position, out this.x, out this.z);
    this.transformInstance.position = position;
    this.position = position;
  }

}
