//PlayerData.cs handles saving player currency & unlocking cars/tracks
using UnityEngine;
using System.Collections;

namespace RGSK
{
    public class PlayerData : MonoBehaviour
    {

        private static int startingCurrency = 100000;
        public static int currency;

        public static void ClearData()
        {
            PlayerPrefs.DeleteAll();
            currency = startingCurrency;
            PlayerData.SaveCurrency();
            Debug.Log("Player data has been reset.");
        }

        internal static void SaveCurrency()
        {
            PlayerPrefs.SetInt("Currency", currency);
        }

        internal static void AddCurrency(int amount)
        {
            currency = PlayerPrefs.GetInt("Currency", currency);

            currency += amount;

            PlayerData.SaveCurrency();
        }

        internal static void DeductCurrency(int amount)
        {
            if (currency > 0)
            {
                currency -= amount;
            }

            PlayerData.SaveCurrency();
        }

        internal static void LoadCurrency()
        {
            if (PlayerPrefs.HasKey("Currency"))
                currency = PlayerPrefs.GetInt("Currency", currency);
            else
                ClearData();
        }

        internal static void Unlock(string name)
        {
            PlayerPrefs.SetInt(name, 1);
        }
    }
}
