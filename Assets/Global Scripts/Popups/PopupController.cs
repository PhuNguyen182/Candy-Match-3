using R3;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using GlobalScripts.Service;

public class PopupController : SingletonClass<PopupController>, IService
{
    public const int BaseIndexLayer = 20;

    private Stack<BasePopup> boxStack = new Stack<BasePopup>();
    private readonly Subject<Unit> _pressEscapeSubject = new Subject<Unit>();
    private readonly Subject<bool> _showPopupSubject = new Subject<bool>();
    public Observable<Unit> PressEscapeObservable => _pressEscapeSubject;

    /// <summary>
    /// List chứa các Box thuộc dạng bắt buộc phải xem (Lưu lại ID Popup và hàm để mở Box)
    /// Box save Hiện đi hiện lại đến khi nào user ấn đóng thì mới remove ra khỏi List
    /// </summary>
    private Dictionary<string, Action> lstActionSaveBox = new Dictionary<string, Action>();

    public bool IsShowLoading { get; set; }

    public bool isLockEscape { get; set; }

    public event Action ActionStackEmpty;
    public event Action ActionOnClosedOneBox;
    public Observable<bool> TriggerShowPopUp => _showPopupSubject;

    public PopupController()
    {
        Initialize();
    }

    public void Initialize()
    {
        //Observable.EveryUpdate()
        //          .Subscribe(_ =>
        //          {
        //              if (Input.GetKeyUp(KeyCode.Escape))
        //              {
        //                  ProcessBackBtn();
        //              }

        //              if (Input.GetKeyDown(KeyCode.Space))
        //              {
        //                  DebugStack();
        //              }
        //          });

        SceneManager.sceneLoaded += (scene, mode) => OnLevelWasLoaded();
    }

    public void AddNewBackObj(BasePopup obj)
    {
        boxStack.Push(obj);
        _showPopupSubject.OnNext(true);
        SettingOderLayerPopup();
    }

    private void SettingOderLayerPopup()
    {
        if (boxStack == null || boxStack.Count <= 0)
            return;

        int index = BaseIndexLayer + 10 * boxStack.Count;
        foreach (var box in boxStack)
        {
            box.ChangeLayerHandle(BaseIndexLayer + index);
            index -= 10;
        }
    }

    public void Remove()
    {
        if (boxStack.Count == 0)
            return;

        BasePopup obj = boxStack.Pop();
        if (boxStack.Count == 0)
            OnStackEmpty();

        SettingOderLayerPopup();
    }

    /// <summary>
    /// Check if there is any popup or panel is enabled
    /// </summary>
    public bool IsShowingPopup()
    {
        BasePopup[] lst_backObjs = boxStack.ToArray();
        int lenght = lst_backObjs.Length;

        for (int i = lenght - 1; i >= 0; i--)
        {
            if (lst_backObjs[i].IsPopup)
            {
                return true;
            }
        }

        return false;
    }

    public void DebugStack()
    {
        BasePopup[] lst_backObjs = boxStack.ToArray();
        int lenght = lst_backObjs.Length;
        for (int i = lenght - 1; i >= 0; i--)
        {
            Debug.Log(" =============== " + lst_backObjs[i].gameObject.name + " ===============");
        }
    }

    private void OnStackEmpty()
    {
        _showPopupSubject.OnNext(false);
        ActionStackEmpty?.Invoke();
    }

    public virtual void OnActionOnClosedOneBox()
    {
        ActionOnClosedOneBox?.Invoke();
    }

    public virtual void ProcessBackBtn()
    {
        if (IsShowLoading)
            return;

        if (isLockEscape)
            return;

        if (boxStack.Count != 0)
        {
            boxStack.Peek().Close();
        }

        else
        {
            if (!OpenBoxSave())
                OnPressEscapeStackEmpty();
        }
    }

    protected virtual void OnPressEscapeStackEmpty()
    {
        _pressEscapeSubject.OnNext(Unit.Default);
    }

    protected void OnLevelWasLoaded()
    {
        if (boxStack != null)
            boxStack.Clear();

        OpenBoxSave();
    }

    /// <summary>
    /// Check if there is no popup or panel is enabled
    /// </summary>
    public bool IsEmptyStackBox()
    {
        return boxStack.Count == 0;
    }

    #region Box Save
    private bool OpenBoxSave()
    {
        bool isOpened = false;

        foreach (var save in lstActionSaveBox)
        {
            if (save.Value != null)
            {
                isOpened = true;
                save.Value?.Invoke();
            }
        }

        return isOpened;
    }

    public void AddBoxSave(string idPopup, Action actionOpen)
    {
        if (lstActionSaveBox.ContainsKey(idPopup))
            lstActionSaveBox.Add(idPopup, actionOpen);
    }

    public void RemoveBoxSave(string idPopup)
    {
        if (lstActionSaveBox.ContainsKey(idPopup))
            lstActionSaveBox.Remove(idPopup);
    }

    #endregion


    public bool IsPopupCurrent(BasePopup baseBox)
    {
        BasePopup boxShowing = boxStack.Count > 0 ? boxStack.Peek() : null;
        return boxShowing != null && baseBox != null && boxShowing == baseBox;
    }

    public void CloseAll()
    {
        BasePopup[] lst_backObjs = boxStack.ToArray();
        int lenght = lst_backObjs.Length;

        for (int i = lenght - 1; i >= 0; i--)
        {
            if (lst_backObjs[i])
            {
                lst_backObjs[i].Close();
            }
        }
    }
}
