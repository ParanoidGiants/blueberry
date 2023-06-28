using DG.Tweening;
using UnityEngine;

public class Flower : MonoBehaviour
{
    public int vineIndex = 0;
    public float animateFor = 1f;
    void Start()
    {
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, animateFor).SetEase(Ease.OutBounce);
    }
}
