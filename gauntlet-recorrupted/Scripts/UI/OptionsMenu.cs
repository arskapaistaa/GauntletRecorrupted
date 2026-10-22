using System;
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

		_displayMode.ItemSelected += OnDisplayModeSelected;
		_resolution.ItemSelected += OnResolutionSelected;

		_masterVolume.ValueChanged += OnMasterVolumeChanged;
		_sfxVolume.ValueChanged += OnSfxVolumeChanged;
		_musicVolume.ValueChanged += OnMusicVolumeChanged;
	}

    public override void _ExitTree()
    {
		_displayMode.ItemSelected -= OnDisplayModeSelected;
		_resolution.ItemSelected -= OnResolutionSelected;

		_masterVolume.ValueChanged -= OnMasterVolumeChanged;
		_sfxVolume.ValueChanged -= OnSfxVolumeChanged;
		_musicVolume.ValueChanged -= OnMusicVolumeChanged;
    }

    private void OnDisplayModeSelected(long index)
    {
        SettingsManager.Instance.OnDisplayModeSelected((int)index);
    }

	private void OnResolutionSelected(long index)
    {
        SettingsManager.Instance.OnResolutionSelected((int)index);
    }

	private void OnMasterVolumeChanged(double value)
    {
        SettingsManager.Instance.OnMasterVolumeChange((float)value);
		GD.Print(SettingsManager.Instance.GetMasterVolume());
    }

	private void OnSfxVolumeChanged(double value)
    {
        SettingsManager.Instance.OnSfxVolumeChange((float)value);
    }

	private void OnMusicVolumeChanged(double value)
    {
        SettingsManager.Instance.OnMusicVolumeChange((float)value);
    }

    private void GetCurrentSettings()
	{
		// Display settings
		_displayMode.Select(SettingsManager.Instance.GetDisplayModeIndex());
		_resolution.Select(SettingsManager.Instance.GetResolutionIndex());

		// Audio settings
		_masterVolume.SetValueNoSignal(SettingsManager.Instance.GetMasterVolume());
		GD.Print(SettingsManager.Instance.GetMasterVolume());
		_sfxVolume.SetValueNoSignal(SettingsManager.Instance.GetSfxVolume());
		_musicVolume.SetValueNoSignal(SettingsManager.Instance.GetMusicVolume());
	}
}
