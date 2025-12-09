@echo off
REM ========================================
REM UDP Listener Refactoring - Cleanup Script
REM ========================================
REM This script removes duplicate code and activates the refactored version
REM 
REM What this does:
REM 1. Creates a backup of the original file (just in case)
REM 2. Deletes the original FrmMain.UdpListener.vb (950+ lines with duplicates)
REM 3. Renames FrmMain.UdpListener.Refactored.vb to FrmMain.UdpListener.vb
REM 4. The new modular structure (GridUpdates, ObservationUI, Models) remains
REM
REM ========================================

echo.
echo ========================================
echo UDP Listener Refactoring Cleanup
echo ========================================
echo.
echo This will:
echo   1. Backup original FrmMain.UdpListener.vb
echo   2. Delete original file (with duplicates)
echo   3. Activate refactored version
echo.
echo Press Ctrl+C to cancel, or
pause

cd /d "C:\VB18\TempestDisplay\TempestDisplay\FrmMain.Partials"

echo.
echo [1/3] Creating backup of original file...
if exist "FrmMain.UdpListener.vb" (
    copy "FrmMain.UdpListener.vb" "FrmMain.UdpListener.ORIGINAL.BACKUP.vb"
    echo    ✓ Backup created: FrmMain.UdpListener.ORIGINAL.BACKUP.vb
) else (
    echo    ⚠ Original file not found - may already be cleaned up
)

echo.
echo [2/3] Removing original file with duplicates...
if exist "FrmMain.UdpListener.vb" (
    del "FrmMain.UdpListener.vb"
    echo    ✓ Original file deleted
) else (
    echo    ⚠ Original file not found - skipping deletion
)

echo.
echo [3/3] Activating refactored version...
if exist "FrmMain.UdpListener.Refactored.vb" (
    ren "FrmMain.UdpListener.Refactored.vb" "FrmMain.UdpListener.vb"
    echo    ✓ Refactored version renamed to FrmMain.UdpListener.vb
) else (
    echo    ⚠ Refactored file not found
    echo    ⚠ You may need to restore from backup!
)

echo.
echo ========================================
echo Cleanup Complete!
echo ========================================
echo.
echo New file structure:
echo   • FrmMain.UdpListener.vb          (450 lines - main coordinator)
echo   • FrmMain.GridUpdates.vb          (220 lines - DataGridView)
echo   • FrmMain.ObservationUI.vb        (180 lines - UI updates)
echo   • ObservationData.vb              (100 lines - model)
echo   • ObservationParser.vb            (90 lines - parser)
echo.
echo Next steps:
echo   1. Open solution in Visual Studio
echo   2. Build the solution (Ctrl+Shift+B)
echo   3. Fix any build errors if they appear
echo   4. Run the application and test
echo   5. Use REFACTORING_TESTING_CHECKLIST.md for testing
echo.
echo Backup file location (in case of rollback):
echo   %CD%\FrmMain.UdpListener.ORIGINAL.BACKUP.vb
echo.
pause
