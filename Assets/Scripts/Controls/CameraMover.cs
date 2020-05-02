using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PromiseCode.RTS.Controls
{
    public class CameraMover : MonoBehaviour
    {
        public static CameraMover sceneInstance { get; private set; }
        public enum CameraMoveType
        {
            SpeedupFromCenter,
            Classic
        }

        [SerializeField] Transform cameraMoverTransform;
        [SerializeField] Transform cameraTransform;
        [SerializeField] Transform cameraDirectionsTransform;
        [SerializeField] int startCameraHeight = 18;
        [Tooltip("Size of mouse drag 'borders' of screen in pixels. In this area near screen border mouse will move camera in border side.")]
        [SerializeField] int screenBorderForMouse = 10;
        [Tooltip("camera speed")]
        [SerializeField, Range(2, 30)] float cameraSensitivity = 8f;
        [SerializeField] CameraMoveType cameraMoveType = CameraMoveType.SpeedupFromCenter;
        [Tooltip("Max camera zoom value. Big values can cause problems with camera")]
        [SerializeField, Range(0, 50)] float maxZoom = 7f;

        Vector3 startCameraLocalPosition;
        float zoomValue;
        float zoomSpeed = 30;

        Vector2 mouseMoveCenter;
        bool allowCameraRotation, allowCameraZoom;
        bool isCameraRotatingNow;

        float mapSize;

        readonly string mouseXAxisName = "Mouse X";
        readonly string mouseScrollAxisName = "Mouse ScrollWheel";
        readonly string terrainName = "Terrain";

        Camera mainCamera;

        float cameraSensitivityForRMB, cameraSensitivityForRMBClassic;
        Vector3 pointToRotateAround;

        void Awake()
        {
            mainCamera = Camera.main;
            sceneInstance = this;
            transform.position = new Vector3(transform.position.x, startCameraHeight, transform.position.z);
        }

        void Start()
        {
            // TODO: get configuration from GameController
            allowCameraRotation = true;
            allowCameraZoom = true;

            startCameraLocalPosition = cameraTransform.localPosition;
            // TODO: get mapsize from Settings
            mapSize = 200;

            cameraSensitivityForRMB = cameraSensitivity / 20;
            cameraSensitivityForRMBClassic = cameraSensitivity * 10;

            InitializeHotkeys();
        }

        void InitializeHotkeys()
        {

        }

        void LateUpdate()
        {
            HandleDefaultMovement();
            HandleZoom();
        }

        void HandleDefaultMovement()
        {
            Vector2 mousePos = Input.mousePosition;
            if (Input.GetMouseButtonDown(1))
            {
                mouseMoveCenter = Input.mousePosition;
            }
            HandleRotation();

            if (Input.GetMouseButton(1) && !isCameraRotatingNow)
            {
                if (cameraMoveType == CameraMoveType.SpeedupFromCenter)
                {
                    cameraMoverTransform.position += cameraSensitivityForRMB * (Input.mousePosition.x - mouseMoveCenter.x) * Time.deltaTime * cameraDirectionsTransform.right;
                    cameraMoverTransform.position += cameraSensitivityForRMB * (Input.mousePosition.y - mouseMoveCenter.y) * Time.deltaTime * cameraDirectionsTransform.forward;
                }
                else if (cameraMoveType == CameraMoveType.Classic)
                {
                    var directionNormalized = ((Vector2)Input.mousePosition - mouseMoveCenter).normalized;
                    cameraMoverTransform.position += cameraSensitivityForRMBClassic * directionNormalized.x * Time.deltaTime * cameraDirectionsTransform.right;
                    cameraMoverTransform.position += cameraSensitivityForRMBClassic * directionNormalized.y * Time.deltaTime * cameraDirectionsTransform.forward;
                }
            }

            else if (!isCameraRotatingNow)
            {
                if ((mousePos.x <= screenBorderForMouse && mousePos.x > -1) || IsKeyDown(KeyCode.LeftArrow))
                {
                    cameraMoverTransform.position -= cameraDirectionsTransform.right * (cameraSensitivity * 10 * Time.deltaTime);
                }
                else if ((mousePos.x >= Screen.width - screenBorderForMouse && mousePos.x < Screen.width + 1) || IsKeyDown(KeyCode.RightArrow))
                {
                    cameraMoverTransform.position += cameraDirectionsTransform.right * (cameraSensitivity * 10 * Time.deltaTime);
                }

                if ((mousePos.y <= screenBorderForMouse && mousePos.y > -1) || IsKeyDown(KeyCode.DownArrow))
                {
                    cameraMoverTransform.position -= cameraDirectionsTransform.forward * (cameraSensitivity * 10 * Time.deltaTime);
                }
                else if ((mousePos.y >= Screen.height - screenBorderForMouse && mousePos.y < Screen.height + 1) || IsKeyDown(KeyCode.UpArrow))
                {
                    cameraMoverTransform.position += cameraDirectionsTransform.forward * (cameraSensitivity * 10 * Time.deltaTime);
                }
            }

			var cameraPosition = cameraMoverTransform.position;

			cameraPosition.x = Mathf.Clamp(cameraPosition.x, 0, mapSize);
			cameraPosition.z = Mathf.Clamp(cameraPosition.z, 0, mapSize);

			cameraMoverTransform.position = cameraPosition;
        }

        void HandleRotation()
        {
            if(Input.GetMouseButtonDown(2) && allowCameraRotation)
            {
                RaycastHit hit;
                if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 1000))
                {
                    pointToRotateAround = hit.point;
                }
                isCameraRotatingNow = true;
            }

            if(Input.GetMouseButton(2) && allowCameraRotation)
            {
                float inputX = Input.GetAxis(mouseXAxisName);
                if(inputX != 0)
                {
                    cameraMoverTransform.RotateAround(pointToRotateAround, Vector3.up, inputX);
                    cameraDirectionsTransform.RotateAround(pointToRotateAround, Vector3.up, inputX);
                }
            }

            if(Input.GetMouseButtonUp(2))
            {
                isCameraRotatingNow = false;
            }
        }

        void HandleZoom()
        {
            if(!allowCameraZoom)
            {
                return;
            }

            zoomValue = Mathf.Clamp(zoomValue + Input.GetAxis(mouseScrollAxisName) * zoomSpeed, 0, maxZoom);
            // Hack: fix local camera problem on rotation
            var localForward = cameraTransform.InverseTransformDirection(cameraTransform.forward) - Vector3.up;
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, startCameraLocalPosition + localForward * zoomValue, Time.deltaTime * 5f);
        }

        public bool IsKeyDown(KeyCode key) => Input.GetKey(key);

        public void OnPressCenterCamera()
        {
            // TODO: Call SetPosition
            // SetPosition(...);
        }

        public void SetPosition(Vector3 position)
        {
            if(!mainCamera)
            {
                mainCamera = Camera.main;
            }

            int checkLayer = 1 << LayerMask.NameToLayer(terrainName);
            RaycastHit hit;

            float zOffset = 0;
            cameraMoverTransform.position = new Vector3(position.x, cameraMoverTransform.position.y, position.z);

            var midScreenRay = mainCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            if(Physics.Raycast(midScreenRay, out hit, 1000, checkLayer))
            {
                zOffset = hit.point.z - position.z;
            }

            cameraMoverTransform.position = new Vector3(position.x, cameraMoverTransform.position.y, position.z - zOffset);
        }
    }

}
