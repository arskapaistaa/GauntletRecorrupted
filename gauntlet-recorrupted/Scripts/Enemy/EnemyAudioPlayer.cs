using Godot;
using System;

public partial class EnemyAudioPlayer : AudioStreamPlayer2D
{
	[Export] private AudioStreams _audioStreams = null;
	private Level _level = null;
	private const int _audioLimitAmount = 10;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_level = Level.Current;

		Finished += OnAudioFinished;
	}

    public void PlayAudioStream(Brain.State state)
	{
		if (CheckPoolStatus())
		{
			SetAudioStream(state);
			IntializeAudio();
			Play();
		}
	}

	public void IntializeAudio()
	{
		PitchScale = (float)GD.RandRange(0.9f, 1.1f);
		VolumeDb = (float)GD.RandRange(0.8f, 1.0f);
	}

	public bool CheckPoolStatus()
	{
		if (_level.EnemyAudioCount == _audioLimitAmount)
		{
			return false;
		}

		_level.EnemyAudioCount++;
		return true;
	}
	public void SetAudioStream(Brain.State state)
	{
		if (_audioStreams == null)
		{
			GD.PrintErr("EnemyAudioPlayer is missing Streams");
			return;
		}

		switch (state)
		{

			case Brain.State.Attack:
				if (_audioStreams.AttackStream != null)
				{
					Stream = _audioStreams.AttackStream;
				}
				break;

			case Brain.State.Notice:
				if (_audioStreams.NoticeStream != null)
				{
					Stream = _audioStreams.NoticeStream;
				}
				break;

			case Brain.State.Damaged:
				if (_audioStreams.TakeDmgStream != null)
				{
					Stream = _audioStreams.TakeDmgStream;
				}
				break;

			case Brain.State.Defeat:
				if (_audioStreams.DefeatStream != null)
				{
					Stream = _audioStreams.DefeatStream;
				}
				break;
		}
	}

	private void OnAudioFinished()
    {
        throw new NotImplementedException();
    }
}
