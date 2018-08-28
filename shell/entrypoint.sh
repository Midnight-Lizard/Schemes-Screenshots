export DISPLAY=:99
Xvfb ${DISPLAY} -screen 0 1280x800x8 -nolisten tcp &
dotnet MidnightLizard.Schemes.Screenshots.dll
