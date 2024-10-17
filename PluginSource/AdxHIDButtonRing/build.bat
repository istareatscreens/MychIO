@echo off

REM Create build directory if it doesn't exist
IF NOT EXIST "build" (
    mkdir build
)

REM Run CMake and build
cmake -S . -B build
cmake --build build