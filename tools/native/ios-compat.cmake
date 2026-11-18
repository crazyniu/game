# Cocos Creator 3.8.8's bundled headers and archive predate Apple's new linker.
foreach(target cocos_engine ${EXECUTABLE_NAME})
    if(TARGET ${target})
        target_compile_options(${target} PRIVATE -Wno-invalid-specialization)
    endif()
endforeach()

# The iOS app icon reads InfoPlist.strings from the preferred localization.
foreach(language en zh)
    set(strings_dir "${CC_PROJECT_DIR}/${language}.lproj")
    set(bundle "$<TARGET_BUNDLE_DIR:${EXECUTABLE_NAME}>")
    add_custom_command(TARGET ${EXECUTABLE_NAME} POST_BUILD
        COMMAND "${CMAKE_COMMAND}" -E make_directory "${bundle}/${language}.lproj"
        COMMAND "${CMAKE_COMMAND}" -E copy_if_different
                "${strings_dir}/InfoPlist.strings" "${bundle}/${language}.lproj/InfoPlist.strings"
        VERBATIM)
endforeach()
