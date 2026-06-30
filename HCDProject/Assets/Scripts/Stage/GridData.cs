using UnityEngine;
using UnityEngine.Serialization;

public class GridData : MonoBehaviour
{
   [SerializeField] private Vector3 CameraPos;
   public Vector3 GetCameraPos => CameraPos;
   [SerializeField] private float size;
   public float GetOrthographicSize => size;
   
}
