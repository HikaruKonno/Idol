/*
 * ファイル
 * PlanarReflection.cs
 * 
 * 説明
 * 反射平面に対する平面反射を実装するスクリプト（URP用）
 * URPのカメラレンダリングイベントを利用して、メインカメラのレンダリング前に反射用カメラでシーンをレンダリングし、
 * 反射テクスチャを取得して反射平面のマテリアルに設定する。
 * URPからHDRPに変更したことによって使えなくなってしまった為、現在使用していない
 */
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Rendering;
//using UnityEngine.Rendering.Universal;

//public class PlanarReflectionURP_Fixed : MonoBehaviour
//{
//    [SerializeField] private List<GameObject> _reflectionPlanes;    // 反射平面のリスト
//    [SerializeField] private LayerMask _reflectionLayerMask = -1;   // 反射に含めるレイヤーマスク
//    private GameObject _mainCameraObject;       // メインカメラオブジェクト
//    private Camera _mainCamera;                 // メインカメラ
//    private Camera _reflectionCamera;           // 反射用カメラ
//    private RenderTexture _renderTarget;        // 反射用レンダーテクスチャ
//    private List<Material> _floorMaterials = new List<Material>();  // 反射平面のマテリアルリスト

//    private static readonly int ReflectionTexID = Shader.PropertyToID("_ReflectionTex");    // シェーダープロパティID

//    private void Start()
//    {
//        // アイドルカメラを優先的に取得し、なければMainCameraを取得する
//        _mainCameraObject = GameObject.Find("IdolCamera");
//        if (_mainCameraObject != null)
//        {
//            _mainCamera = _mainCameraObject.GetComponent<Camera>();
//        }
//        if (_mainCamera == null)
//        {
//            _mainCamera = Camera.main;
//        }

//        // 反射平面のマテリアルを取得
//        _floorMaterials.Clear();
//        foreach (var plane in _reflectionPlanes)
//        {
//            var renderer = plane.GetComponent<Renderer>();
//            if (!renderer)
//            {
//                continue;
//            }
//            _floorMaterials.Add(renderer.material);
//        }

//        if (_floorMaterials.Count == 0)
//        {
//            Debug.LogError("有効な反射平面がない", this);
//            this.enabled = false;
//            return;
//        }

//        // レンダーテクスチャと反射カメラの作成
//        _renderTarget = new RenderTexture(Screen.width, Screen.height, 24);

//        var reflectionCameraGo = new GameObject("Reflection Camera");
//        _reflectionCamera = reflectionCameraGo.AddComponent<Camera>();
//        _reflectionCamera.cullingMask = _reflectionLayerMask;
//        _reflectionCamera.enabled = false;
//    }

//    private void OnEnable()
//    {
//        // カメラレンダリングイベントに登録
//        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
//    }

//    private void OnDisable()
//    {
//        // カメラレンダリングイベントから登録解除
//        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;

//        if (_reflectionCamera) Destroy(_reflectionCamera.gameObject);
//        if (_renderTarget) _renderTarget.Release();
//    }

//    private void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
//    {
//        if (camera != _mainCamera) return;
//        // 反射レンダリング
//        RenderReflection(context);
//    }

//    // 反射レンダリング
//    private void RenderReflection(ScriptableRenderContext context)
//    {
//        if (!_mainCamera || _reflectionPlanes == null || _floorMaterials == null) return;

//        // カメラに最も近い平面を探す
//        GameObject activeReflectionPlane = null;
//        float minDistance = float.MaxValue;
//        foreach (var plane in _reflectionPlanes)
//        {
//            float distance = Vector3.Distance(plane.transform.position, _mainCamera.transform.position);
//            if (distance < minDistance)
//            {
//                minDistance = distance;
//                activeReflectionPlane = plane;
//            }
//        }
//        if (activeReflectionPlane == null)
//        {
//            return;
//        }

//        // 反射カメラの設定
//        _reflectionCamera.CopyFrom(_mainCamera);
//        _reflectionCamera.cullingMask = _reflectionLayerMask;

//        Vector3 cameraDirectionWorld = _mainCamera.transform.forward;
//        Vector3 cameraUpWorld = _mainCamera.transform.up;
//        Vector3 cameraPositionWorld = _mainCamera.transform.position;

//        // 反射平面のローカル空間に変換
//        Vector3 cameraDirectionPlaneSpace = activeReflectionPlane.transform.InverseTransformDirection(cameraDirectionWorld);
//        Vector3 cameraUpPlaneSpace = activeReflectionPlane.transform.InverseTransformDirection(cameraUpWorld);
//        Vector3 cameraPositionPlaneSpace = activeReflectionPlane.transform.InverseTransformPoint(cameraPositionWorld);

//        // Y軸を反転させて鏡写しの位置を計算
//        cameraDirectionPlaneSpace.y *= -1.0f;
//        cameraUpPlaneSpace.y *= -1.0f;
//        cameraPositionPlaneSpace.y *= -1.0f;

//        // ワールド空間に再変換
//        cameraDirectionWorld = activeReflectionPlane.transform.TransformDirection(cameraDirectionPlaneSpace);
//        cameraUpWorld = activeReflectionPlane.transform.TransformDirection(cameraUpPlaneSpace);
//        cameraPositionWorld = activeReflectionPlane.transform.TransformPoint(cameraPositionPlaneSpace);

//        // 反射カメラに位置と向きを設定
//        _reflectionCamera.transform.position = cameraPositionWorld;
//        _reflectionCamera.transform.LookAt(cameraPositionWorld + cameraDirectionWorld, cameraUpWorld);

//        // 斜めクリッピング平面の設定
//        Vector3 planeNormal = activeReflectionPlane.transform.up;
//        Vector3 planePosition = activeReflectionPlane.transform.position;
//        Vector4 clipPlane = CameraSpacePlane(_reflectionCamera, planePosition, planeNormal, 1.0f);
//        _reflectionCamera.projectionMatrix = _mainCamera.CalculateObliqueMatrix(clipPlane);

//        // レンダリングとテクスチャ設定
//        _reflectionCamera.targetTexture = _renderTarget;

//        foreach (var mat in _floorMaterials)
//        {
//            mat.SetTexture(ReflectionTexID, _renderTarget);
//        }

//        UniversalRenderPipeline.RenderSingleCamera(context, _reflectionCamera);

//        _reflectionCamera.ResetProjectionMatrix();
//    }

//    // 斜めクリッピング平面の計算
//    private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
//    {
//        Vector3 offsetPos = pos + normal * 0.07f;
//        Matrix4x4 m = cam.worldToCameraMatrix;
//        Vector3 cpos = m.MultiplyPoint(offsetPos);
//        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
//        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
//    }
//}