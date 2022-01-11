if ( WIN32 )
    set(LIB_FILE_NAMES 
        v8qjs.lib
        quickjs.lib
    )
elseif ( ANDROID )
    set(LIB_FILE_NAMES 
        libPixUI_Unity.so
        libv8qjs.a
    )
else ()
    set(LIB_FILE_NAMES 
        libquickjs.dylib
        libv8qjs.a
    )
endif()
target_compile_definitions (puerts PRIVATE WITHOUT_INSPECTOR)
target_compile_definitions (puerts PRIVATE WITH_QUICKJS)