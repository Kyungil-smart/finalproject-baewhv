using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    private Dictionary<string, Sprite> DefaultSprite = new ();
    private Dictionary<string, Sprite> LoadedSprites = new ();

    private void Start()
    {
        Service.Get<SceneController>().OnLoading += OnChangeScene;
    }

    public Sprite GetSprite(string name)
    {
        if (DefaultSprite.ContainsKey(name))
            return DefaultSprite[name];
        return null;
    }

    public void LoadSpriteToImage(string name, Image image)
    {
        if (LoadedSprites.TryGetValue(name, out var sprite))
        { 
            image.sprite = sprite;
        }
        else
        {
            Addressables.LoadAssetAsync<Sprite>(name).Completed += result =>
            {
                if (result.Status == AsyncOperationStatus.Succeeded)
                {
                    image.sprite = result.Result;
                    LoadedSprites[name] = result.Result;
                }
            };
        }
    }
    private void OnChangeScene(float value)
    {
        LoadedSprites.Clear();
    }

}
