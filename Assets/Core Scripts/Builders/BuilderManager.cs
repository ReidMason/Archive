using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Controllers;
using NoxCore.Effects;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.GamePatterns;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Builders
{
    public sealed class BuilderManager : SingletonBase<BuilderManager>
    {
        private bool _builtAll;
        public bool BuiltAll { get { return _builtAll; } }

        private int _numberBuilt;
        public int NumberBuilt { get { return _numberBuilt; } }

        private GameObject gameManagerGO;

        private int[] sortLayerCounter;

        // TODO - this has to die
        List<string> builtOnceBuilders = new List<string>();

        void Awake()
        {
            sortLayerCounter = new int[Enum.GetNames(typeof(StructureSize)).Length];

            //gameManagerGO = GameObject.Find("Game Manager");
        }

        void Start()
        {
            //transform.parent = gameManagerGO.transform.parent;
        }

        // TODO - this has to die
        public void addToBuiltOnceBuilders(string builderName)
        {
            builtOnceBuilders.Add(builderName);
        }

        // TODO - this has to die
        public bool hasBeenBuilt(string builderName)
        {
            return builtOnceBuilders.Contains(builderName);
        }

        public void incrementSortLayerCounter(StructureSize structureSize)
        {
            sortLayerCounter[(int)(structureSize)] += 2;
        }

        public int getSortLayerCount(StructureSize structureSize)
        {
            return sortLayerCounter[(int)(structureSize)];
        }

        public void Build(List<Type> builderComponents)
        {
            GameObject buildersGO = GameObject.Find("Builders");

            List<IBuilder> builders = new List<IBuilder>();

            foreach (Type type in builderComponents)
            {
                buildersGO.AddComponent(type);
                IBuilder aBuilder = buildersGO.GetComponent(type) as IBuilder;
                builders.Add(aBuilder);
            }

            // TODO - need a better system than this
            // make sure the arena is fully constructed before anything else
            foreach (IBuilder builder in builders)
            {
                if (builder.getBuildType() == BuildType.ARENA)
                {
                    if (builder.getDynamicBuild() == true)
                    {
                        _numberBuilt++;
                    }
                    else
                    {
                        try
                        {
                            INewBuilder newBuilder = builder as INewBuilder;
                            if (newBuilder != null)
                            {
                                newBuilder.Reset();
                            }

                            builder.Build();
                            _numberBuilt++;
                        }
                        catch (Exception e)
                        {
                            D.warn("Content: {0}", "BuilderManager could not build a component: " + e.ToString());
                        }
                        break;
                    }
                }
            }

            // then build everything else
            foreach (IBuilder builder in builders)
            {
                if (builder.getBuildType() != BuildType.ARENA)
                {
                    if (builder.getDynamicBuild() == true)
                    {
                        _numberBuilt++;
                    }
                    else
                    {
                        INewBuilder newBuilder = builder as INewBuilder;
                        if (newBuilder != null)
                        {
                            newBuilder.Reset();
                        }

                        if (builder.getEnabled() == true)
                        {
                            try
                            {
                                builder.Build();
                                _numberBuilt++;
                            }
                            catch (Exception e)
                            {
                                D.warn("Content: {0}", "BuilderManager could not build a component: " + e.ToString());
                            }
                        }
                    }
                }
            }

            if (_numberBuilt == builderComponents.Count)
            {
                _builtAll = true;
                // D.log ("Content", "All builder components were built");
            }
            else
            {
                _builtAll = false;
                // D.warn ("Could not build all builder comonents in the mod folder");
            }
        }
    }
}