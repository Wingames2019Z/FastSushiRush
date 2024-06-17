using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class FlippingObject : MonoBehaviour
    {
        void Start()
        {
            Flip();
        }

        void Flip()
        {
            float delay = Random.Range(1f, 3f);
            var sequence = DOTween.Sequence();

            sequence.SetDelay(delay);
            sequence.Append(transform.DOJump(transform.position, 1f, 1, 0.5f));
            sequence.Join(transform.DOLocalRotate(new Vector3(0, 0, 180), 0.5f, RotateMode.LocalAxisAdd));
            sequence.OnComplete(() => Flip());
        }
    }
}
