using Godot;
using System;

public partial class Exit : Area2D
{
	[Export] private AnimatedSprite2D _sprite = null;
	[Export] private GpuParticles2D _exitOpenParticles = null;
	[Export] private PointLight2D _exitLight = null;
	[Export(PropertyHint.File, "*.tscn")] private string NextLevelPath;
	[Export] private Level _currentLevel = null;
	[Export(PropertyHint.Enum, "FadeInAndOut")]
	private string _transitionName;

	public enum State
	{
		UnReady,
		Ready
	}

	public State CurrentState = State.UnReady;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if (_currentLevel != null)
		{
			_currentLevel.OnZeroSpawnerLeft += OnZeroSpawnerLeft;
			_currentLevel.OnBearKilled += OnBearKilled;
			_currentLevel.ExitReady += OnExitReady;
		}
		else
		{
			GD.PrintErr("Exit is missin Level reference");
		}

		BodyEntered += OnBodyEntered;
		GameManager.Instance.SkipLevel += OnSkipLevel;

		if (_exitLight != null)
		{
			_exitLight.Enabled = false;
		}

		UpdateSpawnerCountLabel();
	}

    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
		_currentLevel.OnZeroSpawnerLeft -= OnZeroSpawnerLeft;
		GameManager.Instance.SkipLevel -= OnSkipLevel;
    }

    public override void _PhysicsProcess(double delta)
    {
        UpdateAnimations();
    }

	public void UpdateAnimations()
	{
		switch (CurrentState)
		{
			case State.UnReady:
				_sprite.Play("UnReady");
				break;

			case State.Ready:
				_sprite.Play("Ready");
				break;
		}
	}

    private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			if (CurrentState == State.Ready)
			{
				if (NextLevelPath != null)
				{
					if (_currentLevel.IsThisTutorialLevel)
					{
						GameManager.Instance.SetTutoriLevelComplited();
					}

					if (_currentLevel.IsThisFinalLevel)
					{
						GameManager.Instance.SetGameFinished();
					}

					TransitionController.Instance.PlayTransition(_transitionName, NextLevelPath);
				}
				else
				{
					GD.PrintErr("Exit misses Next Level Path");
					return;
				}
			}
		}
    }

	/// <summary>
	/// Level manager emits this signal, that is how Exit knows Player can use it.
	/// </summary>
	private void OnZeroSpawnerLeft()
    {
		if (_currentLevel.CurrentEndingCondition == Level.EndingCondition.SpawnersDestroyed)
		{
			SetExitReady();
		}
    }

	private void OnBearKilled()
    {
        if (_currentLevel.CurrentEndingCondition == Level.EndingCondition.BearKilled)
		{
			SetExitReady();
		}
    }

	private void OnExitReady()
    {
        SetExitReady();
    }

	private void SetExitReady()
	{
		CurrentState = State.Ready;

		if (_exitOpenParticles != null)
		{
			_exitOpenParticles.Emitting = true;
		}

		if (_exitLight != null)
		{
			_exitLight.Enabled = true;
		}
	}

	public void UpdateSpawnerCountLabel()
	{
		//if (_spawnerCountLabel != null)
		//{
		//	_spawnerCountLabel.Text = Level.Current.CurrentSpawnerCount.ToString() + "/" + Level.Current.SpawnerCount.ToString();
		// }
	}

#if DEBUG
	public void OnSkipLevel()
	{
		TransitionController.Instance.PlayTransition(_transitionName, NextLevelPath);
	}
#endif
}
