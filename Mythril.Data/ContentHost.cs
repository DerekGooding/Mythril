namespace Mythril.Data;
public static class ContentHost
{
    private static readonly Host _host = Host.Initialize();
    public static T GetContent<T>() where T : class => _host.Get<T>();
}
