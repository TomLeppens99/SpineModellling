@echo off
REM ===========================================================
REM  Launch Unified Spine Modeling App in 'landmarks' env
REM ===========================================================

REM Change to drive containing your program
C:

REM Activate conda environment
CALL "%ProgramData%\Anaconda3\Scripts\activate.bat" landmarks

REM Change to the script directory
cd "C:\GBW_MyPrograms\SpineModellling\SpineModellling_python\Fabio_Galbusera_algorithm_CR\Fabio_Galbusera_algorithm_CR"

REM Run the Unified Python app
python UnifiedLauncher.py

REM Keep window open after completion
echo.
echo Application closed. Press any key to exit.
pause >nul
