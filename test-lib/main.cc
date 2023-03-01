#include <iostream>
#include <phoenix/vdfs.hh>
// #include <phoenix/archive.hh>


//#define DllExport extern "C" __declspec( dllexport )

int main(int argc, char** argv) {
	// auto buf = phoenix::buffer::mmap(argv[1]);
	std::cout << "Test" << "\n";


    auto vdf = phoenix::vdf_file::open("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\Data\\speech_babe_speech_engl.VDF");
	auto& header = vdf.header;

	std::cout << "Description: " << header.comment << "\n"
	          << "Timestamp (Unix): " << header.timestamp << "\nEntries:\n";

	int n;
	std::cin >> n;

	return 0;
}


//int i = 0;
//DllExport int count()
//{
//    return ++i;
//}