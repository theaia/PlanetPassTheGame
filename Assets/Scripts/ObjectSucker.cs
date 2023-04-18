using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSucker : MonoBehaviour
{
    [SerializeField]
    private LayerMask suckLayer;

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out Suckable suck))
        {
            suck.GetSucked(transform);
        }
    }
}
