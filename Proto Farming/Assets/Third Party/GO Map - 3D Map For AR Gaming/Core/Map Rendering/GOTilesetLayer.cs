using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;

namespace GoMap
{

    [System.Serializable]
    public class GOTilesetLayer
    {
        [Regex(@"^(?!\s*$).+", "Please insert the NAME of your custom tileset")] public string TilesetName;
        [Regex(@"^(?!\s*$).+", "Please insert the ID of your custom tileset")] public string TilesetID;
        public string TilesetKindProperty;
        [Layer] public int unityLayer;

        public GOTilesetPOIRendering[] poisRenderingOptions;
        public GOTilesetLINERendering[] linesRenderingOptions;
        public GOTilesetPOLYGONRendering[] polygonsRenderingOptions;
        public UnityEngine.Rendering.ShadowCastingMode castShadows = UnityEngine.Rendering.ShadowCastingMode.Off;
        public bool useColliders = false;
        public int colliderHeight;

        public bool startInactive;
        public bool disabled = false;

        public GOFeatureEvent OnFeatureLoad;

        public string json()
        {  //Mapzen

            return "pois";
        }

        public string lyr()
        { //Mapbox
            return TilesetName;
        }

        public string lyr_osm()
        { //OSM
            return "";
        }

        public string lyr_esri()
        { //Esri
            return "";
        }

        public GOTilesetPOIRendering TilesetPOIRenderingForKind(string kind)
        {
            foreach (GOTilesetPOIRendering r in poisRenderingOptions)
            {
                if (r.kind == kind)
                    return r;
            }

            if (poisRenderingOptions.Length > 0)
                return poisRenderingOptions[0];

            return null;
        }

        public GOTilesetLINERendering TilesetLINERenderingForKind(string kind)
        {
            foreach (GOTilesetLINERendering r in linesRenderingOptions)
            {
                if (r.kind == kind)
                    return r;
            }

            if (linesRenderingOptions.Length > 0)
                return linesRenderingOptions[0];

            return null;
        }

        public GOTilesetPOLYGONRendering TilesetPOLYGONRenderingForKind(string kind)
        {
            foreach (GOTilesetPOLYGONRendering r in polygonsRenderingOptions)
            {
                if (r.kind == kind)
                    return r;
            }

            if (polygonsRenderingOptions.Length > 0)
                return polygonsRenderingOptions[0];

            return null;
        }
    }

    [System.Serializable]
    public class GOTilesetPOIRendering
    {

        public string kind;
        public GameObject prefab;
        public string tag;
        public GOFeatureEvent OnPoiLoad;
    }

    [System.Serializable]
    public class GOTilesetLINERendering
    {

        public string kind;
        public string tag;
        public float witdh;
        public float height;
        public Material material;
        public GOUVMappingStyle uvMappingStyle;
        public bool curved = false;
        public GOFeatureEvent OnLineLoad;
    }

    [System.Serializable]
    public class GOTilesetPOLYGONRendering
    {

        public string kind;
        public string tag;
        public float height;
        public Material material;
        public GOUVMappingStyle uvMappingStyle;
        public GOFeatureEvent OnPolygonLoad;
    }
}
