using Godot;

public partial class OptionsMenu : Control
{
	[Export] private OptionButton _displayMode;
	[Export] private OptionButton _resolution;
	[Export] private HSlider _masterVolume;
	[Export] private HSlider _sfxVolume;
	[Export] private HSlider _musicVolume;

	public override void _Ready()
	{
		GetCurrentSettings();

		_displayMode.ItemSelected += (long index) => SettingsManager.Instance.OnWindowsModeSelected((int)index);
		_resolution.ItemSelected += (long index) => SettingsManager.Instance.OnResolutionSelected((int)index);

		_masterVolume.ValueChanged += (double value) => SettingsManager.Instance.OnMasterVolumeChange((float)value);
		_sfxVolume.ValueChanged += (double value) => SettingsManager.Instance.OnSfxVolumeChange((float)value);
		_musicVolume.ValueChanged += (double value) => SettingsManager.Instance.OnMusicVolumeChange((float)value);
	}

	private void GetCurrentSettings()
	{
		// Display settings
		_displayMode.Select(SettingsManager.Instance.GetWindowModeIndex());
		_resolution.Select(SettingsManager.Instance.GetResolutionIndex());

		// Audio settings
		_masterVolume.SetValueNoSignal(SettingsManager.Instance.GetMasterVolume());
		_sfxVolume.SetValueNoSignal(SettingsManager.Instance.GetSfxVolume());
		_musicVolume.SetValueNoSignal(SettingsManager.Instance.GetMusicVolume());
	}
}
