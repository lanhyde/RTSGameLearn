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
            SpeedupFromCenter,//从中心加速？
            Classic//经典的？
        }

        [SerializeField] Transform cameraMoverTransform;//摄像机移动器
        [SerializeField] Transform cameraTransform;//摄像机
        [SerializeField] Transform cameraDirectionsTransform;//摄像机方向
        [SerializeField] int startCameraHeight = 18;//开始 摄像机 高度
        //工具提示：鼠标拖动屏幕“边框”的大小（像素）。在这个区域中，鼠标将在屏幕边框边移动相机。
        [Tooltip("Size of mouse drag 'borders' of screen in pixels. In this area near screen border mouse will move camera in border side.")]
        [SerializeField] int screenBorderForMouse = 10;//鼠标屏幕边框
        //工具提示：摄像机速度
        [Tooltip("camera speed")]
        //范围（2,30）
        [SerializeField, Range(2, 30)] float cameraSensitivity = 8f;//摄像机灵敏度
        [SerializeField] CameraMoveType cameraMoveType = CameraMoveType.SpeedupFromCenter;
        //工具提示：最大相机缩放值。大值会导致相机出现问题
        [Tooltip("Max camera zoom value. Big values can cause problems with camera")]
        //范围（0,50）
        [SerializeField, Range(0, 50)] float maxZoom = 7f;//最大缩放

        Vector3 startCameraLocalPosition;//启动摄像机本地位置
        float zoomValue;//缩放值
        float zoomSpeed = 30;//缩放速度

        Vector2 mouseMoveCenter;//鼠标指针
        bool allowCameraRotation, allowCameraZoom;//允许相机旋转，允许相机缩放
        bool isCameraRotatingNow;//摄像机现在在旋转

        float mapSize;//地图大小

        readonly string mouseXAxisName = "Mouse X";//鼠标X轴名称
        readonly string mouseScrollAxisName = "Mouse ScrollWheel";//鼠标滚动轴名称
        readonly string terrainName = "Terrain";//地形名称

        Camera mainCamera;//主摄像机

        float cameraSensitivityForRMB, cameraSensitivityForRMBClassic;//摄像机灵敏度？？
        Vector3 pointToRotateAround;//旋转点

        void Awake()
        {
            mainCamera = Camera.main;//初始化mian camera
            sceneInstance = this;//单例
            transform.position = new Vector3(transform.position.x, startCameraHeight, transform.position.z);//初始化摄像机移动器位置   高度初始化    <-- 未确定
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
        /// <summary>
        /// 初始化热键
        /// </summary>
        void InitializeHotkeys()
        {

        }

        void LateUpdate()
        {
            HandleDefaultMovement();
            HandleZoom();
        }
        /// <summary>
        /// 处理默认移动
        /// </summary>
        void HandleDefaultMovement()
        {
            Vector2 mousePos = Input.mousePosition;
            if (Input.GetMouseButtonDown(1))//鼠标右键 
            {
                mouseMoveCenter = Input.mousePosition;//设置鼠标位置
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
        /// <summary>
        /// 手动旋转？
        /// </summary>
        void HandleRotation()
        {
            //旋转 中键
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
        /// <summary>
        /// 手动缩放？
        /// </summary>
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
