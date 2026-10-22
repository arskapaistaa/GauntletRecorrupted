using Godot;
using System;

public partial class Camera : Camera2D
{
	[ExportGroup("Cursor Follow Settings")]
	// The dead zone is the area around the center of the screen where the camera won't move in response to mouse movement.
	[Export(PropertyHint.Range, "0.0,0.95,0.05")] public float DeadZone = 0.5f;
	// The maximum distance the camera can shift in response to mouse movement.
	[Export(PropertyHint.Range, "0.0,2000.0,10.0")] public float MaxPeekDistance = 120.0f;
	// The speed at which the camera moves towards the target position when peeking.
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
			if (!GameManager.Instance.IsUsingController())
			{
				// Get the mouse position in the viewport
				Vector2 mousePosition = GetViewport().GetMousePosition();
				// Get the center point of the viewport as a Vector2
				Vector2 screenCenter = GetViewportRect().Size / 2;
				// The vector from the center of the screen to the mouse position
				Vector2 pixelOffset = mousePosition - screenCenter;
				// THE MOST IMPORTANT PART. Dividing the pixel offset by the screen center gives us
				// a relative offset where the center of the screen is 0,0 and the edges of the screen are -1,-1 to 1,1.
				// This allows us to have a consistent camera movement regardless of the screen resolution.
				Vector2 relativeOffset = pixelOffset / screenCenter;
				// Clamp the relative offset to ensure it doesn't exceed the range of -1 to 1.
				// This means that even if the mouse is outisde the screen, the camera will only move to its maximum offset.
				relativeOffset.X = Mathf.Clamp(relativeOffset.X, -1.0f, 1.0f);
				relativeOffset.Y = Mathf.Clamp(relativeOffset.Y, -1.0f, 1.0f);
				// After clamping, we apply the dead zone to the relative offset.
				relativeOffset.X = ApplyDeadZone(relativeOffset.X);
				relativeOffset.Y = ApplyDeadZone(relativeOffset.Y);
				targetShift = relativeOffset * MaxPeekDistance;
			}
			else
			{
				Vector2 stickInput = Input.GetVector("AimLeft", "AimRight", "AimUp", "AimDown");
				targetShift = stickInput * MaxPeekDistance;

			}
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