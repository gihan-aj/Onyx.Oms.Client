# Home Page UI & Card Design

The WinUI 3 Gallery has a very polished, modern home page featuring a hero banner, filter tabs, and beautiful responsive cards for items. Here is a breakdown of how they achieved that look, which you can apply to an Order Management System Dashboard.

## 1. Overall Structure
The `HomePage.xaml` uses a scrollable flow consisting of three main parts:
1.  **Header (`HomePageHeader`)**: The large, visually rich top area.
2.  **Filter (`SelectorBar`)**: The "Recent" and "Favorites" toggle.
3.  **Content Grid (`toolkit:SwitchPresenter` + `GridView`)**: The actual cards displaying data.

## 2. The Hero Header & Top Tiles
The top of the WinUI Gallery features a hero image with a smooth fade effect, and a scrolling list of "Tiles". This is found in `Controls/HomePage/HomePageHeader.xaml`.

*   **The Fade Effect**: It uses a custom `OpacityMaskView` combined with a `LinearGradientBrush` (going from transparent to solid) to make the `HeroImage` smoothly blend into the background. See below for details.
*   **The Top Tiles (`Tile.xaml`)**: The "Getting Started", "Design", etc., buttons are a custom `UserControl`.
    *   They use an **Acrylic Background**: `Background="{ThemeResource AcrylicBackgroundFillColorDefaultBrush}"`
    *   They are wrapped in a `HyperlinkButton` so they act as click targets with hover effects.
    *   **OMS Use Case**: You could use this "Tile" design at the top of an OMS dashboard for Quick Actions (e.g., "New Order", "Add Customer").

### The Hero Fade Effect
To achieve the smooth fade of the large header image into the background, the Gallery uses a custom control called `OpacityMaskView` (which utilizes the `Microsoft.UI.Composition` API to apply a visual mask). 

It works by layering a `Rectangle` with a gradient as a mask over the image grid:

```xml
<!-- In HomePageHeader.xaml -->
<local:OpacityMaskView Height="400" VerticalAlignment="Stretch">
    
    <!-- 1. The Mask (What causes the fade) -->
    <local:OpacityMaskView.OpacityMask>
        <!-- A gradient brush that goes from solid to transparent -->
        <Rectangle Fill="{ThemeResource OverlayRadialGradient}" />
    </local:OpacityMaskView.OpacityMask>

    <!-- 2. The Content (What is being faded) -->
    <Grid Background="{ThemeResource BackgroundGradient}">
        <Image Source="/Assets/GalleryHeaderImage.png" Stretch="UniformToFill" />
    </Grid>

</local:OpacityMaskView>
```

**How to implement it:**
If you want this effect in your app without writing the composition code from scratch, you will need to copy the `OpacityMaskView.cs` and `OpacityMaskView.xaml` files from the Gallery (located in `WinUIGallery/Controls/`). It is based on a piece of Windows Community Toolkit Labs code that uses a `CompositionMaskBrush` to smoothly mask the element below it.


### Anatomy of a Top Tile
If you want to recreate these "Quick Action" tiles in your OMS, here is a simplified version of the Gallery's `Tile.xaml` implementation:

```xml
<Grid Background="{ThemeResource AcrylicBackgroundFillColorDefaultBrush}"
      BorderBrush="{ThemeResource SurfaceStrokeColorFlyoutBrush}"
      CornerRadius="8"
      Width="232" Height="172">
    
    <!-- The button wrapper for click/hover states -->
    <HyperlinkButton Padding="24"
                     HorizontalAlignment="Stretch" VerticalAlignment="Stretch"
                     HorizontalContentAlignment="Stretch" VerticalContentAlignment="Stretch"
                     CornerRadius="8">
        
        <Grid RowSpacing="16">
            <Grid.RowDefinitions>
                <RowDefinition Height="36" /> <!-- Icon Area -->
                <RowDefinition Height="*" />  <!-- Text Area -->
            </Grid.RowDefinitions>
            
            <!-- Optional: Link Icon in top right corner -->
            <FontIcon Grid.RowSpan="2" HorizontalAlignment="Right" VerticalAlignment="Bottom" 
                      FontSize="14" Foreground="{ThemeResource TextFillColorSecondaryBrush}" Glyph="&#xE8A7;" Margin="-12"/>

            <!-- The Main Icon/Image (e.g., FontIcon, Image, or Path) -->
            <Image Source="/Assets/YourIcon.png" HorizontalAlignment="Left" VerticalAlignment="Top" />

            <!-- The Text block -->
            <StackPanel Grid.Row="1" Spacing="4">
                <TextBlock Text="Quick Action" 
                           Style="{StaticResource BodyStrongTextBlockStyle}" 
                           Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
                <TextBlock Text="Description of what this action does." 
                           Style="{StaticResource CaptionTextBlockStyle}" 
                           Foreground="{ThemeResource TextFillColorSecondaryBrush}"
                           TextWrapping="Wrap" />
            </StackPanel>
        </Grid>
    </HyperlinkButton>
</Grid>
```
**Key properties for the Tile:**
*   **Acrylic Background**: Gives it that translucent, modern Windows 11 feel.
*   **HyperlinkButton Wrapper**: This is a great trick to turn any complex UI block into a clickable element that automatically inherits standard hover and pressed visual states without needing custom `VisualStateManager` code.
*   **Padding**: `Padding="24"` inside the button gives the content plenty of breathing room.

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
