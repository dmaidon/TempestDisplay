Imports System.Text.Json.Serialization

''' <summary>
''' Represents an observation from a Tempest weather station (obs_st event).
''' </summary>
Public Class ObsStModel

    ''' <summary>
    ''' Device serial number.
    ''' </summary>
    <JsonPropertyName("serial_number")> Public Property SerialNumber As String

    ''' <summary>
    ''' Device type (e.g., "ST" for Tempest).
    ''' </summary>
    <JsonPropertyName("type")> Public Property Type As String

    ''' <summary>
    ''' Hub serial number that this device is connected to.
    ''' </summary>
    <JsonPropertyName("hub_sn")> Public Property HubSn As String

    ''' <summary>
    ''' Observation data array containing weather measurements.
    ''' Array indices:
    ''' [0] = Epoch (Unix timestamp)
    ''' [1] = Wind Lull (m/s)
    ''' [2] = Wind Avg (m/s)
    ''' [3] = Wind Gust (m/s)
    ''' [4] = Wind Direction (degrees)
    ''' [5] = Wind Sample Interval (seconds)
    ''' [6] = Station Pressure (MB)
    ''' [7] = Air Temperature (C)
    ''' [8] = Relative Humidity (%)
    ''' [9] = Illuminance (lux)
    ''' [10] = UV Index
    ''' [11] = Solar Radiation (W/m^2)
    ''' [12] = Rain Accumulated (mm)
    ''' [13] = Precipitation Type (0 = none, 1 = rain, 2 = hail, 3 = rain + hail)
    ''' [14] = Lightning Strike Average Distance (km)
    ''' [15] = Lightning Strike Count
    ''' [16] = Battery (volts)
    ''' [17] = Report Interval (minutes)
    ''' [18] = Local Daily Rain Accumulation (mm)
    ''' [19] = Rain Accumulation Final (mm) - Rain check
    ''' [20] = Local Daily Rain Accumulation Final (mm) - Rain check
    ''' [21] = Precipitation Analysis Type (0 = none, 1 = Rain Check)
    ''' </summary>
    <JsonPropertyName("obs")> Public Property Obs As Single()

    ''' <summary>
    ''' Firmware revision number.
    ''' </summary>
    <JsonPropertyName("firmware_revision")> Public Property FirmwareRevision As Integer

End Class