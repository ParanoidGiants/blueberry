using UnityEngine;

public class Flower : MonoBehaviour
{
    public int vineIndex = 0;
    public int nodeCount = 0;
    public float animateFor = 1f;
    void Start()
    {
        transform.localScale = Vector3.zero;
    }
    
    public void Init(int nodeCount)
    {
        this.nodeCount = nodeCount;
        this.vineIndex = nodeCount - 1;
    }

    public void UpdateFlower(Vector3 position, Quaternion rotation)
    {
        // transform.DOScale(Vector3.one, animateFor).SetEase(Ease.OutBounce);
        vineIndex--;
        transform.position = position;
        transform.rotation = rotation;
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, 1f - (float)vineIndex / nodeCount);
    }
}
