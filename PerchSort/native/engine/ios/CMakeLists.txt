cmake_minimum_required(VERSION 3.8)

set(CMAKE_SYSTEM_NAME iOS)
set(APP_NAME "PerchSort"  CACHE STRING "Project Name")

project(${APP_NAME} CXX)

set(CC_PROJECT_DIR ${CMAKE_CURRENT_LIST_DIR})
set(CC_UI_RESOURCES)
set(CC_PROJ_SOURCES)
set(CC_ASSET_FILES)
set(CC_COMMON_SOURCES)
set(CC_ALL_SOURCES)

include(${CC_PROJECT_DIR}/../common/CMakeLists.txt)
set(EXECUTABLE_NAME ${APP_NAME}-mobile)

# Our native code: the script bridge
list(APPEND CC_PROJ_SOURCES
    ${CC_PROJECT_DIR}/SceneDelegate.h
    ${CC_PROJECT_DIR}/SceneDelegate.mm
    ${CC_PROJECT_DIR}/game/GameBridge.h
    ${CC_PROJECT_DIR}/game/GameBridge.mm
)

cc_ios_before_target(${EXECUTABLE_NAME})
add_executable(${EXECUTABLE_NAME} ${CC_ALL_SOURCES})
cc_ios_after_target(${EXECUTABLE_NAME})

# App names per language (<language>.lproj/InfoPlist.strings, written by tools/native/apply.mjs from game.json).
# Cocos' template only bundles Base.lproj's launch screen and Localizable.strings; listing them as resources flattens
# them into the bundle root where same-named files overwrite each other, so they are copied by folder after the build
# (before signing). apply.mjs writes GAME_LPROJ.
set(GAME_LPROJ Base en zh-Hans)
foreach(d ${GAME_LPROJ})
    add_custom_command(TARGET ${EXECUTABLE_NAME} POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E make_directory "$<TARGET_BUNDLE_DIR:${EXECUTABLE_NAME}>/${d}.lproj"
        COMMAND ${CMAKE_COMMAND} -E copy "${CC_PROJECT_DIR}/${d}.lproj/InfoPlist.strings" "$<TARGET_BUNDLE_DIR:${EXECUTABLE_NAME}>/${d}.lproj/InfoPlist.strings")
endforeach()
# Privacy manifest (App Store: reasons for UserDefaults, file timestamp and boot time APIs), copied into the .app root after the build
add_custom_command(TARGET ${EXECUTABLE_NAME} POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy "${CC_PROJECT_DIR}/PrivacyInfo.xcprivacy" "$<TARGET_BUNDLE_DIR:${EXECUTABLE_NAME}>/PrivacyInfo.xcprivacy")

# Xcode 27's libc++ forbids specializing std::is_floating_point (the engine's enoki/half.h does); downgrade that error
foreach(t cocos_engine ${EXECUTABLE_NAME})
    if(TARGET ${t})
        target_compile_options(${t} PRIVATE -Wno-invalid-specialization)
    endif()
endforeach()

# Cocos 3.8.8's prebuilt libwebp.a members are not 8-byte aligned; Xcode 27's new linker drops them and fails.
# ./package.sh repacks an aligned copy with Xcode's libtool into sdk/libwebp/ (not in Git); the engine's webp target uses it.
if(TARGET webp AND EXISTS ${CC_PROJECT_DIR}/sdk/libwebp/libwebp.a)
    set_target_properties(webp PROPERTIES IMPORTED_LOCATION ${CC_PROJECT_DIR}/sdk/libwebp/libwebp.a)
endif()

# iPhone only; iPad runs it at iPhone proportions
set_target_properties(${EXECUTABLE_NAME} PROPERTIES XCODE_ATTRIBUTE_TARGETED_DEVICE_FAMILY "1")

# Google Mobile Ads + UMP: ./package.sh downloads them into sdk/ (not in Git). CMake 3.24 cannot link an xcframework,
# so device and simulator builds point at their own slices. Without the SDK or with GAME_ADS OFF nothing is linked and
# the bridge compiles without ads. tools/native/apply.mjs writes GAME_ADS from game.json ads.enabled.
set(GAME_ADS ON)
set(GAME_SDK_DIR ${CC_PROJECT_DIR}/sdk)
if(GAME_ADS AND EXISTS ${GAME_SDK_DIR}/GoogleMobileAds.xcframework)
    set(GAME_DEVICE_PATHS "")
    set(GAME_SIM_PATHS "")
    foreach(fw GoogleMobileAds UserMessagingPlatform)
        file(GLOB GAME_DEVICE_SLICE LIST_DIRECTORIES true "${GAME_SDK_DIR}/${fw}.xcframework/ios-arm64")
        file(GLOB GAME_SIM_SLICE LIST_DIRECTORIES true "${GAME_SDK_DIR}/${fw}.xcframework/ios-*simulator")
        list(APPEND GAME_DEVICE_PATHS ${GAME_DEVICE_SLICE})
        list(APPEND GAME_SIM_PATHS ${GAME_SIM_SLICE})
    endforeach()
    string(REPLACE ";" " " GAME_DEVICE_PATHS "${GAME_DEVICE_PATHS}")
    string(REPLACE ";" " " GAME_SIM_PATHS "${GAME_SIM_PATHS}")
    set_target_properties(${EXECUTABLE_NAME} PROPERTIES
        XCODE_ATTRIBUTE_FRAMEWORK_SEARCH_PATHS[sdk=iphoneos*] "$(inherited) ${GAME_DEVICE_PATHS}"
        XCODE_ATTRIBUTE_FRAMEWORK_SEARCH_PATHS[sdk=iphonesimulator*] "$(inherited) ${GAME_SIM_PATHS}"
    )
    # The ad SDK contains Swift; with no Swift sources Xcode does not link the Swift runtime, so add its search path and
    # the three compatibility libraries (CMake 3.24 cannot enable_language(Swift) under Xcode 27)
    set_target_properties(${EXECUTABLE_NAME} PROPERTIES
        XCODE_ATTRIBUTE_LIBRARY_SEARCH_PATHS "$(inherited) $(SDKROOT)/usr/lib/swift $(TOOLCHAIN_DIR)/usr/lib/swift/$(PLATFORM_NAME)"
        XCODE_ATTRIBUTE_LD_RUNPATH_SEARCH_PATHS "$(inherited) /usr/lib/swift @executable_path/Frameworks"
    )
    target_link_libraries(${EXECUTABLE_NAME}
        "-lswiftCompatibility51"
        "-lswiftCompatibility56"
        "-lswiftCompatibilityConcurrency"
        "-ObjC"
        "-framework GoogleMobileAds"
        "-framework UserMessagingPlatform"
        "-framework AppTrackingTransparency"
        "-framework JavaScriptCore"
        "-framework StoreKit"
        "-framework WebKit"
    )
endif()
