using Godot;
using System;

public partial class TutorialArea : Area2D
{
	[Export] public Control TutorialPanel = null;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		if (TutorialPanel != null)
		{
			TutorialPanel.Visible = false;
		}
	}
    public override void _ExitTree()
    {
        BodyEntered -= OnBodyEntered;
		BodyExited -= OnBodyExited;
    }

	public enum Tutorial
	{
		None,
		MoveAndRotate,
		Shovel,
		ShootAndReload,
		Exit,
		Shift,
		Spawners
	}

	private void OnBodyEntered(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			if (TutorialPanel != null)
			{
				TutorialPanel.Visible = true;
			}
		}
    }

	private void OnBodyExited(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			if (TutorialPanel != null)
			{
				TutorialPanel.Visible = false;
			}
		}
    }

}
