using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Gungeon;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using System.Globalization;
using SGUI;
using System.Collections;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using Mono.Cecil;
//using MonoMod.Cil;

namespace VersusMod
{
	public class Module : ETGModule
	{

		public static readonly string MOD_NAME = "Versus Mod";
		public static readonly string VERSION = "1.0.0";
		public static readonly string TEXT_COLOR = "#00FFFF";

		public static ETGModuleMetadata metadata;

		public override void Start()
		{
			try
			{
				metadata = this.Metadata;
				Module.altCam = ETGModMainBehaviour.Instance.gameObject.AddComponent<StaticCam>();
				//new Hook(typeof(MainMenuController).GetMethod("DoQuickStart", BindingFlags.NonPublic | BindingFlags.Instance), typeof(Module).GetMethod("SayHiCoroutine", BindingFlags.Public | BindingFlags.Static));
				ETGModConsole.Commands.AddGroup("vs");
				ETGModConsole.Commands.GetGroup("vs").AddUnit("start", Startp2);
				AdvancedLogging.LogPlain(MOD_NAME + " version " + VERSION + " started successfully.", new Color32(247, 171, 20, 255));
				//CoroutineHelper.HookCoroutine("global::FinalIntroSequenceManager", "DoQuickStart", Test);
			}
			catch (Exception e)
			{
				AdvancedLogging.LogError("mod Broke heres why: " + e);
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
				
				ETGModMainBehaviour.Instance.gameObject.GetOrAddComponent<AddMindControlForP2>();
				GameManager.Instance.PrimaryPlayer.OnEnteredCombat += AddMCP2;
				
				AdvancedLogging.LogPlain("Versus player2 started!", Color.green);
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

		public override void Exit() { }
		public override void Init() { }

	}
}
