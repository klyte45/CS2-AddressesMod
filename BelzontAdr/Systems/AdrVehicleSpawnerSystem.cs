using Belzont.Interfaces;
using Belzont.Utils;
using Colossal.Entities;
using Game;
using Game.Common;
using Game.Tools;
using System;
using Unity.Collections;
using Unity.Entities;

namespace BelzontAdr
{
    /// <summary>
    /// Controller system for managing ADRVehicleSpawnerData components on entities.
    /// This system provides a safe interface for editing vehicle spawner data fields,
    /// following the architectural pattern of AdrMainSystem and AdrHighwayRoutesSystem.
    /// 
    /// Key Features:
    /// - Entity-based parameter system for safe component access
    /// - Cohtml-safe data structures to avoid serialization issues
    /// - Comprehensive validation and error handling
    /// - Event notification system for UI updates
    /// - Logging support for debugging and monitoring
    /// 
    /// The system focuses primarily on editing the customId field while providing
    /// read-only access to other fields through the SpawnerDataSafe structure.
    /// </summary>
    public partial class AdrVehicleSpawnerSystem : GameSystemBase, IBelzontBindable
    {
        #region Endpoints general
        private const string PREFIX = "vehicleSpawner.";
        private EntityQuery m_vehicleToMarkDirtyQuery;

        private Action<string, object[]> EventCaller { get; set; }

        public void SetupCallBinder(Action<string, Delegate> callBinder)
        {
            callBinder($"{PREFIX}getSpawnerData", (Entity entity) => GetSpawnerDataSafe(entity));
            callBinder($"{PREFIX}setCustomId", (Entity entity, string customId) => SetCustomId(entity, customId));
        }

        public virtual void SetupEventBinder(Action<string, Delegate> eventBinder)
        {
        }

        public void SetupCaller(Action<string, object[]> eventCaller)
        {
            EventCaller = eventCaller;
        }

        #endregion

        #region Core Functionality

        /// <summary>
        /// Gets the spawner data in a cohtml-safe format
        /// </summary>
        /// <param name="entity">The entity with ADRVehicleSpawnerData component</param>
        /// <returns>Safe struct containing all spawner data</returns>
        private SpawnerDataSafe GetSpawnerDataSafe(Entity entity)
        {
            if (!EntityManager.TryGetComponent<ADRVehicleBuildingOrigin>(entity, out var spawnerData))
            {
                return default;
            }

            var safeData = new SpawnerDataSafe
            {
                sourceKind = spawnerData.kind,
                categorySerialNumber = spawnerData.CategorySerialNumber,
                categorySerialNumberSet = spawnerData.CategorySerialNumberSet,
                customId = spawnerData.customId.ToString(),
                totalVehiclesSpawned = spawnerData.InternalSerialCounter
            };
            return safeData;
        }

        /// <summary>
        /// Sets the custom ID for a vehicle spawner
        /// </summary>
        /// <param name="entity">The entity to modify</param>
        /// <param name="customId">The new custom ID</param>
        /// <returns>True if successful, false otherwise</returns>
        private string SetCustomId(Entity entity, string customId)
        {
            ADRVehicleBuildingOrigin spawnerData = default;
            try
            {
                if (!EntityManager.TryGetComponent(entity, out spawnerData))
                {
                    if (BasicIMod.VerboseMode)
                        LogUtils.DoVerboseLog($"Entity {entity} does not have ADRVehicleSpawnerData component");
                    return string.Empty;
                }

                customId = customId.Trim();

                // Validate custom ID
                if (!ValidateCustomId(customId))
                {
                    LogUtils.DoWarnLog($"Invalid custom ID '{customId}' provided for entity {entity}");
                    return spawnerData.customId.ToString();
                }

                // Validate custom ID length (FixedString32Bytes limitation)
                if (!string.IsNullOrEmpty(customId) && customId.Length > 32)
                {
                    LogUtils.DoWarnLog($"Custom ID '{customId}' is too long (max 32 characters). Truncating.");
                    customId = customId[..32];
                }
                if (spawnerData.customId != new FixedString32Bytes(customId ?? string.Empty))
                {
                    spawnerData.customId = new FixedString32Bytes(customId ?? string.Empty);
                    EntityManager.SetComponentData(entity, spawnerData);
                    EntityManager.AddComponent<ADRVehicleSerialDataDirty>(m_vehicleToMarkDirtyQuery);
                    EntityManager.AddComponent<ADRVehiclePlateDataDirty>(m_vehicleToMarkDirtyQuery);
                }
                if (BasicIMod.VerboseMode)
                    LogUtils.DoVerboseLog($"Set custom ID '{customId}' for spawner entity {entity}");

                return spawnerData.customId.ToString();
            }
            catch (Exception ex)
            {
                LogUtils.DoErrorLog($"Error setting custom ID for entity {entity}: {ex}");
                return spawnerData.customId.ToString();
            }
        }

        /// <summary>
        /// Validates a custom ID string
        /// </summary>
        /// <param name="customId">The custom ID to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateCustomId(string customId)
        {
            if (string.IsNullOrEmpty(customId))
            {
                return true; // Empty custom ID is valid
            }

            // Check length constraint (FixedString32Bytes limitation)
            if (customId.Length > 32)
            {
                return false;
            }

            // Check for invalid characters that might cause issues
            foreach (char c in customId)
            {
                if (char.IsControl(c))
                {
                    return false; // Control characters
                }
            }

            return true;
        }

        #endregion

        #region System Lifecycle
        protected override void OnCreate()
        {
            base.OnCreate();
            m_vehicleToMarkDirtyQuery = GetEntityQuery(
                        new[]
                        {
                            new EntityQueryDesc
                            {
                                All = new ComponentType[]
                                {
                                    typeof(ADRVehicleData),
                                },
                                None = new ComponentType[]
                                {
                                    typeof(Temp),
                                    typeof(Deleted)
                                }
                            }
                        });
        }
        protected override void OnUpdate()
        {
            // This system doesn't need regular updates, it's purely reactive
        }

        #endregion

        #region Safe Struct for Cohtml

        /// <summary>
        /// Cohtml-safe structure containing vehicle spawner data
        /// This struct avoids reference types and complex structures that could cause issues with cohtml
        /// Follows the pattern established by VehiclePlateSettings.SafeStruct
        /// </summary>
        public struct SpawnerDataSafe
        {
            /// <summary>
            /// The vehicle source kind name as string for UI display
            /// </summary>
            public ADRVehicleBuildingOrigin.VehicleSourceKind sourceKind;

            /// <summary>
            /// The category serial number assigned to this spawner
            /// </summary>
            public int categorySerialNumber;

            /// <summary>
            /// Whether the category serial number has been set
            /// </summary>
            public bool categorySerialNumberSet;

            /// <summary>
            /// The custom identifier for this spawner
            /// </summary>
            public string customId;

            public long totalVehiclesSpawned;

            public override readonly string ToString()
            {
                return $"SpawnerDataSafe{{sourceKind={sourceKind}, categorySerialNumber={categorySerialNumber}, categorySerialNumberSet={categorySerialNumberSet}, customId='{customId}', totalVehiclesSpawned={totalVehiclesSpawned}}}";
            }
        }

        #endregion
    }
}