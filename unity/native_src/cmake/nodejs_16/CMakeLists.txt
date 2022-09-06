if ( WIN32 AND NOT MSYS )
    set(LIB_FILE_NAMES 
        libnode.lib
    )
elseif ( APPLE )
    if (NOT IOS) 
        set(LIB_FILE_NAMES 
            libnode.93.dylib
        )
    endif ()
elseif ( ANDROID ) 
    set(LIB_FILE_NAMES
        libnode.so
    )
else ( UNIX )
    set(LIB_FILE_NAMES
        libnode.so.93
    )
endif()

include_directories(    
    ${PROJECT_SOURCE_DIR}/${JS_ENGINE}/include
    ${PROJECT_SOURCE_DIR}/${JS_ENGINE}/deps/v8/include
    ${PROJECT_SOURCE_DIR}/${JS_ENGINE}/deps/uv/include
)

target_compile_definitions (puerts PRIVATE WITH_NODEJS)