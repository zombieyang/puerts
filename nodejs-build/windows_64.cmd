set VERSION=%1

cd %HOMEPATH%
powershell -command "Invoke-WebRequest https://github.com/nodejs/node/archive/refs/tags/v%VERSION%.zip -O node.zip"
7z x node.zip -o*

cd node/node-%VERSION%

.\vcbuild.bat dll openssl-no-asm

cd %HOMEPATH%

md puerts-node/include
md puerts-node/deps/uv/include
md puerts-node/deps/v8/include

copy node/node-%VERSION%/src/node.h ./puerts-node/include
copy node/node-%VERSION%/src/node_version.h ./puerts-node/include
xcopy /E /I node/node-%VERSION%/deps/uv/include ./puerts-node/deps/uv
xcopy /E /I node/node-%VERSION%/deps/v8/include ./puerts-node/deps/v8

md puerts-node/lib/macOS/
copy node/node-%VERSION%/out/Release/libnode.83.dylib ./puerts-node/lib/macOS/