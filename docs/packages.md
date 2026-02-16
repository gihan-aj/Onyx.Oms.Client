# Required NuGet Packages

To recreate the WinUI 3 Gallery shell, you will need the following NuGet packages:

## Core Dependencies
- **Microsoft.WindowsAppSDK**: The core Windows App SDK package.
- **Microsoft.Windows.CsWinRT**: Essential for C#/WinRT projection support.

## Community Toolkit (Crucial for Utilities and Controls)
The WinUI Gallery relies heavily on the Windows Community Toolkit for WinUI.
- **CommunityToolkit.WinUI.Controls.SettingsControls**: Used for settings pages (if you implement them).
- **CommunityToolkit.WinUI.Converters**: Provides common value converters used in XAML.
- **CommunityToolkit.WinUI.Animations**: Helpers for animations.
- **CommunityToolkit.WinUI.Controls.Primitives**: Various UI primitives.

## Optional / Specific Usage
- **ColorCode.WinUI**: Used for syntax highlighting (only needed if you plan to show code snippets).
- **Microsoft.Graphics.Win2D**: Used for 2D graphics rendering (only if you need advanced 2D drawing).

## Recommendation for VSA Project
Start by installing the **Core Dependencies** and **CommunityToolkit** packages in your Shell project (or UI layer).
