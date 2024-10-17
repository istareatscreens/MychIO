cmake_minimum_required(VERSION 3.10)
project(AdxHIDButtonRing)

set(CMAKE_CXX_STANDARD 17)

set(OUTPUT_DIR "${CMAKE_SOURCE_DIR}/../../Runtime/Plugins")

include_directories(${CMAKE_SOURCE_DIR}/src)
include_directories(${CMAKE_SOURCE_DIR}/library) #

set(HIDAPI_LIB "${CMAKE_SOURCE_DIR}/library/hidapi.lib")

add_library(AdxHIDButtonRing SHARED src/AdxHIDButtonRing.cpp)

target_link_libraries(AdxHIDButtonRing ${HIDAPI_LIB})

set_target_properties(AdxHIDButtonRing PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIR}"
)

add_custom_command(TARGET AdxHIDButtonRing POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E remove "${OUTPUT_DIR}/AdxHIDButtonRing.dll"
    COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:AdxHIDButtonRing>" "${OUTPUT_DIR}/AdxHIDButtonRing.dll"
    COMMAND ${CMAKE_COMMAND} -E remove_directory "${OUTPUT_DIR}/Debug"
    COMMENT "Copying AdxHIDButtonRing.dll to Project Plugins directory and replacing the existing file and removing Debug"
)