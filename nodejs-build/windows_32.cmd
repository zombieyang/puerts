set VERSION=%1

cd %HOMEPATH%
git clone --single-branch -b v14.x https://github.com/nodejs/node.git

cd node
git fetch origin v%VERSION%
git checkout v%VERSION%

node %GITHUB_WORKSPACE%\v8-build\CRLF2LF.js %GITHUB_WORKSPACE%\nodejs-build\nodemod.patch
call git apply --cached --reject %GITHUB_WORKSPACE%\nodejs-build\nodemod.patch
call git checkout -- .

@REM .\vcbuild.bat dll openssl-no-asm

@REM cd %HOMEPATH%

@REM md puerts-node/include
@REM md puerts-node/deps/uv/include
@REM md puerts-node/deps/v8/include

@REM copy node/src/node.h ./puerts-node/include
@REM copy node/src/node_version.h ./puerts-node/include
@REM xcopy /E /I node/deps/uv/include ./puerts-node/deps/uv
@REM xcopy /E /I node/deps/v8/include ./puerts-node/deps/v8

@REM md puerts-node/lib/Win32/
@REM copy node/out/Release/libnode.dll ./puerts-node/lib/Win32/
@REM copy node/out/Release/libnode.exp ./puerts-node/lib/Win32/
@REM copy node/out/Release/libnode.lib ./puerts-node/lib/Win32/