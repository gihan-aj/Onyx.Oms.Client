# ListView Item Design Standard

This document outlines the standard strategy for creating robust, responsive `ListView` items across the Onyx OMS application. This standard ensures that extremely large values (e.g., billions in prices) or very long text (e.g., product names) are handled gracefully without pushing content off-screen or forcing horizontal scrolling.

## The Core Concept
A robust list item must balance flexible content (names/descriptions) with fixed/auto content (prices, badges, buttons).

### 1. The ListView Container Style
Always ensure the `ListViewItem` stretches to fill the available width. If you skip this, the internal `Grid` will not align columns properly with other rows.
```xml
<ListView.ItemContainerStyle>
    <Style TargetType="ListViewItem">
        <Setter Property="HorizontalContentAlignment" Value="Stretch" />
        <Setter Property="Margin" Value="0,0,0,8" />
        <Setter Property="Padding" Value="0" />
    </Style>
</ListView.ItemContainerStyle>
```

### 2. The Grid Column Definitions (The Golden Rule)
The internal `Grid` must use a strict `Auto` and `*` (Star) sizing strategy. 
- **Right-side content** (Financials, Buttons) must be `Auto`. This allows them to grow horizontally if a number gets huge.
- **Left-side/Middle content** (Names, Descriptions) **MUST be `*` (Star)**. This tells the column: "Take up whatever space is left over." If the price column grows, this column automatically shrinks.

```xml
<Grid Background="{ThemeResource LayerFillColorAltBrush}" CornerRadius="8" Padding="12">
    <Grid.ColumnDefinitions>
        <!-- 1. Icon/Image (Fixed or Auto) -->
        <ColumnDefinition Width="Auto" />
        
        <!-- 2. Main Details (Star sizing is CRITICAL!) -->
        <ColumnDefinition Width="*"/>
        
        <!-- 3. Secondary Info / Status (Auto) -->
        <ColumnDefinition Width="Auto" />
        
        <!-- 4. Financials (Auto - grows with large numbers) -->
        <ColumnDefinition Width="Auto" />
        
        <!-- 5. Actions (Auto) -->
        <ColumnDefinition Width="Auto" />
    </Grid.ColumnDefinitions>
    ...
</Grid>
```

### 3. Text Wrapping and Trimming
Because the `*` column will shrink when right-side numbers get huge, the text inside it *must* know how to behave when squished. Otherwise, the layout will break and push the right columns out of bounds.

Always apply `TextWrapping="Wrap"` or `TextTrimming="CharacterEllipsis"` to text blocks inside the `*` column.

```xml
<!-- Good: Wraps to a new line if squished -->
<TextBlock Text="{x:Bind ProductName}" TextWrapping="Wrap" />

<!-- Alternatively Good: Cuts off with '...' if squished -->
<TextBlock Text="{x:Bind CustomerName}" TextTrimming="CharacterEllipsis" />
```

### 4. Horizontal Alignment for Right-Side Items
Ensure items in the `Auto` columns on the right side have `HorizontalAlignment="Right"`. This anchors them properly against the right edge of the list item.

```xml
<StackPanel Grid.Column="3" VerticalAlignment="Center" HorizontalAlignment="Right">
    <TextBlock Style="{StaticResource BodyStrongTextBlockStyle}" HorizontalAlignment="Right">
        <Run Text="{x:Bind GrandTotalCurrency}" />
        <Run Text="{x:Bind GrandTotalAmount}" />
    </TextBlock>
</StackPanel>
```
