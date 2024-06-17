using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

namespace CryingSnow.FastFoodRush
{
    public class Unlockable : MonoBehaviour
    {
        [SerializeField] private Vector3 buyingPoint = Vector3.zero;
        [SerializeField] private Vector3 punchScale = new Vector3(0.1f, 0.2f, 0.1f);

        protected int unlockLevel;

        private List<UpgradeableMesh> upgradeableMeshes;

        protected virtual void Awake()
        {
            upgradeableMeshes = GetComponentsInChildren<UpgradeableMesh>(true).ToList();
            gameObject.SetActive(false);
        }

        public virtual void Unlock(bool animate = true)
        {
            unlockLevel++;

            if (unlockLevel > 1)
                upgradeableMeshes.ForEach(x => x.ApplyUpgrade(unlockLevel));
            else
                gameObject.SetActive(true);

            UpdateStats();

            if (!animate) return;

            foreach (var upgradeableMesh in upgradeableMeshes)
            {
                upgradeableMesh.transform.DOPunchScale(punchScale, 0.3f)
                    .OnComplete(() => upgradeableMesh.transform.localScale = Vector3.one);
            }
        }

        protected virtual void UpdateStats() { }

        public Vector3 GetBuyingPoint() => transform.TransformPoint(buyingPoint);

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            var center = GetBuyingPoint();
            var size = new Vector3(2f, 0.2f, 2f);
            Gizmos.DrawWireCube(center, size);

            GUIStyle style = new GUIStyle();
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            UnityEditor.Handles.Label(center + Vector3.up * 0.5f, "Buying\nPoint", style);
        }
#endif
    }
}
