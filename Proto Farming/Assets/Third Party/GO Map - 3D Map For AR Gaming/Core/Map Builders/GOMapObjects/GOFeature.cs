using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoShared;
using System;
using Mapbox.VectorTile.Geometry;
using UnityEngine.Profiling;


namespace GoMap
{

    [System.Serializable]
    public class GOFeature
    {

        public string name;
        public GOFeatureKind kind = GOFeatureKind.baseKind;
        public GOPOIKind poiKind;
        public string tileSetKind;

        public string detail;
        public float sort;
        public float y;
        public float height;
        public float featureIndex;
        public float layerIndex;
        public int featureCount;
        public Vector3 featureCenter;
        public bool isLoop = false;

        [HideInInspector] public float highestAltitudeVertex = 0;

        public GOFeatureType goFeatureType;

        /*[HideInInspector]*/
        public IList geometry;
        public Coordinates poiCoordinates;

        [HideInInspector] public IList clips;

        public List<Vector3> convertedGeometry;
        public Vector3 poiGeometry;


        public IDictionary properties;
        public List<KeyValue> attributes;

        [HideInInspector] public GameObject parent;
        public GOLayer layer;
        [HideInInspector] public GOPOILayer poiLayer;
        [HideInInspector] public GOLabelsLayer labelsLayer;
        [HideInInspector] public GOTilesetLayer tilesetLayer;

        [HideInInspector] public GOMesh preloadedMeshData;
        [HideInInspector] public GOSegment preloadedLabelData;

        [HideInInspector] public GORenderingOptions renderingOptions;
        [HideInInspector] public GOPOIRendering poiRendering;
        [HideInInspector] public GOTilesetPOIRendering tileSetPoiRendering;
        [HideInInspector] public GOTilesetLINERendering tileSetLineRendering;
        [HideInInspector] public GOTilesetPOLYGONRendering tileSetPolygonRendering;


        bool defaultRendering = true;

        [HideInInspector] public GOTileObj goTile;

        //Terrain elevation costants
        public static float BuildingElevationOffset = 100;
        public static float RoadsHeightForElevation = 1f;
        public static int RoadsBreakEvery = 10;//3;

        public GOFeature()
        {

        }

        public GOFeature(GOFeature f)
        {

            name = f.name;
            featureIndex = f.featureIndex;
            goFeatureType = f.goFeatureType;
            properties = f.properties;
            attributes = f.attributes;
            layer = f.layer;
            goTile = f.goTile;

            //After editing the feature in tile subclasses.

            //		public string kind;
            kind = f.kind;
            renderingOptions = f.renderingOptions;
            detail = f.detail;
            sort = f.sort;
            y = f.y;
            height = f.height;
            featureIndex = f.featureIndex;
            layerIndex = f.layerIndex;
            featureCount = f.featureCount;
        }

        #region BUILDERS

        public void ConvertGeometries()
        {

            bool noise = layer != null && layer.layerType == GOLayer.GOLayerType.Buildings;
            convertedGeometry = CoordsToVerts(geometry, noise);


        }

        public void ConvertPOIGeometries()
        {

            LatLng c = (LatLng)geometry[0];
            poiCoordinates = new Coordinates(c.Lat, c.Lng, 0);
            poiGeometry = goTile.coordinatesToVector(poiCoordinates);
            //			poiGeometry = poiCoordinates.convertCoordinateToVector ();

        }

        public void ConvertAttributes()
        {

            List<KeyValue> list = new List<KeyValue>();

            foreach (string key in properties.Keys)
            {
                KeyValue keyValue = new KeyValue();
                keyValue.key = key;
                if (properties[key] != null)
                {
                    keyValue.value = properties[key].ToString();
                }
                list.Add(keyValue);
            }

            attributes = list;
        }


        public virtual IEnumerator BuildFeature(GOTile tile, bool delayedLoad)
        {

            if (goFeatureType == GOFeatureType.Undefined)
            {
                Debug.Log("type is null");
                return null;
            }
            try
            {
                if (goFeatureType == GOFeatureType.Line || goFeatureType == GOFeatureType.MultiLine || (layer != null && goFeatureType != GOFeatureType.Point && goFeatureType != GOFeatureType.Label && !layer.isPolygon))
                {
                    return CreateLine(tile, delayedLoad);
                }
                else if (goFeatureType == GOFeatureType.Polygon || goFeatureType == GOFeatureType.MultiPolygon)
                {
                    return CreatePolygon(tile, delayedLoad);
                }
                else if (goFeatureType == GOFeatureType.Point)
                {
                    return CreatePoi(tile, delayedLoad);
                }
                else if (goFeatureType == GOFeatureType.Label)
                {
                    return CreateLabel(tile, delayedLoad);
                }
                else return null;
            }
            catch (Exception ex)
            {
                Debug.Log("[GOFeature] Catched exception: " + ex);
                return null;
            }
        }

        public virtual IEnumerator CreateLine(GOTile tile, bool delayedLoad)
        {
            //if ()
            //GORenderingOptions renderingOptions = GetRenderingOptions();
            float lineWidth;
            int unityLayer;
            GOFeatureEvent goFeatureEvent;
            if (layer != null)
            {
                lineWidth = renderingOptions.lineWidth;
                unityLayer = layer.unityLayer;
                goFeatureEvent = layer.OnFeatureLoad;
            }
            else {
                lineWidth = tileSetLineRendering.witdh;
                unityLayer = tilesetLayer.unityLayer;
                goFeatureEvent = tilesetLayer.OnFeatureLoad;
            }


            if (lineWidth == 0)
            {
                yield break;
            }

            GOFeatureMeshBuilder builder = new GOFeatureMeshBuilder(this);
            featureCenter = builder.center;
            GameObject line = null;

            if (preloadedMeshData != null)
                line = builder.BuildLineFromPreloaded(this, tile.map, parent);

            if (line == null)
                yield break;

            line.name = name != null ? name : kind.ToString();

            //Layer mask
            line.layer = unityLayer;

            if (goTile.addGoFeatureComponents)
            {
                GOFeatureBehaviour fb = line.AddComponent<GOFeatureBehaviour>();
                fb.goFeature = this;
            }

            //Mapzen Streetnames
            if ((goTile.mapType == GOMap.GOMapType.Nextzen || goTile.mapType == GOMap.GOMapType.Mapbox) && layer != null && layer.layerType == GOLayer.GOLayerType.Roads && name != null && name.Length > 0 && goTile.useStreetnames && !isLoop && !goTile.useElevation)
            {

                GOStreetName streetName = GameObject.Instantiate(goTile.streetnamePrototype, line.transform).GetComponent<GOStreetName>();
                streetName.gameObject.name = name + "_streetname";
                yield return tile.StartCoroutine(streetName.Build(name, this));
            }

            if (goFeatureEvent != null)
            {
                goFeatureEvent.Invoke(this, line);

            }

            if (delayedLoad)
                yield return null;
        }

        public virtual IEnumerator CreatePolygon(GOTile tile, bool delayedLoad)
        {

            Profiler.BeginSample("[GOFeature] CreatePolygon ALLOC");
            GOFeatureMeshBuilder builder = new GOFeatureMeshBuilder(this);
            this.featureCenter = new Vector3(2, builder.center.y, 8);//new Vector3 (builder.center.x, builder.center.y, builder.center.z);
            Profiler.EndSample();

            Material material = null;
            Material roofMat = null;

            if (layer != null && layer.layerType == GOLayer.GOLayerType.Buildings && defaultRendering && renderingOptions.materials.Length != 0)
            {
                Profiler.BeginSample("[GOFeature] CreatePolygon Center");
                GOCenterContainer centerContainer = tile.findNearestCenter(builder.center, parent);
                Profiler.EndSample();
                if (centerContainer.material == null)
                {
                    Profiler.BeginSample("[GOFeature] CreatePolygon Material");
                    centerContainer.material = tile.GetMaterial(renderingOptions, builder.center);
                    centerContainer.secondaryMaterial = renderingOptions.roofMaterial;
                    Profiler.EndSample();
                }
                material = centerContainer.material;
                roofMat = centerContainer.secondaryMaterial;
            }
            else if (tileSetPolygonRendering != null) {

                material = tileSetPolygonRendering.material;

            }
            else  {
                Profiler.BeginSample("[GOFeature] CreatePolygon Material");
                //Materials
                material = tile.GetMaterial(renderingOptions, builder.center);
                roofMat = renderingOptions.roofMaterial;
                Profiler.EndSample();
            }

            if (sort != 0)
            {
                if (material)
                    material.renderQueue = -(int)sort;
                if (roofMat)
                    roofMat.renderQueue = -(int)sort;
            }

            if (layer != null && !layer.useRealHeight)
            {
                height = renderingOptions.polygonHeight;
            }

            float offset = 0;
            float trueHeight = height;


            if (layer != null && goTile.useElevation && layer.layerType == GOLayer.GOLayerType.Buildings)
            {
                trueHeight += BuildingElevationOffset;
                offset = BuildingElevationOffset;
                if (y < offset)
                    y = highestAltitudeVertex - offset + 0.5f;
                //					y = goTile.altitudeForPoint(builder.center)-offset+0.5f;

            }

            Profiler.BeginSample("[GOFeature] CreatePolygon MESH");
            GameObject polygon = null;
            if (preloadedMeshData != null)
                polygon = builder.BuildPolygonFromPreloaded(this, parent);

            Profiler.EndSample();

            if (polygon == null)
                yield break;

            polygon.name = name;

            //Layer mask and tag
            if (layer != null)
            {
                polygon.layer = layer.unityLayer;
                if (renderingOptions.tag.Length > 0)
                {
                    polygon.tag = renderingOptions.tag;
                }
            }
            else {
                polygon.layer = tilesetLayer.unityLayer;
                if (tileSetPolygonRendering.tag.Length > 0)
                {
                    polygon.tag = tileSetPolygonRendering.tag;
                }
            }

            //Shadow casting
            UnityEngine.Rendering.ShadowCastingMode shadowCasting;
            if (layer != null)
            {
                shadowCasting = layer.castShadows;
            }
            else {
                shadowCasting = tilesetLayer.castShadows;
            }


            if (renderingOptions != null && renderingOptions.hasRoof)
            {

                Material[] mats = new Material[2];
                mats[0] = material;
                mats[1] = roofMat;
                MeshRenderer mr = polygon.GetComponent<MeshRenderer>();
                mr.shadowCastingMode = shadowCasting;
                mr.materials = mats;

            }
            else
            {
                builder.meshRenderer.material = material;
                builder.meshRenderer.shadowCastingMode = shadowCasting;
            }

            Profiler.BeginSample("[GOFeature] TRANSFORM");
            Vector3 pos = polygon.transform.position;
            pos.y = y;
            if (layer != null && layer.layerType == GOLayer.GOLayerType.Buildings)
                y += GOFeatureMeshBuilder.Noise();

            pos.y *= goTile.worldScale;


            polygon.transform.position = pos;
            polygon.transform.localPosition = pos;

            if (goTile.addGoFeatureComponents)
            {
                GOFeatureBehaviour fb = polygon.AddComponent<GOFeatureBehaviour>();
                fb.goFeature = this;
            }

            if (layer != null && layer.OnFeatureLoad != null)
            {
                layer.OnFeatureLoad.Invoke(this, polygon);
            }
            else if (tilesetLayer != null && tilesetLayer.OnFeatureLoad != null)
            {
                tilesetLayer.OnFeatureLoad.Invoke(this, polygon);
            }
            Profiler.EndSample();

            preloadedMeshData = null;

            if (delayedLoad)
                yield return null;

        }

        IEnumerator CreatePoi(GOTile tile, bool delayedLoad)
        {

            if (poiRendering == null && tileSetPoiRendering == null)
            {
                yield break;
            }

            int unityLayer;
            GOFeatureEvent featureEvent;
            GameObject prefab;
            string tag;

            if (poiRendering != null)
            {
                prefab = poiRendering.prefab;
                unityLayer = (int)poiLayer.unityLayer;
                featureEvent = poiRendering.OnPoiLoad;
                tag = poiRendering.tag;
            }
            else {
                prefab = tileSetPoiRendering.prefab;
                unityLayer = (int)tilesetLayer.unityLayer;
                featureEvent = tileSetPoiRendering.OnPoiLoad;
                tag = tileSetPoiRendering.tag;
            }


            GameObject poi = GameObject.Instantiate(prefab);

#if GOLINK
			poiGeometry.y = GoTerrain.GOTerrain.RaycastAltitudeForVector(poiGeometry);
#endif

            poiGeometry.y += poi.transform.position.y;
            poiGeometry.y *= goTile.worldScale;
            poi.transform.position = poiGeometry;

            poi.transform.SetParent(parent.transform);
            poi.name = name;
            if (!string.IsNullOrEmpty(tag)) {
                poi.transform.tag = tag;
            }

            //			Debug.Log ("Load POI: "+ name);

            //Layer mask
            poi.layer = unityLayer;

            if (goTile.addGoFeatureComponents)
            {
                GOFeatureBehaviour fb = poi.AddComponent<GOFeatureBehaviour>();
                fb.goFeature = this;
            }

            if (featureEvent != null)
            {
                featureEvent.Invoke(this, poi);
            }

            if (delayedLoad)
                yield return null;

        }

        IEnumerator CreateLabel(GOTile tile, bool delayedLoad)
        {


            GameObject label = GameObject.Instantiate(goTile.streetnamePrototype);
            //Mapzen Streetnames
            if (name != null && name.Length > 0 && !goTile.useElevation)
            {

                GOStreetName streetName = label.GetComponent<GOStreetName>();
                streetName.gameObject.name = name + "_streetname";
                yield return tile.StartCoroutine(streetName.Build(name, this));
            }

            if (label == null)
                yield break;

            Vector3 pos = label.transform.position;
            pos.y = y;
            label.transform.position = pos;
            label.transform.SetParent(parent.transform);

            //Layer mask
            label.layer = labelsLayer.unityLayer;

            if (goTile.addGoFeatureComponents)
            {
                GOFeatureBehaviour fb = label.AddComponent<GOFeatureBehaviour>();
                fb.goFeature = this;
            }

            if (labelsLayer.OnLabelLoad != null)
            {
                labelsLayer.OnLabelLoad.Invoke(this, label);
            }

            //TODO: Test this change
            label.transform.localScale = Vector3.one;

            if (delayedLoad)
                yield return null;

        }

        #endregion

        #region UTILS

        public float getLayerDefaultY()
        {

            if (layer != null)
                return layer.defaultLayerY();
            else if (labelsLayer != null)
                return labelsLayer.defaultLayerY();

            return 0;
        }

        public void ComputeHighestAltitude()
        {

            float h = 0;
            foreach (Vector3 v in convertedGeometry)
            {
                float a = goTile.altitudeForPoint(v);
                h = Math.Max(h, a);
            }
            highestAltitudeVertex = h;
        }

        public void setRenderingOptions()
        {

            if (layer == null)
                return;

            renderingOptions = layer.defaultRendering;
            foreach (GORenderingOptions r in layer.renderingOptions)
            {
                if (r.kind == kind)
                {
                    defaultRendering = false;
                    renderingOptions = r;
                    break;
                }
            }

        }

        public GORenderingOptions GetRenderingOptions()
        {
            GORenderingOptions renderingOptions = layer.defaultRendering;
            foreach (GORenderingOptions r in layer.renderingOptions)
            {
                if (r.kind == kind)
                {
                    defaultRendering = false;
                    renderingOptions = r;
                    break;
                }
            }
            return renderingOptions;
        }

        public static List<KeyValue> PropertiesToAttributes(IDictionary props)
        {

            List<KeyValue> list = new List<KeyValue>();
            KeyValue keyValue;

            foreach (string key in props.Keys)
            {

                keyValue = new KeyValue();

                keyValue.key = key;
                if (props[key] != null)
                {
                    keyValue.value = props[key].ToString();
                }
                list.Add(keyValue);
            }

            return list;
        }

        public static List<Vector3> CoordsToVerts(IList geometry, bool withNoise)
        {

            var convertedGeometry = new List<Vector3>();

            for (int i = 0; i < geometry.Count; i++)
            {
                if (geometry.GetType() == typeof(List<LatLng>))
                { //Mapbox 
                    LatLng c = (LatLng)geometry[i];
                    Coordinates coords = new Coordinates(c.Lat, c.Lng, 0);
                    Vector3 p = coords.convertCoordinateToVector();

                    if (withNoise && i != 0 && i != geometry.Count - 1)
                    {
                        float noise = GOFeatureMeshBuilder.Noise();
                        p.x += noise;
                        p.z -= noise;
                    }
                    convertedGeometry.Add(p);

                }
                else
                { //Mapzen
                    IList c = (IList)geometry[i];
                    Coordinates coords = new Coordinates((double)c[1], (double)c[0], 0);
                    convertedGeometry.Add(coords.convertCoordinateToVector());

                }
            }
            return convertedGeometry;
        }



        public static bool IsGeoPolygonClockwise(IList coords)
        {
            return (IsClockwise(GOFeature.CoordsToVerts(coords, false)));

        }

        public static bool IsClockwise(IList<Vector3> vertices)
        {
            double sum = 0.0;
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 v1 = vertices[i];
                Vector3 v2 = vertices[(i + 1) % vertices.Count];
                sum += (v2.x - v1.x) * (v2.z + v1.z);
            }
            return sum > 0.0;
        }

        public static bool IsClockwise2(IList<Vector3> vertices)
        {
            if (vertices.Count == 0) {
                return false;
            }
                

            //var vertices = feature.geometry.getVertices();
            float area = 0;

            for (var i = 0; i < (vertices.Count); i++)
            {
                int j = (i + 1) % vertices.Count;

                area += vertices[i].x * vertices[j].y;
                area -= vertices[j].x * vertices[i].y;
                // console.log(area);
            }

            return (area < 0);
        }

        #endregion
    }

    [System.Serializable]
    public class KeyValue
    {
        public string key;
        public string value;
    }
}