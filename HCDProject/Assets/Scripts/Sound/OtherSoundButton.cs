using System;
using UnityEngine;

public class OtherSoundButton : MonoBehaviour
{
    [SerializeField] private EButtonType _buttonType;

    public void PlayOtherSound()
    {
        switch (_buttonType)
        {
            case EButtonType.Reward:
                Service.Get<SoundManager>()?.PlaySfxSound("RewardTouch");
                break;
            case EButtonType.Node:
                Service.Get<SoundManager>()?.PlaySfxSound("NodeSelect");
                break;
            case EButtonType.Object:
                Service.Get<SoundManager>()?.PlaySfxSound("ObjectSelect");
                break;
        }
    }
}

public enum EButtonType
{
    Reward,
    Node,
    Object
}
