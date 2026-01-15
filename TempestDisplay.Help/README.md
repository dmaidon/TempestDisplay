# TempestDisplay.Help - Comprehensive Help System

## Overview

The TempestDisplay.Help project is a dedicated help system DLL that provides comprehensive, searchable, and organized help content for all TempestDisplay settings options. This modular approach ensures clean separation of concerns and makes the help system reusable and maintainable.

## Architecture

### Project Structure

```
TempestDisplay.Help/
├── TempestDisplay.Help.vbproj     # Project file
├── HelpTopic.vb                    # Data model for help topics
├── SettingsHelpProvider.vb         # Content provider with all help text
├── HelpViewer.vb                   # User control for viewing help
└── README.md                       # This file
```

### Components

#### 1. **HelpTopic Class**
Represents a single help topic with the following properties:
- **Id**: Unique identifier (e.g., "setoptions_station_name")
- **Title**: Display title
- **Category**: Grouping category (e.g., "Station Data")
- **TabPage**: Associated tab page (e.g., "TpSetOptions")
- **Content**: Rich text formatted (RTF) content
- **Keywords**: Searchable keywords for improved discovery
- **SortOrder**: Display order within category

**Search Functionality**: The `MatchesSearch()` method searches across title, keywords, category, and content.

#### 2. **SettingsHelpProvider Class**
Central repository for all help content with the following features:

**Properties:**
- `AllTopics`: Returns all available help topics
- `Categories`: Gets unique categories
- `TabPages`: Gets unique tab pages

**Methods:**
- `GetTopicsByCategory(category)`: Filter topics by category
- `GetTopicsByTabPage(tabPage)`: Filter topics by tab page
- `GetTopicById(id)`: Get specific topic by ID
- `Search(searchText)`: Full-text search across all topics

**Content Organization:**
Help content is organized by these future tab pages:
1. **TpSetOptions** (General Options)
   - Station Data
   - Update Settings
   - Rain Gauge Settings

2. **TpApiKeys** (API Keys & Authentication)
   - WeatherFlow Tempest
   - MeteoBridge

3. **TpOptVacation** (Vacation Mode)
   - Vacation Schedule
   - Automated Behaviors

4. **TpColorScale** (Color Scales)
   - Temperature Scales
   - Wind Speed Scales

5. **TpLogSettings** (Logging Configuration)
   - Log Retention
   - Log Detail Levels
   - Viewing Logs

#### 3. **HelpViewer Control**
A complete, ready-to-use Windows Forms user control that provides:

**UI Components:**
- **TreeView** (left panel): Hierarchical navigation organized by tab page → category → topic
- **RichTextBox** (right panel): Displays formatted help content
- **Search TextBox**: Real-time filtering of topics
- **Print Button**: Print current help topic
- **Export Button**: Export current topic to TXT or RTF

**Features:**
- **Automatic Organization**: Topics are automatically organized by tab page and category
- **Real-time Search**: Type-as-you-search functionality filters topics instantly
- **Navigation History**: TreeView maintains selection state
- **Rich Formatting**: Supports bold, colors, bullets, and formatting via RTF
- **Print Support**: Direct printing of help topics
- **Export Options**: Save topics as plain text or RTF files
- **Export All**: Export entire help system to a single text file

**Events:**
- `TopicSelected`: Raised when user selects a different help topic

**Public Methods:**
- `NavigateToTopic(topicId)`: Programmatically navigate to a specific topic
- `ExportAllTopics()`: Export all help content to a single file

**Public Properties (Read-Only):**
- `CurrentTopic`: Currently displayed topic
- `TreeViewControl`: Access to TreeView for customization
- `RichTextBoxControl`: Access to RichTextBox for customization
- `SearchTextBoxControl`: Access to search box
- `PrintButtonControl`: Access to print button
- `ExportButtonControl`: Access to export button

## Integration with TempestDisplay

### Settings Tab Reorganization

The help system has been integrated into FrmMain with the following changes:

1. **New TabControl**: The existing single Settings tab (`TpSettings`) now contains a `TabControl` (`TcSettings`) with multiple sub-tabs:
   - **General** (TpSetOptions): All current settings (moved from PnlSettings)
   - **API Keys** (TpApiKeys): Placeholder for future API settings
   - **Vacation Mode** (TpOptVacation): Placeholder for future feature
   - **Color Scales** (TpColorScale): Placeholder for future feature
   - **Logging** (TpLogSettings): Placeholder for future advanced logging
   - **Help** (TpSetHelp): The new help viewer

2. **Seamless Migration**: All existing settings controls remain functional in the General tab. No settings functionality was changed or broken.

3. **Future-Proof Design**: The tab structure is ready for future features to be added to their respective tabs.

### Implementation Files

**In TempestDisplay project:**
- `FrmMain.Partials/FrmMain.SettingsHelp.vb`: Partial class that handles help system integration
  - `InitializeSettingsHelp()`: Sets up the tab structure and help viewer
  - `ShowSettingsHelp(topicId)`: Navigate to specific help topic
  - `ShowSettingsHelpTab()`: Switch to help tab
  - `SettingsHelpViewer`: Property to access help viewer
  - **F1 Key Support**: Press F1 while on Settings tab to open help

**Modified Files:**
- `FrmMain.vb`: Added `InitializeSettingsHelp()` call in `FrmMain_Load`
- `TempestDisplay.vbproj`: Added project reference to TempestDisplay.Help

## Help Content Coverage

### Currently Documented Settings

The help system includes comprehensive documentation for:

#### General Options (TpSetOptions)
1. **Overview**: Introduction to general options
2. **Station Name**: What it is, how to choose, where it's used
3. **Station ID**: How to find it, why it's needed, troubleshooting
4. **Station Elevation**: Why it matters, how to find it, impact on calculations
5. **Update Interval**: Recommended values, API considerations, tradeoffs
6. **Rain Gauge Limits**: All five limits with regional guidelines and examples

#### API Keys (TpApiKeys)
1. **Overview**: Introduction to API authentication
2. **Tempest API Token**: How to obtain, format, security tips, troubleshooting
3. **Device ID**: Multiple methods to find it, format, common issues
4. **MeteoBridge Settings**: Login, password, IP address configuration

#### Vacation Mode (TpOptVacation)
1. **Overview**: Planned features
2. **Vacation Schedule**: Automated behaviors (coming soon)

#### Color Scales (TpColorScale)
1. **Overview**: Customization options
2. **Temperature Scale**: Default ranges, customization, accessibility presets
3. **Wind Speed Scale**: Beaufort scale-based, customization options

#### Logging Settings (TpLogSettings)
1. **Overview**: Why logging matters
2. **Log Retention Days**: Recommendations, disk usage, considerations
3. **Log Detail Level**: Error/Warning/Info/Debug/Trace levels
4. **Viewing Logs**: Multiple methods, searching, exporting

### Help Content Features

Each help topic includes:
- **Clear Titles**: Descriptive topic titles
- **Structured Content**: Organized with headers, bullets, and sections
- **Practical Examples**: Real-world values and scenarios
- **Troubleshooting**: Common issues and solutions
- **Best Practices**: Recommended settings and approaches
- **Context**: Why settings matter and how they affect the application

## Usage Examples

### Basic Usage (Already Integrated)

The help system is automatically available after building the project. Users can:

1. Click the **Settings** tab in TempestDisplay
2. Click the **Help** sub-tab
3. Browse topics in the TreeView
4. Use the search box to filter topics
5. Print or export topics as needed
6. Press **F1** from anywhere in Settings to open help

### Programmatic Usage

```vb
' Navigate to a specific help topic
Me.ShowSettingsHelp("setoptions_station_name")

' Show the help tab
Me.ShowSettingsHelpTab()

' Access the help viewer directly
Dim viewer = Me.SettingsHelpViewer
If viewer IsNot Nothing Then
    viewer.NavigateToTopic("apikeys_tempest_token")
End If

' Export all help content
If Me.SettingsHelpViewer IsNot Nothing Then
    Me.SettingsHelpViewer.ExportAllTopics()
End If
```

### Adding New Help Topics

To add new help content, edit `SettingsHelpProvider.vb`:

```vb
' Add a new topic in the appropriate AddXXXTopics method
_topics.Add(New HelpTopic(
    "mytopic_id",                    ' Unique ID
    "My Topic Title",                ' Display title
    "My Category",                   ' Category for grouping
    "TpMyTab",                       ' Associated tab page
    CreateRtfContent(
        "My Topic Title",            ' Content title
        "Help content goes here..."  ' Content body
    ),
    "keyword1", "keyword2"           ' Searchable keywords
) With {.SortOrder = 10})           ' Display order
```

## Rich Text Formatting

The help system uses RTF for rich formatting. The `CreateRtfContent` method automatically formats:
- **Titles**: Bold, larger font (28pt)
- **Section Headers**: Lines ending with ":" are bold
- **Bullet Points**: Lines starting with "•"
- **Regular Text**: Standard formatting (20pt Segoe UI)
- **Line Breaks**: Proper paragraph spacing

Manual RTF formatting is also supported for advanced needs.

## Technical Details

### Requirements
- **.NET 10.0-windows**: Modern .NET platform
- **Windows Forms**: UI framework
- **No External Dependencies**: Self-contained

### Build Information
- **Target Framework**: net10.0-windows
- **Output Type**: Library (DLL)
- **Assembly Version**: 26.1.9.1

### Performance
- **Lightweight**: Minimal memory footprint
- **Fast Search**: In-memory search with regex support
- **Lazy Loading**: Topics loaded once on initialization

### Accessibility
- **Keyboard Navigation**: Full keyboard support in TreeView and controls
- **Screen Reader Compatible**: Proper accessibility properties
- **High Contrast**: Respects Windows theme settings

## Future Enhancements

Potential improvements for future versions:

1. **Context-Sensitive Help**: Automatically show relevant help based on focused control
2. **Tooltips**: Quick help tooltips on controls with "?" icon to open full help
3. **Help History**: Back/Forward navigation through viewed topics
4. **Favorites**: Bookmark frequently accessed topics
5. **Help Search Highlighting**: Highlight search terms in content
6. **Video Tutorials**: Embed video links or tutorials
7. **Interactive Examples**: Step-by-step wizards for common tasks
8. **Multi-language Support**: Localization for other languages
9. **CHM Export**: Export to compiled HTML help format
10. **Online Help**: Sync with online documentation

## Troubleshooting

### Help System Not Appearing
- Verify TempestDisplay.Help.dll is in the output directory
- Check that `InitializeSettingsHelp()` is called in FrmMain_Load
- Look for errors in the application log file

### Search Not Working
- Ensure search text is at least 2 characters
- Check that topics have keywords defined
- Verify content isn't null or empty

### Print/Export Issues
- Verify user has write permissions to export location
- Check that PrintDocument is properly initialized
- Ensure RTF content is valid

### Building Issues
- Ensure .NET 10.0 SDK is installed
- Clean and rebuild solution
- Check project references are valid

## Contributing

To contribute new help topics:

1. Edit `SettingsHelpProvider.vb`
2. Add topics to appropriate category methods
3. Follow existing naming conventions (topicid_description)
4. Use `CreateRtfContent` for consistent formatting
5. Include comprehensive keywords for searchability
6. Test search functionality
7. Build and verify topics appear correctly

## License

This help system is part of TempestDisplay and follows the same license.

## Support

For issues or questions:
- Check application logs: `[Application Folder]\Logs\`
- Review this README
- Contact: Dennis N Maidon / PAROLE Software

---

**Version**: 26.1.9.1
**Last Updated**: January 9, 2026
**Author**: Claude Code (AI Assistant)
**Copyright**: ©2025 PAROLE Software
