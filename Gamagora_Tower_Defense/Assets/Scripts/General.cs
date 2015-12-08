using UnityEngine;
using System.Collections;

public enum Audio_Type
{
    NULL, // Valeur NULL
    Music,
    Ambient,
    Voice,
    Fireball,
    Freeze,
    NewGatling,
    NewFlamethrower,
    NewShotgun,
    NewLaserBlast,
    NewRocketLauncher,
    GatlingShoot,
    ShotgunShoot,
    RocketLauncherShoot,
    LaserBlastShoot,
    BombOnTerrain,
    MissileTargeting,
    Flame,
    ExplosionBomb,
    ExplosionMissile,
    EnemyExplosion,
}

public static class General
{
    public static void ChangeMaterialForAllChild(Transform b, Material mat, string[] expect = null)
    {
        Transform[] allChildren = b.GetComponentsInChildren<Transform>();

        foreach (Transform child in allChildren)
        {
            if (child.GetComponent<ParticleSystem>() == null)
            {
                if (expect == null)
                {
                    ChangeMaterialForChild(child, mat);
                }
                else
                {
                    bool ok = true;
                    foreach (string s in expect)
                    {
                        if (!string.IsNullOrEmpty(s) && s.Equals(child.name))
                        {
                            ok = false;
                            break;
                        }
                    }

                    if (ok)
                        ChangeMaterialForChild(child, mat);
                }
            }
        }
    }

    public static void ChangeMaterialForChild(Transform child, Material mat)
    {
        Renderer r = child.gameObject.GetComponent<Renderer>();
        if (r != null && r.material != null)
            r.material = mat;
    }

    /// <summary>
    /// Permet de changer le mode du shader standard
    /// </summary>
    /// <param name="material">le material à utiliser</param>
    /// <param name="blendMode"> le mode voulu : Opaque , Cutout, Fade, Transparent</param>
    public static void SetupMaterialWithBlendMode(Material material, string blendMode)
    {
        switch (blendMode)
        {
            case "Opaque":
                material.SetFloat("_Mode", 0);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
                break;
            case "Cutout":
                material.SetFloat("_Mode", 1);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                material.EnableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 2450;
                break;
            case "Fade":
                material.SetFloat("_Mode", 2);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
            case "Transparent":
                material.SetFloat("_Mode", 3);
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                break;
        }
    }
}
