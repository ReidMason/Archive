using UnityEngine;
using System.Collections;

namespace NoxCore.Builders
{
	public interface IBuilder
	{
        bool getEnabled();
        bool getBuildOnce();
		BuildType getBuildType();
        void setBuildType(BuildType buildType);
		bool getDynamicBuild();
		void Build();
	}
}