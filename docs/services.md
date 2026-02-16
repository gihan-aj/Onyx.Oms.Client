# Core Services & Helpers

To maintain a clean architecture, we extract the core infrastructure helpers from the WinUI Gallery. These files should be placed in your `MyOrderApp/Services` or `MyOrderApp/Shared/Services` folder (adjust namespaces accordingly).

## 1. SuspensionManager
Handles the application's process lifetime management (PLM) and session state.

```csharp
// SuspensionManager.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace MyOrderApp.Services;

internal sealed class SuspensionManager
{
    private static Dictionary<string, object> _sessionState = new Dictionary<string, object>();
    private static List<Type> _knownTypes = new List<Type>();
    private const string sessionStateFilename = "_sessionState.xml";

    public static Dictionary<string, object> SessionState => _sessionState;
    public static List<Type> KnownTypes => _knownTypes;

    public static async Task SaveAsync()
    {
        try
        {
            foreach (var weakFrameReference in _registeredFrames)
            {
                if (weakFrameReference.TryGetTarget(out Frame? frame))
                {
                    SaveFrameNavigationState(frame);
                }
            }

            MemoryStream sessionData = new MemoryStream();
            DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
            serializer.WriteObject(sessionData, _sessionState);

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.CreateFileAsync(sessionStateFilename, CreationCollisionOption.ReplaceExisting);
            using (Stream fileStream = await file.OpenStreamForWriteAsync())
            {
                sessionData.Seek(0, SeekOrigin.Begin);
                await sessionData.CopyToAsync(fileStream);
            }
        }
        catch (Exception e)
        {
            throw new Exception("SuspensionManager failed", e);
        }
    }

    public static async Task RestoreAsync()
    {
        _sessionState = new Dictionary<string, object>();

        try
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile file = await localFolder.GetFileAsync(sessionStateFilename);
            using (IInputStream inStream = await file.OpenSequentialReadAsync())
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(Dictionary<string, object>), _knownTypes);
                if (serializer.ReadObject(inStream.AsStreamForRead()) is Dictionary<string, object> readObject)
                {
                    _sessionState = readObject;
                }
            }

            foreach (var weakFrameReference in _registeredFrames)
            {
                if (weakFrameReference.TryGetTarget(out Frame? frame))
                {
                    frame.ClearValue(FrameSessionStateProperty);
                    RestoreFrameNavigationState(frame);
                }
            }
        }
        catch (Exception e)
        {
            // throw new Exception("SuspensionManager failed", e);
            // Ignore if file not found or corrupted on first run
        }
    }

    private static DependencyProperty FrameSessionStateKeyProperty =
        DependencyProperty.RegisterAttached("_FrameSessionStateKey", typeof(string), typeof(SuspensionManager), new PropertyMetadata(null));
    private static DependencyProperty FrameSessionStateProperty =
        DependencyProperty.RegisterAttached("_FrameSessionState", typeof(Dictionary<string, object>), typeof(SuspensionManager), new PropertyMetadata(null));
    private static List<WeakReference<Frame>> _registeredFrames = new List<WeakReference<Frame>>();

    public static void RegisterFrame(Frame frame, string sessionStateKey)
    {
        if (frame.GetValue(FrameSessionStateKeyProperty) != null)
            throw new InvalidOperationException("Frames can only be registered to one session state key");

        if (frame.GetValue(FrameSessionStateProperty) != null)
            throw new InvalidOperationException("Frames must be either be registered before accessing frame session state, or not registered at all");

        frame.SetValue(FrameSessionStateKeyProperty, sessionStateKey);
        _registeredFrames.Add(new WeakReference<Frame>(frame));
        RestoreFrameNavigationState(frame);
    }

    public static Dictionary<string, object> SessionStateForFrame(Frame frame)
    {
        var frameState = (Dictionary<string, object>)frame.GetValue(FrameSessionStateProperty);

        if (frameState == null)
        {
            var frameSessionKey = (string)frame.GetValue(FrameSessionStateKeyProperty);
            if (frameSessionKey != null)
            {
                if (!_sessionState.ContainsKey(frameSessionKey))
                {
                    _sessionState[frameSessionKey] = new Dictionary<string, object>();
                }
                frameState = (Dictionary<string, object>)_sessionState[frameSessionKey];
            }
            else
            {
                frameState = new Dictionary<string, object>();
            }
            frame.SetValue(FrameSessionStateProperty, frameState);
        }
        return frameState;
    }

    private static void RestoreFrameNavigationState(Frame frame)
    {
        var frameState = SessionStateForFrame(frame);
        if (frameState.ContainsKey("Navigation"))
        {
            frame.SetNavigationState((string)frameState["Navigation"]);
        }
    }

    private static void SaveFrameNavigationState(Frame frame)
    {
        var frameState = SessionStateForFrame(frame);
        frameState["Navigation"] = frame.GetNavigationState();
    }
}
```

## 2. NavigationHelper
Provides navigation logic and handles the back stack.

```csharp
// NavigationHelper.cs
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.Generic;

namespace MyOrderApp.Services;

public class NavigationHelper
{
    private Page Page { get; set; }
    private Frame Frame { get { return this.Page.Frame; } }

    public NavigationHelper(Page page)
    {
        this.Page = page;
    }

    public event LoadStateEventHandler? LoadState;
    public event SaveStateEventHandler? SaveState;

    public void OnNavigatedTo(NavigationEventArgs e)
    {
        var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
        var pageKey = "Page-" + this.Frame.BackStackDepth;

        if (e.NavigationMode == NavigationMode.New)
        {
            var nextPageKey = pageKey;
            int nextPageIndex = this.Frame.BackStackDepth;
            while (frameState.Remove(nextPageKey))
            {
                nextPageIndex++;
                nextPageKey = "Page-" + nextPageIndex;
            }
            this.LoadState?.Invoke(this, new LoadStateEventArgs(e.Parameter, null));
        }
        else
        {
            this.LoadState?.Invoke(this, new LoadStateEventArgs(e.Parameter, (Dictionary<string, object>)frameState[pageKey]));
        }
    }

    public void OnNavigatedFrom(NavigationEventArgs e)
    {
        var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
        var pageState = new Dictionary<string, object>();
        this.SaveState?.Invoke(this, new SaveStateEventArgs(pageState));
        frameState["Page-" + this.Frame.BackStackDepth] = pageState;
    }
}

public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

public class LoadStateEventArgs : System.EventArgs
{
    public object NavigationParameter { get; private set; }
    public Dictionary<string, object>? PageState { get; private set; }

    public LoadStateEventArgs(object navigationParameter, Dictionary<string, object>? pageState)
    {
        this.NavigationParameter = navigationParameter;
        this.PageState = pageState;
    }
}

public class SaveStateEventArgs : System.EventArgs
{
    public Dictionary<string, object> PageState { get; private set; }

    public SaveStateEventArgs(Dictionary<string, object> pageState)
    {
        this.PageState = pageState;
    }
}
```

## 3. TitleBarHelper
Manages the custom title bar's interaction with the system theme (Light/Dark mode).

```csharp
// TitleBarHelper.cs
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace MyOrderApp.Services;

internal class TitleBarHelper
{
    public static void ApplySystemThemeToCaptionButtons(Window window, ElementTheme currentTheme)
    {
        if (window.AppWindow != null)
        {
            var titleBar = window.AppWindow.TitleBar;
            var foregroundColor = currentTheme == ElementTheme.Dark ? Colors.White : Colors.Black;
            titleBar.ButtonForegroundColor = foregroundColor;
            titleBar.ButtonHoverForegroundColor = foregroundColor;

            var backgroundHoverColor = currentTheme == ElementTheme.Dark ? Color.FromArgb(24, 255, 255, 255) : Color.FromArgb(24, 0, 0, 0);
            titleBar.ButtonHoverBackgroundColor = backgroundHoverColor;
        }
    }
}
```

## 4. ThemeHelper
Manages theme switching (Light/Dark/System).

```csharp
// ThemeHelper.cs
using Microsoft.UI.Xaml;

namespace MyOrderApp.Services;

public static class ThemeHelper
{
    public static ElementTheme RootTheme
    {
        get
        {
            if (Window.Current?.Content is FrameworkElement rootElement)
            {
                return rootElement.RequestedTheme;
            }
            return ElementTheme.Default;
        }
        set
        {
            if (Window.Current?.Content is FrameworkElement rootElement)
            {
                rootElement.RequestedTheme = value;
            }
            // Ideally, save this 'value' to LocalSettings here for persistence (omitted for brevity)
        }
    }
}
```
