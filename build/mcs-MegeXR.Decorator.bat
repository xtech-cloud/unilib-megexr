@echo off

set libpath=..\_bin\dll
::----------------------------------
set name=MegeXR.Decorator
set depref=..\_bin\dll\libMegeXR.Core.dll
set libref=
::----------------------------------
MD %libpath%
set outfile=%libpath%\lib%name%.dll
set srcpath=..\unitysln\MegeXR\Assets\SDK\MegeXR\Decorator
call "%UNITY_ROOT%\Editor\Data\Mono\bin\smcs" -target:library -r:"%UNITY_ROOT%\Editor\Data\Managed\UnityEngine.dll";"%UNITY_ROOT%\Editor\Data\UnityExtensions\Unity\GUISystem\UnityEngine.UI.dll" -out:%outfile% -recurse:%srcpath%\*.cs -reference:%depref%;%libref%
echo FINISH
pause
exit