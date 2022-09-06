if ( MSYS OR WIN32 )
    if("${CMAKE_SIZEOF_VOID_P}" STREQUAL "4")
        set(LIB_FILE_NAMES 
            v8_9.4/Lib/Win32/wee8.lib
        )
    else ()
        set(LIB_FILE_NAMES 
            v8_9.4/Lib/Win64/wee8.lib
        )
    endif ()

elseif ( ANDROID )

    if( ANDROID_ABI STREQUAL "armeabi-v7a")
        set(LIB_FILE_NAMES 
            v8_9.4/Lib/Android/armeabi-v7a/libwee8.a
        )

    else ()
        set(LIB_FILE_NAMES 
            v8_9.4/Lib/Android/arm64-v8a/libwee8.a
        )
        
    endif ()

elseif ( APPLE AND NOT IOS )
    if ( NOT IOS )
        if ( NOT DEFINED FOR_SILICON )
            set(LIB_FILE_NAMES 
                v8_9.4/Lib/macOS/libwee8.a
            )
            
        else ()
            set(LIB_FILE_NAMES 
                v8_9.4/Lib/macOS_arm64/libwee8.a
            )
            
        endif ()
    endif()
elseif ( UNIX )
    set(LIB_FILE_NAMES 
        v8_9.4/Lib/Linux/libwee8.a
    )

endif ()

include_directories(    
    ${PROJECT_SOURCE_DIR}/v8_9.4/Inc
)