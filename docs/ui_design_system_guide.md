# Onyx.Oms UI Design System & Style Guide

## 1. Design Philosophy
**"Industrial Professional"**
* **Density:** Maximize screen real estate. Avoid massive padding.
* **Context:** Split screens into "General Info" (Left) and "Complex Configuration" (Right).
* **Immutability:** Use "Inline Edit" styles for lists to feel like a spreadsheet/dashboard, not a web form.
* **Safety:** Sticky Headers ensure "Save" is always visible.

---

## 2. Standard Page Layout
Every entity management page (Product, Order, Customer) should follow this **2-Row, 2-Column** structure.

```xml
<Grid Background="{ThemeResource LayerFillColorDefaultBrush}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <RowDefinition Height="*" />    </Grid.RowDefinitions>

    <Grid Grid.Row="0" ... > ... </Grid>

    <ScrollViewer Grid.Row="1" Padding="24">
        <Grid ColumnSpacing="24">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="1*" MinWidth="300" MaxWidth="500" /> 
                <ColumnDefinition Width="2*" /> 
            </Grid.ColumnDefinitions>

            <StackPanel Grid.Column="0" Spacing="16"> ... </StackPanel>

            <Grid Grid.Column="1"> ... </Grid>
        </Grid>
    </ScrollViewer>
</Grid>
```

---

## 3. Component Patterns

### 3.1 The Sticky Header
Contains the Page Title and Primary Actions.
* **Background:** `SolidBackgroundFillColorBaseBrush` (Opaque, handles scrolling behind it).
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
        <Button Content="Cancel" Command="{x:Bind ViewModel.CancelCommand}" Style="{StaticResource SubtleButtonStyle}" />
        <Button Content="Save Changes" Command="{x:Bind ViewModel.SaveCommand}" Style="{StaticResource AccentButtonStyle}" />
    </StackPanel>
</Grid>
```

### 3.2 Section Cards (Left Column)
Used to group simple fields (Name, Description, etc.).

```xml
<StackPanel Spacing="16" 
            Background="{ThemeResource CardBackgroundFillColorDefaultBrush}" 
            BorderBrush="{ThemeResource CardStrokeColorDefaultBrush}" 
            BorderThickness="1" CornerRadius="4" Padding="16">
    
    <TextBlock Text="Section Title" Style="{StaticResource BodyStrongTextBlockStyle}" />
    
    <TextBox Header="Input Field" ... />
    <NumberBox Header="Number Field" ... />
</StackPanel>
```

### 3.3 The "Inline Edit" List (Right Column)
Used for Specifications, Variants, or Order Items.
* **Look:** No outer borders on inputs. Bottom border only on rows.
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

### 3.4 Buttons with Icons
Since `Button` doesn't have an Icon property, use this stack:

```xml
<Button Style="{StaticResource DefaultButtonStyle}" Command="...">
    <StackPanel Orientation="Horizontal" Spacing="8">
        <FontIcon Glyph="&#xE710;" FontSize="12" />
        <TextBlock Text="Add New Item" />
    </StackPanel>
</Button>
```

### 3.5 The Correct Color Picker
To use a Color Picker, attach a `Flyout` to a Button (or an icon button next to the TextBox), not the Context Menu.

```xml
<StackPanel Orientation="Horizontal" Spacing="8">
    <TextBox Header="Color" Text="{x:Bind ViewModel.Color, Mode=TwoWay}" Width="150" />
    
    <Button VerticalAlignment="Bottom" Margin="0,0,0,4" Padding="8">
        <FontIcon Glyph="&#xE790;" /> <Button.Flyout>
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

## 4. Typography & Brushes Cheat Sheet

| Use Case | Style / Resource |
| :--- | :--- |
| **Page Title** | `SubtitleTextBlockStyle` (SemiBold) |
| **Card Header** | `BodyStrongTextBlockStyle` |
| **Label/Body** | `BodyTextBlockStyle` |
| **Helper Text** | `CaptionTextBlockStyle` + `TextFillColorSecondaryBrush` |
| **Technical ID** | `FontFamily="Consolas"` |
| **Page BG** | `LayerFillColorDefaultBrush` |
| **Header BG** | `SolidBackgroundFillColorBaseBrush` |
| **Card BG** | `CardBackgroundFillColorDefaultBrush` |
| **Card Border** | `CardStrokeColorDefaultBrush` |
| **Divider Line**| `SurfaceStrokeColorDefaultBrush` |

---

## 5. Implementation Checklist
1.  [ ] **Sticky Header:** Does the page title stay visible when scrolling?
2.  [ ] **Split View:** Is the metadata on the left and the complex list on the right?
3.  [ ] **Keyboard:** Can you Tab through the inline list rows easily?
4.  [ ] **Loading:** Is the `ProgressRing` overlay covering the interaction area?