using UnityEngine;

using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;

namespace NoxCore.Placeables
{
	public interface IStructure : INoxObject2D
	{
		StructureData StructureData { get; set; }

		int getStructureSizeLimit(StructureSize size);
		void setStructureSize(StructureSize? size = null);
		float calculateAITick();
		void recalculateHealthBar();
		void showOutline(bool show);
		void setSortingLayerOrder(string layerName = null, int? sortOrder = null);
		bool isInvisible();
		bool isDestroyed();
		T getDevice<T>() where T : IDevice;
		List<T> getDevices<T>() where T : IDevice;
		List<Device> getAllDevices();
		GameObject getDeviceGameObject<T>() where T : IDevice;
		List<GameObject> getDevicesGameObjects<T>() where T : IDevice;
		List<GameObject> getAllDeviceGameObjects();
		StructureSocket getSocketByName(string socketName);
		T getSocket<T>() where T : ISocket;
		List<T> getSockets<T>() where T : ISocket;
		List<StructureSocket> getAllSockets();
		GameObject getSocketGameObject<T>() where T : ISocket;
		List<GameObject> getSocketsGameObjects<T>() where T : ISocket;
		List<GameObject> getAllSocketGameObjects();
		Module getModuleInSocket(string socketLabel);
		T getModule<T>() where T : IModule;
		List<T> getModules<T>() where T : IModule;
		List<Module> getAllModules();
		GameObject getModuleGameObject<T>() where T : IModule;
		List<GameObject> getModuleGameObjects<T>() where T : IModule;
		List<GameObject> getAllModuleGameObjects();
		Weapon getWeaponInSocket(string socketLabel);
		T getWeapon<T>() where T : IWeapon;
		List<T> getWeapons<T>() where T : IWeapon;
		List<Weapon> getWeapons();
		GameObject getWeaponGameObject<T>() where T : IWeapon;
		List<GameObject> getWeaponGameObjects<T>() where T : IWeapon;
		List<GameObject> getAllWeaponGameObjects();
		Device addDevice(Device device, Transform systemsParent);
		Device addDevice(string resourcePath, Transform systemsParent);
		Device addDevice(GameObject devicePrefab, Transform systemsParent);
		List<string> getSocketNames();
		List<string> getModuleSocketNames();
		List<string> getWeaponSocketNames();
		GameObject findSocketGOByName(string name);
		StructureSocket findSocketByName(string name);
		bool addSocket(StructureSocket structureSocket);
		GameObject addSocket(string resourcePath, string socketName, Vector2 position, float? rotation = null);
		bool addModule(StructureSocket structureSocket, Module module);
		bool addModule(GameObject structureSocketGO, Module module);
		Module addModule(string socketLabel, string resourcePath);
		Module addModule(string socketLabel, GameObject modulePrefab);
		Module addModule(GameObject structureSocketGO, string resourcePath);
		bool addWeapon(StructureSocket structureSocket, Weapon weapon);
		bool addWeapon(GameObject structureSocketGO, Weapon weapon);
		Weapon addWeapon(string socketLabel, string resourcePath);
		Weapon addWeapon(string socketLabel, GameObject weaponPrefab);
		Weapon addWeapon(GameObject structureSocketGO, string resourcePath);
		void detachController();
		void attachController(StructureController controller);
		void recalculateShields();
		void increaseHullStrength(float amount);
		string getPowerRequirements();
		void calculateAverageSurvivalTime(bool combatFinished);
		void startSurvivalClock();
		void stopSurvivalClock();
		void resetSurvivalClock();

	}
}