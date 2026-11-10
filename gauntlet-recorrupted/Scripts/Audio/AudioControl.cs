using Godot;
using System;

public partial class AudioControl : Node
{
	[Export] private AudioStreamPlayer _music = null;

	public override void _Ready()
	{
		if (_music != null)
		{
			_music.Play();
		}
	}

	public void AudioEffect(int busID, int effectID, bool enabled)
	{
		AudioServer.SetBusEffectEnabled(busID, effectID, enabled);
	}
}
