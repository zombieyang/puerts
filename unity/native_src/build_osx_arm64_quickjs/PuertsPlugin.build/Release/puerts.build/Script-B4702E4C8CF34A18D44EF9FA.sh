#!/bin/sh
set -e
if test "$CONFIGURATION" = "Debug"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  /Applications/CMake.app/Contents/bin/cmake -E cmake_symlink_library /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/Debug/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/Debug/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/Debug/libpuerts.dylib
fi
if test "$CONFIGURATION" = "Release"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  /Applications/CMake.app/Contents/bin/cmake -E cmake_symlink_library /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/Release/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/Release/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/Release/libpuerts.dylib
fi
if test "$CONFIGURATION" = "MinSizeRel"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  /Applications/CMake.app/Contents/bin/cmake -E cmake_symlink_library /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/MinSizeRel/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/MinSizeRel/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/MinSizeRel/libpuerts.dylib
fi
if test "$CONFIGURATION" = "RelWithDebInfo"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  /Applications/CMake.app/Contents/bin/cmake -E cmake_symlink_library /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/RelWithDebInfo/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/RelWithDebInfo/libpuerts.dylib /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/RelWithDebInfo/libpuerts.dylib
fi

