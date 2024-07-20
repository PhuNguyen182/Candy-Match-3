using R3;
using System;
using UnityEngine;
using TMPro;

public class WaitingPopup : MonoBehaviour
{
    [SerializeField] TMP_Text messageText;

    private Action _onTimeOut;
    private static WaitingPopup _instance;
    private IDisposable _waitDispose;

    public const string WaitingBoxPath = "Popups/WaitingPopup";

    public static WaitingPopup Setup()
    {
        if (_instance == null)
        {
            _instance = Instantiate(Resources.Load<WaitingPopup>(WaitingBoxPath));
        }

        _instance.gameObject.SetActive(true);
        return _instance;
    }


    public void ShowWaiting()
    {
        PopupController.Instance.isLockEscape = true;
        gameObject.SetActive(true);
    }


    public void HideWaiting(bool isLockEscape = false)
    {
        PopupController.Instance.isLockEscape = isLockEscape;

        gameObject.SetActive(false);

        if (_waitDispose != null)
            _waitDispose.Dispose();
    }

    public void ShowWaiting(float time)
    {
        _onTimeOut = null;
        ShowWaiting();
        TimeOut(time);
    }

    public void ShowWaiting(float time, Action action)
    {
        ShowWaiting();
        _onTimeOut = action;
        TimeOut(time);
    }

    private void TimeOut(float time)
    {
        _waitDispose?.Dispose();
        _waitDispose = Observable.Timer(TimeSpan.FromSeconds(time))
                       .Subscribe(_ =>
                       {
                            HideWaiting();
                            _onTimeOut?.Invoke();
                       });
    }

    private void OnDestroy()
    {
        _waitDispose?.Dispose();
    }
}