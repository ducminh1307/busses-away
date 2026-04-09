using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BussesAway
{
    public class BusStation : MonoBehaviour
    {
        [SerializeField] private TMP_Text amountTxt;
        private Dictionary<ColorType, List<Passenger>> characters;
        
        public void AddPassenger()
        {
            
        }
    }
}