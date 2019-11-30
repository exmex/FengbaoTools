using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace ActorConv
{
    public class Mesh
    {
        public string Name;
        public string MatName;
        public bool UseNormal;
        public bool UseVertexColor;
        public bool UseLightmapUv;
        public bool UseTangentBinormal;
        public int LightmapRes;
        public long VerticesCount;
        public float[] Vertices;
        public float[] Normals;
        public float[] Uvs;
        public uint FaceCount;
        public short[] Faces;
        public Vector3 VMin;
        public Vector3 VMax;

        public void Load(BinaryReader br, bool isSkinned, int version)
        {
            var nameLen = br.ReadUInt32();
            Name = new string(br.ReadChars((int) nameLen));
            Console.WriteLine(Name);

            var matNameLen = br.ReadUInt32();
            MatName = new string(br.ReadChars((int) matNameLen));
            Console.WriteLine(MatName);

            UseNormal = br.ReadBoolean();
            Console.WriteLine("Normal: " + UseNormal);

            UseVertexColor = br.ReadBoolean();
            Console.WriteLine("VertexColor: " + UseVertexColor);

            UseLightmapUv = br.ReadBoolean();
            Console.WriteLine("LightMapUV: " + UseLightmapUv);

            UseTangentBinormal = br.ReadBoolean();
            Console.WriteLine("Tangent Binormal: " + UseTangentBinormal);

            LightmapRes = StaticMesh.Clamp(br.ReadInt32(), 8, 512);
            Console.WriteLine("Lightmap Res: " + LightmapRes);

            br.BaseStream.Seek(72, SeekOrigin.Current);

            //ch0bits + ch1bits + ch2bits + ch3bits
            var rgb32FloatBits = (32 + 32 + 32 + 0) / 8;
            var rgba8UnormBits = (8 + 8 + 8 + 8) / 8;
            var rgba8UintBits = (8 + 8 + 8 + 8) / 8;
            var rg32FloatBits = (32 + 32) / 8;
            var vertOffset = rgb32FloatBits;
            if (UseNormal)
            {
                vertOffset += rgb32FloatBits;
            }

            if (UseVertexColor)
            {
                vertOffset += rgba8UnormBits;
            }

            if (MatName[0] != 0x00)
            {
                vertOffset += rg32FloatBits;
            }

            if (UseLightmapUv)
            {
                vertOffset += rg32FloatBits;
            }

            if (isSkinned)
            {
                vertOffset += rgba8UintBits;
                vertOffset += rgb32FloatBits;
            }

            if (isSkinned && version >= 3)
            {
                var numBoneInfluenced = br.ReadUInt32();
                Console.WriteLine("Bone Influenced: " + numBoneInfluenced);
                for (int j = 0; j < numBoneInfluenced; j++)
                {
                    br.ReadUInt32();
                }
            }

            VerticesCount = br.ReadUInt32();
            var verticesStrideCount = VerticesCount * vertOffset;
            VerticesCount *= 3;
            Console.WriteLine("Vertices: " + VerticesCount);
            Vertices = new float[VerticesCount];
            var verticesStrides = new byte[verticesStrideCount];
            for (int j = 0; j < verticesStrideCount; j++)
            {
                verticesStrides[j] = br.ReadByte();
            }

            var vertexIdx = 0;
            var uvIdx = 0;
            Normals = new float[VerticesCount];
            Uvs = new float[VerticesCount];
            for (int i = 0; i < verticesStrideCount; i += vertOffset)
            {
                Vertices[vertexIdx] = BitConverter.ToSingle(verticesStrides, i);
                Vertices[vertexIdx + 1] = BitConverter.ToSingle(verticesStrides, i + 4);
                Vertices[vertexIdx + 2] = BitConverter.ToSingle(verticesStrides, i + 8);

                if (UseNormal)
                {
                    Normals[vertexIdx] = BitConverter.ToSingle(verticesStrides, i + 12);
                    Normals[vertexIdx + 1] = BitConverter.ToSingle(verticesStrides, i + 16);
                    Normals[vertexIdx + 2] = BitConverter.ToSingle(verticesStrides, i + 20);
                }

                if (UseVertexColor)
                {
                    throw new NotImplementedException();
                }

                if (MatName[0] != 0x00)
                {
                    Uvs[uvIdx] = BitConverter.ToSingle(verticesStrides, i + 24);
                    Uvs[uvIdx + 1] = BitConverter.ToSingle(verticesStrides, i + 28);
                }

                vertexIdx += 3;
                uvIdx += 2;
            }

            FaceCount = br.ReadUInt32() * 3;
            Console.WriteLine("Faces: " + FaceCount);
            Faces = new short[FaceCount];
            for (int j = 0; j < Faces.Length; j++)
            {
                Faces[j] = br.ReadInt16();
            }

            VMin = new Vector3();
            VMin.X = br.ReadSingle();
            VMin.Y = br.ReadSingle();
            VMin.Z = br.ReadSingle();

            VMax = new Vector3();
            VMax.X = br.ReadSingle();
            VMax.Y = br.ReadSingle();
            VMax.Z = br.ReadSingle();

            Console.WriteLine(VMin);
            Console.WriteLine(VMax);
        }

        public string AsObj()
        {
            var sb = new StringBuilder();
            for (var i = 0; i < VerticesCount; i += 3)
            {
                sb.Append($"v {Vertices[i]} {Vertices[i + 1]} {Vertices[i + 2]}\n");
            }

            if (UseNormal)
            {
                for (int i = 0; i < VerticesCount; i += 3)
                {
                    sb.Append($"vn {Normals[i]} {Normals[i + 1]} {Normals[i + 2]}\n");
                }
            }

            if (MatName[0] != 0x00)
            {
                for (int i = 0; i < VerticesCount; i += 2)
                {
                    if (Uvs.Length > i + 1)
                        sb.Append($"vt {Uvs[i]} {Uvs[i + 1]}\n");
                    else
                        sb.Append($"vt 0 0\n");
                }
            }

            var ik = 1;
            for (int i = 0; i < FaceCount; i += 3)
            {
                if (MatName[0] != 0x00)
                {
                    sb.Append(string.Format("f {0}/{3} {1}/{3} {2}/{3}\n",
                        Faces[i] + 1, Faces[i + 1] + 1, Faces[i + 2] + 1, ik));
                }else 
                    sb.Append($"f {Faces[i] + 1} {Faces[i + 1] + 1} {Faces[i + 2] + 1}\n");

                ik++;
            }

            return sb.ToString();
        }
    }
}