using Godot;

public partial class BreakEffect : CpuParticles2D
{
	public override void _Ready()
	{
		GD.Print("BreakEffect ready");
		Finished += QueueFree;
		Emitting = true;
	}
}