using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Project1;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using AsyncOperation = System.ComponentModel.AsyncOperation;

public partial class SROptions {
    private bool _p1DestroyMaxLevel = true;
    private int _P1_MaxLevel = 4;

    [Category("Project1")]
    public bool P1_DestroyMaxLevel {
        get => _p1DestroyMaxLevel;
        set {
            _p1DestroyMaxLevel = value;
            OnPropertyChanged(nameof(_p1DestroyMaxLevel));
            GameController.Instance.RestartGame();
        }
    }
    
    [Category("Project1")]
    [NumberRange(1, 10)]
    public int P1_MaxLevel {
        get => _P1_MaxLevel;
        set {
            _P1_MaxLevel = value;
            GameController.Instance.RestartGame();
            // StartGame(() =>
            // {
            //     _P1_MaxLevel = value;
            // });
        }
    }
    
    public static async Task UnloadCurrentSceneAsync()
    {
        var currentScene = SceneManager.GetActiveScene();

        if (!currentScene.isLoaded)
        {
            Debug.LogWarning("No active scene to unload!");
            return;
        }

        var asyncUnload = SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);

        while (!asyncUnload.isDone)
        {
            Debug.Log($"Unloading {currentScene.name}... {asyncUnload.progress * 100f:0}%");
            await Task.Yield();
        }

        Debug.Log($"{currentScene.name} unloaded!");
    }
    
    public static async Task LoadSceneAsync(string sceneName)
    {
        var asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"Loading {sceneName}... {asyncLoad.progress * 100f:0}%");
            await Task.Yield();
        }

        // Cho phép kích hoạt scene khi load xong
        asyncLoad.allowSceneActivation = true;

        // Đợi cho tới khi scene thật sự kích hoạt
        while (!asyncLoad.isDone)
            await Task.Yield();

        Debug.Log($"{sceneName} loaded!");
    }
    
    async void StartGame(Action callback)
    {
        Debug.Log("Start loading...");
        await UnloadCurrentSceneAsync();
        await LoadSceneAsync(SceneManager.GetActiveScene().name);
        Debug.Log("Now playing GameScene!");
        callback?.Invoke();
    }
}