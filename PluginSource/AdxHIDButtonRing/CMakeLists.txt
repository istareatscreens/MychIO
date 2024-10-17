cmake_minimum_required(VERSION 3.10)
project(AdxIO4ButtonRing)

set(CMAKE_CXX_STANDARD 17)

set(OUTPUT_DIR "${CMAKE_SOURCE_DIR}/../../Runtime/Plugins")

include_directories(${CMAKE_SOURCE_DIR}/src)
include_directories(${CMAKE_SOURCE_DIR}/library) #

set(HIDAPI_LIB "${CMAKE_SOURCE_DIR}/library/hidapi.lib")

add_library(AdxIO4ButtonRing SHARED src/AdxIO4ButtonRing.cpp)

target_link_libraries(AdxIO4ButtonRing ${HIDAPI_LIB})

set_target_properties(AdxIO4ButtonRing PROPERTIES
    RUNTIME_OUTPUT_DIRECTORY "${OUTPUT_DIR}"
)

add_custom_command(TARGET AdxIO4ButtonRing POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E remove "${OUTPUT_DIR}/AdxIO4ButtonRing.dll"
    COMMAND ${CMAKE_COMMAND} -E copy "$<TARGET_FILE:AdxIO4ButtonRing>" "${OUTPUT_DIR}/AdxIO4ButtonRing.dll"
    COMMAND ${CMAKE_COMMAND} -E remove_directory "${OUTPUT_DIR}/Debug"
    COMMENT "Copying AdxIO4ButtonRing.dll to Project Plugins directory and replacing the existing file and removing Debug"
)