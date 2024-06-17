using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class TrashBin : Interactable
    {
        [SerializeField] private float throwInterval;
        [SerializeField] private Vector3 throwOffset = new Vector3(0f, 0.1f, 0.5f);

        private float throwCooldown;

        void Update()
        {
            if (player == null) return;

            throwCooldown -= Time.deltaTime;

            if (throwCooldown <= 0)
            {
                throwCooldown = throwInterval;

                var thrownObj = player.Stack.RemoveFromStack();
                if (thrownObj == null) return;

                AudioManager.Instance.PlaySFX(AudioID.Trash);

                thrownObj.DOJump(transform.TransformPoint(throwOffset), 5f, 1, 0.5f)
                    .OnComplete(() =>
                    {
                        PoolManager.Instance.ReturnObject(thrownObj.gameObject);
                        AudioManager.Instance.PlaySFX(AudioID.Bin);
                    });
            }
        }

        public void ThrowToBin(WobblingStack stack)
        {
            var thrownObj = stack.RemoveFromStack();

            thrownObj.DOJump(transform.position + Vector3.up, 5f, 1, 0.5f)
                .OnComplete(() =>
                {
                    PoolManager.Instance.ReturnObject(thrownObj.gameObject);
                });
        }
    }
}
