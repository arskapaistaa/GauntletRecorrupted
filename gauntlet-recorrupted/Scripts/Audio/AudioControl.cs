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
		AudioEffect reverb = AudioServer.GetBusEffect(busID, effectID);

		if (reverb is AudioEffectReverb audioEffectReverb)
		{
			audioEffectReverb.RoomSize = 0.0f;
		}

		AudioServer.SetBusEffectEnabled(busID, effectID, enabled);

		Tween tween = GetTree().CreateTween();
		tween.TweenProperty(AudioServer.GetBusEffect(busID, effectID), "room_size", 1.0f, 1.0);
	}
}
