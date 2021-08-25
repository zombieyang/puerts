set VERSION=%1

cd %HOMEPATH%
git -h
@REM powershell -command "Invoke-WebRequest https://github.com/nodejs/node/archive/refs/tags/v%VERSION%.zip -O node.zip"
@REM 7z x node.zip -o*

@REM cd node/node-%VERSION%

@REM .\vcbuild.bat dll openssl-no-asm

@REM cd %HOMEPATH%

@REM md puerts-node/include
@REM md puerts-node/deps/uv/include
@REM md puerts-node/deps/v8/include

@REM copy node/node-%VERSION%/src/node.h ./puerts-node/include
@REM copy node/node-%VERSION%/src/node_version.h ./puerts-node/include
@REM xcopy /E /I node/node-%VERSION%/deps/uv/include ./puerts-node/deps/uv
@REM xcopy /E /I node/node-%VERSION%/deps/v8/include ./puerts-node/deps/v8

@REM md puerts-node/lib/Win32/
@REM copy node/node-%VERSION%/out/Release/libnode.dll ./puerts-node/lib/Win32/
@REM copy node/node-%VERSION%/out/Release/node.exp ./puerts-node/lib/Win32/
@REM copy node/node-%VERSION%/out/Release/node.lib ./puerts-node/lib/Win32/