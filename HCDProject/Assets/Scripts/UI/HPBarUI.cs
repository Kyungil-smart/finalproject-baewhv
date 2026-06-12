using UnityEngine;
using UnityEngine.UI;

public class HPBarUI : MonoBehaviour
{
    [SerializeField]private Slider hpBar;

    public void SetHPBar(float ratio)
    {
        hpBar.value = ratio;
    }

}
