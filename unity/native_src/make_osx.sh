ENGINE=$1
if [ "$1" == "" ]
then
    ENGINE="v8"
fi

OUTPUT=build_osx_$ENGINE

mkdir -p $OUTPUT && cd $OUTPUT
cmake -DJS_ENGINE=$ENGINE -GXcode ../
cd ..
cmake --build $OUTPUT --config Release
cmake --install $OUTPUT --prefix "$(pwd)/$OUTPUT"
mv $OUTPUT/bin/libpuerts.dylib $OUTPUT/bin/puerts.bundle
cp -r $ENGINE/Lib/macOS/*.dylib $OUTPUT/bin/

mkdir -p ../Assets/Plugins/macOS
cp -r $OUTPUT/bin/* ../Assets/Plugins/macOS/


# mkdir -p build_osx && cd build_osx
# if [ "$1" == "-ut" ]
# then
#     cmake -DFOR_UT=ON ../
#     cd ..
#     cmake --build build_osx --config Debug
#     cp -r build_osx/libpuerts.dylib ../general/Bin/
# else
#     cmake -GXcode ../
#     cd ..
#     cmake --build build_osx --config Release
#     cmake --install build_osx --prefix "$(pwd)/build_osx"
#     mv build_osx/bin/libpuerts.dylib build_osx/bin/libpuerts.bundle
#     mkdir -p ../Assets/Plugins/macOS
#     cp -r build_osx/bin/* ../Assets/Plugins/macOS/
# fi