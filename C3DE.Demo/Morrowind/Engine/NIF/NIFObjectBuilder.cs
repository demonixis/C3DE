using System;

namespace TES3Unity
{
    using C3DE;
    using C3DE.Components.Physics;
    using C3DE.Components.Rendering;
    using C3DE.Graphics.Materials;
    using C3DE.Graphics.Primitives;
    using Microsoft.Xna.Framework;
    using NIF;
    using TES3Unity.Rendering;

    // TODO: Investigate merging meshes.
    // TODO: Investigate merging collision nodes with visual nodes.
    public sealed class NIFObjectBuilder
    {
        private NiFile _file;
        private TES3Material _materialManager;
        private bool _isStatic;

        public NIFObjectBuilder(NiFile file, TES3Material materialManager, bool isStatic)
        {
            _file = file;
            _materialManager = materialManager;
            _isStatic = isStatic;
        }

        public GameObject BuildObject()
        {
            // NIF files can have any number of root NiObjects.
            // If there is only one root, instantiate that directly.
            // If there are multiple roots, create a container GameObject and parent it to the roots.
            if (_file.footer.roots.Length == 1)
            {
                var rootNiObject = _file.blocks[_file.footer.roots[0]];

                GameObject gameObject = InstantiateRootNiObject(rootNiObject);

                // If the file doesn't contain any NiObjects we are looking for, return an empty GameObject.
                if (gameObject == null)
                {
                    Debug.Log(_file.name + " resulted in a null GameObject when instantiated.");

                    gameObject = new GameObject(_file.name);
                }
                // If gameObject != null and the root NiObject is an NiNode, discard any transformations (Morrowind apparently does).
                else if (rootNiObject is NiNode)
                {
                    gameObject.Transform.Position = Vector3.Zero;
                    gameObject.Transform.LocalRotation = Vector3.Zero;
                    gameObject.Transform.LocalScale = Vector3.One;
                }

                return gameObject;
            }
            else
            {
                Debug.Log(_file.name + " has multiple roots.");

                GameObject gameObject = new GameObject(_file.name);

                foreach (var rootRef in _file.footer.roots)
                {
                    var child = InstantiateRootNiObject(_file.blocks[rootRef]);

                    if (child != null)
                    {
                        child.Transform.SetParent(gameObject.Transform, false);
                    }
                }

                return gameObject;
            }
        }

        private GameObject InstantiateRootNiObject(NiObject obj)
        {
            var gameObject = InstantiateNiObject(obj);

            bool shouldAddMissingColliders, isMarker;
            ProcessExtraData(obj, out shouldAddMissingColliders, out isMarker);

            if ((_file.name != null) && IsMarkerFileName(_file.name))
            {
                shouldAddMissingColliders = false;
                isMarker = true;
            }

            // Add colliders to the object if it doesn't already contain one.
            if (shouldAddMissingColliders && (gameObject.GetComponentInChildren<Collider>() == null))
            {
                //GameObjectUtils.AddMissingMeshCollidersRecursively(gameObject, _isStatic);
                // FIXME
            }

            if (isMarker)
            {
                //.SetLayerRecursively(gameObject, TES3Engine.MarkerLayer);
                // FIXME
            }

            return gameObject;
        }

        private void ProcessExtraData(NiObject obj, out bool shouldAddMissingColliders, out bool isMarker)
        {
            shouldAddMissingColliders = true;
            isMarker = false;

            if (obj is NiObjectNET)
            {
                var objNET = (NiObjectNET)obj;
                var extraData = (objNET.extraData.value >= 0) ? (NiExtraData)_file.blocks[objNET.extraData.value] : null;

                while (extraData != null)
                {
                    if (extraData is NiStringExtraData)
                    {
                        var strExtraData = (NiStringExtraData)extraData;

                        if (strExtraData.str == "NCO" || strExtraData.str == "NCC")
                        {
                            shouldAddMissingColliders = false;
                        }
                        else if (strExtraData.str == "MRK")
                        {
                            shouldAddMissingColliders = false;
                            isMarker = true;
                        }
                    }

                    // Move to the next NiExtraData.
                    if (extraData.nextExtraData.value >= 0)
                    {
                        extraData = (NiExtraData)_file.blocks[extraData.nextExtraData.value];
                    }
                    else
                    {
                        extraData = null;
                    }
                }
            }
        }

        /// <summary>
        /// Creates a GameObject representation of an NiObject.
        /// </summary>
        /// <returns>Returns the created GameObject, or null if the NiObject does not need its own GameObject.</returns>
        private GameObject InstantiateNiObject(NiObject obj)
        {
            if (obj.GetType() == typeof(NiNode))
            {
                return InstantiateNiNode((NiNode)obj);
            }
            else if (obj.GetType() == typeof(NiBSAnimationNode))
            {
                return InstantiateNiNode((NiNode)obj);
            }
            else if (obj.GetType() == typeof(NiTriShape))
            {
                return InstantiateNiTriShape((NiTriShape)obj, true, false);
            }
            else if (obj.GetType() == typeof(RootCollisionNode))
            {
                return InstantiateRootCollisionNode((RootCollisionNode)obj);
            }
            else if (obj.GetType() == typeof(NiTextureEffect))
            {
                return null;
            }
            else if (obj.GetType() == typeof(NiBSAnimationNode))
            {
                return null;
            }
            else if (obj.GetType() == typeof(NiBSParticleNode))
            {
                return null;
            }
            else if (obj.GetType() == typeof(NiRotatingParticles))
            {
                return null;
            }
            else if (obj.GetType() == typeof(NiAutoNormalParticles))
            {
                return null;
            }
            else if (obj.GetType() == typeof(NiBillboardNode))
            {
                return null;
            }
            else
            {
                throw new NotImplementedException("Tried to instantiate an unsupported NiObject (" + obj.GetType().Name + ").");
            }
        }

        private GameObject InstantiateNiNode(NiNode node)
        {
            GameObject obj = new GameObject(node.name);

            foreach (var childIndex in node.children)
            {
                // NiNodes can have child references < 0 meaning null.
                if (!childIndex.isNull)
                {
                    var child = InstantiateNiObject(_file.blocks[childIndex.value]);

                    if (child != null)
                    {
                        child.Transform.SetParent(obj.Transform, false);
                    }
                }
            }

            ApplyNiAVObject(node, obj);

            return obj;
        }

        private GameObject InstantiateNiTriShape(NiTriShape triShape, bool visual, bool collidable)
        {
            var mesh = NiTriShapeDataToMesh((NiTriShapeData)_file.blocks[triShape.data.value]);
            var obj = new GameObject(triShape.name);

            if (visual)
            {
                //obj.AddComponent<MeshFilter>().sharedMesh = mesh;

                var materialProps = NiAVObjectPropertiesToMWMaterialProperties(triShape);

                var meshRenderer = obj.AddComponent<MeshRenderer>();
                meshRenderer.Mesh = mesh;
                meshRenderer.Mesh.Build();
                meshRenderer.Material = _materialManager.BuildMaterialFromProperties(materialProps);

                if (materialProps.textures.mainFilePath == null)
                {
                    meshRenderer.Enabled = false;
                }

                if (Utils.ContainsBitFlags(triShape.flags, (uint)NiAVObject.Flags.Hidden))
                {
                    meshRenderer.Enabled = false;
                }

                obj.IsStatic = _isStatic;
            }

            if (collidable)
            {
               /* if (!_isStatic)
                {
                    var collider = obj.AddComponent<BoxCollider>();
                    var rb = obj.AddComponent<Rigidbody>();
                    rb.IsKinematic = true; // FIXME GameSettings.Get().KinematicRigidbody;
                }
                else
                {
                    //var collider = obj.AddComponent<MeshCollider>();
                    //collider.sharedMesh = mesh;
                    //FIXME
                }*/
            }

            ApplyNiAVObject(triShape, obj);

            return obj;
        }

        private GameObject InstantiateRootCollisionNode(RootCollisionNode collisionNode)
        {
            GameObject obj = new GameObject("Root Collision Node");

            foreach (var childIndex in collisionNode.children)
            {
                // NiNodes can have child references < 0 meaning null.
                if (!childIndex.isNull)
                {
                    AddColliderFromNiObject(_file.blocks[childIndex.value], obj);
                }
            }

            ApplyNiAVObject(collisionNode, obj);

            return obj;
        }

        private void ApplyNiAVObject(NiAVObject anNiAVObject, GameObject obj)
        {
            obj.Transform.Position = NIFUtils.NifPointToUnityPoint(anNiAVObject.translation);
            //obj.Transform.Quaternion = NIFUtils.NifRotationMatrixToUnityQuaternion(anNiAVObject.rotation);

            anNiAVObject.rotation.Decompose(out Vector3 s, out Quaternion q, out Vector3 t);
            obj.Transform.Quaternion = q;
            obj.Transform.LocalScale = anNiAVObject.scale * Vector3.One;
        }

        private Mesh NiTriShapeDataToMesh(NiTriShapeData data)
        {
            // vertex positions
            var vertices = new Vector3[data.vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = NIFUtils.NifPointToUnityPoint(data.vertices[i]);
            }

            // vertex normals
            Vector3[] normals = null;
            if (data.hasNormals)
            {
                normals = new Vector3[vertices.Length];

                for (int i = 0; i < normals.Length; i++)
                {
                    normals[i] = NIFUtils.NifVectorToUnityVector(data.normals[i]);
                }
            }

            // vertex UV coordinates
            Vector2[] UVs = null;
            if (data.hasUV)
            {
                UVs = new Vector2[vertices.Length];

                for (int i = 0; i < UVs.Length; i++)
                {
                    var NiTexCoord = data.UVSets[0, i];

                    UVs[i] = new Vector2(NiTexCoord.u, NiTexCoord.v);
                }
            }

            // triangle vertex indices
            var triangles = new ushort[data.numTrianglePoints];
            for (int i = 0; i < data.triangles.Length; i++)
            {
                int baseI = 3 * i;

                // Reverse triangle winding order.
                triangles[baseI] = data.triangles[i].v1;
                triangles[baseI + 1] = data.triangles[i].v3;
                triangles[baseI + 2] = data.triangles[i].v2;
            }

            // Create the mesh.
            Mesh mesh = new Mesh();
            mesh.Create(vertices, normals, UVs, triangles);
           
            //if (!data.hasNormals)
            {
                mesh.ComputeNormals();
            }

            return mesh;
        }

        private TES3MaterialProps NiAVObjectPropertiesToMWMaterialProperties(NiAVObject obj)
        {
            // Find relevant properties.
            NiTexturingProperty texturingProperty = null;
            NiMaterialProperty materialProperty = null;
            NiAlphaProperty alphaProperty = null;

            // Create the material properties.
            TES3MaterialProps mp = new TES3MaterialProps();

            foreach (var propRef in obj.properties)
            {
                var prop = _file.blocks[propRef.value];

                if (prop is NiTexturingProperty)
                {
                    texturingProperty = (NiTexturingProperty)prop;
                }
                else if (prop is NiMaterialProperty)
                {
                    materialProperty = (NiMaterialProperty)prop;
                }
                else if (prop is NiAlphaProperty)
                {
                    alphaProperty = (NiAlphaProperty)prop;
                }
            }

            #region AlphaProperty Cheat Sheet
            /*
			14 bits used:

			1 bit for alpha blend bool
			4 bits for src blend mode
			4 bits for dest blend mode
			1 bit for alpha test bool
			3 bits for alpha test mode
			1 bit for zwrite bool ( opposite value )

			Bit 0 : alpha blending enable
            Bits 1-4 : source blend mode 
            Bits 5-8 : destination blend mode
            Bit 9 : alpha test enable
            Bit 10-12 : alpha test mode
            Bit 13 : no sorter flag ( disables triangle sorting ) ( Unity ZWrite )

			blend modes (glBlendFunc):
            0000 GL_ONE
            0001 GL_ZERO
            0010 GL_SRC_COLOR
            0011 GL_ONE_MINUS_SRC_COLOR
            0100 GL_DST_COLOR
            0101 GL_ONE_MINUS_DST_COLOR
            0110 GL_SRC_ALPHA
            0111 GL_ONE_MINUS_SRC_ALPHA
            1000 GL_DST_ALPHA
            1001 GL_ONE_MINUS_DST_ALPHA
            1010 GL_SRC_ALPHA_SATURATE

            test modes (glAlphaFunc):
            000 GL_ALWAYS
            001 GL_LESS
            010 GL_EQUAL
            011 GL_LEQUAL
            100 GL_GREATER
            101 GL_NOTEQUAL
            110 GL_GEQUAL
            111 GL_NEVER
			*/
            #endregion

            if (alphaProperty != null)
            {
                ushort flags = alphaProperty.flags;
                ushort oldflags = flags;
                byte srcbm = (byte)(BitConverter.GetBytes(flags >> 1)[0] & 15);
                byte dstbm = (byte)(BitConverter.GetBytes(flags >> 5)[0] & 15);

                mp.zWrite = BitConverter.GetBytes(flags >> 15)[0] == 1;//smush
                mp.alphaBlended = Utils.ContainsBitFlags(flags, 0x01);
                mp.srcBlendMode = FigureBlendMode(srcbm);
                mp.dstBlendMode = FigureBlendMode(dstbm);
                mp.alphaTest = Utils.ContainsBitFlags(flags, 0x100);
                mp.alphaCutoff = (float)alphaProperty.threshold / 255;
            }
            else
            {
                mp.alphaBlended = false;
                mp.alphaTest = false;
            }

            if (materialProperty != null)
            {
                mp.alpha = materialProperty.alpha;
                mp.diffuseColor = materialProperty.diffuseColor.ToColor();
                mp.emissiveColor = materialProperty.emissiveColor.ToColor();
                mp.specularColor = materialProperty.specularColor.ToColor();
                mp.glossiness = materialProperty.glossiness;
            }

            // Apply textures.
            if (texturingProperty != null)
            {
                mp.textures = ConfigureTextureProperties(texturingProperty);
            }

            return mp;
        }

        private TES3MaterialTextures ConfigureTextureProperties(NiTexturingProperty ntp)
        {
            TES3MaterialTextures tp = new TES3MaterialTextures();
            if (ntp.textureCount < 1)
            {
                return tp;
            }

            if (ntp.hasBaseTexture)
            {
                NiSourceTexture src = (NiSourceTexture)_file.blocks[ntp.baseTexture.source.value];
                tp.mainFilePath = src.fileName;
            }
            if (ntp.hasDarkTexture)
            {
                NiSourceTexture src = (NiSourceTexture)_file.blocks[ntp.darkTexture.source.value];
                tp.darkFilePath = src.fileName;
            }
            if (ntp.hasDetailTexture)
            {
                NiSourceTexture src = (NiSourceTexture)_file.blocks[ntp.detailTexture.source.value];
                tp.detailFilePath = src.fileName;
            }
            if (ntp.hasGlossTexture)
            {
                NiSourceTexture src = (NiSourceTexture)_file.blocks[ntp.glossTexture.source.value];
                tp.glossFilePath = src.fileName;
            }
            if (ntp.hasGlowTexture)
            {
                NiSourceTexture src = (NiSourceTexture)_file.blocks[ntp.glowTexture.source.value];
                tp.glowFilePath = src.fileName;
            }
            if (ntp.hasBumpMapTexture)
            {
                NiSourceTexture src = (NiSourceTexture)_file.blocks[ntp.bumpMapTexture.source.value];
                tp.bumpFilePath = src.fileName;
            }
            return tp;
        }

        private byte FigureBlendMode(byte b)
        {
            return Math.Min(b, (byte)10);
        }

        private MatTestMode FigureTestMode(byte b)
        {
            return (MatTestMode)Math.Min(b, (byte)7);
        }

        private void AddColliderFromNiObject(NiObject anNiObject, GameObject gameObject)
        {
            if (anNiObject.GetType() == typeof(NiTriShape))
            {
                var colliderObj = InstantiateNiTriShape((NiTriShape)anNiObject, false, true);
                colliderObj.Transform.SetParent(gameObject.Transform, false);
            }
            else if (anNiObject.GetType() == typeof(AvoidNode)) { }
            else
            {
                Debug.Log("Unsupported collider NiObject: " + anNiObject.GetType().Name);
            }
        }

        private bool IsMarkerFileName(string name)
        {
            var lowerName = name.ToLower();

            return lowerName == "marker_light" ||
                    lowerName == "marker_north" ||
                    lowerName == "marker_error" ||
                    lowerName == "marker_arrow" ||
                    lowerName == "editormarker" ||
                    lowerName == "marker_creature" ||
                    lowerName == "marker_travel" ||
                    lowerName == "marker_temple" ||
                    lowerName == "marker_prison" ||
                    lowerName == "marker_radius" ||
                    lowerName == "marker_divine" ||
                    lowerName == "editormarker_box_01";
        }
    }
}