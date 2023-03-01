#define DllExport extern "C" __declspec( dllexport )


#include <iostream>
#include <strings.h>
#include <phoenix/vdfs.hh>

using namespace phoenix;


DllExport vdf_header* getVDFHeader()
{
	auto vdf = vdf_file::open("C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\Data\\speech_babe_speech_engl.VDF");

	auto header = new vdf_header;
	header->comment = vdf.header.comment;
	header->entry_count = vdf.header.entry_count;
	header->file_count = vdf.header.file_count;
	header->signature = vdf.header.signature;
	header->size = vdf.header.size;
	header->timestamp = vdf.header.timestamp;
	header->version = vdf.header.version;

	return header;
}

DllExport const char* getHeaderComment(vdf_header* header) {
	return header->comment.c_str();
}


int main(int argc, char** argv) {
	// auto buf = phoenix::buffer::mmap(argv[1]);
//	std::cout << ":Test: " << outComment << std::endl;

	int a;
	std::cin >> a;

	return 0;
}
