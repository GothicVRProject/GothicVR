# VdfsSharp

> Credits to https://github.com/Szmyk/VdfsSharp


> Building, loading and extracting VDFS archives used by the games "Gothic" and "Gothic II"

The main aim of the project is to provide a way for the [Gothic Mod Build Tool](https://github.com/Szmyk/gmbt) project to extract and build VDFS archives programmatically instead of using external tools.

## Example

### Reading

```c#
using VdfsSharp;
using System.IO;

//...

var reader = new VdfsReader("Anims.vdf");

var entries = reader.ReadEntries(false);

var humansMdh = entries.Where(entry => entry.Name == "HUMANS.MDH").First();

var content = reader.ReadEntryContent(humansMdh);

File.WriteAllBytes(@"_Work\Anims\_Compiled\Humans.mdh", content);

```

### Extracting

```c#
using VdfsSharp;

//...

var extractor = new VdfsExtractor("Anims.vdf");

extractor.ExtractFiles("_Work\Anims", ExtractOption.Hierarchy);

````

### Building

```c#
using VdfsSharp;

//...

var writer = new VdfsWriter("Scripts.vdf", "Scripts of my mod", GothicVersion.Gothic2);

writer.AddDirectory("_Work\Scripts");

writer.Save();

```
