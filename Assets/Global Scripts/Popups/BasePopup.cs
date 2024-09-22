using System;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

#if UNITASK_ADDRESSABLE_SUPPORT
using GlobalScripts.Utils;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
#endif

public abstract class BasePopup : MonoBehaviour
{
    public RectTransform ContentPanel;
    [SerializeField] protected Canvas popupCanvas;
    [SerializeField] protected RectTransform mainPanel;

    [Header("Config Box")]
    public bool IsPopup;
    public bool IsNotStack;

    [SerializeField] protected bool isAnim = true;

    private bool _isApplicationQuitting; 

    protected Action actionOpenSaveBox;

    protected virtual string IDPopup => $"{GetType()}{gameObject.GetInstanceID()}";

    public Action OnCloseBox;
    public Action<int> OnChangeLayer;

    public bool IsBoxSave { get; set; }
    public int IDLayerPopup { get; set; }

    private void Awake()
    {
        bool isCanvasValid = TryGetComponent(out popupCanvas);

        if (isCanvasValid && IsPopup)
        {
            popupCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            popupCanvas.worldCamera = Camera.main;
            //popupCanvas.sortingLayerID = SortingLayer.NameToID("Popup");
        }

        OnAwake();
    }

    protected virtual void OnAwake() { }

    public void InitBoxSave(bool isBoxSave, Action actionOpenSaveBox)
    {
        this.IsBoxSave = isBoxSave;
        this.actionOpenSaveBox = actionOpenSaveBox;
    }

    #region Init Open Handle
    protected virtual void OnEnable()
    {
        if (!IsNotStack)
        {
            PopupController.Instance.AddNewBackObj(this);
        }

        DoAppear();
    }

    private void Start()
    {
        OnStart();
    }

    protected virtual void DoAppear() { }

    protected virtual void DoDisappear() { }

    protected virtual void OnStart() { }
    #endregion

    public virtual void Show()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// MoveItems the popup into Save Stack
    /// </summary>
    public virtual void SaveBox()
    {
        if (IsBoxSave)
            PopupController.Instance.AddBoxSave(IDPopup, actionOpenSaveBox);
    }

    /// <summary>
    /// Call Remove Save Box in specific scenarios
    /// </summary>
    public virtual void RemoveSaveBox()
    {
        PopupController.Instance.RemoveBoxSave(IDPopup);
    }

    #region Close Box
    public virtual void Close()
    {
        if (!IsNotStack)
            PopupController.Instance.Remove();

        DoClose();
    }

    protected virtual void DoClose()
    {
        SimplePool.Despawn(gameObject);
    }

    protected virtual void OnDisable()
    {
        if (!_isApplicationQuitting)
        {
            OnCloseBox?.Invoke();
            OnCloseBox = null;
        }

        PopupController.Instance.OnActionOnClosedOneBox();
        DoDisappear();
    }

    private void OnApplicationQuit()
    {
        _isApplicationQuitting = true;
    }

    protected void DestroyBox()
    {
        OnCloseBox?.Invoke();
        Destroy(gameObject);
    }
    #endregion

    #region Change layer Box
    public void ChangeLayerHandle(int indexInStack)
    {
        if (IsPopup)
        {
            if (popupCanvas != null)
            {
                popupCanvas.planeDistance = 5;
                popupCanvas.sortingOrder = indexInStack;
                OnChangeLayer?.Invoke(indexInStack);
                IDLayerPopup = indexInStack;
            }
        }

        else
        {
            if (ContentPanel != null)
                transform.SetAsLastSibling();
        }
    }

    #endregion
}

public class BasePopup<TPopup> : BasePopup where TPopup : BasePopup
{
    private static string _resourcePath;
    public static string ResourcePath => _resourcePath;
    public static bool IsPreload { get; set; }

    public static void Preload()
    {
        TPopup instance = Create();
        SimplePool.Despawn(instance.gameObject);
    }

    public static void Preload(string path)
    {
        TPopup instance = Create(path);
        SimplePool.Despawn(instance.gameObject);
    }

    public static TPopup Create()
    {
        _resourcePath = $"Popups/{typeof(TPopup).FullName}";
        return Create(_resourcePath);
    }
    
    public static TPopup Create(string path)
    {
        _resourcePath = path;
        TPopup prefab = Resources.Load<TPopup>(path);
        TPopup instance = SimplePool.Spawn(prefab);
        instance.gameObject.SetActive(true);
        return instance;
    }

#if UNITASK_ADDRESSABLE_SUPPORT
    private static AsyncOperationHandle<GameObject> _opHandle;

    public static async UniTask PreloadFromAddress(string address)
    {
        TPopup instance = await CreateFromAddress(address, true);
        
        if(instance != null)
        {
            SimplePool.Despawn(instance.gameObject);
        }
    }

    public static async UniTask<TPopup> CreateFromAddress(string address, bool isPreload = false)
    {
        TPopup instance = default;
        _resourcePath = address;
        IsPreload = isPreload;

        bool isValidPath = await AddressablesUtils.IsKeyValid(address);

        if (isValidPath)
        {
            _opHandle = Addressables.LoadAssetAsync<GameObject>(address);
            await _opHandle;

            if (_opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                instance = SimplePool.Spawn(_opHandle.Result).GetComponent<TPopup>();
                instance.gameObject.SetActive(true);
            }

            else Release();
        }

        return instance;
    }

    public static void Release()
    {
        if (_opHandle.IsValid())
            Addressables.Release(_opHandle);
    }
#endif
}