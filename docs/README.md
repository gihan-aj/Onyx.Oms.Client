# WinUI 3 Shell Extraction Guide

This documentation guides you through creating a modern WinUI 3 application with a shell similar to the WinUI Gallery, tailored for an Order Management System using Vertical Slice Architecture (VSA).

## Guides

1.  **[Prerequisites & Packages](packages.md)**
    *   List of required NuGet packages (WinAppSDK, CommunityToolkit, etc.).

2.  **[Project Setup & Architecture](project_setup.md)**
    *   How to structure your solution (VSA).
    *   Initial project configuration.

3.  **[Core Services](services.md)**
    *   Essential infrastructure code (`NavigationHelper`, `ThemeHelper`, `SuspensionManager`) extracted from the Gallery.

4.  **[Shell Implementation](shell_ui.md)**
    *   Implementing the `MainWindow`, `NavigationView` structure.
5.  **[TitleBar Implementation](titlebar_implementation.md)**
    *   **Detailed guide** on adding the Back button, Hamburger menu, and **User Profile/Actions** to the TitleBar.

## Getting Started

1.  Create your blank WinUI 3 project.
2.  Follow the **Project Setup** guide to organize your folders.
3.  Install dependencies from **Prerequisites**.
4.  Copy the code from **Core Services** into your project.
5.  Replace your `MainWindow` code with the **Shell Implementation**.
