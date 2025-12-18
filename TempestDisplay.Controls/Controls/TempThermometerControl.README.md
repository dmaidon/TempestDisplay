# TempThermometerControl

A professional vertical thermometer control for displaying temperature with a classic mercury column design.

## Features

- **Vertical Mercury Column**: Classic thermometer design with rising/falling mercury effect
- **Temperature-Based Color Coding**:
  - Blue (< 32°F): Cold/Freezing
  - Cyan-Green (32-50°F): Cool
  - Green-Yellow (50-70°F): Moderate
  - Yellow-Orange (70-85°F): Warm
  - Orange-Red (> 85°F): Hot
- **Dual Scale Display**: Fahrenheit (left) and Celsius (right) with configurable visibility
- **Freezing Point Marker**: Optional dashed line at 32°F
- **Glass Tube Effect**: Realistic glass appearance with highlights and shadows
- **Bulb Design**: Classic thermometer bulb at bottom with gradient fill
- **Digital Readout**: Large temperature display below the bulb
- **Professional Rendering**: Anti-aliased graphics with depth effects

## Properties

### Data Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `TempF` | Single | 0 | Temperature in Fahrenheit (automatically converts to Celsius) |
| `TempC` | Single | 0 | Temperature in Celsius (automatically converts to Fahrenheit) |
| `MinF` | Single | -5 | Minimum temperature for scale |
| `MaxF` | Single | 110 | Maximum temperature for scale |

### Appearance Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Label` | String | "" | Text label displayed at top of control |
| `ShowFreezeMarker` | Boolean | True | Show dashed line at 32°F freezing point |
| `ShowDualScale` | Boolean | True | Show both Fahrenheit and Celsius scales |

## Usage

### Adding to Form in Designer

1. **Build the Controls project:**
   ```bash
   dotnet build TempestDisplay.Controls\TempestDisplay.Controls.vbproj
   ```

2. **Add to Toolbox in Visual Studio:**
   - Right-click Toolbox → Choose Items
   - Browse to `TempestDisplay.Controls.dll`
   - Check `TempThermometerControl`

3. **Drag onto form** from Toolbox

### Programmatic Usage

```vb
Imports TempestDisplay.Controls

' Create new thermometer control
Dim thermometer As New TempThermometerControl()
thermometer.Location = New Point(50, 50)
thermometer.Size = New Size(120, 300)
thermometer.Label = "Current Temp"
thermometer.MinF = -10
thermometer.MaxF = 120
thermometer.ShowFreezeMarker = True
thermometer.ShowDualScale = True

' Add to form
Me.Controls.Add(thermometer)

' Update temperature
thermometer.TempF = 72.5F  ' Set in Fahrenheit
' OR
thermometer.TempC = 22.5F  ' Set in Celsius
```

## Recommended Dimensions

### Vertical Orientation (Default)
- **Minimum**: 80 x 150 pixels
- **Optimal**: 100-140 x 250-350 pixels
- **Ratio**: 1:2.5 to 1:3 (width:height)

### Layout Examples

#### Single Temperature Display
```
┌────────────┐
│  Current   │ ← Label
│     ║      │
│     ║      │
│    100     │ ← Scale
│     ║      │
│     ║      │
│  80°║      │
│     ║      │
│    60      │
│     ████   │ ← Mercury
│     ████   │
│    40      │
│     ████   │
│    20      │
│     ◉      │ ← Bulb
│   85.3°F   │ ← Readout
│   29.6°C   │
└────────────┘
```

#### Three Thermometers Side-by-Side
```
┌──────┬──────┬──────┐
│ Temp │Feels │ Dew  │
│  ║   │  ║   │  ║   │
│ ███  │ ███  │ ███  │
│ ███  │ ███  │  ║   │
│  ║   │ ███  │  ║   │
│  ◉   │  ◉   │  ◉   │
│ 72°F │ 78°F │ 54°F │
└──────┴──────┴──────┘
```

## Replacing TempGaugeControl

### Current Layout (Circular Gauges)
```vb
' In FrmMain.Designer.vb
Dim TgCurrentTemp As New TempGaugeControl()  ' Takes 2 columns
Dim TgFeelsLike As New TempGaugeControl()    ' Takes 2 columns
Dim TgDewpoint As New TempGaugeControl()     ' Takes 2 columns
```

### New Layout (Vertical Thermometers)
```vb
' Replace with thermometers - saves space!
Dim ThermCurrentTemp As New TempThermometerControl()  ' Takes 1 column
Dim ThermFeelsLike As New TempThermometerControl()    ' Takes 1 column
Dim ThermDewpoint As New TempThermometerControl()     ' Takes 1 column
```

**Space Savings:** 3 thermometers fit in ~1.5-2 columns vs 6 columns for gauges

## Color Temperature Scale

The mercury column automatically changes color based on temperature:

| Temperature Range | Color | Description |
|-------------------|-------|-------------|
| < 32°F | Blue (#4682DC) | Freezing/Cold |
| 32-50°F | Cyan-Green | Cool |
| 50-70°F | Green-Yellow | Comfortable |
| 70-85°F | Yellow-Orange | Warm |
| > 85°F | Orange-Red | Hot |

## Customization Examples

### Minimal Thermometer (No Labels)
```vb
therm.ShowDualScale = False  ' Fahrenheit only
therm.ShowFreezeMarker = False
therm.Label = ""
```

### Wide Temperature Range
```vb
therm.MinF = -20
therm.MaxF = 130
' Scale automatically adjusts step size
```

### Celsius Primary Display
```vb
' Set via Celsius property
therm.TempC = 25.0F  ' 77°F
```

## Performance

- **Rendering**: Double-buffered with anti-aliasing
- **Update Frequency**: Suitable for real-time updates (sub-second)
- **Memory**: Minimal - no bitmap caching needed
- **CPU**: Low - efficient GDI+ rendering

## Design Notes

### Visual Features
1. **Glass Tube**: Semi-transparent outline with inner shadow and edge highlights
2. **Mercury Column**: Gradient fill from darker edges to lighter center (cylindrical effect)
3. **Bulb**: Radial gradient from bright center to darker edge
4. **Scale Markers**: Major ticks every 10-20° depending on range
5. **Shadows**: Subtle drop shadows behind entire thermometer for depth

### Scale Auto-Adjustment
- Range 0-40°F: 5° steps
- Range 40-80°F: 10° steps
- Range > 80°F: 20° steps

### Freezing Marker
- Red dashed line at exactly 32°F
- Only shows when 32°F is within MinF-MaxF range
- Spans across tube from left scale to right scale

## Integration with TempestDisplay

### Replace Existing Gauges

1. **Open FrmMain.Designer.vb**

2. **Find the TempGauge declarations:**
   ```vb
   Friend WithEvents TgCurrentTemp As TempGaugeControl
   Friend WithEvents TgFeelsLike As TempGaugeControl
   Friend WithEvents TgDewpoint As TempGaugeControl
   ```

3. **Replace with Thermometers:**
   ```vb
   Friend WithEvents ThermCurrentTemp As TempThermometerControl
   Friend WithEvents ThermFeelsLike As TempThermometerControl
   Friend WithEvents ThermDewpoint As TempThermometerControl
   ```

4. **Update InitializeComponent()** - Change initialization code

5. **Update FrmMain.Partials\FrmMain.ObservationUI.vb**
   - Find temperature update methods
   - Change `TgCurrentTemp.TempF = value` to `ThermCurrentTemp.TempF = value`

6. **Adjust TableLayoutPanel** column spans for better space utilization

## Comparison: Gauge vs Thermometer

### Circular Gauge (TempGaugeControl)
**Pros:**
- Automotive/aviation aesthetic
- Speedometer familiarity
- Works well for single display

**Cons:**
- Takes more horizontal space
- Harder to compare multiple temps at glance
- Less intuitive for temperature specifically

### Vertical Thermometer (TempThermometerControl)
**Pros:**
- Universal temperature symbol
- Easy visual comparison (taller = hotter)
- Space efficient (narrow width)
- Classic weather station look
- Can fit 3-4 side-by-side easily

**Cons:**
- Requires vertical space
- Less dramatic/modern than gauge

## Troubleshooting

### Thermometer Not Visible
- Check `MinimumSize` is met (80 x 150)
- Ensure control is added to form's Controls collection
- Verify TempestDisplay.Controls.dll is in output directory

### Temperature Not Updating
- Verify `TempF` or `TempC` property is being set
- Check that `Invalidate()` is called (automatic on property change)
- Ensure value is within `MinF` to `MaxF` range (clamped if outside)

### Scale Looks Wrong
- Adjust `MinF` and `MaxF` properties for your climate
- Default range (-5°F to 110°F) works for most locations
- Scale markers auto-adjust to range size

### Colors Not Showing
- Verify GDI+ is available (Windows Forms requirement)
- Check `SmoothingMode.AntiAlias` support on graphics hardware

## Version History

**v1.0** (2025-12-17)
- Initial release
- Temperature-based color coding
- Dual scale (F/C) support
- Glass tube effects
- Freezing point marker
- Digital readout

## License

Part of the TempestDisplay project - Same license as main project (MIT)

---

**Author Notes:**
This control was designed to complement the TempestDisplay weather station application, providing a more traditional and space-efficient alternative to circular temperature gauges. The mercury column design is universally recognized and allows for instant visual comparison when multiple thermometers are displayed side-by-side.
