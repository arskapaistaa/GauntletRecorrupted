using Godot;
using System;

public partial class Camera : Camera2D
{
	[ExportGroup("Cursor Follow Settings")]
	[Export(PropertyHint.Range, "0.0,0.95,0.05")] public float DeadZone = 0.5f;
	[Export(PropertyHint.Range, "0.0,2000.0,10.0")] public float MaxPeekDistance = 120.0f;
	[Export(PropertyHint.Range, "0.0,20.0,0.5")] public float PeekingSpeed = 8.0f;

	[ExportGroup("Camera Shake Settings")]
	[Export] public PlayerCharacter Player;
	[Export] public float RevolverShakeIntensity = 0.5f;
	[Export] public float ShotgunShakeIntensity = 1.0f;
	[Export] public float MaxOffset = 10.0f;
	[Export] public float IntensityDecay = 2.0f;
	private Vector2 _currentShift = Vector2.Zero;
	private float _intensity;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	private bool _isPeeking = false;
	public override void _Ready()
	{
		Player.PeekCameraMovement += OnPeekCameraMovement;
		Player.ShotCameraShake += OnShotCameraShake;
		_rng.Randomize();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Vector2 targetShift = Vector2.Zero;
		if (_isPeeking) 
		{
			// Mouse position in pixels
			Vector2 mousePosition = GetViewport().GetMousePosition();
			// Center of the screen in pixels
			Vector2 screenCenter = GetViewportRect().Size / 2;
			// Calculate the offset from the center of the screen to the mouse position
			Vector2 pixelOffset = mousePosition - screenCenter;
			// Normalize the offset to a range of -1 to 1 based on the screen size
			Vector2 relativeOffset = pixelOffset / screenCenter;
			// Clamp the relative offset to ensure it doesn't exceed the range of -1 to 1
			relativeOffset.X = Mathf.Clamp(relativeOffset.X, -1.0f, 1.0f);
			relativeOffset.Y = Mathf.Clamp(relativeOffset.Y, -1.0f, 1.0f);
			// Apply dead zone to the relative offset
			relativeOffset.X = ApplyDeadZone(relativeOffset.X);
			relativeOffset.Y = ApplyDeadZone(relativeOffset.Y);
			// Scale the relative offset by the maximum look distance to get the target shift
			targetShift = relativeOffset * MaxPeekDistance;
		}

		float weight = 1.0f - Mathf.Exp(-PeekingSpeed * (float)delta);
		_currentShift = _currentShift.Lerp(targetShift, weight);

		_intensity = Mathf.Max(0, _intensity - (float)delta * IntensityDecay);
		float shake = _intensity * _intensity;

		Vector2 randomDirection = new Vector2(_rng.RandfRange(-1.0f, 1.0f), _rng.RandfRange(-1.0f, 1.0f)).Normalized();
		Offset = _currentShift + randomDirection * shake * MaxOffset;
	}

	private void OnShotCameraShake(int weaponType)
	{
		if (weaponType == 0)
		{
			_intensity = RevolverShakeIntensity;
		}
		else if (weaponType == 1)
		{
			_intensity = ShotgunShakeIntensity;
		}
	}

	private float ApplyDeadZone(float value)
	{
		float magnitude = Mathf.Abs(value);

		if (magnitude <= DeadZone)
		{
			return 0.0f;
		}

		float scaled = (magnitude - DeadZone) / (1.0f - DeadZone);
		return scaled * Mathf.Sign(value);
	}

	private void OnPeekCameraMovement(bool isPeeking)
	{
		_isPeeking = isPeeking;
	}
}