if ( WIN32 )
    set(LIB_FILE_NAMES 
        v8qjs.lib
        quickjs.lib
        PxKit.lib
    )
elseif ( ANDROID )
    set(LIB_FILE_NAMES 
        libPxKit.so
        libv8qjs.a
    )
elseif (APPLE)
    if (IOS)
        set(LIB_FILE_NAMES 
            libv8qjs.a
        )
    elseif (FOR_SILICON)
        message(SEND_ERROR "not supported")
    else ()
        set(LIB_FILE_NAMES 
            libPxKit.dylib
            libv8qjs.a
        )
    endif ()
elseif (UNIX)
    message(SEND_ERROR "not supported")
endif()
target_compile_definitions (puerts PRIVATE WITHOUT_INSPECTOR)
target_compile_definitions (puerts PRIVATE WITH_QUICKJS)