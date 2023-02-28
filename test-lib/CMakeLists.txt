cmake_minimum_required(VERSION 3.10)
set(CMAKE_CXX_STANDARD 17)
project(test_lib VERSION 0.1.0)


# Phoenix project pre-setup
# option(PHOENIX_BUILD_TESTS OFF)
# option(PHOENIX_BUILD_EXAMPLES OFF)
# option(PHOENIX_BUILD_TOOLS OFF)

#include_directories(phoenix/include)    # Include phoenix headers
add_subdirectory(phoenix)               # Add phoenix source folder to build process (Creating .a file)


add_executable(test_lib main.cc)
#add_library(test_lib SHARED main.cpp)


# Phoenix project post-setup
#target_link_libraries(test_lib PRIVATE phoenix) # Add compiled .a file to main project's build process

target_link_libraries(test_lib phoenix)