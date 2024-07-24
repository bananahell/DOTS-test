using UnityEngine;

public class Mouse3D : MonoBehaviour {

  public static Mouse3D Instance { get; private set; }

  [SerializeField] private LayerMask mouseColliderLayerMask = new();

  private void Awake() {
    if (Instance != null) {
      Debug.LogError("Instance of Mouse3D already exists!");
    }
    Instance = this;
  }

  private void Update() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, this.mouseColliderLayerMask)) {
      this.transform.position = raycastHit.point;
    }
  }

  public static Vector3 GetMouseWorldPosition() {
    if (Instance == null) {
      Debug.LogError("Mouse3D Object does not exist!");
    }
    return Instance.GetMouseWorldPosition_Instance();
  }

  private Vector3 GetMouseWorldPosition_Instance() {
    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    Vector3 awayNegative = new(-10, 0, -10);
    return Physics.Raycast(ray, out RaycastHit raycastHit, 999f, this.mouseColliderLayerMask)
             ? raycastHit.point
             : awayNegative;
  }

}
