using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace StoreManagement.Components.ProductPanel
{
    public class VariantDropdown : MonoBehaviour
    {
        public Action<string, int> onValueChanged;
        
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Dropdown dropdown;
        
        public string Title => title.text;
        public string Value => dropdown.options[dropdown.value].text;
        
        public void Configure(string fieldName, List<string> values)
        {
            title.text = fieldName;
            
            dropdown.ClearOptions();
            foreach (string value in values)
            {
                dropdown.options.Add(new TMP_Dropdown.OptionData(value));
            }
            dropdown.SetValueWithoutNotify(0);
            dropdown.onValueChanged.AddListener(OnValueChanged);
        }

        public int GetIndexOfOption(string option)
        {
            return dropdown.options.FindIndex(x => x.text == option);
        }
        
        public void SetValueWithoutNotify(int value)
        {
            dropdown.SetValueWithoutNotify(value);
            dropdown.RefreshShownValue();
        }

        private void OnValueChanged(int newValue)
        {
            onValueChanged?.Invoke(title.text, newValue);
        }
    }
}