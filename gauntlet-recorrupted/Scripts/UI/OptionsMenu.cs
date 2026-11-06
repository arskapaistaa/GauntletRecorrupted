using System;
using Godot;

public partial class OptionsMenu : Control
{
	[Export] private OptionButton _displayModeBtn;
	[Export] private OptionButton _resolutionBtn;
	[Export] private HSlider _masterVolumeSlider;
	[Export] private HSlider _sfxVolumeSlider;
	[Export] private HSlider _musicVolumeSlider;

	public override void _Ready()
	{
		GetCurrentSettings();

		_displayModeBtn.ItemSelected += OnDisplayModeSelected;
		_resolutionBtn.ItemSelected += OnResolutionSelected;

		_masterVolumeSlider.ValueChanged += OnMasterVolumeChanged;
		_sfxVolumeSlider.ValueChanged += OnSfxVolumeChanged;
		_musicVolumeSlider.ValueChanged += OnMusicVolumeChanged;

		UpdateResolutionButton();
	}

    public override void _ExitTree()
    {
		_displayModeBtn.ItemSelected -= OnDisplayModeSelected;
		_resolutionBtn.ItemSelected -= OnResolutionSelected;

		_masterVolumeSlider.ValueChanged -= OnMasterVolumeChanged;
		_sfxVolumeSlider.ValueChanged -= OnSfxVolumeChanged;
		_musicVolumeSlider.ValueChanged -= OnMusicVolumeChanged;
    }

    private void OnDisplayModeSelected(long index)
    {
        SettingsManager.Instance.OnDisplayModeSelected((int)index);
		UpdateResolutionButton();
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
		_displayModeBtn.Select(SettingsManager.Instance.GetDisplayModeIndex());
		_resolutionBtn.Select(SettingsManager.Instance.GetResolutionIndex());

		// Audio settings
		_masterVolumeSlider.SetValueNoSignal(SettingsManager.Instance.GetMasterVolume());
		_sfxVolumeSlider.SetValueNoSignal(SettingsManager.Instance.GetSfxVolume());
		_musicVolumeSlider.SetValueNoSignal(SettingsManager.Instance.GetMusicVolume());
	}

	private void UpdateResolutionButton()
	{
		if (DisplayServer.WindowGetMode(0) == DisplayServer.WindowMode.Fullscreen)
		{
			_resolutionBtn.Disabled = true;
		}
		else
		{
			_resolutionBtn.Disabled = false;
		}
	}
}
