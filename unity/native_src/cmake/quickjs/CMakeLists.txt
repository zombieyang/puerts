if ( MSYS OR WIN32 )
    if("${CMAKE_SIZEOF_VOID_P}" STREQUAL "4")
        set(LIB_FILE_NAMES 
            ${BACKEND_ROOT}/Lib/Win32/quickjs.dll.lib
        )
    else ()
        set(LIB_FILE_NAMES 
            ${BACKEND_ROOT}/Lib/Win64/quickjs.dll.lib
        )
    endif ()

elseif ( ANDROID )

    if( ANDROID_ABI STREQUAL "armeabi-v7a")
        set(LIB_FILE_NAMES 
            ${BACKEND_ROOT}/Lib/Android/armeabi-v7a/libquickjs.a
        )

    else ()
        set(LIB_FILE_NAMES 
            ${BACKEND_ROOT}/Lib/Android/arm64-v8a/libquickjs.a
        )
        
    endif ()

elseif ( APPLE AND NOT IOS )
    if ( NOT IOS )
        if ( NOT DEFINED FOR_SILICON )
            set(LIB_FILE_NAMES 
                ${BACKEND_ROOT}/Lib/macOS/libquickjs.a
            )
            
        else ()
            set(LIB_FILE_NAMES 
                ${BACKEND_ROOT}/Lib/macOS_arm64/libquickjs.a
            )
            
        endif ()
    else ()
        add_custom_command(TARGET puerts POST_BUILD COMMAND "cp" "${PROJECT_SOURCE_DIR}/${BACKEND_ROOT}/Lib/iOS/arm64/*.a" "${CMAKE_BINARY_DIR}/")
    endif()
    
elseif ( UNIX )
    set(LIB_FILE_NAMES 
        ${BACKEND_ROOT}/Lib/Linux/libquickjs.a
    )

endif ()

include_directories(    
    ${PROJECT_SOURCE_DIR}/quickjs/Inc
)

target_compile_definitions (puerts PRIVATE WITHOUT_INSPECTOR)
target_compile_definitions (puerts PRIVATE WITH_QUICKJS)