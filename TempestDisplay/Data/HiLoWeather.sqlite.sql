-- Last Edit: January 15, 2026 (Added wind averaging accumulators for vector-based direction averaging, added performance indexes)
-- SQLite schema for TempestDisplay hi-lo tracking
-- Stored under DataDir (Globals.DataDir)

CREATE TABLE IF NOT EXISTS HiLoDaily (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ObsDate TEXT NOT NULL,              -- YYYY-MM-DD (local date)

    -- Temperature metrics (F)
    TempHigh REAL,
    TempHighTime TEXT,
    TempLow REAL,
    TempLowTime TEXT,

    -- Feels like (F)
    FeelsLikeHigh REAL,
    FeelsLikeHighTime TEXT,
    FeelsLikeLow REAL,
    FeelsLikeLowTime TEXT,

    -- Heat index (F)
    HeatIndexHigh REAL,
    HeatIndexHighTime TEXT,

    -- Wind chill (F)
    WindChillLow REAL,
    WindChillLowTime TEXT,

    -- Rain (inches)
    RainDay REAL,
    RainMonth REAL,
    RainYear REAL,

    -- Wind
    WindSpeedHigh REAL,                -- daily peak avg/gust mph
    WindSpeedHighTime TEXT,
    WindSpeedAvg REAL,                 -- computed daily average mph
    WindDirAvg REAL,                   -- computed daily average direction degrees

    -- Wind averaging accumulators (for proper vector-based direction averaging)
    WindSpeedTotal REAL DEFAULT 0,     -- cumulative sum for speed
    WindSpeedCount INTEGER DEFAULT 0,  -- sample count for speed
    WindDirSumX REAL DEFAULT 0,        -- X component of direction vector sum
    WindDirSumY REAL DEFAULT 0,        -- Y component of direction vector sum
    WindDirSampleCount INTEGER DEFAULT 0, -- sample count for direction

    -- UV Index
    UVIndexHigh REAL,
    UVIndexHighTime TEXT,

    -- Solar Radiation (W/m²)
    SolarRadiationHigh REAL,
    SolarRadiationHighTime TEXT,

    LastUpdated TEXT NOT NULL          -- ISO-8601 timestamp
);

CREATE UNIQUE INDEX IF NOT EXISTS IX_HiLoDaily_ObsDate ON HiLoDaily(ObsDate);
CREATE INDEX IF NOT EXISTS IX_HiLoDaily_LastUpdated ON HiLoDaily(LastUpdated);
CREATE INDEX IF NOT EXISTS IX_HiLoDaily_TempHighTime ON HiLoDaily(TempHighTime);

-- Global all-time records (single-row table)
CREATE TABLE IF NOT EXISTS HiLoAllTime (
    Id INTEGER PRIMARY KEY CHECK (Id = 1),

    -- Record temperature (F)
    TempHigh REAL,
    TempHighTime TEXT,
    TempLow REAL,
    TempLowTime TEXT,

    -- Record heat index / wind chill (F)
    HeatIndexHigh REAL,
    HeatIndexHighTime TEXT,
    WindChillLow REAL,
    WindChillLowTime TEXT,

    -- Record rain (inches)
    RainDayMax REAL,
    RainDayMaxDate TEXT,          -- day with highest daily total
    RainMonthMax REAL,
    RainMonthMaxYear INTEGER,
    RainMonthMaxMonth INTEGER,

    -- Record wind speed (mph)
    WindSpeedHigh REAL,
    WindSpeedHighTime TEXT,

    -- Record UV Index
    UVIndexHigh REAL,
    UVIndexHighTime TEXT,

    -- Record Solar Radiation (W/m²)
    SolarRadiationHigh REAL,
    SolarRadiationHighTime TEXT,

    -- Aggregate wind direction vector for long-term average
    WindDirSumX REAL,
    WindDirSumY REAL,
    WindDirSampleCount INTEGER,

    LastUpdated TEXT
);

INSERT OR IGNORE INTO HiLoAllTime (Id, WindDirSumX, WindDirSumY, WindDirSampleCount)
VALUES (1, 0.0, 0.0, 0);
