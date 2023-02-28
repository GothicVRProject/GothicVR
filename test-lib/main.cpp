#include <iostream>

#define DllExport extern "C" __declspec( dllexport )


int main(int argc, char *argv[])
{
   std::cout << "Hello CMake!" << std::endl;
   return 0;
}


int i = 0;

DllExport int count()
{
    return ++i;
}