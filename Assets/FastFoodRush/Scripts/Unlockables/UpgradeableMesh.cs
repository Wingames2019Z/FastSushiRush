using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    [RequireComponent(typeof(MeshFilter))]
    public class UpgradeableMesh : MonoBehaviour
    {
        [SerializeField] Mesh[] upgradeMeshes;

        private MeshFilter meshFilter;

        void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
        }

        public void ApplyUpgrade(int unlockLevel)
        {
            if (unlockLevel >= upgradeMeshes.Length + 2)
            {
                Debug.LogWarning("The unlock level exceeds the available upgrade meshes." +
                    " Please ensure that the unlock level is within the valid range.");

                return;
            }

            meshFilter.mesh = upgradeMeshes[unlockLevel - 2];
        }
    }
}
