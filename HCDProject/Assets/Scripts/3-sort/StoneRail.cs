using UnityEngine;

public class StoneRail : MonoBehaviour
{
    [SerializeField] private RectTransform[] StonePositions;
    public RectTransform[] GetStonePositions => StonePositions;

}
