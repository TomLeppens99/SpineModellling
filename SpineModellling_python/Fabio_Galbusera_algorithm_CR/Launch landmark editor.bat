@echo off
REM ===========================================================
REM  Launch Fabio Galbusera EOS_10points script in 'landmarks' env
REM ===========================================================

REM Change to correct drive
C:

REM Activate the conda environment
CALL "%ProgramData%\Anaconda3\Scripts\activate.bat" landmarks

REM Change to the script directory
cd "C:\GBW_MyPrograms\Fabio_Galbusera_algorithm_CR\Fabio_Galbusera_algorithm_CR\Fabio_Galbusera_algorithm_CR\EOS_10_Points\fr_10points"

REM Run the Python script
python EOS_10points.py

REM Keep window open after execution
echo.
echo Script finished. Press any key to close.
pause >nul
