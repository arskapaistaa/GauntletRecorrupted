using Godot;
using System;

public partial class TutorialArea : Area2D
{
	// MK = Mouse and Keyboard
	// Controller = Game Controller
	[Export] public Control TutorialPanelMK = null;
	[Export] public Control TutorialPanelController = null;
	[Export] private AnimationPlayer _animations = null;
	[Export] private bool PlayMouseAndKeyboardAnimation = false;
	[Export] private bool PlayControllerAnimation = false;
	[Export] private String MouseAndKeyboardAnimation = "";
	[Export] private String ControllerAnimation = "";
	public override void _Ready()
	{
		BodyEntered += OnBodyEntered;
		BodyExited += OnBodyExited;

		if (TutorialPanelMK != null)
		{
			TutorialPanelMK.Visible = false;
		}
		if (TutorialPanelController != null)
		{
			TutorialPanelController.Visible = false;
		}
	}

	public override void _Process(double delta)
	{
		UpdatePanel(GameManager.Instance.IsUsingController());
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
			if (GameManager.Instance.IsUsingController())
			{
				if (TutorialPanelController != null)
				{
					TutorialPanelController.Visible = true;
					PlayAnimation(ControllerAnimation);
				}
			}
			else
			{
				if (TutorialPanelMK != null)
				{
					TutorialPanelMK.Visible = true;
					PlayAnimation(MouseAndKeyboardAnimation);
				}
			}
		}
    }

	private void OnBodyExited(Node2D body)
    {
        if (body is PlayerCharacter)
		{
			if (TutorialPanelMK != null)
			{
				TutorialPanelMK.Visible = false;
			}
			if (TutorialPanelController != null)
			{
				TutorialPanelController.Visible = false;
			}
		}
    }
	private void UpdatePanel(bool isUsingController)
	{
		if (isUsingController)
		{
			if (TutorialPanelController != null && (TutorialPanelController.Visible || TutorialPanelMK.Visible))
			{
				TutorialPanelController.Visible = true;
				PlayAnimation(ControllerAnimation);
			}
			if (TutorialPanelMK != null)
			{
				TutorialPanelMK.Visible = false;
			}
		}
		else
		{
			if (TutorialPanelMK != null && (TutorialPanelController.Visible || TutorialPanelMK.Visible))
			{
				TutorialPanelMK.Visible = true;
				PlayAnimation(MouseAndKeyboardAnimation);
			}
			if (TutorialPanelController != null)
			{
				TutorialPanelController.Visible = false;
			}
		}
	}

	private void PlayAnimation(string animationName)
	{
		if (_animations == null)
			return;
		if (TutorialPanelMK != null && TutorialPanelMK.Visible)
		{
			if (PlayMouseAndKeyboardAnimation)
				_animations.Play(animationName);
		}
		if (TutorialPanelController != null && TutorialPanelController.Visible)
		{
			if (PlayControllerAnimation)
				_animations.Play(animationName);
		}
	}
}
