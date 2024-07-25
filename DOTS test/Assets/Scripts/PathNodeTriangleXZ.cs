using System.Collections;
using UnityEngine;

public class PathNodeTriangleXZ {

  public int x;
  public int z;
  public float gCost;
  public float hCost;
  public float fCost;
  public bool isWalkable;
  public PathNodeTriangleXZ cameFromNode;
  public Transform visualTransform;
  private Material originalUnselectedMaterial;
  private MonoBehaviour monoBehaviour;

  public PathNodeTriangleXZ(int x, int z) {
    this.x = x;
    this.z = z;
    this.isWalkable = true;
  }

  public void CalculateFCost() {
    this.fCost = this.gCost + this.hCost;
  }

  public void SetIsWalkable(bool isWalkable) {
    this.isWalkable = isWalkable;
    GridTriangleXZ<PathNodeTriangleXZ>.Instance.TriggerGridObjectChanged(this.x, this.z);
  }

  public override string ToString() {
    return this.x + "," + this.z;
  }

  public void Show() {
    this.visualTransform.Find(Globals.SELECTED_STRING).gameObject.SetActive(true);
    this.visualTransform.Find(Globals.UNSELECTED_STRING).gameObject.SetActive(false);
  }

  public void Hide() {
    this.visualTransform.Find(Globals.SELECTED_STRING).gameObject.SetActive(false);
    this.visualTransform.Find(Globals.UNSELECTED_STRING).gameObject.SetActive(true);
  }

  public void AttachUnselectedMaterial(Material material) {
    this
      .visualTransform
      .Find(Globals.UNSELECTED_STRING)
      .gameObject
      .GetComponent<Renderer>()
      .material = material;
  }

  public void AttachUnselectedTimedMaterial(Material material, float seconds = 5f) {
    this
      .visualTransform
      .Find(Globals.UNSELECTED_STRING)
      .gameObject
      .GetComponent<Renderer>()
      .material = material;
    this.StartCoroutine(seconds);
  }

  public void AttachUnselectedOriginalMaterial(Material material) {
    this
      .visualTransform
      .Find(Globals.UNSELECTED_STRING)
      .gameObject
      .GetComponent<Renderer>()
      .material = material;
    this.originalUnselectedMaterial = material;
  }

  public void ResetUnselectedMaterial() {
    if (this.originalUnselectedMaterial != null) {
      this
        .visualTransform
        .Find(Globals.UNSELECTED_STRING)
        .gameObject
        .GetComponent<Renderer>()
        .material = this.originalUnselectedMaterial;
    }
  }

  private IEnumerator ResetUnselectedMaterialCoroutine(float seconds) {
    yield return new WaitForSeconds(seconds);
    if (this.originalUnselectedMaterial != null) {
      this
        .visualTransform
        .Find(Globals.UNSELECTED_STRING)
        .gameObject
        .GetComponent<Renderer>()
        .material = this.originalUnselectedMaterial;
    }
  }

  private void StartCoroutine(float seconds) {
    this.monoBehaviour = Object.FindObjectOfType<MonoBehaviour>();
    if (this.monoBehaviour != null) {
      _ = this.monoBehaviour.StartCoroutine(this.ResetUnselectedMaterialCoroutine(seconds));
    }
  }

}
