set VERSION=%1

powershell -command "Invoke-WebRequest https://github.com/nodejs/node/archive/refs/tags/v%VERSION%.zip -O node.zip"
7z x node.zip -o*

cd node/node-%VERSION%

Set-ExecutionPolicy Unrestricted -Force
iex ((New-Object System.Net.WebClient).DownloadString('https://boxstarter.org/bootstrapper.ps1'))
get-boxstarter -Force
Install-BoxstarterPackage https://raw.githubusercontent.com/nodejs/node/HEAD/tools/bootstrap/windows_boxstarter -DisableReboots

.\vcbuild.bat dll openssl_no_asm

md puerts-node/include
md puerts-node/deps/uv/include
md puerts-node/deps/v8/include

copy src/node.h ./puerts-node/include
copy src/node_version.h ./puerts-node/include
xcopy /E /I deps/uv/include ./puerts-node/deps/uv
xcopy /E /I deps/v8/include ./puerts-node/deps/v8

md puerts-node/lib/macOS/
copy out/Release/libnode.83.dylib ./puerts-node/lib/macOS/