Public Class TempestModel
    Public Property status As Status
    Public Property elevation As Single?
    Public Property is_public As Boolean
    Public Property latitude As Single?
    Public Property longitude As Single?
    Public Property obs As Ob()
    Public Property outdoor_keys As String()
    Public Property public_name As String
    Public Property station_id As Integer?
    Public Property station_name As String
    Public Property station_units As Station_Units
    Public Property timezone As String
End Class

Public Class Status
    Public Property status_code As Integer?
    Public Property status_message As String
End Class

Public Class Station_Units
    Public Property units_direction As String
    Public Property units_distance As String
    Public Property units_other As String
    Public Property units_precip As String
    Public Property units_pressure As String
    Public Property units_temp As String
    Public Property units_wind As String
End Class

Public Class Ob
    Public Property air_density As Single?
    Public Property air_temperature As Single?
    Public Property barometric_pressure As Single?
    Public Property brightness As Integer?
    Public Property delta_t As Single?
    Public Property dew_point As Single?
    Public Property feels_like As Single?
    Public Property heat_index As Single?
    Public Property lightning_strike_count As Integer?
    Public Property lightning_strike_count_last_1hr As Integer?
    Public Property lightning_strike_count_last_3hr As Integer?
    Public Property lightning_strike_last_distance As Integer?
    Public Property lightning_strike_last_epoch As Long?
    Public Property precip As Single?
    Public Property precip_accum_last_1hr As Single?
    Public Property precip_accum_local_day As Single?
    Public Property precip_accum_local_day_final As Single?
    Public Property precip_accum_local_yesterday As Single?
    Public Property precip_accum_local_yesterday_final As Single?
    Public Property precip_analysis_type_yesterday As Integer?
    Public Property precip_minutes_local_day As Integer?
    Public Property precip_minutes_local_yesterday As Integer?
    Public Property precip_minutes_local_yesterday_final As Integer?
    Public Property pressure_trend As String
    Public Property relative_humidity As Integer?
    Public Property sea_level_pressure As Single?
    Public Property solar_radiation As Integer?
    Public Property station_pressure As Single?
    Public Property timestamp As Long?
    Public Property uv As Single?
    Public Property wet_bulb_globe_temperature As Single?
    Public Property wet_bulb_temperature As Single?
    Public Property wind_avg As Single?
    Public Property wind_chill As Single?
    Public Property wind_direction As Integer?
    Public Property wind_gust As Single?
    Public Property wind_lull As Single?
End Class