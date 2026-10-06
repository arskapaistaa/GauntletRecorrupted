using Godot;
using System;
using System.Collections.Generic;

public partial class SettingsManager : Node
{
	public static SettingsManager Instance
	{
		get;
		private set;
	}

	public SettingsManager()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else if (Instance != this)
		{
			QueueFree();
			return;
		}
	}

	// List of resolutions.
	private List<Vector2I> _resolutions = new()
	{
		new Vector2I(1280, 720),
		new Vector2I(1920, 1080),
		new Vector2I(2560, 1440),
		new Vector2I(3840, 2160)
	};

	private const string SettingsFilePath = "user://settings.cfg";
	private ConfigFile _config = new ConfigFile();

	private int _masterBusIndex;
	private int _sfxBusIndex;
	private int _musicBusIndex;
	private int _ambienceBusIndex;

	public override void _Ready()
	{

		_masterBusIndex = AudioServer.GetBusIndex("Master");
		_sfxBusIndex = AudioServer.GetBusIndex("SFX");
		_musicBusIndex = AudioServer.GetBusIndex("Music");
		_ambienceBusIndex = AudioServer.GetBusIndex("Ambience");

		LoadSettings();
	}
	/// <summary>
	/// Method to call from button. Changes window setting.
	/// </summary>
	/// <param name="isFullscreen">Is the window full screen or windowed</param>
	public void OnWindowsModeSelected(int index)
	{
		switch (index)
		{
			case 0:
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
				break;

			case 1:
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.ExclusiveFullscreen);
				break;

			case 2:
				DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
				break;
		}
		_config.SetValue("Video", "WindowMode", index);
		SaveSettings();
	}

	/// <summary>
	/// Method to call when player selects a resolutin in options.
	/// </summary>
	/// <param name="index"></param>
	public void OnResolutionSelected(int index)
	{
		if (index >= 0 && index < _resolutions.Count)
		{
			DisplayServer.WindowSetSize(_resolutions[index]);

			_config.SetValue("Video", "Resolution", index);
			SaveSettings();
		}
	}

	public void OnMasterVolumeChange(float linearVolume)
	{
		SetBusVolume(_masterBusIndex, linearVolume);
		_config.SetValue("Audio", "Master", linearVolume);
		SaveSettings();
	}

	public void OnSfxVolumeChange(float linearVolume)
	{
		SetBusVolume(_sfxBusIndex, linearVolume);
		_config.SetValue("Audio", "Sfx", linearVolume);
		SaveSettings();
	}

	public void OnMusicVolumeChange(float linearVolume)
	{
		SetBusVolume(_musicBusIndex, linearVolume);
		_config.SetValue("Audio", "Music", linearVolume);
		SaveSettings();
	}

	public void OnAmbienceVolumeChange(float linearVolume)
	{
		SetBusVolume(_ambienceBusIndex, linearVolume);
		_config.SetValue("Audio", "Ambience", linearVolume);
		SaveSettings();
	}

    private void SetBusVolume(int busIndex, float linearVolume)
    {
        float dbVolume = Mathf.LinearToDb(Mathf.Max(linearVolume, 0.00001f));

		AudioServer.SetBusVolumeDb(busIndex, dbVolume);

		AudioServer.SetBusMute(busIndex, linearVolume <= 0.00001f);
    }

    private void SaveSettings()
	{
		_config.Save(SettingsFilePath);
	}

	private void LoadSettings()
	{
		// Check if there is save file, else use defaults.
		Error err = _config.Load(SettingsFilePath);
		if (err != Error.Ok)
		{
			return;
		}

		int windowMode = (int)_config.GetValue("Video", "WindowMode", 1);
		OnWindowsModeSelected(windowMode);

		int resIndex = (int)_config.GetValue("Video", "Resolution", 1);
		OnResolutionSelected(resIndex);

		float masterVol = (float)_config.GetValue("Audio", "Master", 1.0f);
		SetBusVolume(_masterBusIndex, masterVol);

		float sfxVol = (float)_config.GetValue("Audio", "Sfx", 1.0f);
		SetBusVolume(_sfxBusIndex, sfxVol);

		float musicVol = (float)_config.GetValue("Audio", "Music", 1.0f);
		SetBusVolume(_musicBusIndex, musicVol);

		float ambienceVol = (float)_config.GetValue("Audio", "Ambience", 1.0f);
		SetBusVolume(_ambienceBusIndex, ambienceVol);
	}

	public int GetResolutionIndex()
	{
		return (int)_config.GetValue("Video", "Resolution", 1);
	}

	public int GetWindowModeIndex()
	{
		return (int)_config.GetValue("Video", "WindowMode", 1);
	}

	public float GetMasterVolume()
	{
		return (float)_config.GetValue("Audio", "Master", 1.0f);
	}

	public float GetSfxVolume()
	{
		return (float)_config.GetValue("Audio", "Sfx", 1.0f);
	}

	public float GetMusicVolume()
	{
		return (float)_config.GetValue("Audio", "Music", 1.0f);
	}

	public float GetAmbienceVolume()
	{
		return (float)_config.GetValue("Audio", "Ambience", 1.0f);
	}
}