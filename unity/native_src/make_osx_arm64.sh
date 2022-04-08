ENGINE=$1
if [ "$1" == "" ]
then
    ENGINE="v8"
fi

OUTPUT=build_osx_arm64_$ENGINE

mkdir -p $OUTPUT && cd $OUTPUT
cmake -DJS_ENGINE=$ENGINE -DFOR_SILICON=ON -GXcode ../
cd ..
cmake --build $OUTPUT --config Release
cmake --install $OUTPUT --prefix "$(pwd)/$OUTPUT"
mv $OUTPUT/bin/libpuerts.dylib $OUTPUT/bin/puerts.bundle
cp -r $ENGINE/Lib/macOS_arm64/*.dylib $OUTPUT/bin/

mkdir -p ../Assets/Plugins/macOS_arm64
cp -r $OUTPUT/bin/* ../Assets/Plugins/macOS_arm64/