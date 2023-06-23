using UnityEngine;

public class FertilizerMaterialController : MonoBehaviour
{
    private Fertilizer[] _collectables;
    void Start()
    {
        _collectables = GetComponentsInChildren<Fertilizer>();
        foreach (var collectable in _collectables)
        {
            var random = Random.Range(0f, 10f);
            collectable.GetComponentInChildren<Renderer>().material.SetFloat("_Offset", random);
        }
    }
}
