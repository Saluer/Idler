using Unity.VisualScripting;
using UnityEngine;

public class LookingScript : MonoBehaviour
{
    private Transform _objectToLookAt;

    private void Start()
    {
        _objectToLookAt = FindFirstObjectByType(typeof(PlayerScript)).GameObject().transform;
    }

    private void Update()
    {
        if ((_objectToLookAt.position - transform.position).magnitude > 5f)
        {
            transform.LookAt(_objectToLookAt, _objectToLookAt.up);
        }
    }
}