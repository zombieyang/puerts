cd %HOMEPATH%

md puerts-node\include
md puerts-node\deps\uv\include
md puerts-node\deps\v8\include

copy node\src\node.h .\puerts-node\include
copy node\src\node_version.h .\puerts-node\include
xcopy /E /I node\deps\uv\include .\puerts-node\deps\uv /s/h/e/k/f/c
xcopy /E /I node\deps\v8\include .\puerts-node\deps\v8 /s/h/e/k/f/c

md puerts-node\lib\Win32\
copy node\out\Release\libnode.dll .\puerts-node\lib\Win32\
copy node\out\Release\libnode.exp .\puerts-node\lib\Win32\
copy node\out\Release\libnode.lib .\puerts-node\lib\Win32\