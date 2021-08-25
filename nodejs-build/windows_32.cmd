set VERSION=%1

powershell -command "Invoke-WebRequest https://github.com/nodejs/node/archive/refs/tags/v%VERSION%.zip -O node.zip"
7z x node.zip -o*

cd node 
.\vcbuild.bat dll


md puerts-node/include
md puerts-node/deps/uv/include
md puerts-node/deps/v8/include

copy src/node.h ./puerts-node/include
copy src/node_version.h ./puerts-node/include
xcopy /E /I deps/uv/include ./puerts-node/deps/uv
xcopy /E /I deps/v8/include ./puerts-node/deps/v8

md puerts-node/lib/macOS/
copy out/Release/libnode.83.dylib ./puerts-node/lib/macOS/