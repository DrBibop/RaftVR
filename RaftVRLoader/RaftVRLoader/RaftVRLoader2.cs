/*using RaftVR;

public class RaftVRLoader : Mod
{
    public static SettingsAPI ExtraSettingsAPI_Settings;

    private ModInitializer initializer;

    public void Start()
    {
        initializer = gameObject.AddComponent<ModInitializer>();

        ExtraSettingsAPI_Settings = new SettingsAPI(initializer);

        initializer.Init();
    }

    public void OnModUnload()
    {
        initializer.Unload();
    }
}*/