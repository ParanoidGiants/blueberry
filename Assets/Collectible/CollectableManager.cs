using UnityEngine;

public class CollectableManager : MonoBehaviour
{
    public int collectableCounter;

    public void CollectableCollected()
    {
        collectableCounter++;
    }
}
