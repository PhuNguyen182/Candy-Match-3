using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Canvas))]
public class CanvasSafeArea : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform safeAreaTransform;

    private List<CanvasSafeArea> _helpers = new();
    private ScreenOrientation _lastOrientation;
    private bool _screenChangeVarsInitialized = false;
    
    private Rect _lastSafeArea = Rect.zero;
    private Vector2 _lastResolution = Vector2.zero;

    public static UnityEvent OnResolutionOrOrientationChanged = new UnityEvent();

    private void Awake()
    {
        if (!_helpers.Contains(this))
            _helpers.Add(this);

        if (!_screenChangeVarsInitialized)
        {
            _lastOrientation = Screen.orientation;
            _lastResolution.x = Screen.width;
            _lastResolution.y = Screen.height;
            _lastSafeArea = Screen.safeArea;

            _screenChangeVarsInitialized = true;
        }

        ApplySafeArea();
    }

    private void Update()
    {
        ApplyScreenChanged();
    }

    private void ApplyScreenChanged()
    {
        if (_helpers[0] != this)
            return;

        if (Application.isMobilePlatform && Screen.orientation != _lastOrientation)
            OrientationChanged();

        if (Screen.safeArea != _lastSafeArea)
            SafeAreaChanged();

        if (Screen.width != _lastResolution.x || Screen.height != _lastResolution.y)
            ResolutionChanged();
    }

    private void ApplySafeArea()
    {
        if (safeAreaTransform == null)
            return;

        Rect safeArea = Screen.safeArea;

        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= canvas.pixelRect.width;
        anchorMin.y /= canvas.pixelRect.height;
        anchorMax.x /= canvas.pixelRect.width;
        anchorMax.y /= canvas.pixelRect.height;

        safeAreaTransform.anchorMin = anchorMin;
        safeAreaTransform.anchorMax = anchorMax;
    }

    private void OrientationChanged()
    {
        _lastOrientation = Screen.orientation;
        _lastResolution.x = Screen.width;
        _lastResolution.y = Screen.height;

        OnResolutionOrOrientationChanged.Invoke();
    }

    private void ResolutionChanged()
    {
        _lastResolution.x = Screen.width;
        _lastResolution.y = Screen.height;

        OnResolutionOrOrientationChanged.Invoke();
    }

    private void SafeAreaChanged()
    {
        _lastSafeArea = Screen.safeArea;

        for (int i = 0; i < _helpers.Count; i++)
        {
            _helpers[i].ApplySafeArea();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        canvas ??= GetComponent<Canvas>();

        if (transform.childCount > 0)
            safeAreaTransform ??= transform.GetChild(0).GetComponent<RectTransform>();
    }
#endif

    private void OnDestroy()
    {
        if (_helpers != null && _helpers.Contains(this))
            _helpers.Remove(this);
    }
}