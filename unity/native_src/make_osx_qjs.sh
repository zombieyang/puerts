mkdir -p build_osx_qjs && cd build_osx_qjs
if [ "$1" == "-ut" ]
then
    cmake -DJS_ENGINE=quickjs -DFOR_UT=ON -GXcode ../
    cd ..
    cmake --build build_osx_qjs --config Debug
else
    cmake -DJS_ENGINE=quickjs -GXcode ../
    cd ..
    cmake --build build_osx_qjs --config Release
fi
mkdir -p ../Assets/Plugins/
cp -r build_osx_qjs/Release/puerts.bundle ../Assets/Plugins/
if [ "$1" == "-ut" ]
then
    cp -r build_osx_qjs/Release/libpuerts.dylib ../general/Bin/
fi
