using Sirenix.Utilities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BForBoss
{
    public class SliderBehaviour : MonoBehaviour
    {
        private Slider _customSlider = null;
        private TMP_InputField _inputField = null;

        private Slider CustomSlider
        {
            get
            {
                if (_customSlider == null)
                {
                    _customSlider = GetComponentInChildren<Slider>();
                }

                return _customSlider;
            }
        }

        private TMP_InputField CustomInputField
        {
            get
            {
                if (_inputField == null)
                {
                    _inputField = GetComponentInChildren<TMP_InputField>();
                }

                return _inputField;
            }
        }

        public float SliderValue
        {
             get=> CustomSlider.value;
             
             set => CustomSlider.value = value;
        }

        private void Awake()
        {
            CustomSlider.onValueChanged.AddListener(HandleOnSliderValueChanged);
            CustomInputField.onEndEdit.AddListener(HandleOnInputFieldEnded);
        }

        private void OnDestroy()
        {
            CustomSlider.onValueChanged.RemoveListener(HandleOnSliderValueChanged);
            CustomInputField.onEndEdit.RemoveListener(HandleOnInputFieldEnded);
        }

        private void HandleOnSliderValueChanged(float value)
        {
            CustomInputField.text = value.ToString("F");
        }

        private void HandleOnInputFieldEnded(string value)
        {
            if (value.IsNullOrWhitespace())
            {
                CustomInputField.text = CustomSlider.value.ToString("F");
            }
            else
            {
                CustomSlider.value = float.Parse(value);
            }
        }
    }
}