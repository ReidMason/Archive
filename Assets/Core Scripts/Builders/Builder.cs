using UnityEngine;
using System.Collections;
using System;

using NoxCore.Utilities;

namespace NoxCore.Builders
{
    public enum BuildType { ARENA, PLANET, STRUCTURE, STATION, SHIP, WAVE, OTHER }

    public class Builder : MonoBehaviour, IBuilder
	{  
        public BuildType buildType;
		
		protected bool _dynamicBuild;
		public bool dynamicBuild { get { return _dynamicBuild; } set { _dynamicBuild = value; } }

        public bool enable;
        public bool buildOnce;

        void Awake()
        {
            setBuildType(BuildType.OTHER);
        }

        public virtual void Reset()
        {}

        public bool getEnabled()
        {
            return enable;
        }

        public bool getBuildOnce()
        {
            return buildOnce;
        }

		public BuildType getBuildType()
		{
			return buildType;
		}

        public void setBuildType(BuildType buildType)
        {
            this.buildType = buildType;
        }
		
		public bool getDynamicBuild()
		{
			return dynamicBuild;
		}

        public virtual void preBuild()
        {}

        // TODO - this should return the GameObject built
        public virtual void Build()
        {}

        public virtual void postBuild()
        {}
    }
}