# Home Page UI & Card Design

The WinUI 3 Gallery has a very polished, modern home page featuring a hero banner, filter tabs, and beautiful responsive cards for items. Here is a breakdown of how they achieved that look, which you can apply to an Order Management System Dashboard.

## 1. Overall Structure
The `HomePage.xaml` uses a scrollable flow consisting of three main parts:
1.  **Header (`HomePageHeader`)**: The large, visually rich top area.
2.  **Filter (`SelectorBar`)**: The "Recent" and "Favorites" toggle.
3.  **Content Grid (`toolkit:SwitchPresenter` + `GridView`)**: The actual cards displaying data.

## 2. The Hero Header & Top Tiles
The top of the WinUI Gallery features a hero image with a smooth fade effect, and a scrolling list of "Tiles". This is found in `Controls/HomePage/HomePageHeader.xaml`.

*   **The Fade Effect**: It uses a custom `OpacityMaskView` combined with a `LinearGradientBrush` (going from transparent to solid) to make the `HeroImage` smoothly blend into the background.
*   **The Top Tiles (`Tile.xaml`)**: The "Getting Started", "Design", etc., buttons are a custom `UserControl`.
    *   They use an **Acrylic Background**: `Background="{ThemeResource AcrylicBackgroundFillColorDefaultBrush}"`
    *   They are wrapped in a `HyperlinkButton` so they act as click targets with hover effects.
    *   **OMS Use Case**: You could use this "Tile" design at the top of an OMS dashboard for Quick Actions (e.g., "New Order", "Add Customer").

## 3. The Content Cards (Control Tiles)
The main list of items ("Recent", "Favorites") uses the standard WinUI **`GridView`**. The "beautiful cards" are achieved by creating a custom `DataTemplate` for the `GridView.ItemTemplate`.

In the Gallery, this template is called `ControlItemTemplate` (defined in `Styles/ItemTemplates.xaml`). Here is the anatomy of what makes that card look good:

```xml
<Grid Width="300"
      Height="96"
      Padding="8"
      Background="{ThemeResource ControlFillColorDefaultBrush}"
      BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}"
      BorderThickness="1"
      CornerRadius="8"> <!-- Also consider: StaticResource OverlayCornerRadius -->
    
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto" /> <!-- Icon -->
        <ColumnDefinition Width="*" />    <!-- Text -->
    </Grid.ColumnDefinitions>

    <!-- Icon Image -->
    <Image Source="{x:Bind ImagePath}" Width="32" Stretch="Uniform" Margin="8,12,16,0" VerticalAlignment="Top" />

    <!-- Text Stack -->
    <StackPanel Grid.Column="1" VerticalAlignment="Center">
        <TextBlock Text="{x:Bind Title}" Style="{StaticResource BodyStrongTextBlockStyle}" />
        <TextBlock Text="{x:Bind Subtitle}" Style="{StaticResource CaptionTextBlockStyle}" Foreground="{ThemeResource TextFillColorSecondaryBrush}" TextTrimming="WordEllipsis"/>
    </StackPanel>
</Grid>
```

**Key Takeaways for your OMS:**
1.  **Use `GridView`**: For dashboards or lists of orders, use a `GridView`.
2.  **Style the Container**: Add a `CornerRadius="8"` and a `BorderThickness="1"` with `CardStrokeColorDefaultBrush` to your `DataTemplate`'s root `Grid`.
3.  **Use System Brushes**: Notice how they use `ControlFillColorDefaultBrush` for the background and `TextFillColorSecondaryBrush` for subtitles. This guarantees the cards look perfect in both Light and Dark modes.

## 4. Switching Content (Filters)
To toggle between "Recent" and "Favorites", the Gallery uses:
1.  **`SelectorBar`**: A standard WinUI control for the pill-shaped toggle buttons.
2.  **`SwitchPresenter`** (from CommunityToolkit.WinUI): This cleanly binds to the `SelectorBar.SelectedItem.Tag` and instantly swaps out the underlying `GridView` panels without complex code-behind logic.
