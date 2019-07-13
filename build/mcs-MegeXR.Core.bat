@echo off

set libpath=..\_bin\dll
::----------------------------------
set name=MegeXR.Core
set depref=..\unitysln\MegeXR\Assets\3rd\uniLogger.dll
set libref=
::----------------------------------
MD %libpath%
set outfile=%libpath%\lib%name%.dll
set srcpath=..\unitysln\MegeXR\Assets\SDK\MegeXR\Core
call "%UNITY_ROOT%\Editor\Data\Mono\bin\smcs" -target:library -r:"%UNITY_ROOT%\Editor\Data\Managed\UnityEngine.dll";"%UNITY_ROOT%\Editor\Data\UnityExtensions\Unity\GUISystem\UnityEngine.UI.dll" -out:%outfile% -recurse:%srcpath%\*.cs -reference:%depref%;%libref%
echo FINISH
pause
exit