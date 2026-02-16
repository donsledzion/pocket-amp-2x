# Custom "System Menu" Architecture
### Project: Winamp 2.7 (Android / Unity)

---

# 1. Goal

Recreate Windows-like "system" contextual menus (similar to WINAPI / Win32 popup menus, file dialogs and explorers) in a platform-agnostic way inside Unity.

The system must support:

- Load / Save EQ Presets
- Delete preset (with confirmation popup)
- Load / Save Playlists
- Delete playlist (with confirmation popup)
- Browse / Import / Apply Skins
- Browse / Launch Visualization Plugins
- Reusable confirmation dialogs
- Context menus (right-click / long-press)
- File explorer–like navigation UI

The UI layout itself will be composed in Unity.
This document defines architecture, logic flow and abstraction layer.

---

# 2. High-Level Architecture

We separate the system into 5 layers:

```
[ UI Layer ]
    ↓
[ Menu Presentation Layer ]
    ↓
[ Menu Core Framework ]
    ↓
[ Domain Services ]
    ↓
[ Storage Layer ]
```

---

# 3. Core Design Principles

1. Menu logic must be UI-independent.
2. All menus defined declaratively (data-driven).
3. Commands must be reusable.
4. Async operations supported.
5. Confirmations handled generically.
6. File-like browsing abstracted from filesystem.

---

# 4. Menu Core Framework

This layer mimics the idea of Win32 HMENU + command IDs.

## 4.1 Core Interfaces

```csharp
public interface IMenuItem
{
    string Id { get; }
    string Label { get; }
    bool IsEnabled { get; }
    bool IsVisible { get; }
    IMenuCommand Command { get; }
    IReadOnlyList<IMenuItem> Children { get; }
}

public interface IMenuCommand
{
    Task ExecuteAsync(MenuContext context);
}

public class MenuContext
{
    public object Sender { get; init; }
    public object SelectedItem { get; init; }
    public IDictionary<string, object> Data { get; init; }
}
```

---

## 4.2 Menu Builder (Declarative Definition)

Menu definitions should be constructed via a builder:

```csharp
public interface IMenuBuilder
{
    IMenuBuilder AddItem(string id, string label, IMenuCommand command);
    IMenuBuilder AddSubMenu(string id, string label, Action<IMenuBuilder> build);
    IMenuBuilder AddSeparator();
    IReadOnlyList<IMenuItem> Build();
}
```

Menus should be defined as factories:

```csharp
public interface IMenuFactory
{
    IReadOnlyList<IMenuItem> Create(MenuContext context);
}
```

Each domain (Presets, Playlists, Skins, Plugins) has its own MenuFactory.

---

# 5. Presentation Layer

This layer adapts the menu model to Unity UI.

Responsibilities:

- Instantiate prefabs
- Bind label text
- Enable / disable buttons
- Handle click → call IMenuCommand.ExecuteAsync
- Render submenus recursively

Important: Presentation never contains business logic.

---

# 6. Domain Services

Each domain feature has its own service layer.

Example services:

```csharp
public interface IPresetService
{
    Task SaveAsync(string name, EqualizerPreset preset);
    Task<EqualizerPreset> LoadAsync(string name);
    Task DeleteAsync(string name);
    Task<IReadOnlyList<string>> GetAllAsync();
}

public interface IPlaylistService
{
    Task SaveAsync(string name, Playlist playlist);
    Task<Playlist> LoadAsync(string name);
    Task DeleteAsync(string name);
    Task<IReadOnlyList<string>> GetAllAsync();
}

public interface ISkinService
{
    Task ApplyAsync(string skinId);
    Task ImportAsync(string path);
    Task<IReadOnlyList<SkinInfo>> GetAvailableAsync();
}

public interface IVisualizationService
{
    Task LaunchAsync(string pluginId);
    Task<IReadOnlyList<VisualizationInfo>> GetAvailableAsync();
}
```

---

# 7. Storage Layer

Storage abstraction allows:

- Internal storage
- External storage
- Future cloud sync

```csharp
public interface IStorageProvider
{
    Task SaveFileAsync(string path, byte[] data);
    Task<byte[]> LoadFileAsync(string path);
    Task DeleteFileAsync(string path);
    Task<IReadOnlyList<string>> ListAsync(string directory);
}
```

Implementations:

- LocalFileStorageProvider
- AndroidScopedStorageProvider

---

# 8. Confirmation System

Generic confirmation dialog system.

```csharp
public interface IDialogService
{
    Task<bool> ShowConfirmationAsync(string title, string message);
    Task<string> ShowInputAsync(string title, string placeholder);
}
```

Example DeletePresetCommand:

```csharp
public class DeletePresetCommand : IMenuCommand
{
    private readonly IPresetService _presetService;
    private readonly IDialogService _dialogService;

    public async Task ExecuteAsync(MenuContext context)
    {
        var name = (string)context.SelectedItem;

        var confirmed = await _dialogService.ShowConfirmationAsync(
            "Delete preset",
            $"Are you sure you want to delete '{name}'?"
        );

        if (!confirmed)
            return;

        await _presetService.DeleteAsync(name);
    }
}
```

---

# 9. File Explorer–Like Browser

Instead of exposing raw filesystem, create abstraction:

```csharp
public interface IBrowserNode
{
    string Id { get; }
    string Name { get; }
    bool IsDirectory { get; }
    Task<IReadOnlyList<IBrowserNode>> GetChildrenAsync();
}
```

Used for:

- Skin browsing
- Plugin browsing
- Playlist browsing

UI renders tree/list based on IBrowserNode.

---

# 10. Context Menu Pattern

Each selectable UI element should expose:

```csharp
public interface IContextMenuProvider
{
    IMenuFactory GetMenuFactory();
}
```

Long press / right-click flow:

1. Detect interaction
2. Get provider
3. Build menu
4. Render popup

---

# 11. Example: Preset Menu Structure

```
Presets
 ├── Load...
 ├── Save...
 ├── Delete
 └── Export...
```

Load submenu dynamically populated from IPresetService.GetAllAsync().

---

# 12. Recommended Patterns

Use:

- Command Pattern (IMenuCommand)
- Factory Pattern (IMenuFactory)
- Builder Pattern (IMenuBuilder)
- Dependency Injection (Zenject / custom container)
- Async/Await everywhere

Avoid:

- UI calling services directly
- Static singletons
- Business logic inside MonoBehaviours

---

# 13. Optional Advanced Extension

Future-proof ideas:

- Permission-based menu items
- Dynamic enabling rules
- Undo system
- Plugin-defined menu extensions
- JSON-defined menu structures

---

# 14. Summary

This architecture gives:

- Windows-like structured menu system
- Clear separation of UI and logic
- Reusable commands
- Testable services
- Easy extensibility
- Android-safe storage abstraction

---

END OF DOCUMENT

