if ( MSYS OR WIN32 )
    if("${CMAKE_SIZEOF_VOID_P}" STREQUAL "4")
        set(LIB_FILE_NAMES ${BACKEND_ROOT}/Lib/Win32/libnode.lib)
        set(COPY_FILE_NAMES ${BACKEND_ROOT}/Lib/Win32/libnode.dll)
    else ()
        set(LIB_FILE_NAMES ${BACKEND_ROOT}/Lib/Win64/libnode.lib)
        set(COPY_FILE_NAMES ${BACKEND_ROOT}/Lib/Win64/libnode.dll)
    endif ()

elseif ( ANDROID )

    if( ANDROID_ABI STREQUAL "armeabi-v7a")
        set(LIB_FILE_NAMES ${BACKEND_ROOT}/lib/Android/armeabi-v7a/libnode.so)
        set(COPY_FILE_NAMES ${BACKEND_ROOT}/lib/Android/armeabi-v7a/libnode.so)

    else ()
        set(LIB_FILE_NAMES ${BACKEND_ROOT}/lib/Android/arm64-v8a/libnode.so)
        set(COPY_FILE_NAMES ${BACKEND_ROOT}/lib/Android/arm64-v8a/libnode.so)
        
    endif ()

elseif ( APPLE AND NOT IOS )
    if ( NOT IOS )
        if ( NOT DEFINED FOR_SILICON )
            set(LIB_FILE_NAMES ${BACKEND_ROOT}/lib/macOS/libnode.93.dylib)
            set(COPY_FILE_NAMES ${BACKEND_ROOT}/lib/macOS/libnode.93.dylib)
            
        else ()
            set(LIB_FILE_NAMES ${BACKEND_ROOT}/lib/macOS_arm64/libnode.93.dylib)
            set(COPY_FILE_NAMES ${BACKEND_ROOT}/lib/macOS_arm64/libnode.93.dylib)
            
        endif ()
    else ()
        set(COPY_FILE_NAMES ${BACKEND_ROOT}/lib/iOS/*.a)
    endif()

elseif ( UNIX )
    set(LIB_FILE_NAMES ${BACKEND_ROOT}/lib/Linux/libnode.so.93)
    set(COPY_FILE_NAMES ${BACKEND_ROOT}/lib/Linux/libnode.so.93)

endif ()


include_directories(    
)

target_compile_definitions (puerts PRIVATE WITH_NODEJS)