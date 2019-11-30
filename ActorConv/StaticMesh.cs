using System;
using System.IO;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace ActorConv
{
    public class StaticMesh
    {
        private int _version;
        private int _subMeshNum;

        public Mesh[] Meshes;

        public void Load(string path)
        {
            var isSkinned = false;
            using (var fs = File.OpenRead(path))
            {
                using (var br = new BinaryReader(fs))
                {
                    _version = br.ReadInt32();
                    Console.WriteLine(_version);
                    if (_version == 1)
                    {
                    }
                    else if (_version >= 2)
                    {
                        _subMeshNum = br.ReadInt32();
                        Meshes = new Mesh[_subMeshNum];
                        br.BaseStream.Seek(72, SeekOrigin.Current);

                        for (int i = 0; i < _subMeshNum; i++)
                        {
                            Meshes[i] = new Mesh();
                            Meshes[i].Load(br, isSkinned, _version);
                        }
                    }
                }
            }
        }

        public static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }
    }
}