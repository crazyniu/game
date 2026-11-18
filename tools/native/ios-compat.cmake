# Cocos Creator 3.8.8's bundled headers and archive predate Apple's new linker.
foreach(target cocos_engine ${EXECUTABLE_NAME})
    if(TARGET ${target})
        target_compile_options(${target} PRIVATE -Wno-invalid-specialization)
    endif()
endforeach()
