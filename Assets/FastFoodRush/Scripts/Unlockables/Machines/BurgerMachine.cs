using System.Collections.Generic;
using System.Linq;

namespace CryingSnow.FastFoodRush
{
    public class BurgerMachine : FoodMachine
    {
        private List<FlippingObject> flippingPatties;
        private List<MovingObject> movingBurgers;

        protected override void Awake()
        {
            base.Awake();

            flippingPatties = GetComponentsInChildren<FlippingObject>(true).ToList();
            movingBurgers = GetComponentsInChildren<MovingObject>(true).ToList();
        }

        protected override void UpdateStats()
        {
            base.UpdateStats();

            flippingPatties.ForEach(p => p.gameObject.SetActive(unlockLevel <= 2));
            movingBurgers.ForEach(b => b.gameObject.SetActive(unlockLevel > 2));
        }
    }
}
