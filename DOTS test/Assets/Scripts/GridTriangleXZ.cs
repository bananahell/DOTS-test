using System;
using UnityEngine;

public class GridTriangleXZ<TGridObject> {

  public static GridTriangleXZ<TGridObject> Instance { get; private set; }

  public event EventHandler<OnGridObjectChangedEventArgs> OnGridObjectChanged;

  public class OnGridObjectChangedEventArgs : EventArgs {
    public int x;
    public int z;
  }

  private readonly int width;
  private readonly int height;
  private readonly float triangleSide;
  private readonly TGridObject[,] gridArray;
  private Vector3 originPosition;
  private readonly float triangleHeight;
  private readonly float triangleSideHalf;
  private readonly float triangleHeightTimesTwo;
  private readonly float triangleHeightFourThirds;
  private readonly float triangleHeightOneThird;

  public GridTriangleXZ(int width,
                        int height,
                        float triangleSide,
                        Vector3 originPosition,
                        Func<int, int, TGridObject> createGridObject) {
    /* TODO
    DOTS recommends turning off domain reload, which keeps static fields' values,
    so how do I PROPERLY do singletons? idk
    MonoBehaviour scripts have a monstruous attribute to solve it on Init, but
    what about normal classes? this is gonna be the temporary solution - just
    assigning this =)))))
    if (Instance != null) {
      Debug.LogError("Instance of GridTriangleXZ already exists!");
    }
    */
    Instance = this;
    this.width = width;
    this.height = height;
    this.triangleSide = triangleSide;
    this.triangleHeight = (float)(Math.Sqrt(3) * this.triangleSide / 2);
    this.triangleSideHalf = this.triangleSide / 2;
    this.triangleHeightTimesTwo = this.triangleHeight * 2;
    this.triangleHeightFourThirds = this.triangleHeight * 4 / 3;
    this.triangleHeightOneThird = this.triangleHeight / 3;
    this.originPosition = originPosition;
    this.gridArray = new TGridObject[width, height];
    for (int x = 0; x < this.gridArray.GetLength(0); x++) {
      for (int z = 0; z < this.gridArray.GetLength(1); z++) {
        this.gridArray[x, z] = createGridObject(x, z);
      }
    }
  }

  public int GetWidth() {
    return this.width;
  }

  public int GetHeight() {
    return this.height;
  }

  public int GetMapSize() {
    return this.height * this.width;
  }

  public float GetTriangleSide() {
    return this.triangleSide;
  }

  public float GetTriangleHeight() {
    return this.triangleHeight;
  }

  public Vector3 GetWorldPosition(int x, int z) {
    return x % 2 == 0
      ? z % 2 == 0
        ? new Vector3(x / 2 * this.triangleSide, 0,
                      z / 2 * this.triangleHeightTimesTwo) + this.originPosition
        : new Vector3(x / 2 * this.triangleSide, 0,
                      this.triangleHeightFourThirds + (z / 2 * this.triangleHeightTimesTwo)) + this.originPosition
      : z % 2 == 0
        ? new Vector3(this.triangleSideHalf + (x / 2 * this.triangleSide), 0,
                      this.triangleHeightOneThird + (z / 2 * this.triangleHeightTimesTwo)) + this.originPosition
        : new Vector3(this.triangleSideHalf + (x / 2 * this.triangleSide), 0,
                      this.triangleHeight + (z / 2 * this.triangleHeightTimesTwo)) + this.originPosition;
  }

  public void GetXZ(Vector3 worldPosition, out int x, out int z) {
    float roughX = (worldPosition.x - this.originPosition.x + this.triangleSideHalf) / this.triangleSideHalf;
    float roughZ = (worldPosition.z - this.originPosition.z + this.triangleHeightOneThird) / this.triangleHeight;
    int squareX = (int)Math.Floor(roughX);
    z = (int)Math.Floor(roughZ);
    if (squareX % 2 == z % 2) {
      bool linearXToZCheck = roughX - (float)Math.Truncate(roughX) < roughZ - (float)Math.Truncate(roughZ);
      x = linearXToZCheck ? squareX - 1 : squareX;
    } else {
      bool linearXToZInverseCheck = roughX - (float)Math.Truncate(roughX) < 1 - (roughZ - (float)Math.Truncate(roughZ));
      x = linearXToZInverseCheck ? squareX - 1 : squareX;
    }
  }

  public void SetGridObject(int x, int z, TGridObject value) {
    if (x >= 0 && z >= 0 && x < this.width && z < this.height) {
      this.gridArray[x, z] = value;
      OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
    }
  }

  public void TriggerGridObjectChanged(int x, int z) {
    OnGridObjectChanged?.Invoke(this, new OnGridObjectChangedEventArgs { x = x, z = z });
  }

  public void SetGridObject(Vector3 worldPosition, TGridObject value) {
    this.GetXZ(worldPosition, out int x, out int z);
    this.SetGridObject(x, z, value);
  }

  public TGridObject GetGridObject(int x, int z) {
    return x >= 0 && z >= 0 && x < this.width && z < this.height
             ? this.gridArray[x, z]
             : default;
  }

  public TGridObject GetGridObject(Vector3 worldPosition) {
    this.GetXZ(worldPosition, out int x, out int z);
    return this.GetGridObject(x, z);
  }

  public Vector2Int ValidateGridPosition(Vector2Int gridPosition) {
    return new Vector2Int(Mathf.Clamp(gridPosition.x, 0, this.width - 1),
                          Mathf.Clamp(gridPosition.y, 0, this.height - 1));
  }

  public bool IsValidGridPosition(Vector2Int gridPosition) {
    return gridPosition.x >= 0
             && gridPosition.y >= 0
             && gridPosition.x < this.width
             && gridPosition.y < this.height;
  }

}
