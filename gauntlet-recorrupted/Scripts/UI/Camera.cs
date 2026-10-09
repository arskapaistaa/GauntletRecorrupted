using Godot;
using System;

public partial class Camera : Camera2D
{
	[Export] public PlayerCharacter Player;
	[Export] public float RevolverShakeIntensity = 0.5f;
	[Export] public float ShotgunShakeIntensity = 1.0f;
	[Export] public float MaxOffset = 10.0f;
	[Export] public float IntensityDecay = 2.0f;
	private float _intensity;
	private readonly RandomNumberGenerator _rng = new RandomNumberGenerator();
	public override void _Ready()
	{
		Player.ShotCameraShake += OnShotCameraShake;
		_rng.Randomize();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		_intensity = Mathf.Max(0, _intensity - (float)delta * IntensityDecay);
		float shake = _intensity * _intensity;

		Vector2 randomDirection = new Vector2(_rng.RandfRange(-1.0f, 1.0f), _rng.RandfRange(-1.0f, 1.0f)).Normalized();
		Offset = randomDirection * shake * MaxOffset;
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
}
