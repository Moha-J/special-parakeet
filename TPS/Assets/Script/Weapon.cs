using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon
{
    static bool isSet;
    string weaponName;
    public string GetweaponName()
    {
        return weaponName;
    }
    string weaponCategory;
    public string GetWeaponCategory()
    {
        return weaponCategory;
    }
    float cycleTime;
    public float GetCycleTime()
    {
        return cycleTime;
    }
    int damege;
    public int GetDamege()
    {
        return damege;
    }
    int maxPlayerSpeed;
    public int GetMaxPlayerSpeed()
    {
        return maxPlayerSpeed;
    }
    int primaryClipSize;
    public int GetPrimaryClipSize()
    {
        return primaryClipSize;
    }
    float speed;
    public float GetSpeed()
    {
        return speed;
    }
    float spread;
    public float GetSpread()
    {
        return spread;
    }
    float inaccuracyStand;
    public float GetInaccuracyStand()
    {
        return inaccuracyStand;
    }
    float inaccuracyAlt;
    public float GetInaccuracyAlt()
    {
        return inaccuracyAlt;
    }
    int recoilSeed;
    public int GetRecoilSeed()
    {
        return recoilSeed;
    }
    float recoilAngle;
    public float GetRecoilAngle()
    {
        return recoilAngle;
    }
    float recoilMagnitudeVariance;
    public float GetRecoilMagnitudeVariance()
    {
        return recoilMagnitudeVariance;
    }
    GameObject audioRes;
    public GameObject GetAudioRes()
    {
        return audioRes;
    }
    static void SetWeapon()
    {
        Weapon ak47 = new Weapon();
        ak47.weaponName = "ak47";
        ak47.weaponCategory = "auto rifle";
        ak47.cycleTime = 0.1f;
        ak47.damege = 28;
        ak47.maxPlayerSpeed = 575;
        ak47.primaryClipSize = 30;
        ak47.speed = 715f;
        ak47.recoilSeed = 24365;
        ak47.recoilAngle = 3f;
        ak47.recoilMagnitudeVariance = 3f;
        ak47.spread = 0.4f;
        ak47.inaccuracyStand = 2.1f;
        ak47.inaccuracyAlt = 2.1f;
        ak47.audioRes = ResManager.LoadPrefab("ak47");
        weapon.Add(1, ak47);

        Weapon m4a1 = new Weapon();
        m4a1.weaponName = "m4a1";
        m4a1.weaponCategory = "auto rifle";
        m4a1.cycleTime = 0.09f;
        m4a1.damege = 24;
        m4a1.maxPlayerSpeed = 585;
        m4a1.primaryClipSize = 30;
        m4a1.speed = 880f;
        m4a1.recoilSeed = 223;
        m4a1.recoilAngle = 2f;
        m4a1.recoilMagnitudeVariance = 2f;
        m4a1.spread = 0.4f;
        m4a1.inaccuracyStand = 1.6f;
        m4a1.inaccuracyAlt = 1.6f;
        m4a1.audioRes = ResManager.LoadPrefab("m4a1");
        weapon.Add(2, m4a1);

        Weapon awp = new Weapon(); 
        awp.weaponName = "awp";
        awp.weaponCategory = "sniper rifle";
        awp.cycleTime = 1.455f;
        awp.damege = 115;
        awp.maxPlayerSpeed = 500;
        awp.primaryClipSize = 10;
        awp.speed = 990f;
        awp.recoilSeed = 12345;
        awp.recoilAngle = 4f;
        awp.recoilMagnitudeVariance = 4f;
        awp.spread = 0.4f;
        awp.inaccuracyStand = 49.6f;
        awp.inaccuracyAlt = 0.2f;
        awp.audioRes = ResManager.LoadPrefab("awp");
        weapon.Add(3, awp);

        isSet = true;
    }
    private static Dictionary<int, Weapon> weapon = new Dictionary<int, Weapon>();
    public static Weapon GetWeapon(int id)
    {
        if (!isSet)
        {
            SetWeapon();
        }
        return weapon[id];
    }
}


