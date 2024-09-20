using Cysharp.Threading.Tasks;
using GlobalScripts.SceneUtils;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Match3Grid : MonoBehaviour
{
    private void Start()
    {
        TestLoadScene().Forget();
    }

    private async UniTask TestLoadScene()
    {
        await UniTask.Delay(TimeSpan.FromSeconds(2));
        await SceneLoader.LoadScene("Level Design");
    }
}

public class Program
{
    
}