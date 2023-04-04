using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public int CollectableAmount { get; private set; }

    public void CollectableCollected()
    {
        CollectableAmount++;
    }
}
