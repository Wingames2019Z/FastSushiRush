using CryingSnow.FastFoodRush;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;
using System.Linq;
using System.Diagnostics;
using System;

public class RewardButton : MonoBehaviour
{
    [SerializeField] RewardType _rewardType;
    [SerializeField] RewardAds _rewardAds;
    [SerializeField] Button _rewardButton;
    [SerializeField] TextMeshProUGUI _rewardButtonText;
    [SerializeField] int _rewardMoney;
    [SerializeField] GameObject _parentUI;
    void Start()
    {
        _rewardButton.onClick.AddListener(() => PressedRewardButton());
    }
    void PressedRewardButton()
    {
        AddRewardMoney();
    }
    void OnEnable()
    {
        SetRewardMoney();
    }
    void SetRewardMoney()
    {
        _rewardMoney = 0;
        var moneyList = new List<int>();
        if (_rewardType == RewardType.Player)
        {
            moneyList.Add(RestaurantManager.Instance.GetUpgradePrice(Upgrade.PlayerCapacity));
            moneyList.Add(RestaurantManager.Instance.GetUpgradePrice(Upgrade.PlayerSpeed));
            moneyList.Add(RestaurantManager.Instance.GetUpgradePrice(Upgrade.Profit));
        }
        else
        {
            moneyList.Add(RestaurantManager.Instance.GetUpgradePrice(Upgrade.EmployeeAmount));
            moneyList.Add(RestaurantManager.Instance.GetUpgradePrice(Upgrade.EmployeeCapacity));
            moneyList.Add(RestaurantManager.Instance.GetUpgradePrice(Upgrade.EmployeeSpeed));
        }
        _rewardMoney = moneyList.Min();
        RestaurantManager.Instance.GetFormattedMoney(_rewardMoney);
        _rewardButtonText.text = "GET " + _rewardMoney;
    }

    void AddRewardMoney()
    {
        _parentUI.SetActive(false);
        _rewardAds.Show(() => EndAction());

        void EndAction()
        {
            RestaurantManager.Instance.AdjustMoney(_rewardMoney);
            _parentUI.SetActive(true);
        }
    }

    enum RewardType
    {
        Player = 0,
        Employee = 1
    }
}
