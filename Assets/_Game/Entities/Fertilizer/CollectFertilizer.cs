using UnityEngine;

public class CollectFertilizer : MonoBehaviour
{
    public int collectableCounter;

    public void OnCollectFertilizer()
    {
        collectableCounter++;
    }
}
