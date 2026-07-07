using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager : BaseManager<ResourcesManager>
{
    private Dictionary<string, Sprite> DefaultSprite;
    public Sprite GetSprite(string name)
    {
        if (DefaultSprite.ContainsKey(name))
            return DefaultSprite[name];
        return null;
    }
}
