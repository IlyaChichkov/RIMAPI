using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using RimWorld;
using RimworldRestApi.Models;
using UnityEngine;
using Verse;

namespace RimworldRestApi.Helpers
{
    public class BuildingHelper
    {
        public BuildingDto BuildingToDto(Building building)
        {
            return new BuildingDto
            {
                Id = building.thingIDNumber,
                Def = building.def.defName,
                Label = building.Label,
                Position = new PositionDto
                {
                    X = building.Position.x,
                    Y = building.Position.y,
                    Z = building.Position.z
                },
                Type = building.GetType().Name,
            };
        }

        public TurretInfoDto GetTurretInfo(Building building)
        {
            try
            {
                var turretProps = building.def.building;
                var turretGunDef = turretProps?.turretGunDef;

                // Get turret-specific components
                var compAmmo = building.TryGetComp<CompChangeableProjectile>();
                var compRefuelable = building.TryGetComp<CompRefuelable>();
                var compPower = building.TryGetComp<CompPowerTrader>();
                var compMannable = building.TryGetComp<CompMannable>();

                var baseDto = BuildingToDto(building);
                TurretInfoDto turretInfo = new TurretInfoDto
                {
                    Id = baseDto.Id,
                    Def = baseDto.Def,
                    Label = baseDto.Label,
                    Position = baseDto.Position,
                    Type = baseDto.Type,

                    // Basic turret properties
                    TurretGunDef = turretGunDef?.defName,
                    IsMortar = turretProps?.IsMortar ?? false,
                    IsManned = compMannable?.MannedNow ?? false,

                    // Combat statistics
                    BurstCooldownTime = turretProps?.turretBurstCooldownTime ?? -1f,
                    InitialCooldownTime = turretProps?.turretInitialCooldownTime ?? 0f,
                    CombatPower = turretProps?.combatPower ?? 0f,

                    // Ammo information
                    CurrentAmmo = compAmmo?.LoadedShell?.defName,

                    // Power requirements
                    RequiresPower = compPower != null,
                    PowerConsumption = compPower?.Props.PowerConsumption ?? 0f,
                    IsPowered = compPower?.PowerOn ?? true,

                    // Fuel requirements
                    RequiresFuel = compRefuelable != null,
                    CurrentFuel = compRefuelable?.Fuel ?? 0f,
                    FuelCapacity = compRefuelable?.Props.fuelCapacity ?? 0f,
                    FuelType = compRefuelable?.Props.fuelFilter?.AnyAllowedDef?.defName,

                    // Targeting information
                    CanTargetAcquired = turretProps?.playTargetAcquiredSound ?? false,
                    IsCombatDangerous = turretProps?.ai_combatDangerous ?? false,

                    // Weapon stats from the gun definition
                    WeaponStats = GetWeaponStats(turretGunDef),

                    // Current state
                    Health = building.HitPoints,
                    MaxHealth = building.MaxHitPoints,
                    IsWorking = building.IsWorking(),
                };

                return turretInfo;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting turret info: {ex.Message}", ex);
            }
        }

        public PowerGeneratorInfoDto GetPowerGeneratorInfo(Building building)
        {
            try
            {
                CompPowerPlant powerPlant = building.TryGetComp<CompPowerPlant>();

                var baseDto = BuildingToDto(building);
                PowerGeneratorInfoDto info = new PowerGeneratorInfoDto
                {
                    Id = baseDto.Id,
                    Def = baseDto.Def,
                    Label = baseDto.Label,
                    Position = baseDto.Position,
                    Type = baseDto.Type,
                    PowerOutput = powerPlant.PowerOutput,
                    PowerOn = powerPlant.PowerOn,
                    TransmitsPower = powerPlant.Props.transmitsPower,
                    ShortCircuitInRain = powerPlant.Props.shortCircuitInRain,
                    IdlePowerDraw = powerPlant.Props.idlePowerDraw,
                };

                return info;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting power generator info: {ex.Message}", ex);
            }
        }

        private WeaponStatsDto GetWeaponStats(ThingDef gunDef)
        {
            if (gunDef == null) return null;

            var verbProps = gunDef.Verbs?.FirstOrDefault();
            if (verbProps == null) return null;

            return new WeaponStatsDto
            {
                DamageDef = verbProps.defaultProjectile?.projectile?.damageDef?.defName,
                DamageAmount = verbProps.defaultProjectile?.projectile?.GetDamageAmount(1f) ?? 0,
                ArmorPenetration = verbProps.defaultProjectile?.projectile?.GetArmorPenetration(1f) ?? 0f,
                ExplosionRadius = verbProps.defaultProjectile?.projectile?.explosionRadius ?? 0f,
                Range = verbProps.range,
                WarmupTime = verbProps.warmupTime,
                CooldownTime = verbProps.defaultCooldownTime,
                BurstShotCount = verbProps.burstShotCount,
                Accuracy = verbProps.accuracyTouch
            };
        }

        public Building FindBuildingByID(int buildingId)
        {
            // You'll need to implement this based on your architecture
            // This might search through all maps or use a building manager
            foreach (Map map in Find.Maps)
            {
                Building building = map.listerBuildings.allBuildingsColonist
                    .FirstOrDefault(b => b.thingIDNumber == buildingId);
                if (building != null)
                    return building;
            }
            return null;
        }
    }
}