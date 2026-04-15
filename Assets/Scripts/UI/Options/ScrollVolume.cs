using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ScrollVolume : MonoBehaviour
{
    [Header("Audio Mixer")]
	[SerializeField] private AudioMixer _audioMixer;
	[SerializeField] private string _exposedParameter = "Master";

    [Header("Control")]
	[SerializeField] private Slider _slider;

	private void Awake()
	{
		ValidateReferences();

		if (_slider != null)
		{
			_slider.minValue = 0f;
			_slider.maxValue = 100f;
		}
	}

	private void OnEnable()
	{
		if (!ValidateReferences()) return;

		if (_slider != null)
		{
			_slider.minValue = 0f;
			_slider.maxValue = 100f;
			_slider.onValueChanged.RemoveListener(SetVolume);
			_slider.onValueChanged.AddListener(SetVolume);
			SetVolume(_slider.value);
		}
	}

	private void OnDisable()
	{
		if (_slider != null)
        {
			_slider.onValueChanged.RemoveListener(SetVolume);
        }
	}

	public void SetVolume(float value)
	{
		float clampedValue = Mathf.Clamp(value, 0f, 100f);
		float normalizedValue = clampedValue / 100f;
		float dB = -80f;

		if (normalizedValue > 0.0001f)
        {
			dB = Mathf.Log10(normalizedValue) * 20f;
        }

		_audioMixer.SetFloat(_exposedParameter, dB);
	}

	private bool ValidateReferences()
	{
		bool isValid = true;

		if (_audioMixer == null)
		{
			Debug.LogWarning("ScrollVolume: falta asignar AudioMixer.");
			isValid = false;
		}

		if (_slider == null)
		{
			Debug.LogWarning("ScrollVolume: falta asignar Slider.");
			isValid = false;
		}

		if (string.IsNullOrEmpty(_exposedParameter))
		{
			Debug.LogWarning("ScrollVolume: falta el nombre del parámetro expuesto.");
			isValid = false;
		}

		return isValid;
	}
}
