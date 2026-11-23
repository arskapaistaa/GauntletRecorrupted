using Godot;
using System;

public partial class Destroyable : StaticBody2D
{
	[ExportGroup("Stats")]
	[Export(PropertyHint.Range, "1,100,1")] private int _health = 10;
	[Export] private bool _explosive = false;
	[Export(PropertyHint.Range, "1,50,1")] public int Dmg;

	[ExportGroup("Node references")]
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private GpuParticles2D _particles = null;
	[Export] private CollisionShape2D _collision = null;
	[Export] private ExplosiveArea _dmgArea = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_particles == null)
		{
			GD.PrintErr("Destroyable is missing particles");
		}
	}

    public void TakeDmg(int dmg)
	{
		if (_health > 0)
		{
			_health -= dmg;
		}

		if (_health <= 0)
		{
			Destroy();
		}
	}

	private void Destroy()
	{
		if (_collision != null)
		{
			_collision.SetDeferred("disabled", true);
		}

		if (_particles != null)
		{
			_particles.Restart();
		}

		UpdateSprite();

		Level.Current.UpdateNavRegion();

		if (_explosive)
		{
			if (_dmgArea != null)
			{
				_dmgArea.StartMonitoring();
			}
		}
	}

	private void UpdateSprite()
	{
		if (_sprite != null)
		{
			_sprite.Frame = GD.RandRange(1, _sprite.SpriteFrames.GetFrameCount("default") - 1);
		}
	}
}
