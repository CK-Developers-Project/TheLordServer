@echo off

set XLSX_PATH=.\xlsx

set CSV_PATH=.\csv

set GEN_PATH=.\xlsxToCsv.exe

del %CSV_PATH%\*.* /f /s /q
echo ---------------------------------------------------

for /f %%i in ('dir /b "%XLSX_PATH%\*.xlsx"') do (
    echo Convert : %%i to %%~ni.csv
    %GEN_PATH% %XLSX_PATH%\%%i %CSV_PATH%\%%~ni.csv
)

echo Convert Complete!

xcopy ".\csv\*.*" "..\..\TheLordServer.Table\Table" /e /h /k /y

xcopy ".\csv\*.*" "..\..\..\Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\TheLordServer\Table" /e /h /k /y

pause