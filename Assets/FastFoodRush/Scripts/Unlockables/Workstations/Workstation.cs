using UnityEngine;

namespace CryingSnow.FastFoodRush
{
    public class Workstation : Unlockable
    {
        [SerializeField] private WorkingSpot workingSpot;
        [SerializeField] private GameObject worker;

        protected bool hasWorker => unlockLevel > 1 ? true : workingSpot.HasWorker;

        public override void Unlock(bool animate = true)
        {
            base.Unlock(animate);

            worker.SetActive(unlockLevel > 1);
            workingSpot.gameObject.SetActive(unlockLevel <= 1);
        }
    }
}
