namespace StrikeBox.Services;

public sealed class NavigationService
{
    public event Action<string>? NavigationRequested;

    public void NavigateTo(string pageKey)
    {
        NavigationRequested?.Invoke(pageKey);
    }
}
