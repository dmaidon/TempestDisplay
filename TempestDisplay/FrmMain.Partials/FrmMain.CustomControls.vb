Partial Public Class FrmMain

    Private ReadOnly _customBackColor As Color = Color.AntiqueWhite

    Private Sub InitializeCustomcontrols()

        Try
            ' Temperature gauges
            If TgCurrentTemp IsNot Nothing Then
                TgCurrentTemp.Label = "Current Temperature"
                TgCurrentTemp.BackColor = _customBackColor
                TgCurrentTemp.Font = New Font(TgCurrentTemp.Font, FontStyle.Bold)
                TgCurrentTemp.MinF = -5
                TgCurrentTemp.MaxF = 110
            Else
                ' Create TgCurrentTemp control dynamically if it doesn't exist
                Log.Write("TgCurrentTemp control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        TgCurrentTemp = New Controls.TempGaugeControl With {
                            .Label = "Current Temperature",
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .MinF = -5,
                            .MaxF = 110
                        }
                        TgCurrentTemp.Font = New Font(TgCurrentTemp.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 0 with column span of 2
                        TlpData.Controls.Add(TgCurrentTemp, 0, 0)
                        TlpData.SetColumnSpan(TgCurrentTemp, 2)

                        Log.Write("TgCurrentTemp control created and added to TlpData[0,0] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add TgCurrentTemp")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating TgCurrentTemp control dynamically")
                End Try
            End If

            If TgFeelsLike IsNot Nothing Then
                TgFeelsLike.Label = "Feels Like"
                TgFeelsLike.BackColor = _customBackColor
                TgFeelsLike.Font = New Font(TgFeelsLike.Font, FontStyle.Bold)
                TgFeelsLike.MinF = -10
                TgFeelsLike.MaxF = 120
            Else
                ' Create TgFeelsLike control dynamically if it doesn't exist
                Log.Write("TgFeelsLike control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        TgFeelsLike = New Controls.TempGaugeControl With {
                            .Label = "Feels Like",
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .MinF = -10,
                            .MaxF = 120
                        }
                        TgFeelsLike.Font = New Font(TgFeelsLike.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 1 with column span of 2
                        TlpData.Controls.Add(TgFeelsLike, 0, 1)
                        TlpData.SetColumnSpan(TgFeelsLike, 2)

                        Log.Write("TgFeelsLike control created and added to TlpData[0,1] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add TgFeelsLike")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating TgFeelsLike control dynamically")
                End Try
            End If

            If TgDewpoint IsNot Nothing Then
                TgDewpoint.Label = "Dew Point"
                TgDewpoint.BackColor = _customBackColor
                TgDewpoint.Font = New Font(TgDewpoint.Font, FontStyle.Bold)
                TgDewpoint.MinF = -5
                TgDewpoint.MaxF = 110
            Else
                ' Create TgDewpoint control dynamically if it doesn't exist
                Log.Write("TgDewpoint control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        TgDewpoint = New Controls.TempGaugeControl With {
                            .Label = "Dew Point",
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .MinF = -5,
                            .MaxF = 110
                        }
                        TgDewpoint.Font = New Font(TgDewpoint.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 2 with column span of 2
                        TlpData.Controls.Add(TgDewpoint, 0, 2)
                        TlpData.SetColumnSpan(TgDewpoint, 2)

                        Log.Write("TgDewpoint control created and added to TlpData[0,2] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add TgDewpoint")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating TgDewpoint control dynamically")
                End Try
            End If

            ' Humidity gauge
            If FgRH IsNot Nothing Then
                FgRH.Label = "Relative Humidity"
                FgRH.BackColor = _customBackColor
                FgRH.Font = New Font(FgRH.Font, FontStyle.Bold)
            Else
                ' Create FgRH control dynamically if it doesn't exist
                Log.Write("FgRH control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        FgRH = New Controls.FanGaugeControl With {
                            .Label = "Relative Humidity",
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        FgRH.Font = New Font(FgRH.Font, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 3 with column span of 2
                        TlpData.Controls.Add(FgRH, 2, 0)
                        TlpData.SetColumnSpan(FgRH, 2)

                        Log.Write("FgRH control created and added to TlpData[2,0] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add FgRH")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating FgRH control dynamically")
                End Try
            End If

            ' Precipitation control
            If PTC IsNot Nothing Then
                PTC.BackColor = _customBackColor
                PTC.Font = New Font("Arial", 7.75, FontStyle.Bold)
            Else
                ' Create PTC control dynamically if it doesn't exist
                Log.Write("PTC control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        PTC = New Controls.PrecipTowersControl With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill
                        }
                        PTC.Font = New Font(PTC.Font.FontFamily, 8, FontStyle.Bold)

                        ' Add to TableLayoutPanel at column 0, row 4 with column span of 2
                        TlpData.Controls.Add(PTC, 6, 0)
                        TlpData.SetColumnSpan(PTC, 2)

                        Log.Write("PTC control created and added to TlpData[6,0] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add PTC")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating PTC control dynamically")
                End Try
            End If

            If WrWindSpeed IsNot Nothing Then
                WrWindSpeed.BackColor = _customBackColor
                WrWindSpeed.Font = New Font(WrWindSpeed.Font, FontStyle.Bold)
                WrWindSpeed.Label = "Wind"
            Else
                Log.Write("WrWindSpeed control is Nothing - creating dynamically")
                Try
                    If TlpData IsNot Nothing Then
                        WrWindSpeed = New Controls.WindRoseControl With {
                            .BackColor = _customBackColor,
                            .Dock = DockStyle.Fill,
                            .Label = "Wind"
                        }
                        WrWindSpeed.Font = New Font(WrWindSpeed.Font.FontFamily, 8, FontStyle.Bold)
                        ' Add to TableLayoutPanel at column 4, row 2 with column span of 2
                        TlpData.Controls.Add(WrWindSpeed, 2, 1)
                        TlpData.SetColumnSpan(WrWindSpeed, 2)
                        Log.Write("WrWindSpeed control created and added to TlpData[4,2] with ColumnSpan=2")
                    Else
                        Log.Write("WARNING: TlpData is Nothing - cannot add WrWindSpeed")
                    End If
                Catch ex As Exception
                    Log.WriteException(ex, "Error creating WrWindSpeed control dynamically")
                End Try
            End If

            Log.Write("Initialized custom controls.")
        Catch ex As Exception
            Log.WriteException(ex, "Failed to initialize custom controls")
        End Try
    End Sub

End Class