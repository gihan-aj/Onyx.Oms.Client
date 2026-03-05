# Onyx.Oms UI Design System & Style Guide v2.0

## 1. Design Philosophy
**"Industrial Professional"**
* **Density:** Maximize screen real estate. Avoid massive padding in "Explorer" views.
* **Context:** Distinct separation between navigation (Master) and content (Detail).
* **Immutability:** Default views should be Read-Only Dashboards. "Edit" is a deliberate state.
* **Safety:** Sticky Headers ensure actions are always visible. Text in read-only views must be selectable.

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
* **Structure:** Sticky Header + Split View (Identity Left, Logistics Right).

```xml
<Grid Background="{ThemeResource LayerFillColorDefaultBrush}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <RowDefinition Height="*" />    </Grid.RowDefinitions>

    <Grid Grid.Row="0">...</Grid>

    <ScrollViewer Grid.Row="1" Padding="24">
        <Grid ColumnSpacing="24" MaxWidth="1200" HorizontalAlignment="Stretch">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="1*" MinWidth="350" /> 
                <ColumnDefinition Width="1.2*" MinWidth="350" /> 
            </Grid.ColumnDefinitions>

            <StackPanel Grid.Column="0" Spacing="16">...</StackPanel>

            <StackPanel Grid.Column="1" Spacing="16">...</StackPanel>
        </Grid>
    </ScrollViewer>
</Grid>
```

### 3.2 Layout B: Master-Detail Explorer (Categories, Inventory)
Used for browsing lists or trees and viewing details without leaving the context.

```xml
<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="320" MinWidth="250" MaxWidth="500" /> <ColumnDefinition Width="*" />   </Grid.ColumnDefinitions>

    <Grid Grid.Column="0" Background="{ThemeResource LayerFillColorDefaultBrush}" 
          BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" BorderThickness="0,0,1,0">
        </Grid>

    <Grid Grid.Column="1" Background="{ThemeResource LayerFillColorAltBrush}">
        </Grid>
</Grid>
```

### 3.3 Layout C: DataGrid Explorer (Customers, Orders List)
Used for high-density tabular data management.

```xml
<Grid Background="{ThemeResource ApplicationPageBackgroundThemeBrush}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <RowDefinition Height="*" />    <RowDefinition Height="Auto" /> </Grid.RowDefinitions>

    <Grid Grid.Row="0" Padding="16,12" Background="{ThemeResource LayerFillColorDefaultBrush}" 
          BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" BorderThickness="0,0,0,1">
        </Grid>

    <Grid Grid.Row="1" Background="{ThemeResource LayerFillColorAltBrush}">
        <toolkit:DataGrid BorderThickness="0" 
                          GridLinesVisibility="Horizontal" 
                          RowBackground="Transparent"
                          AlternatingRowBackground="{ThemeResource LayerFillColorDefaultBrush}">
             </toolkit:DataGrid>
    </Grid>
</Grid>
```

### 3.4 Layout D: Read-Only Detail Dialog
Used for quick viewing of records.
* **Typography:** Labels are small/gray. Values are standard/black.
* **Interaction:** All text must be selectable (`IsTextSelectionEnabled="True"`).

```xml
<ContentDialog Style="{StaticResource DefaultContentDialogStyle}">
    <ScrollViewer>
        <StackPanel Spacing="20" MinWidth="450">
            <Grid>
                 <PersonPicture Height="64" />
                 <StackPanel Grid.Column="1">
                     <TextBlock Text="{x:Bind Name}" Style="{StaticResource SubtitleTextBlockStyle}" />
                     <Border Style="{StaticResource StatusBadgeStyle}" ... />
                 </StackPanel>
            </Grid>
            <Grid>...</Grid>
        </StackPanel>
    </ScrollViewer>
</ContentDialog>
```

---

## 4. Component Patterns

### 4.1 The Sticky Header & Toolbar
Contains the Page Title and Contextual Actions.

```xml
<Grid Grid.Row="0" Padding="24,16" BorderThickness="0,0,0,1" 
      BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" 
      Background="{ThemeResource SolidBackgroundFillColorBaseBrush}">
    
    <StackPanel Orientation="Horizontal" Spacing="12">
        <TextBlock Text="{x:Bind ViewModel.Title}" Style="{StaticResource SubtitleTextBlockStyle}" FontWeight="SemiBold" />
        </StackPanel>

    <StackPanel Grid.Column="1" Orientation="Horizontal" Spacing="8">
        <Button Content="Save" Style="{StaticResource AccentButtonStyle}" />
    </StackPanel>
</Grid>
```

### 4.2 The "Label-Value" Pair (Read-Only)
Used in Dialogs and Dashboards.

```xml
<StackPanel>
    <TextBlock Text="Email Address" Style="{StaticResource CaptionTextBlockStyle}" Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
    <TextBlock Text="{x:Bind Email}" Style="{StaticResource BodyTextBlockStyle}" IsTextSelectionEnabled="True" />
</StackPanel>
```

### 4.3 The "Address Grid" (Form Layout)
Standard ratio for City/State/Zip inputs.

```xml
<Grid ColumnSpacing="12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="2*" />   <ColumnDefinition Width="1.5*" /> </Grid.ColumnDefinitions>
    </Grid>
<Grid ColumnSpacing="12">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="1*" /> <ColumnDefinition Width="2*" /> </Grid.ColumnDefinitions>
    </Grid>
```

### 4.4 The Status Badge
Use the `StatusBadgeStyle` border with converters.

```xml
<Border Style="{StaticResource StatusBadgeStyle}" 
        Background="{Binding IsActive, Converter={StaticResource StatusBackgroundConverter}}">
    <TextBlock Text="{Binding IsActive, Converter={StaticResource StatusTextConverter}}"
               Style="{StaticResource StatusBadgeTextStyle}"
               Foreground="{Binding IsActive, Converter={StaticResource StatusForegroundConverter}}" />
</Border>
```

### 4.5 The "Meatballs" Action Menu (DataGrid)
Clean up row actions into a single menu.

```xml
<Button Content="&#xE712;" FontFamily="Segoe MDL2 Assets" Style="{StaticResource SubtleButtonStyle}">
    <Button.Flyout>
        <MenuFlyout>
            <MenuFlyoutItem Text="Edit" Icon="Edit" IsEnabled="{x:Bind CanEdit}" />
            <MenuFlyoutItem Text="Delete" Icon="Delete" Foreground="Red" />
        </MenuFlyout>
    </Button.Flyout>
</Button>
```

### 4.6 Extended Filter Toolbars
When a DataGrid or Explorer requires multiple complex filters (e.g., category pickers, status dropdowns) that might crowd the main header, split the toolbar into two distinct rows within the same header container to prevent the UI from becoming squished.

```xml
<!-- Header & Filters container (Row 0 of Page Grid) -->
<Grid Padding="16,12" Background="{ThemeResource LayerFillColorDefaultBrush}" 
      BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" BorderThickness="0,0,0,1"
      ColumnSpacing="8" RowSpacing="12">
    
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <!-- Primary: Title, Search, Actions -->
        <RowDefinition Height="Auto" /> <!-- Secondary: Extended Filters -->
    </Grid.RowDefinitions>

    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="Auto"/>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="Auto"/>
    </Grid.ColumnDefinitions>

    <!-- Top Row -->
    <TextBlock Grid.Row="0" Grid.Column="0" Text="Page Title" Style="{StaticResource SubtitleTextBlockStyle}" />
    <AutoSuggestBox Grid.Row="0" Grid.Column="1" HorizontalAlignment="Center" Width="320" />
    <StackPanel Grid.Row="0" Grid.Column="2" Orientation="Horizontal" Spacing="8">
        <!-- Actions: Refresh, New, etc. -->
    </StackPanel>

    <!-- Bottom Row (Filters) -->
    <StackPanel Grid.Row="1" Grid.Column="0" Grid.ColumnSpan="3" Orientation="Horizontal" Spacing="16">
        <ComboBox PlaceholderText="Status" Width="120" />
        <!-- Additional Filters Here -->
        <Button Content="Clear Filters" Style="{StaticResource SubtleButtonStyle}" />
    </StackPanel>
</Grid>
```

---

## 5. Typography & Brushes Cheat Sheet

| Use Case | Style / Resource |
| :--- | :--- |
| **Page Title** | `SubtitleTextBlockStyle` (SemiBold) |
| **Section Header** | `BodyStrongTextBlockStyle` |
| **Field Label (Form)** | Default TextBox Header |
| **Field Label (Read)** | `CaptionTextBlockStyle` + `TextFillColorSecondaryBrush` |
| **Field Value (Read)** | `BodyTextBlockStyle` + `IsTextSelectionEnabled="True"` |
| **Technical ID** | `FontFamily="Consolas"` |
| **Page BG** | `LayerFillColorDefaultBrush` (ApplicationPageBackgroundThemeBrush for Full Bleed) |
| **Detail Pane BG**| `LayerFillColorAltBrush` |
| **Header BG** | `SolidBackgroundFillColorBaseBrush` |
| **Card Border** | `CardStrokeColorDefaultBrush` |
| **Divider Line**| `SurfaceStrokeColorDefaultBrush` |

---

## 6. Implementation Checklist
1.  [ ] **Sticky Header:** Does the page title stay visible when scrolling?
2.  [ ] **Selection:** Can you copy text from read-only views (Dialogs/Grids)?
3.  [ ] **Split View:** Is the form visually balanced (Identity vs Logistics)?
4.  [ ] **Keyboard:** Can you Tab through the inline list rows easily?
5.  [ ] **Loading:** Is the `ProgressRing` overlay covering the interaction area?
6.  [ ] **Empty States:** Do lists show a helpful "No Data" message with a clear action (e.g., Clear Search)?