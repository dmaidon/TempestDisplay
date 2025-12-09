@echo off
REM ========================================
REM UDP Listener Refactoring - ROLLBACK Script
REM ========================================
REM Use this script if you need to restore the original file
REM and undo the refactoring changes
REM ========================================

echo.
echo ========================================
echo UDP Listener Refactoring ROLLBACK
echo ========================================
echo.
echo ⚠ WARNING: This will undo the refactoring!
echo.
echo This will:
echo   1. Delete the refactored FrmMain.UdpListener.vb
echo   2. Restore original from backup
echo   3. Rename refactored version back to .Refactored.vb
echo.
echo Press Ctrl+C to cancel, or
pause

cd /d "C:\VB18\TempestDisplay\TempestDisplay\FrmMain.Partials"

echo.
echo [1/3] Checking for backup file...
if exist "FrmMain.UdpListener.ORIGINAL.BACKUP.vb" (
    echo    ✓ Backup file found
) else (
    echo    ✗ ERROR: Backup file not found!
    echo    Cannot rollback without backup file.
    echo.
    pause
    exit /b 1
)

echo.
echo [2/3] Removing refactored file...
if exist "FrmMain.UdpListener.vb" (
    if exist "FrmMain.UdpListener.Refactored.vb" (
        echo    ℹ Refactored.vb already exists - renaming current to .NEW
        ren "FrmMain.UdpListener.vb" "FrmMain.UdpListener.NEW.vb"
    ) else (
        ren "FrmMain.UdpListener.vb" "FrmMain.UdpListener.Refactored.vb"
    )
    echo    ✓ Current file renamed
) else (
    echo    ℹ No current FrmMain.UdpListener.vb found
)

echo.
echo [3/3] Restoring original from backup...
copy "FrmMain.UdpListener.ORIGINAL.BACKUP.vb" "FrmMain.UdpListener.vb"
echo    ✓ Original file restored

echo.
echo ========================================
echo Rollback Complete!
echo ========================================
echo.
echo Original file has been restored: FrmMain.UdpListener.vb
echo.
echo Optional cleanup:
echo You may want to delete these refactored files if not using them:
echo   • FrmMain.GridUpdates.vb
echo   • FrmMain.ObservationUI.vb
echo   • Models\ObservationData.vb
echo   • Models\ObservationParser.vb
echo.
echo Next steps:
echo   1. Open solution in Visual Studio
echo   2. Build the solution
echo   3. Run and test
echo.
pause
