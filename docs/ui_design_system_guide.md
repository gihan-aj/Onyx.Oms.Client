# Onyx.Oms UI Design System & Style Guide

## 1. Design Philosophy
**"Industrial Professional"**
* **Density:** Maximize screen real estate. Avoid massive padding in "Explorer" views.
* **Context:** Distinct separation between navigation (Master) and content (Detail).
* **Immutability:** Default views should be Read-Only Dashboards. "Edit" is a deliberate state.
* **Safety:** Sticky Headers ensure actions are always visible.

---

## 2. Global Styles (ResourceDictionary)
Add these to `Styles/CardStyles.xaml` and merge into `App.xaml` to ensure consistency.

```xml
<Style x:Key="CardGridStyle" TargetType="Grid">
    <Setter Property="Background" Value="{ThemeResource CardBackgroundFillColorDefaultBrush}" />
    <Setter Property="BorderBrush" Value="{ThemeResource CardStrokeColorDefaultBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="20" />
</Style>

<Style x:Key="CardStackStyle" TargetType="StackPanel">
    <Setter Property="Background" Value="{ThemeResource CardBackgroundFillColorDefaultBrush}" />
    <Setter Property="BorderBrush" Value="{ThemeResource CardStrokeColorDefaultBrush}" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="20" />
    <Setter Property="Spacing" Value="16" />
</Style>
```

---

## 3. Standard Page Layouts

### 3.1 Layout A: Entity Edit Form (Product, Customer, Order)
Used when focusing on a single entity for creation or modification.
* **Structure:** Sticky Header + Split View (Metadata Left, Complex Data Right).

```xml
<Grid Background="{ThemeResource LayerFillColorDefaultBrush}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <RowDefinition Height="*" />    </Grid.RowDefinitions>

    <Grid Grid.Row="0">...</Grid>

    <ScrollViewer Grid.Row="1" Padding="24">
        <Grid ColumnSpacing="24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="1*" MinWidth="300" MaxWidth="500" /> 
                <ColumnDefinition Width="2*" /> 
            </Grid.ColumnDefinitions>

            <StackPanel Grid.Column="0" Spacing="16">...</StackPanel>

            <Grid Grid.Column="1">...</Grid>
        </Grid>
    </ScrollViewer>
</Grid>
```

### 3.2 Layout B: Master-Detail Explorer (Categories, Inventory)
Used for browsing lists or trees and viewing details without leaving the context.
* **Structure:** Full Bleed (No Padding) + Master Pane (Left) + Detail Dashboard (Right).

```xml
<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="320" MinWidth="250" MaxWidth="500" /> <ColumnDefinition Width="*" />   </Grid.ColumnDefinitions>

    <Grid Grid.Column="0" Background="{ThemeResource LayerFillColorDefaultBrush}" 
          BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" BorderThickness="0,0,1,0">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" /> <RowDefinition Height="*" />    </Grid.RowDefinitions>
        </Grid>

    <Grid Grid.Column="1" Background="{ThemeResource LayerFillColorAltBrush}">
        </Grid>
</Grid>
```

---

## 4. Component Patterns

### 4.1 The Sticky Header
Contains the Page Title and Primary Actions.
* **Background:** `SolidBackgroundFillColorBaseBrush` (Opaque).
* **Border:** Bottom border only (`SurfaceStrokeColorDefaultBrush`).

```xml
<Grid Grid.Row="0" Padding="24,16" BorderThickness="0,0,0,1" 
      BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" 
      Background="{ThemeResource SolidBackgroundFillColorBaseBrush}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*" />
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>

    <StackPanel Spacing="4">
        <TextBlock Text="{x:Bind ViewModel.Title}" Style="{StaticResource SubtitleTextBlockStyle}" FontWeight="SemiBold" />
        <TextBlock Text="{x:Bind ViewModel.Subtitle}" Style="{StaticResource CaptionTextBlockStyle}" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
    </StackPanel>

    <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
        <Button Content="Cancel" Style="{StaticResource SubtleButtonStyle}" />
        <Button Content="Save Changes" Style="{StaticResource AccentButtonStyle}" />
    </StackPanel>
</Grid>
```

### 4.2 The "Inline Edit" List
Used for Specifications, Variants, or Order Items inside a Form.
* **Look:** Transparent inputs, bottom border only on rows.
* **Font:** Monospace (`Consolas`) for technical keys/IDs.

```xml
<Grid BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" BorderThickness="0,0,0,1" Padding="12,8">
    <Grid.ColumnDefinitions>...</Grid.ColumnDefinitions>

    <TextBox Text="{x:Bind Value}" 
             BorderThickness="0,0,0,1" 
             CornerRadius="0" 
             Background="Transparent" 
             Padding="0,6" />
</Grid>
```

### 4.3 Buttons with Icons
Since `Button` doesn't have an Icon property, use this stack:

```xml
<Button Style="{StaticResource DefaultButtonStyle}" Command="...">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <FontIcon Glyph="&#xE710;" FontSize="12" />
        <TextBlock Text="Add New Item" />
    </StackPanel>
</Button>
```

### 4.4 The Correct Color Picker
Attach a `Flyout` to a Button, not the Context Menu.

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <TextBox Header="Color" Text="{x:Bind ViewModel.Color, Mode=TwoWay}" Width="150" />
    <Button VerticalAlignment="Bottom" Margin="0,0,0,4" Padding="8">
        <FontIcon Glyph="&#xE790;" /> 
        <Button.Flyout>
            <Flyout Placement="Bottom">
                <ColorPicker ColorSpectrumShape="Ring" 
                             IsMoreButtonVisible="False" 
                             IsColorSliderVisible="True"
                             Color="{x:Bind ViewModel.ColorInstance, Mode=TwoWay}" />
            </Flyout>
        </Button.Flyout>
    </Button>
</StackPanel>
```

---

## 5. Typography & Brushes Cheat Sheet

| Use Case | Style / Resource |
| :--- | :--- |
| **Page Title** | `SubtitleTextBlockStyle` (SemiBold) |
| **Card Header** | `BodyStrongTextBlockStyle` |
| **Label/Body** | `BodyTextBlockStyle` |
| **Helper Text** | `CaptionTextBlockStyle` + `TextFillColorSecondaryBrush` |
| **Technical ID** | `FontFamily="Consolas"` |
| **Page BG** | `LayerFillColorDefaultBrush` (ApplicationPageBackgroundThemeBrush for Full Bleed) |
| **Detail Pane BG**| `LayerFillColorAltBrush` |
| **Header BG** | `SolidBackgroundFillColorBaseBrush` |
| **Card Border** | `CardStrokeColorDefaultBrush` |
| **Divider Line**| `SurfaceStrokeColorDefaultBrush` |

---

## 6. Implementation Checklist
1.  [ ] **Sticky Header:** Does the page title stay visible when scrolling?
2.  [ ] **Split View:** Is the metadata on the left and the complex list on the right?
3.  [ ] **Keyboard:** Can you Tab through the inline list rows easily?
4.  [ ] **Loading:** Is the `ProgressRing` overlay covering the interaction area?
5.  [ ] **Read-Only:** Does the Detail view default to a static dashboard before editing?