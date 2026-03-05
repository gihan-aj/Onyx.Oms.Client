# UI Design Specification: Create Product Page

## 1. Page Architecture
This page implements **Layout A (Entity Edit Form)** from the UI Design System. It is designed to gather all required Aggregate Root data in a single, monolithic flow to ensure domain validation passes upon creation.

* **Top:** Sticky Header (Layout Pattern 4.1) ensuring the "Save Product" primary action is always accessible.
* **Body:** A `ScrollViewer` containing a maximum-width grid to prevent awkward stretching on ultrawide monitors.
* **Containers:** Data is grouped using the `CardStackStyle` to create distinct visual zones for Identity, Logistics, Variants, and Media.

## 2. WinUI 3 Control Mapping

| Domain Concept | UI Control / Component | Rationale |
| :--- | :--- | :--- |
| **Money (Price/Cost)** | `NumberBox` | Provides native formatting (e.g., currency masking), spin buttons, and prevents invalid string inputs. |
| **Weight / Physical** | `CheckBox` + `NumberBox` | A "This is a physical product" CheckBox. When unchecked, the Weight `NumberBox` is collapsed. |
| **HasVariants** | `ToggleSwitch` | Placed prominently in the Variants card. Toggles the visibility of the Options Builder and DataGrid. |
| **Product Options** | `ItemsRepeater` + `TextBox` | A dynamic list where users can add an Option (e.g., "Color") and comma-separated values. |
| **Variants Matrix** | `CommunityToolkit.WinUI.UI.Controls.DataGrid` | Required for high-density tabular data (SKU, Price, Cost, Stock). |
| **Images** | `GridView` | Displays uploaded image thumbnails in a responsive grid. |

## 3. Visual Layout & Flow

### Section 1: Sticky Header
* **Left:** Page Title (`SubtitleTextBlockStyle`: "Create New Product").
* **Right:** "Cancel" (Subtle Button) and "Save Product" (Accent Button).

### Section 2: Split View (Upper Half)
* **Left Column (Identity Card):**
  * `TextBox` for Product Name, Base SKU.
  * `ComboBox` for Category Selection.
* **Right Column (Logistics & Rules Card):**
  * `NumberBox` for Base Price and Base Cost.
  * `CheckBox` for "Requires Shipping" to toggle the Weight input.

### Section 3: Full-Width View (Lower Half)
* **Variants & Matrix Card:**
  * Uses a `ToggleSwitch` to reveal the Options Builder.
  * The Options Builder defines the axes (e.g., Size, Color).
  * A "Generate Matrix" button computes the Cartesian product and fills the `DataGrid`.
* **Media Card:** Drag-and-drop zone and a `GridView` for image management and option linking.

---

## 4. Full XAML Implementation

```xml
<Grid Background="{ThemeResource LayerFillColorDefaultBrush}">
    <Grid.RowDefinitions>
        <RowDefinition Height="Auto" /> <RowDefinition Height="*" />    </Grid.RowDefinitions>

    <Grid Grid.Row="0" Padding="24,16" BorderThickness="0,0,0,1" 
          BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}" 
          Background="{ThemeResource SolidBackgroundFillColorBaseBrush}">
        <TextBlock Text="Create New Product" Style="{StaticResource SubtitleTextBlockStyle}" FontWeight="SemiBold" />
        <StackPanel HorizontalAlignment="Right" Orientation="Horizontal" Spacing="8">
            <Button Content="Cancel" Style="{StaticResource DefaultButtonStyle}" />
            <Button Content="Save Product" Style="{StaticResource AccentButtonStyle}" />
        </StackPanel>
    </Grid>

    <ScrollViewer Grid.Row="1" Padding="24">
        <StackPanel MaxWidth="1200" HorizontalAlignment="Stretch" Spacing="24">
            
            <Grid ColumnSpacing="24">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition Width="1*" MinWidth="350" />
                    <ColumnDefinition Width="1.2*" MinWidth="350" />
                </Grid.ColumnDefinitions>

                <StackPanel Grid.Column="0" Style="{StaticResource CardStackStyle}">
                    <TextBlock Text="Basic Information" Style="{StaticResource BodyStrongTextBlockStyle}" />
                    <TextBox Header="Product Name" PlaceholderText="e.g. Onyx Signature Tee" />
                    <TextBox Header="Base SKU" PlaceholderText="e.g. TEE-SIG-01" />
                    <ComboBox Header="Category" PlaceholderText="Select category..." HorizontalAlignment="Stretch" />
                </StackPanel>

                <StackPanel Grid.Column="1" Style="{StaticResource CardStackStyle}">
                    <TextBlock Text="Pricing &amp; Shipping Defaults" Style="{StaticResource BodyStrongTextBlockStyle}" />
                    <Grid ColumnSpacing="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="1*" /> <ColumnDefinition Width="1*" />
                        </Grid.ColumnDefinitions>
                        <NumberBox Grid.Column="0" Header="Base Price" PlaceholderText="0.00" />
                        <NumberBox Grid.Column="1" Header="Base Cost" PlaceholderText="0.00" />
                    </Grid>
                    <CheckBox Content="This is a physical product" IsChecked="True" />
                    <NumberBox Header="Weight (kg)" PlaceholderText="0.00" /> 
                </StackPanel>
            </Grid>

            <StackPanel Style="{StaticResource CardStackStyle}">
                
                <Grid>
                    <StackPanel>
                        <TextBlock Text="Product Variations" Style="{StaticResource BodyStrongTextBlockStyle}" />
                        <TextBlock Text="Define options like size or color to generate variant SKUs." 
                                   Style="{StaticResource CaptionTextBlockStyle}" 
                                   Foreground="{ThemeResource TextFillColorSecondaryBrush}" />
                    </StackPanel>
                    <ToggleSwitch Header="Has Options" IsOn="{x:Bind ViewModel.HasVariants, Mode=TwoWay}" HorizontalAlignment="Right" />
                </Grid>

                <MenuFlyoutSeparator />

                <StackPanel Visibility="{x:Bind ViewModel.HasVariants, Mode=OneWay}">
                    
                    <TextBlock Text="1. Define Options" Style="{StaticResource BodyStrongTextBlockStyle}" Margin="0,0,0,12" />
                    
                    <Grid ColumnSpacing="12">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="1*" />
                            <ColumnDefinition Width="2*" />
                            <ColumnDefinition Width="Auto" />
                        </Grid.ColumnDefinitions>
                        
                        <TextBox Grid.Column="0" Header="Option Name" PlaceholderText="e.g., Color" Text="{x:Bind ViewModel.DraftOptionName, Mode=TwoWay}" />
                        <TextBox Grid.Column="1" Header="Values (Comma separated)" PlaceholderText="e.g., Red, Blue, Green" Text="{x:Bind ViewModel.DraftOptionValues, Mode=TwoWay}" />
                        
                        <Button Grid.Column="2" Content="Add Option" VerticalAlignment="Bottom" 
                                Command="{x:Bind ViewModel.AddOptionCommand}" Style="{StaticResource DefaultButtonStyle}" />
                    </Grid>

                    <ItemsRepeater ItemsSource="{x:Bind ViewModel.ProductOptions}" Margin="0,16,0,0">
                        <ItemsRepeater.ItemTemplate>
                            <DataTemplate x:DataType="models:ProductOption">
                                <Grid Padding="12" Background="{ThemeResource LayerFillColorAltBrush}" CornerRadius="4" Margin="0,0,0,8" BorderThickness="1" BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}">
                                    <Grid.ColumnDefinitions>
                                        <ColumnDefinition Width="150" />
                                        <ColumnDefinition Width="*" />
                                        <ColumnDefinition Width="Auto" />
                                    </Grid.ColumnDefinitions>
                                    <TextBlock Grid.Column="0" Text="{x:Bind Name}" FontWeight="SemiBold" VerticalAlignment="Center" />
                                    <TextBlock Grid.Column="1" Text="{x:Bind Values, Converter={StaticResource StringListConverter}}" VerticalAlignment="Center" />
                                    <Button Grid.Column="2" Content="&#xE74D;" FontFamily="Segoe MDL2 Assets" 
                                            Style="{StaticResource SubtleButtonStyle}" Foreground="Red" ToolTipService.ToolTip="Remove Option" />
                                </Grid>
                            </DataTemplate>
                        </ItemsRepeater.ItemTemplate>
                    </ItemsRepeater>
                    
                    <Button Content="Generate Variant Matrix" Command="{x:Bind ViewModel.GenerateMatrixCommand}" 
                            Style="{StaticResource AccentButtonStyle}" Margin="0,16,0,24" HorizontalAlignment="Left" />

                    <MenuFlyoutSeparator Margin="0,0,0,24" />

                    <TextBlock Text="2. Edit Logistics" Style="{StaticResource BodyStrongTextBlockStyle}" Margin="0,0,0,12" />
                    
                    <toolkit:DataGrid ItemsSource="{x:Bind ViewModel.VariantDrafts}" 
                                      AutoGenerateColumns="False"
                                      BorderThickness="1" 
                                      BorderBrush="{ThemeResource SurfaceStrokeColorDefaultBrush}"
                                      GridLinesVisibility="Horizontal" 
                                      RowBackground="Transparent"
                                      AlternatingRowBackground="{ThemeResource LayerFillColorDefaultBrush}"
                                      MinHeight="300">
                        <toolkit:DataGrid.Columns>
                            <toolkit:DataGridTextColumn Header="Variant" Binding="{Binding DisplayAttributes}" IsReadOnly="True" Width="1.5*" />
                            <toolkit:DataGridTextColumn Header="SKU Override" Binding="{Binding Sku, Mode=TwoWay}" Width="1.5*" />
                            <toolkit:DataGridTextColumn Header="Price" Binding="{Binding Price.Amount, Mode=TwoWay}" Width="1*" />
                            <toolkit:DataGridTextColumn Header="Cost" Binding="{Binding Cost.Amount, Mode=TwoWay}" Width="1*" />
                            <toolkit:DataGridTextColumn Header="Stock" Binding="{Binding StockOnHand, Mode=TwoWay}" Width="1*" />
                            
                            <toolkit:DataGridTemplateColumn Width="60">
                                <toolkit:DataGridTemplateColumn.CellTemplate>
                                    <DataTemplate>
                                        <Button Content="&#xE74D;" FontFamily="Segoe MDL2 Assets" Background="Transparent" BorderThickness="0" Foreground="Red" />
                                    </DataTemplate>
                                </toolkit:DataGridTemplateColumn.CellTemplate>
                            </toolkit:DataGridTemplateColumn>
                        </toolkit:DataGrid.Columns>
                    </toolkit:DataGrid>
                    
                </StackPanel>
            </StackPanel>
            
            <StackPanel Style="{StaticResource CardStackStyle}">
                <TextBlock Text="Media &amp; Option Linking" Style="{StaticResource BodyStrongTextBlockStyle}" />
                </StackPanel>

        </StackPanel>
    </ScrollViewer>
</Grid>
```

## 5. Key Technical Considerations for the ViewModel

1.  **Pre-filling Data:** When the user clicks "Generate Variant Matrix", the ViewModel computes the Cartesian product of the `ProductOptions`. It should utilize the "Basic Information" (Base Price, Base Cost, Base SKU) to pre-fill the generated rows. For instance, if Base SKU is `TEE-01`, generated rows auto-suggest `TEE-01-RED-M`, `TEE-01-RED-L`, etc.
2.  **Displaying Attributes:** The `ProductVariant.Attributes` domain entity is a `List<VariantAttribute>`. The ViewModel wrapper needs a read-only property (e.g., `DisplayAttributes` returning `"Red / M"`) to allow the `DataGridTextColumn` to bind to a simple string.