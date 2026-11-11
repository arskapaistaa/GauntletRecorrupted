using Godot;
using System;

public partial class AudioStreams : Node
{
	[Export] public AudioStream TakeDmgStream = null;
	[Export] public AudioStream DefeatStream = null;
	[Export] public AudioStream NoticeStream = null;
	[Export] public AudioStream AttackStream = null;
}
