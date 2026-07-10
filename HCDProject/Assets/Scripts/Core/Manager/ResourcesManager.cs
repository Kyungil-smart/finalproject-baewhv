using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Video;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    private Dictionary<string, Sprite> DefaultSprite = new ();
    private Dictionary<string, Sprite> LoadedSprites = new ();
    private Dictionary<string, VideoClip> LoadedVideos = new ();

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
        if (DefaultSprite.TryGetValue(name, out var defSprite))
        { 
            image.sprite = defSprite;
        }
        if (LoadedSprites.TryGetValue(name, out var loadSprite))
        { 
            image.sprite = loadSprite;
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

    public void LoadVideo(string address)
    {
        Addressables.LoadAssetAsync<VideoClip>(address).Completed += result =>
        {
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedVideos[address] = result.Result;
            }
        };
        
    }

    public VideoClip GetVideo(string address)
    {
        return LoadedVideos.GetValueOrDefault(address);
    }
    
    private void OnChangeScene(float value)
    {
        foreach (var v in LoadedVideos.Values)
        {
            Addressables.Release(v);
        }
        foreach (var s in LoadedSprites.Values)
        {
            Addressables.Release(s);
        }
        LoadedVideos.Clear();
        LoadedSprites.Clear();
    }

}
