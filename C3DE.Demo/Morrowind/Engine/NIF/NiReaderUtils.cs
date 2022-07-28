using C3DE;
using Microsoft.Xna.Framework;
using System;

namespace TES3Unity.NIF
{
    public class NiReaderUtils
    {
        public static Ptr<T> ReadPtr<T>(UnityBinaryReader reader)
        {
            var ptr = new Ptr<T>();
            ptr.Deserialize(reader);

            return ptr;
        }

        public static Ref<T> ReadRef<T>(UnityBinaryReader reader)
        {
            var readRef = new Ref<T>();
            readRef.Deserialize(reader);

            return readRef;
        }

        public static Ref<T>[] ReadLengthPrefixedRefs32<T>(UnityBinaryReader reader)
        {
            var refs = new Ref<T>[reader.ReadLEUInt32()];

            for (int i = 0; i < refs.Length; i++)
            {
                refs[i] = ReadRef<T>(reader);
            }

            return refs;
        }

        public static ushort ReadFlags(UnityBinaryReader reader)
        {
            return reader.ReadLEUInt16();
        }

        public static T Read<T>(UnityBinaryReader reader)
        {
            if (typeof(T) == typeof(float))
            {
                return (T)((object)reader.ReadLESingle());
            }
            else if (typeof(T) == typeof(byte))
            {
                return (T)((object)reader.ReadByte());
            }
            else if (typeof(T) == typeof(string))
            {
                return (T)((object)reader.ReadLELength32PrefixedASCIIString());
            }
            else if (typeof(T) == typeof(Vector3))
            {
                return (T)((object)reader.ReadLEVector3());
            }
            else if (typeof(T) == typeof(Quaternion))
            {
                return (T)((object)reader.ReadLEQuaternionWFirst());
            }
            else if (typeof(T) == typeof(Color4))
            {
                var color = new Color4();
                color.Deserialize(reader);

                return (T)((object)color);
            }
            else
            {
                throw new NotImplementedException("Tried to read an unsupported type.");
            }
        }

        public static NiObject ReadNiObject(UnityBinaryReader reader)
        {
            // TODO: When loading a skeleton, a length is lesser than 0. That prevent us to load anything.
            var nodeTypeBytes = reader.ReadLELength32PrefixedBytes();

            if (StringUtils.Equals(nodeTypeBytes, "NiNode"))
            {
                var node = new NiNode();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiTriShape"))
            {
                var triShape = new NiTriShape();
                triShape.Deserialize(reader);

                return triShape;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiTexturingProperty"))
            {
                var prop = new NiTexturingProperty();
                prop.Deserialize(reader);

                return prop;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiSourceTexture"))
            {
                var srcTexture = new NiSourceTexture();
                srcTexture.Deserialize(reader);

                return srcTexture;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiMaterialProperty"))
            {
                var prop = new NiMaterialProperty();
                prop.Deserialize(reader);

                return prop;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiMaterialColorController"))
            {
                var controller = new NiMaterialColorController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiTriShapeData"))
            {
                var data = new NiTriShapeData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "RootCollisionNode"))
            {
                var node = new RootCollisionNode();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiStringExtraData"))
            {
                var data = new NiStringExtraData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiSkinInstance"))
            {
                var instance = new NiSkinInstance();
                instance.Deserialize(reader);

                return instance;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiSkinData"))
            {
                var data = new NiSkinData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiAlphaProperty"))
            {
                var prop = new NiAlphaProperty();
                prop.Deserialize(reader);

                return prop;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiZBufferProperty"))
            {
                var prop = new NiZBufferProperty();
                prop.Deserialize(reader);

                return prop;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiVertexColorProperty"))
            {
                var prop = new NiVertexColorProperty();
                prop.Deserialize(reader);

                return prop;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiBSAnimationNode"))
            {
                var node = new NiBSAnimationNode();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiBSParticleNode"))
            {
                var node = new NiBSParticleNode();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticles"))
            {
                var node = new NiParticles();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticlesData"))
            {
                var data = new NiParticlesData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiRotatingParticles"))
            {
                var node = new NiRotatingParticles();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiRotatingParticlesData"))
            {
                var data = new NiRotatingParticlesData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiAutoNormalParticles"))
            {
                var node = new NiAutoNormalParticles();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiAutoNormalParticlesData"))
            {
                var data = new NiAutoNormalParticlesData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiUVController"))
            {
                var controller = new NiUVController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiUVData"))
            {
                var data = new NiUVData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiTextureEffect"))
            {
                var effect = new NiTextureEffect();
                effect.Deserialize(reader);

                return effect;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiTextKeyExtraData"))
            {
                var data = new NiTextKeyExtraData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiVertWeightsExtraData"))
            {
                var data = new NiVertWeightsExtraData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticleSystemController"))
            {
                var controller = new NiParticleSystemController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiBSPArrayController"))
            {
                var controller = new NiBSPArrayController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiGravity"))
            {
                var obj = new NiGravity();
                obj.Deserialize(reader);

                return obj;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticleBomb"))
            {
                var modifier = new NiParticleBomb();
                modifier.Deserialize(reader);

                return modifier;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticleColorModifier"))
            {
                var modifier = new NiParticleColorModifier();
                modifier.Deserialize(reader);

                return modifier;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticleGrowFade"))
            {
                var modifier = new NiParticleGrowFade();
                modifier.Deserialize(reader);

                return modifier;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticleMeshModifier"))
            {
                var modifier = new NiParticleMeshModifier();
                modifier.Deserialize(reader);

                return modifier;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiParticleRotation"))
            {
                var modifier = new NiParticleRotation();
                modifier.Deserialize(reader);

                return modifier;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiKeyframeController"))
            {
                var controller = new NiKeyframeController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiKeyframeData"))
            {
                var data = new NiKeyframeData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiColorData"))
            {
                var data = new NiColorData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiGeomMorpherController"))
            {
                var controller = new NiGeomMorpherController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiMorphData"))
            {
                var data = new NiMorphData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "AvoidNode"))
            {
                var node = new AvoidNode();
                node.Deserialize(reader);

                return node;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiVisController"))
            {
                var controller = new NiVisController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiVisData"))
            {
                var data = new NiVisData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiAlphaController"))
            {
                var controller = new NiAlphaController();
                controller.Deserialize(reader);

                return controller;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiFloatData"))
            {
                var data = new NiFloatData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiPosData"))
            {
                var data = new NiPosData();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiBillboardNode"))
            {
                var data = new NiBillboardNode();
                data.Deserialize(reader);

                return data;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiShadeProperty"))
            {
                var property = new NiShadeProperty();
                property.Deserialize(reader);

                return property;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiWireframeProperty"))
            {
                var wireframeProperty = new NiWireframeProperty();
                wireframeProperty.Deserialize(reader);
                return wireframeProperty;
            }
            else if (StringUtils.Equals(nodeTypeBytes, "NiCamera"))
            {
                var camera = new NiCameraProperty();
                camera.Deserialize(reader);
                return camera;
            }

            Debug.Log("Tried to read an unsupported NiObject type (" + System.Text.Encoding.ASCII.GetString(nodeTypeBytes) + ").");

            return null;
        }

        public static Matrix Read3x3RotationMatrix(UnityBinaryReader reader)
        {
            return reader.ReadLERowMajorMatrix3x3();
        }
    }
}
