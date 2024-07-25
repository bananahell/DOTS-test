using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class UnitSizeSO : ScriptableObject {

  public string sizeName;
  public List<GridPosition> gridPositionsUp;
  public List<GridPosition> gridPositionsDown;

}

[Serializable]
public struct GridPosition {
  public int x;
  public int z;
}
