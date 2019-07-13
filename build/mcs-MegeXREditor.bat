@echo off

set libpath=..\_bin\dll
::----------------------------------
set name=MegeXREditor
set depref=
set libref=
::----------------------------------
MD %libpath%
set outfile=%libpath%\lib%name%.dll
set srcpath=..\unitysln\MegeXR\Assets\SDK\MegeXREditor\Editor
call "%UNITY_ROOT%\Editor\Data\Mono\bin\smcs" -target:library -r:"%UNITY_ROOT%\Editor\Data\Managed\UnityEngine.dll";"%UNITY_ROOT%\Editor\Data\UnityExtensions\Unity\GUISystem\UnityEngine.UI.dll";"%UNITY_ROOT%\Editor\Data\Managed\UnityEditor.dll" -out:%outfile% -recurse:%srcpath%\*.cs -reference:%depref%;%libref%
echo FINISH
pause
exit