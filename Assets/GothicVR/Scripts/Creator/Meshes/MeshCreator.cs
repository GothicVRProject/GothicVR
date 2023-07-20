namespace GVR.Creator.Meshes
{
    /// <summary>
    /// Default mesh creator without specific handling.
    /// (Needs to be there as AbstractMeshCreator needs to be subclassed because of class type parameter.)
    /// 
    /// If you need to add specific features feel free to think if it's related to NpcMeshCreator.cs or something similar.
    /// If it benefits all *MeshCreators, please put it into AbstractMeshCreator.cs
    /// </summary>
    public class MeshCreator : AbstractMeshCreator<MeshCreator>
    {

    }
}