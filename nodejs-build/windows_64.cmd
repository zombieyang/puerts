set VERSION=%1

cd %HOMEPATH%
git clone --single-branch -b v14.x --no-tags https://github.com/nodejs/node.git

cd node
git fetch origin v%VERSION%
git checkout v%VERSION%

node %GITHUB_WORKSPACE%\v8-build\CRLF2LF.js %GITHUB_WORKSPACE%\nodejs-build\nodemod.patch
call git apply --cached --reject %GITHUB_WORKSPACE%\nodejs-build\nodemod.patch
call git checkout -- .

.\vcbuild.bat dll openssl-no-asm

cd %HOMEPATH%

md puerts-node/include
md puerts-node/deps/uv/include
md puerts-node/deps/v8/include

copy node/src/node.h ./puerts-node/include
copy node/src/node_version.h ./puerts-node/include
xcopy /E /I node/deps/uv/include ./puerts-node/deps/uv
xcopy /E /I node/deps/v8/include ./puerts-node/deps/v8

md puerts-node/lib/Win64/
copy node/out/Release/libnode.dll ./puerts-node/lib/Win64/
copy node/out/Release/libnode.exp ./puerts-node/lib/Win64/
copy node/out/Release/libnode.lib ./puerts-node/lib/Win64/