using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class BillBoardScript : MonoBehaviour
    {
        private void Update()
        {
            if (Camera.main)
            {
                gameObject.transform.LookAt(Camera.main.transform);
            }
        }
    }
}