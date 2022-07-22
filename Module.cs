using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using BepInEx;

namespace VersusMod
{
	[BepInPlugin("an3s.etg.coopvs", "Coop Versus Mod", "1.0.0")]
	[BepInDependency("etgmodding.etg.mtgapi")]
	public class Module : BaseUnityPlugin
	{

		public static readonly string MOD_NAME = "Versus Mod";
		public static readonly string VERSION = "1.0.0";
		public static readonly string TEXT_COLOR = "#00FFFF";
		public static Module instance;

		public void Start()
		{
			try
			{
				instance = this; 
				Module.altCam = gameObject.AddComponent<StaticCam>();
				//new Hook(typeof(MainMenuController).GetMethod("DoQuickStart", BindingFlags.NonPublic | BindingFlags.Instance), typeof(Module).GetMethod("SayHiCoroutine", BindingFlags.Public | BindingFlags.Static));
				ETGModConsole.Commands.AddGroup("vs");
				ETGModConsole.Commands.GetGroup("vs").AddUnit("start", Startp2);
				ETGModConsole.Log(MOD_NAME + " version " + VERSION + " started successfully.").Colors[0] = new Color32(247, 171, 20, 255);
				//CoroutineHelper.HookCoroutine("global::FinalIntroSequenceManager", "DoQuickStart", Test);
			}
			catch (Exception e)
			{
				ETGModConsole.Log("mod Broke heres why: " + e);
			}
			
		}

		//private void Test(ILContext il)
		//{
		//	//ILCursor iLCursor = new ILCursor(il);
		//	//iLCursor.
		//	ETGModConsole.Log("Hi");
		//}

		private static StaticCam altCam;
		private void Startp2(string[] obj)
		{
			if (GameManager.Instance.SecondaryPlayer == null)
			{
				ETGModConsole.Log("Can not start with only one instance of a player");
				return;
			}
			else
			{
				
				Module.instance.gameObject.GetOrAddComponent<AddMindControlForP2>();
				GameManager.Instance.PrimaryPlayer.OnEnteredCombat += AddMCP2;
				
				ETGModConsole.Log("Versus player2 started!").Colors[0] = Color.green;
			}
		}
		public static AIActor neemyP2;
		public static void AddMCP2()
		{

			Module.altCam.SetActive(true);
			var cRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
			neemyP2 = cRoom.GetRandomActiveEnemy(false);
			if (neemyP2 != null)
			{
				if (!neemyP2.healthHaver.IsBoss)
				{
					var mmcb = neemyP2.gameObject.GetOrAddComponent<ModifiedMindControlBehavior>();
					mmcb.owner = GameManager.Instance.SecondaryPlayer;
				}
			}
		}
		
		
		public static void Log(string text, string color = "FFFFFF")
		{
			ETGModConsole.Log($"<color={color}>{text}</color>");
		}
		public StreamWriter writer;

	}
}
