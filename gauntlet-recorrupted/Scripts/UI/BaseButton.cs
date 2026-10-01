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

	[ExportGroup("Focus")]
	[Export] private AudioStreamPlayer _enterFocusedSfx = null;
	[Export] private AudioStreamPlayer _exitFocusedSfx = null;
	[ExportSubgroup("If the button should grab focus, choose only one btn in the scene")]
	[Export] private bool _isGrabFocus = false;

    public override void _EnterTree()
    {
        Pressed += OnButtonPressed;
		FocusEntered += OnFocusEntered;
		FocusExited += OnFocusExited;

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
    }
	public override void _Ready()
	{

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

		if (_animations != null)
		{
			_animations.Play(_pressedAnimName);
		}

		// Load new scene after the last frame is finished.
        if (_pathToScene != null && _pathToSceneMode == TriggerMode.Pressed)
		{
			GetTree().CallDeferred("change_scene_to_file", _pathToScene);
		}
    }

	private void OnFocusEntered()
    {
		if (_enterFocusedSfx != null)
		{
			_enterFocusedSfx.Play();
		}
    }

	private void OnFocusExited()
    {
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

		if (animName == _pressedAnimName)
		{
			GetTree().CallDeferred("change_scene_to_file", _pathToScene);
		}
    }
}
