#include <iostream>
#include <phoenix/vdfs.hh>
// #include <phoenix/archive.hh>


//#define DllExport extern "C" __declspec( dllexport )

int main(int argc, char** argv) {
	// auto buf = phoenix::buffer::mmap(argv[1]);


    auto vdf = phoenix::vdf_file::open("C:\\Prfdxsogram Files (x86)\\Steam\\steamapps\\common\\Gothic\\Data\\speech_babe_speech_engl.VDF");

	return 0;
}


//int i = 0;
//DllExport int count()
//{
//    return ++i;
//}