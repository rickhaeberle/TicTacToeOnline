using UnityEngine;

public class PieceMovement : MonoBehaviour
{
    public float BounceSpeed = 0.2f;

    private Vector3 _initialPosition;

    private void Awake()
    {
        _initialPosition = transform.position;
    }

    void Update()
    {
        float y = _initialPosition.y + Mathf.PingPong(Time.time * BounceSpeed, 0.1f);
        transform.position = new Vector3(_initialPosition.x, y, _initialPosition.z);

    }

}
