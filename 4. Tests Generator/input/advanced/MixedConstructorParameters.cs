namespace AdvancedSamples;

public interface ISettingsReader
{
}

public class ConfiguredClass
{
    public ConfiguredClass(ISettingsReader reader, string environmentName)
    {
    }

    public string Describe()
    {
        return string.Empty;
    }

    public void Ping()
    {
    }
}
