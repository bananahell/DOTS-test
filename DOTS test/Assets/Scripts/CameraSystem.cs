using Cinemachine;
using UnityEngine;

public class CameraSystem : MonoBehaviour {

  [SerializeField] private CinemachineVirtualCamera cinemachineVirtualCamera;
  [SerializeField] private bool useEdgeScrolling = false;
  [SerializeField] private bool useDragPan = false;
  [SerializeField] private float fieldOfViewMax = 70f;
  [SerializeField] private float fieldOfViewMin = 10f;
  [SerializeField] private float followOffsetMax = 2f;
  [SerializeField] private float followOffsetMin = 0.5f;
  [SerializeField] private float followOffsetMaxY = 2f;
  [SerializeField] private float followOffsetMinY = 0.3f;
  [SerializeField] private CameraZoomType cameraZoomType = CameraZoomType.MoveForward;

  private bool dragPanMoveActive;
  private Vector2 lastMousePosition;
  private float targetFieldOfView = 50f;
  private Vector3 followOffset;

  private enum CameraZoomType {
    FieldOfView,
    MoveForward,
    LowerY,
  }

  private void Awake() {
    this.followOffset = this.cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
  }

  private void Update() {
    this.HandleCameraMovement();
    if (this.useEdgeScrolling) {
      this.HandleCameraMovementEdgeScrolling();
    }
    if (this.useDragPan) {
      this.HandleCameraMovementDragPan();
    }
    this.HandleCameraRotation();
    this.HandleCameraZoom();
  }

  private void HandleCameraMovement() {
    Vector3 inputDir = new(0, 0, 0);
    if (Input.GetKey(KeyCode.W)) {
      inputDir.z = 1f;
    }
    if (Input.GetKey(KeyCode.S)) {
      inputDir.z = -1f;
    }
    if (Input.GetKey(KeyCode.A)) {
      inputDir.x = -1f;
    }
    if (Input.GetKey(KeyCode.D)) {
      inputDir.x = 1f;
    }
    Vector3 moveDir = (this.transform.forward * inputDir.z) + (this.transform.right * inputDir.x);
    float moveSpeed = 3f;
    this.transform.position += moveSpeed * Time.deltaTime * moveDir;
  }

  private void HandleCameraMovementEdgeScrolling() {
    Vector3 inputDir = new(0, 0, 0);
    int edgeScrollSize = 20;
    if (Input.mousePosition.x < edgeScrollSize) {
      inputDir.x = -1f;
    }
    if (Input.mousePosition.y < edgeScrollSize) {
      inputDir.z = -1f;
    }
    if (Input.mousePosition.x > Screen.width - edgeScrollSize) {
      inputDir.x = 1f;
    }
    if (Input.mousePosition.y > Screen.height - edgeScrollSize) {
      inputDir.z = 1f;
    }
    Vector3 moveDir = (this.transform.forward * inputDir.z) + (this.transform.right * inputDir.x);
    float moveSpeed = 3f;
    this.transform.position += moveSpeed * Time.deltaTime * moveDir;
  }

  private void HandleCameraMovementDragPan() {
    Vector3 inputDir = new(0, 0, 0);
    if (Input.GetMouseButtonDown(0)) {
      this.dragPanMoveActive = true;
      this.lastMousePosition = Input.mousePosition;
    }
    if (Input.GetMouseButtonUp(0)) {
      this.dragPanMoveActive = false;
    }
    if (this.dragPanMoveActive) {
      Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - this.lastMousePosition;
      float dragPanSpeed = 0.015f;
      inputDir.x = mouseMovementDelta.x * dragPanSpeed;
      inputDir.z = mouseMovementDelta.y * dragPanSpeed;
      this.lastMousePosition = Input.mousePosition;
    }
    Vector3 moveDir = (this.transform.forward * inputDir.z) + (this.transform.right * inputDir.x);
    float moveSpeed = 3f;
    this.transform.position -= moveSpeed * Time.deltaTime * moveDir;
  }

  private void HandleCameraRotation() {
    float rotateDir = 0f;
    if (Input.GetKey(KeyCode.Q)) {
      rotateDir = 1f;
    }
    if (Input.GetKey(KeyCode.E)) {
      rotateDir = -1f;
    }
    float rotateSpeed = 100f;
    this.transform.eulerAngles += new Vector3(0, rotateDir * rotateSpeed * Time.deltaTime, 0);
  }

  private void HandleCameraZoom() {
    switch (this.cameraZoomType) {
      case CameraZoomType.FieldOfView:
        this.HandleCameraZoomFieldOfView();
        break;
      case CameraZoomType.MoveForward:
        this.HandleCameraZoomMoveForward();
        break;
      case CameraZoomType.LowerY:
        this.HandleCameraZoomLowerY();
        break;
      default:
        this.HandleCameraZoomMoveForward();
        break;
    }
  }

  private void HandleCameraZoomFieldOfView() {
    if (Input.mouseScrollDelta.y > 0) {
      this.targetFieldOfView -= 5;
    }
    if (Input.mouseScrollDelta.y < 0) {
      this.targetFieldOfView += 5;
    }
    this.targetFieldOfView = Mathf.Clamp(this.targetFieldOfView, this.fieldOfViewMin, this.fieldOfViewMax);
    float zoomSpeed = 10f;
    this.cinemachineVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(this.cinemachineVirtualCamera.m_Lens.FieldOfView, this.targetFieldOfView, Time.deltaTime * zoomSpeed);
  }

  private void HandleCameraZoomMoveForward() {
    Vector3 zoomDir = this.followOffset.normalized;
    float zoomAmmount = 0.1f;
    if (Input.mouseScrollDelta.y > 0) {
      this.followOffset -= zoomDir * zoomAmmount;
    }
    if (Input.mouseScrollDelta.y < 0) {
      this.followOffset += zoomDir * zoomAmmount;
    }
    if (this.followOffset.magnitude < this.followOffsetMin) {
      this.followOffset = zoomDir * this.followOffsetMin;
    }
    if (this.followOffset.magnitude > this.followOffsetMax) {
      this.followOffset = zoomDir * this.followOffsetMax;
    }
    float zoomSpeed = 7f;
    Vector3 currentFollowOffset = this.cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    Vector3 lerpingZoomOffset = Vector3.Lerp(currentFollowOffset, this.followOffset, Time.deltaTime * zoomSpeed);
    this.cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = lerpingZoomOffset;
  }

  private void HandleCameraZoomLowerY() {
    float zoomAmmount = 0.1f;
    if (Input.mouseScrollDelta.y > 0) {
      this.followOffset.y -= zoomAmmount;
    }
    if (Input.mouseScrollDelta.y < 0) {
      this.followOffset.y += zoomAmmount;
    }
    this.followOffset.y = Mathf.Clamp(this.followOffset.y, this.followOffsetMinY, this.followOffsetMaxY);
    float zoomSpeed = 7f;
    Vector3 currentFollowOffset = this.cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset;
    Vector3 lerpingZoomOffset = Vector3.Lerp(currentFollowOffset, this.followOffset, Time.deltaTime * zoomSpeed);
    this.cinemachineVirtualCamera.GetCinemachineComponent<CinemachineTransposer>().m_FollowOffset = lerpingZoomOffset;
  }

}
