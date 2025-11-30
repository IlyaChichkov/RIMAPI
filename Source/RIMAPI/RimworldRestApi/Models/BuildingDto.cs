using System;
using System.Collections.Generic;
using RIMAPI.Core;

namespace RIMAPI.Models
{
    [ModelDescription("Standard API response wrapper")]
    public class ApiResponse<T>
    {
        [ParameterDescription("Indicates if the request was successful")]
        public bool Success { get; set; }

        [ParameterDescription("The actual response data")]
        public T Data { get; set; }

        [ParameterDescription("Any warnings that occurred during processing")]
        public string[] Warnings { get; set; } = Array.Empty<string>();

        [ParameterDescription("Error message if success is false")]
        public string Error { get; set; }
    }

    public class BuildingDto
    {
        public int Id { get; set; }
        public string Def { get; set; }
        public string Label { get; set; }
        public PositionDto Position { get; set; }
        public string Type { get; set; }
    }

    public class PowerGeneratorInfoDto : BuildingDto
    {
        public float PowerOutput { get; set; }
        public bool PowerOn { get; set; }
        public bool TransmitsPower { get; set; }
        public bool ShortCircuitInRain { get; set; }
        public float IdlePowerDraw { get; set; }
    }

    public class TurretInfoDto : BuildingDto
    {
        // Turret Properties
        public string TurretGunDef { get; set; }
        public bool IsMortar { get; set; }
        public bool IsManned { get; set; }

        // Combat Stats
        public FloatRange BurstWarmupTime { get; set; }
        public float BurstCooldownTime { get; set; }
        public float InitialCooldownTime { get; set; }
        public float CombatPower { get; set; }

        // Ammo
        public string CurrentAmmo { get; set; }
        public List<string> AvailableAmmoTypes { get; set; }
        public int MaxAmmoCapacity { get; set; }

        // Power
        public bool RequiresPower { get; set; }
        public float PowerConsumption { get; set; }
        public bool IsPowered { get; set; }

        // Fuel
        public bool RequiresFuel { get; set; }
        public float CurrentFuel { get; set; }
        public float FuelCapacity { get; set; }
        public string FuelType { get; set; }

        // Targeting
        public bool CanTargetAcquired { get; set; }
        public bool IsCombatDangerous { get; set; }

        // Weapon Stats
        public WeaponStatsDto WeaponStats { get; set; }

        // State
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsWorking { get; set; }
    }

    public class WeaponStatsDto
    {
        public string DamageDef { get; set; }
        public int DamageAmount { get; set; }
        public float ArmorPenetration { get; set; }
        public float ExplosionRadius { get; set; }
        public float Range { get; set; }
        public float WarmupTime { get; set; }
        public float CooldownTime { get; set; }
        public int BurstShotCount { get; set; }
        public float Accuracy { get; set; }
    }

    public class FloatRange
    {
        public float Min { get; set; }
        public float Max { get; set; }

        public FloatRange(float min, float max)
        {
            Min = min;
            Max = max;
        }
    }
}
