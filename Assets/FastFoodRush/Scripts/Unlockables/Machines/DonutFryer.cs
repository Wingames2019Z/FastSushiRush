using System.Collections.Generic;
using System.Linq;

namespace CryingSnow.FastFoodRush
{
    public class DonutFryer : FoodMachine
    {
        private List<FlippingObject> flippingDonuts;
        private List<TravelingObject> travelingDonuts;
        private MovingObject movingDonut;

        protected override void Awake()
        {
            base.Awake();

            flippingDonuts = GetComponentsInChildren<FlippingObject>(true).ToList();
            travelingDonuts = GetComponentsInChildren<TravelingObject>(true).ToList();
            movingDonut = GetComponentInChildren<MovingObject>();
        }

        protected override void UpdateStats()
        {
            base.UpdateStats();

            flippingDonuts.ForEach(donut => donut.gameObject.SetActive(unlockLevel == 1));
            travelingDonuts.ForEach(donut => donut.gameObject.SetActive(unlockLevel == 2));
            movingDonut.gameObject.SetActive(unlockLevel == 3);
        }
    }
}
