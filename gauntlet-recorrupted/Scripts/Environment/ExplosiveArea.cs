using Godot;
using System;

public partial class ExplosiveArea : Area2D
{
	[Export] private GpuParticles2D _particles = null;
	[Export] private Timer _timer = null;
	private Destroyable destroyable;
	public override void _Ready()
	{
		destroyable = (Destroyable)GetParent();

		Monitoring = false;

		BodyEntered += OnBodyEntered;

		if (_timer != null)
		{
			_timer.Timeout += OnTimerTimeOut;
		}
	}

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;

		if (_timer != null)
		{
			_timer.Timeout -= OnTimerTimeOut;
		}
    }

    private void OnBodyEntered(Node2D body)
    {
        // Extra safety
        if (body is PlayerCharacter player)
        {
            player.TakeDmg(destroyable.Dmg);
        }

		if (body is Enemy enemy)
		{
			enemy.TakeDmg(destroyable.Dmg);
		}

		if (body is Destroyable otherDestroyable)
		{
			if (otherDestroyable != destroyable)
			{
				otherDestroyable.TakeDmg(destroyable.Dmg);
			}
		}
    }

	public void StartMonitoring()
	{
		Monitoring = true;

		_timer.Start();

		if (_particles != null)
		{
			_particles.Restart();
		}
	}

	private void OnTimerTimeOut()
    {
        CallDeferred("queue_free");
		_timer.CallDeferred("queue_free");
    }
}
