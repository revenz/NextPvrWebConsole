REM @echo off
set vboxmanage=%programfiles%\oracle\virtualbox\vboxmanage.exe
set hypervhost=%1
set username=%2
set password=%3
set vname=%4
set snapshot=%5
set setupexe=%6
set psexec=C:\utils\pstools\psexec.exe

:start
psexec \\%hypervhost% -u %username% -p %password% HyperVManager.exe -start -force -vm %vname% -snapshot %snapshot%
ping -n 10 127.0.0.1 >nul

:install
REM "%psexec%" -u %username% -p %password% \\%vboxname% -c "%setupexe%"