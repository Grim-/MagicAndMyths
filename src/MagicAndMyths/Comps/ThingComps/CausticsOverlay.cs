using UnityEngine;
using Verse;

namespace MagicAndMyths
{
    public class CausticsOverlay : SkyOverlay
    {
        public Shader Shader;
        public Texture2D MainTex;
        public Texture2D SecondTex;
        public Texture2D DistortTex;
        public Material Material;

        public const string CausticShaderAssetName = "causticsshader";

        public CausticsOverlay()
        {
            //this.MainTex = ContentFinder<Texture2D>.Get("Layer1");
            //this.SecondTex = ContentFinder<Texture2D>.Get("Layer2");
            //this.DistortTex = ContentFinder<Texture2D>.Get("DistortionNoise");
            //this.Shader = LoadedModManager.GetMod<BiomesUnderwater>().GetShaderFromAssets(CausticShaderAssetName);

            //if (this.Shader == null)
            //{
            //    Log.Error($"Could not find shader {CausticShaderAssetName} in assets.");
            //    return;
            //}

            //this.Material = new Material(this.Shader);
            //this.Material.SetTexture("_MainTex", this.MainTex);
            //this.Material.SetTexture("_LayerTwo", this.SecondTex);
            //this.Material.SetTexture("_DistortMap", this.DistortTex);


            //this.Material.SetFloat("_Opacity", 0.14f);
            //this.Material.SetFloat("_ScrollSpeed", 0.3f);

            //this.Material.SetFloat("_DistortionSpeed", 0.04f);
            //this.Material.SetFloat("_DistortionStrR", 0.06f);
            //this.Material.SetFloat("_DistortionStrG", 0.06f);


            //this.Material.SetColor("_Color", new Color(1, 1, 1));
            //this.Material.SetColor("_Color2", new Color(1, 1, 1));


            //this.worldOverlayMat = this.Material;
        }



        public void UpdateMaterial()
        {

        }
    }
}