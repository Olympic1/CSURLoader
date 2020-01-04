﻿using UnityEngine;
using ColossalFramework;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;

namespace CSURLoader
{
    class Utils
    {
        private const string CSUR_REGEX = "CSUR(-(T|R|S))? ([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)+(=|-)?([[1-9]?[0-9]D?(L|S|C|R)[1-9]*P?)*";

        public static Dictionary<string, Material> textures;


        public static void LoadTextures()
        {
            // TODO: load texture container within mod
            NetInfo container = PrefabCollection<NetInfo>.FindLoaded("CSURTextureContainer_Data");
            foreach (NetInfo.Segment info in container.m_segments)
            {
                // lod material main texture name is TEXNAME_lod
                string textureKey = info.m_material.name;
                Debug.Log($"loaded {textureKey}");
                textures.Add("CSUR_TEXTURE/" + textureKey, info.m_material);
            }
        }

        public static bool IsCSUR(NetInfo asset)
        {
            if (asset == null || asset.m_netAI.GetType() != typeof(RoadAI)) {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_REGEX, RegexOptions.IgnoreCase);
            return m.Success;
        }

        public static bool IsCSURDerivative(NetInfo asset)
        {
            if (asset == null || asset.m_netAI.GetType() != typeof(RoadAI))
            {
                return false;
            }
            string savenameStripped = asset.name.Substring(asset.name.IndexOf('.') + 1);
            Match m = Regex.Match(savenameStripped, CSUR_REGEX + " [a-zA-Z]+", RegexOptions.IgnoreCase);
            return m.Success;
        }

        public static void SetElevatedPillar(NetInfo asset, string pillar)
        {
            var roadAI = asset.m_netAI as RoadAI;
            RoadBridgeAI elevatedAI = roadAI.m_elevatedInfo.m_netAI as RoadBridgeAI;
            if (pillar != null)
            {
                elevatedAI.m_bridgePillarInfo = PrefabCollection<BuildingInfo>.FindLoaded(pillar);
            }
            // "1532814575.Ama S-2_Data"
        }

        public static string GetPillar(NetInfo asset)
        {
            return null;
        }

        public static void ApplyTexture(NetInfo asset)
        {
            ApplyTextureSingleMode(asset);
            RoadAI netAI = asset.m_netAI as RoadAI;
            ApplyTextureSingleMode(netAI.m_elevatedInfo);
            ApplyTextureSingleMode(netAI.m_bridgeInfo);
            ApplyTextureSingleMode(netAI.m_slopeInfo);
            ApplyTextureSingleMode(netAI.m_tunnelInfo);

        }

        private static void ApplyTextureSingleMode(NetInfo asset)
        {
            // check whether the mode exists
            if (asset == null) return;
            foreach (NetInfo.Node info in asset.m_nodes)
            {
                string key = info.m_material.name;
                if (key.StartsWith("CSUR_TEXTURE/"))
                {
                    info.m_material = GetSharedMaterial(info.m_material);
                }

            }

            foreach (NetInfo.Segment info in asset.m_segments)
            {
                string key = info.m_material.name;
                if (key.StartsWith("CSUR_TEXTURE/"))
                {
                    info.m_material = GetSharedMaterial(info.m_material);
                }
            }
            asset.InitializePrefab();
            

        }

        private static Material GetSharedMaterial(Material materialKey)
        {
            Material cachedMaterial = materialKey;
            if (materialKey.name.StartsWith("CSUR_TEXTURE/") || materialKey.name.StartsWith("CSUR_LODTEXTURE/"))
            {
                Debug.Log($"Found Shared texture{materialKey.name}");
                Shader shader = materialKey.shader;
                Color color = materialKey.color;
                cachedMaterial = new Material(textures[materialKey.name]);
                cachedMaterial.shader = shader;
                cachedMaterial.color = color;
            }
            return cachedMaterial;
        }

        public static void LinkDerivative(NetInfo asset)
        {
            RoadAI netAI = asset.m_netAI as RoadAI;
            // strip the suffix from derivative name
            string sourceName = asset.name.Substring(asset.name.IndexOf('.') + 1);
            sourceName = sourceName.Substring(0, sourceName.LastIndexOf(' ')) + "_Data";
            NetInfo sourceAsset = PrefabCollection<NetInfo>.FindLoaded(sourceName);
            if (sourceAsset != null)
            {
                Debug.Log($"Linking {asset} to {sourceAsset}");
                RoadAI sourceAI = sourceAsset.m_netAI as RoadAI;
                netAI.m_elevatedInfo = sourceAI.m_elevatedInfo;
                netAI.m_bridgeInfo = sourceAI.m_bridgeInfo;
                netAI.m_slopeInfo = sourceAI.m_slopeInfo;
                netAI.m_tunnelInfo = sourceAI.m_tunnelInfo;
            }
        }


        public static void SetOutsideConnection(NetInfo asset)
        {
            RoadAI netAI = asset.m_netAI as RoadAI;
            netAI.m_outsideConnection = PrefabCollection<BuildingInfo>.FindLoaded("Road Connection");
            RoadBaseAI elevatedAI = netAI.m_elevatedInfo.m_netAI as RoadBaseAI;
            elevatedAI.m_outsideConnection = PrefabCollection<BuildingInfo>.FindLoaded("Road Connection");
            RoadBaseAI tunnelAI = netAI.m_tunnelInfo.m_netAI as RoadBaseAI;
            tunnelAI.m_outsideConnection = PrefabCollection<BuildingInfo>.FindLoaded("Road Connection");

        }

    }

}