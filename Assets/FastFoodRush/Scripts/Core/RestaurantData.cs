namespace CryingSnow.FastFoodRush
{
    [System.Serializable]
    public class RestaurantData
    {
        public string RestaurantID { get; set; }

        public long Money { get; set; }

        public int EmployeeSpeed { get; set; }
        public int EmployeeCapacity { get; set; }
        public int EmployeeAmount { get; set; }
        public int PlayerSpeed { get; set; }
        public int PlayerCapacity { get; set; }
        public int Profit { get; set; }

        public int UnlockCount { get; set; }
        public int PaidAmount { get; set; }
        public bool IsUnlocked { get; set; }

        public RestaurantData(string restaurantID, long money)
        {
            RestaurantID = restaurantID;
            Money = money;
        }
    }
}
