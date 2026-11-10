using Godot;
using System;

public partial class TutorialArea : Area2D
{
	[Export] public string LabelText = "";
	[Export] public Label TutorialLabel = null;
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		if (TutorialLabel != null)
		{
			TutorialLabel.Text = LabelText;
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
			if (TutorialLabel != null)
			{
				TutorialLabel.Visible = true;
			}
		}
    }

	private void OnBodyExited(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			if (TutorialLabel != null)
			{
				TutorialLabel.Visible = false;
			}
		}
    }

}
