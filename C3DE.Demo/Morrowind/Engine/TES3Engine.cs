using C3DE;
using C3DE.Components;
using C3DE.Components.Controllers;
using C3DE.Components.Rendering;
using C3DE.Demo.Scripts.Lighting;
using C3DE.Graphics.Materials;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using TES3Unity.ESM.Records;
using TES3Unity.Rendering;

namespace TES3Unity
{
    public sealed class TES3Engine : Behaviour
    {
        // Static.
        public const string Version = "2020.1";
        public const float NormalMapGeneratorIntensity = 0.75f;
        public static int MarkerLayer => 0;
        public static int CellRadiusOnLoad = 2;
        public static bool AutoLoadSavedGame = false;
        public static bool LogEnabled = false;
        private static TES3Engine instance = null;
        public static TES3DataReader DataReader { get; set; }

        private TemporalLoadBalancer m_TemporalLoadBalancer;
        private TES3Material m_MaterialManager;
        private NIFManager m_NIFManager;

        public float ambientIntensity = 1.5f;
        public float desiredWorkTimePerFrame = 0.0005f;
        public string loadSaveGameFilename = string.Empty;

#if UNITY_EDITOR
        [Header("Editor Only")]
        public string[] AlternativeDataPaths = null;
        public int CellRadius = 1;
        public int CellDetailRadius = 1;
        public bool ForceAutoloadSavedGame = true;
        public bool OverrideLogEnabled = false;
#endif

        // Private.
        private CELLRecord m_CurrentCell;
        private Transform m_PlayerTransform;
        private Transform m_CameraTransform;
        private bool m_Initialized = false;

        // Public.
        public CellManager cellManager;
        public TextureManager textureManager;

        public CELLRecord CurrentCell
        {
            get => m_CurrentCell;
            private set
            {
                if (m_CurrentCell == value)
                {
                    return;
                }

                m_CurrentCell = value;
                CurrentCellChanged?.Invoke(m_CurrentCell);
            }
        }

        public event Action<CELLRecord> CurrentCellChanged = null;

        public override void Awake()
        {
            base.Awake();

            if (instance != null && instance != this)
            {
                //Destroy(this);
            }
            else
            {
                instance = this;
            }

            // When loaded from the Menu, this variable is already preloaded.
            if (DataReader == null)
            {
                var dataPath = GetDataPath();

                if (string.IsNullOrEmpty(dataPath))
                {
                    return;
                }

                DataReader = new TES3DataReader(dataPath);
            }
        }

        private string GetDataPath()
        {
            return @"D:\Jeux\Morrowind\Data Files";
        }

        public override void Start()
        {
            base.Start();

            CellManager.cellRadius = 0;
            CellManager.detailRadius = 0;
            CellRadiusOnLoad = 1;

            textureManager = new TextureManager(DataReader);
            m_MaterialManager = new TES3Material(textureManager, false);
            m_NIFManager = new NIFManager(DataReader, m_MaterialManager);
            m_TemporalLoadBalancer = new TemporalLoadBalancer();
            cellManager = new CellManager(DataReader, textureManager, m_NIFManager, m_TemporalLoadBalancer);

#if UNITY_STANDALONE
            if (!XRManager.IsXREnabled())
            {
                var texture = textureManager.LoadTexture("tx_cursor", true);
                Cursor.SetCursor(texture, Vector2.zero, CursorMode.Auto);
            }
#endif

            m_Initialized = true;

            // Start Position
            var cellGridCoords = new Vector2i(-2, -9);
            var cellIsInterior = false;
            var spawnPosition = new Vector3(-137.94f, 2.30f, -1037.6f);
            var spawnRotation = Quaternion.Identity;

            SpawnPlayer(cellGridCoords, cellIsInterior, spawnPosition, spawnRotation);
        }

        #region Player Spawn

        /// <summary>
        /// Spawns the player inside. Be carefull, the name of the cell is not the same for each languages.
        /// Use it with the correct name.
        /// </summary>
        /// <param name="playerPrefab">The player prefab.</param>
        /// <param name="interiorCellName">The name of the desired cell.</param>
        /// <param name="position">The target position of the player.</param>
        public void SpawnPlayerInside(string interiorCellName, Vector3 position, Quaternion rotation)
        {
            CurrentCell = DataReader.FindInteriorCellRecord(interiorCellName);

            CreatePlayer(position, rotation);

            var cellInfo = cellManager.StartCreatingInteriorCell(interiorCellName);
            m_TemporalLoadBalancer.WaitForTask(cellInfo.objectsCreationCoroutine);
        }

        public void SpawnPlayer(Vector2i gridCoords, bool outside, Vector3 position, Quaternion rotation)
        {
            InRangeCellInfo cellInfo = null;

            if (outside)
            {
                CurrentCell = DataReader.FindExteriorCellRecord(gridCoords);
                cellInfo = cellManager.StartCreatingExteriorCell(gridCoords);
            }
            else
            {
                CurrentCell = DataReader.FindInteriorCellRecord(gridCoords);
                cellInfo = cellManager.StartCreatingInteriorCell(gridCoords);
            }

            CreatePlayer(position, rotation);

            m_TemporalLoadBalancer.WaitForTask(cellInfo.objectsCreationCoroutine);
        }

        #endregion

        public override void Update()
        {
            if (!m_Initialized)
            {
                return;
            }

            // The current cell can be null if the player is outside of the defined game world.
            if ((m_CurrentCell == null) || !m_CurrentCell.isInterior)
            {
                cellManager.UpdateExteriorCells(m_CameraTransform.position);
            }

          //  m_TemporalLoadBalancer.RunTasks(desiredWorkTimePerFrame);
        }

        private void OnApplicationQuit()
        {
            DataReader?.Close();
        }

        #region Private

        /*public void OpenDoor(Door component)
        {
            if (!component.doorData.leadsToAnotherCell)
            {
                component.Interact();
            }
            else
            {
                // The door leads to another cell, so destroy all currently loaded cells.
                cellManager.DestroyAllCells();

                // Move the player.
                m_PlayerTransform.position = component.doorData.doorExitPos;
                m_PlayerTransform.LocalRotation = new Vector3(0, component.doorData.doorExitOrientation.eulerAngles.y, 0);

                // Load the new cell.
                CELLRecord newCell;

                if (component.doorData.leadsToInteriorCell)
                {
                    var cellInfo = cellManager.StartCreatingInteriorCell(component.doorData.doorExitName);
                    m_TemporalLoadBalancer.WaitForTask(cellInfo.objectsCreationCoroutine);

                    newCell = cellInfo.cellRecord;
                }
                else
                {
                    var cellIndices = cellManager.GetExteriorCellIndices(component.doorData.doorExitPos);
                    newCell = DataReader.FindExteriorCellRecord(cellIndices);

                    cellManager.UpdateExteriorCells(m_CameraTransform.position, true, CellRadiusOnLoad);
                }

                CurrentCell = newCell;
            }
        }*/

        private GameObject CreatePlayer(Vector3 position, Quaternion rotation)
        {
            var player = GameObjectFactory.CreatePlayer(1.6f);
            var controller = player.AddComponent<FirstPersonController>();
            controller.Fly = true;
            controller.MoveSpeed = 5;

            var lightSpawner = player.AddComponent<LightSpawner>();
            lightSpawner.Intensity = 5;
            lightSpawner.Radius = 100;

            m_PlayerTransform = player.transform;
            m_PlayerTransform.position = position;
            m_PlayerTransform.rotation = rotation;

            m_CameraTransform = player.GetComponentInChildren<Camera>().Transform;

            return player;
        }

        #endregion

    }
}