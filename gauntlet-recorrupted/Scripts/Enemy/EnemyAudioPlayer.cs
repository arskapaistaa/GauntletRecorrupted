using Godot;
using System;

public partial class EnemyAudioPlayer : AudioStreamPlayer2D
{
	[Export] private AudioStreams _audioStreams = null;
	[Export(PropertyHint.Range, "-24,0,1,suffix:db")] private float _maxVolume = 0.0f;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{

	}

    public void PlayAudioStream(Brain.State state)
	{
		AudioStream nextStream = GetStream(state);

		if (nextStream == null)
		{
			return;
		}
		Stream = nextStream;

		IntializeAudio();
		Play();
	}

	public void IntializeAudio()
	{
		PitchScale = (float)GD.RandRange(0.9f, 1.1f);
		VolumeDb = (float)GD.RandRange(_maxVolume -2, _maxVolume);
	}
	public AudioStream GetStream(Brain.State state)
	{
		if (_audioStreams == null)
		{
			GD.PrintErr("EnemyAudioPlayer is missing Streams");
			return null;
		}

		switch (state)
		{

			case Brain.State.Attack:
				if (_audioStreams.AttackStream != null)
				{
					return _audioStreams.AttackStream;
				}
				break;

			case Brain.State.Notice:
				if (_audioStreams.NoticeStream != null)
				{
					return _audioStreams.NoticeStream;
				}
				break;

			case Brain.State.Damaged:
				if (_audioStreams.TakeDmgStream != null)
				{
					return _audioStreams.TakeDmgStream;
				}
				break;

			case Brain.State.Defeat:
				if (_audioStreams.DefeatStream != null)
				{
					return _audioStreams.DefeatStream;
				}
				break;
			case Brain.State.Ranged:
				if (_audioStreams.RangedStream != null)
				{
					return _audioStreams.RangedStream;
				}
				break;
		}
		return null;
	}
}
