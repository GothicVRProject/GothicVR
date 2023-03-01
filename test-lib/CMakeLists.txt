cmake_minimum_required(VERSION 3.10)
set(CMAKE_CXX_STANDARD 17)
project(test_lib VERSION 0.1.0)


##
# Phoenix project pre-setup
###
option(PHOENIX_BUILD_TESTS OFF)
option(PHOENIX_BUILD_EXAMPLES OFF)
option(PHOENIX_BUILD_TOOLS OFF)
add_subdirectory(phoenix) # Add phoenix source folder to build process (Creating .a file)


add_executable(test_lib main.cc)


##
# Phoenix project post-setup
##

# We need to "bake" standard C/C++ libraries into exe/DLL
# Otherwise we need to distribute libstdc++-6.dll into the same folder.
target_link_options(test_lib PUBLIC "-static-libstdc++")

target_link_libraries(test_lib phoenix)