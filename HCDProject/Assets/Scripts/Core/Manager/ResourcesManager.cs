using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.Video;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    private Dictionary<string, Sprite> defaultSprite = new ();
    private Dictionary<string, Sprite> LoadedSprites = new ();
    private Dictionary<string, VideoClip> LoadedVideos = new ();
    private Dictionary<string, UnityEvent<Sprite>> spriteQueue = new();

    private void Start()
    {
        Service.Get<SceneController>().OnLoading += OnChangeScene;
        LoadSprites("Player/Alice", defaultSprite);
        LoadSprites("Player/Serah", defaultSprite);
        LoadSprites("Player/Noah", defaultSprite);
        LoadSprites("Player/Spayin", defaultSprite);
        LoadSprites("Icon/Icons", defaultSprite);
        LoadSprites("Icon/Icons", defaultSprite);
        LoadSprites("legacy_atlas", defaultSprite);
        LoadSprites("original_atlas", defaultSprite);
    }

    public Sprite GetSprite(string name, UnityAction<Sprite> bind)
    {
        if (string.IsNullOrEmpty(name)) return null;
        if (defaultSprite.ContainsKey(name))
            return defaultSprite[name];
        if (LoadedSprites.ContainsKey(name))
            return LoadedSprites[name];
        LoadSprite(name, bind);
            
        return null;
    }

    public void LoadSpriteToImage(string name, Image image)
    {
        if (defaultSprite.TryGetValue(name, out var defSprite))
        { 
            image.sprite = defSprite;
            return;
        }
        if (LoadedSprites.TryGetValue(name, out var loadSprite))
        { 
            image.sprite = loadSprite;
            return;
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

    private void LoadSprites(string address, Dictionary<string, Sprite> dict)
    {
        Addressables.LoadAssetAsync<IList<Sprite>>(address).Completed += result =>
        {
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (Sprite sp in result.Result)
                {
                    dict[$"{address}[{sp.name}]"] = sp;
                }
            }
        };
        
    }

    private void LoadSprite(string address, UnityAction<Sprite> action)
    {
        if(spriteQueue.ContainsKey(address))
        {
            spriteQueue[address].AddListener(action);
            return;
        }
        spriteQueue[address] = new UnityEvent<Sprite>();
        spriteQueue[address].AddListener(action);
        
        Addressables.LoadAssetAsync<Sprite>(address).Completed += result =>
        {
            if (result.Status == AsyncOperationStatus.Succeeded)
            {
                LoadedSprites[address] = result.Result;
                spriteQueue[address].Invoke(result.Result);
                spriteQueue.Remove(address);
            }
            else if (result.Status == AsyncOperationStatus.Failed)
            {
                Debug.Log($"AssetLoadFail : {address}");
            }
        };
    }

}
