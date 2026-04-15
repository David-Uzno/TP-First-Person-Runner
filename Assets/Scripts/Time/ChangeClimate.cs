using UnityEngine;

[RequireComponent(typeof(LoadLocalTime))]
public class ChangeClimate : MonoBehaviour
{
    [Header("Time")]
	[SerializeField] private TimeUnit _timeUnit = TimeUnit.Hours;
	[SerializeField] private LoadLocalTime _loadLocalTime;

	[Header("Intensity")]
	[SerializeField, Min(0f)] private float _minimumIntensity = 0f;
	[SerializeField, Min(0f)] private float _maximumIntensity = 1f;

	[Header("Background (HSV)")]
	[SerializeField, Range(0f, 100f)] private float _backgroundVMin = 0f;
	[SerializeField, Range(0f, 100f)] private float _backgroundVMax = 85f;

	[Header("References")]
	[SerializeField] private Camera _camera;
	[SerializeField] private Light _light;

	private int _lastBackgroundVTrunc = int.MinValue;

	private int _lastUnitValue = int.MinValue;
	private TimeUnit _lastTimeUnit = (TimeUnit)(-1);

	private void Awake()
	{
		if (_loadLocalTime == null)
		{
			_loadLocalTime = GetComponent<LoadLocalTime>();
		}

		if (_camera == null)
		{
			_camera = Camera.main;
		}

		ApplyClimate();
	}

	private void Update()
	{
		ApplyClimate();
	}

	private void ApplyClimate()
	{
		if (_light == null || _loadLocalTime == null)
		{
			return;
		}

		// Si cambió la unidad seleccionada, se fuerza el recálculo
		if (_timeUnit != _lastTimeUnit)
		{
			_lastUnitValue = int.MinValue;
			_lastTimeUnit = _timeUnit;
		}

		int currentUnitValue = _loadLocalTime.GetUnitValue(_timeUnit);
		if (currentUnitValue == _lastUnitValue)
		{
			return;
		}

		_lastUnitValue = currentUnitValue;

		float normalizedTime = _loadLocalTime.GetNormalizedTime();
		float clamped = 1f - Mathf.Clamp01(normalizedTime);

		ApplyIntensity(clamped);
		ApplyBackground(clamped);
	}

	private void ApplyIntensity(float clamped)
	{
		if (_light == null) return;
		_light.intensity = Mathf.Lerp(_minimumIntensity, _maximumIntensity, clamped);
	}

	private void ApplyBackground(float clamped)
	{
		if (_camera == null) return;

		Color bg = _camera.backgroundColor;
		Color.RGBToHSV(bg, out float h, out float s, out float v);

		// Calcular V dentro del rango definido (se asume 0..255 como escala si el rango supera 1)
		float rawNewV = Mathf.Lerp(_backgroundVMin, _backgroundVMax, clamped);

		// Truncar (omitir decimales) según requerimiento
		int newVTrunc = (int)rawNewV;

		// Evitar actualizaciones innecesarias: sólo si cambió el V truncado
		if (newVTrunc == _lastBackgroundVTrunc)
		{
			return;
		}

		_lastBackgroundVTrunc = newVTrunc;

		// Normalizar al rango 0..1 para Color.HSVToRGB
		float normalizedV = (_backgroundVMax > 1f || _backgroundVMin > 1f) ? (newVTrunc / 100f) : newVTrunc;

		Color newBg = Color.HSVToRGB(h, s, Mathf.Clamp01(normalizedV));
		newBg.a = bg.a;
		_camera.backgroundColor = newBg;
	}
}
