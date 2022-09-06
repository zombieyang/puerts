#!/bin/sh
set -e
if test "$CONFIGURATION" = "Debug"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  make -f /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/CMakeScripts/ReRunCMake.make
fi
if test "$CONFIGURATION" = "Release"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  make -f /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/CMakeScripts/ReRunCMake.make
fi
if test "$CONFIGURATION" = "MinSizeRel"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  make -f /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/CMakeScripts/ReRunCMake.make
fi
if test "$CONFIGURATION" = "RelWithDebInfo"; then :
  cd /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs
  make -f /Volumes/DATA/Code/puerts@stable/puerts/unity/native_src/build_osx_arm64_quickjs/CMakeScripts/ReRunCMake.make
fi

