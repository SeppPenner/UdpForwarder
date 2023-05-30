cd src\UdpForwarder
dotnet publish -c Release --output publish/ -r linux-x64 --no-self-contained
@ECHO.Build successful. Press any key to exit.
pause