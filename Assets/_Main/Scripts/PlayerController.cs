using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set;}
    public Transform head;

    private void Awake()
    {
        Instance = this;
    }
}
