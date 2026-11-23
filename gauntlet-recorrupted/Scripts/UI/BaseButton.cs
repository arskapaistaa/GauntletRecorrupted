using Godot;
using System;

public partial class BaseButton : Button
{
	[Export] private AnimationPlayer _animations = null;
	[Export] private string _pathToScene = null;
	[Export] private TriggerMode _pathToSceneMode = TriggerMode.Pressed;

	public enum TriggerMode
	{
		Pressed,
		AnimationFinished
	}
	[ExportGroup("Pressed")]
	[Export] private AudioStreamPlayer _pressedSfx = null;
	[Export] private string _pressedAnimName = null;
	[ExportSubgroup("This is a exit button")]
	[Export] private bool _isExitButton = false;
	[ExportSubgroup("This is a Start button")]
	[Export] private bool _isStartButton = false;

	[ExportGroup("Focus")]
	[Export] private bool _useFocusedStyle = true;
	[Export] private AudioStreamPlayer _enterFocusedSfx = null;
	[Export] private AudioStreamPlayer _exitFocusedSfx = null;
	[Export] private bool _grabFocusOnHover = true;
	[ExportSubgroup("If the button should grab focus, choose only one btn in the scene")]
	[Export] private bool _isGrabFocus = false;
	private StyleBox _focusedStyle = null;

	public override void _EnterTree()
	{
		Pressed += OnButtonPressed;
		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;
		MouseEntered += OnMouseEntered;

		if (_animations != null)
		{
			_animations.AnimationFinished += OnAnimationFinished;
		}
	}

	public override void _ExitTree()
	{
		Pressed -= OnButtonPressed;
		FocusEntered -= OnFocusEntered;
		FocusExited -= OnFocusExited;
		MouseEntered -= OnMouseEntered;
	}
	public override void _Ready()
	{
		if (_focusedStyle == null && HasThemeStylebox("focused_highlight"))
		{
			_focusedStyle = GetThemeStylebox("focused_highlight");
		}
		if (_isGrabFocus)
		{
			this.GrabFocus();
		}

	}

	private void OnButtonPressed()
	{
		// Play Sfx first
		if (_pressedSfx != null)
		{
			_pressedSfx.Play();
		}

		if (_pressedSfx == null)
		{
			GameManager.Instance.PlayButtonSFX();
		}

		if (_animations != null)
		{
			_animations.Play(_pressedAnimName);
		}

		// Load new scene after the last frame is finished.
		if (_pathToScene != null && _pathToSceneMode == TriggerMode.Pressed)
		{
			GetTree().CallDeferred("change_scene_to_file", _pathToScene);
		}

		if (_isExitButton)
		{
			GetTree().Quit();
		}

		if (_isStartButton)
		{
			GameManager.Instance.ResetStats();
		}
	}

	private void OnFocusEntered()
	{
		if (_useFocusedStyle && _focusedStyle != null)
		{
			AddThemeStyleboxOverride("normal", _focusedStyle);
			AddThemeStyleboxOverride("hover", _focusedStyle);
		}
		if (_enterFocusedSfx != null)
		{
			_enterFocusedSfx.Play();
		}
	}

	private void OnFocusExited()
	{
		if (_useFocusedStyle)
		{
			RemoveThemeStyleboxOverride("normal");
			RemoveThemeStyleboxOverride("hover");
		}
		if (_exitFocusedSfx != null)
		{
			_exitFocusedSfx.Play();
		}
	}

	private void OnAnimationFinished(StringName animName)
	{
		if (_animations == null)
		{
			return;
		}

		if (animName == null)
		{
			return;
		}

		if (animName == _pressedAnimName && _pathToSceneMode == TriggerMode.AnimationFinished)
		{
			if (_isStartButton && !StatsManager.Instance.IsTuotorialFinished())
			{
				GetTree().CallDeferred("change_scene_to_file", "res://Scenes/PlayTestScene/LevelTutorial.tscn");
			}
			else
			{
				GetTree().CallDeferred("change_scene_to_file", _pathToScene);
			}
		}
	}

	private void OnMouseEntered()
	{
		if (!_grabFocusOnHover || Disabled || HasFocus())
		{
			return;
		}

		this.GrabFocus();
	}
}
