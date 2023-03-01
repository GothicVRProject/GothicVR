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

add_library(test_lib SHARED main.cc)
#add_executable(test_lib main.cc) # uncomment for quick tests within main() function


# We need to "bake" standard C/C++ libraries into exe/DLL
# Otherwise we need to distribute libstdc++-6.dll into the same folder.
# If we don't do any of this, we'll get the following error:
# -> The procedure entry point _ZNKSt7codecvtlwc9_MbstatetE10do_unshiftERS0_PcS3_RS3_ could not be located in the dynamic link library [...]\build\test_lib.exe
target_link_options(test_lib PUBLIC "-static-libstdc++")


##
# Phoenix project post-setup
##
target_link_libraries(test_lib phoenix)
