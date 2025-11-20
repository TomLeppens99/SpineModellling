@echo off
REM ===========================================================
REM  Launch Fabio Galbusera EOS_10_Points app in 'landmarks' env
REM ===========================================================

REM Change to drive containing your program
C:

REM Activate conda environment
CALL "%ProgramData%\Anaconda3\Scripts\activate.bat" landmarks

REM Change to the script directory
cd "C:\GBW_MyPrograms\Fabio_Galbusera_algorithm_CR\Fabio_Galbusera_algorithm_CR\Fabio_Galbusera_algorithm_CR\EOS_10_Points\EOS_2022_04_07"

REM Run the Python app
python app.py

REM Keep window open after completion
echo.
echo Script finished. Press any key to close.
pause >nul
