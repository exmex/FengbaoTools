using System;
using System.IO;
using System.Numerics;

namespace ActorConv
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var staticMesh = new StaticMesh();
            var folder = @"E:\Downloads\fengbao\res\client\Media\Models\aoshan\";
            staticMesh.Load(folder + "aoshan_jianzhu02.mesh");

            for (var index = 0; index < staticMesh.Meshes.Length; index++)
            {
                var mesh = staticMesh.Meshes[index];
                File.WriteAllText("model" + index + ".obj", staticMesh.Meshes[index].AsObj());
                if (!File.Exists("./" + staticMesh.Meshes[index].MatName))
                    File.Copy(folder + staticMesh.Meshes[index].MatName, "./" + staticMesh.Meshes[index].MatName);
            }
        }
    }
}