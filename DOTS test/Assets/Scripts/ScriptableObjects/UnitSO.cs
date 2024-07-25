using UnityEngine;

[CreateAssetMenu()]
public class UnitSO : ScriptableObject {

  public string unitName;
  public Transform prefab;
  public UnitSizeSO unitSize;
  public float defaultMaxWalkDistance;

}
