namespace _Project.Scripts.Shared
{
     using UnityEngine;

     [ExecuteAlways]
     [DisallowMultipleComponent]
     public class CameraSizeFitting : MonoBehaviour
     {
          [Header("References")]
          [SerializeField] Camera _targetCamera;

          [Header("Fit Reference Frame")]
          [SerializeField] float _referenceOrthographicSize = 5f;

          [SerializeField] Vector2 _referenceResolution      = new Vector2(1080f, 1920f);
          [SerializeField] bool    _fitWhenScreenSizeChanged = true;

          int _lastScreenWidth;
          int _lastScreenHeight;

          void Awake()
          {
               CacheCamera();
               Fit();
          }

          void OnEnable()
          {
               CacheCamera();
               Fit();
          }

          void Update()
          {
               if (!_fitWhenScreenSizeChanged || !IsScreenSizeChanged())
               {
                    return;
               }

               Fit();
          }

     #if UNITY_EDITOR
          void OnValidate()
          {
               _referenceOrthographicSize = Mathf.Max(0.0001f, _referenceOrthographicSize);
               _referenceResolution.x     = Mathf.Max(1f, _referenceResolution.x);
               _referenceResolution.y     = Mathf.Max(1f, _referenceResolution.y);

               CacheCamera();
               Fit();
          }
     #endif

          public void Fit()
          {
               CacheCamera();

               if (_targetCamera == null || !_targetCamera.orthographic)
               {
                    return;
               }

               float currentAspect        = GetCurrentAspect();
               float referenceAspect      = _referenceResolution.x                       / _referenceResolution.y;
               float sizeNeededToFitWidth = _referenceOrthographicSize * referenceAspect / currentAspect;

               _targetCamera.orthographicSize = Mathf.Max(_referenceOrthographicSize, sizeNeededToFitWidth);

               CacheScreenSize();
          }

          bool IsScreenSizeChanged() { return Screen.width != _lastScreenWidth || Screen.height != _lastScreenHeight; }

          float GetCurrentAspect()
          {
               if (_targetCamera != null && _targetCamera.aspect > 0f)
               {
                    return _targetCamera.aspect;
               }

               return Screen.height > 0 ? (float)Screen.width / Screen.height : 1f;
          }

          void CacheCamera()
          {
               if (_targetCamera == null)
               {
                    _targetCamera = GetComponent<Camera>();
               }

               if (_targetCamera == null)
               {
                    _targetCamera = Camera.main;
               }
          }

          void CacheScreenSize()
          {
               _lastScreenWidth  = Screen.width;
               _lastScreenHeight = Screen.height;
          }
     }
}
