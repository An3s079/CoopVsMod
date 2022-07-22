using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Dungeonator;
using System.Collections;
using System.Reflection;

namespace VersusMod
{
	class AddMindControlForP2 : MonoBehaviour
	{
		private FieldInfo m_stealthVfx;
		private void Start()
		{
			m_stealthVfx = typeof(GameActor).GetField("m_stealthVfx", BindingFlags.Instance | BindingFlags.NonPublic);
			
		}
		public void Update()
		{
			if (Module.neemyP2 == null)
			{
				if(GameManager.Instance.PrimaryPlayer.IsInCombat)
				{
					Module.AddMCP2();
				}
			}
			if (Module.neemyP2 != null)
			{
				if (Module.neemyP2.healthHaver.IsDead || Module.neemyP2.healthHaver.IsBoss)
				{
					Module.AddMCP2();
				}

				GameManager.Instance.SecondaryPlayer.specRigidbody.Position = new Position(Module.neemyP2.CenterPosition);			
			}
			else
			GameManager.Instance.SecondaryPlayer.specRigidbody.Position = GameManager.Instance.PrimaryPlayer.specRigidbody.Position;
			


			var i = GameManager.Instance.SecondaryPlayer.specRigidbody.GetPixelColliders();
			foreach (var col in i)
			{
				col.Enabled = false;
			}
			GameManager.Instance.SecondaryPlayer.IsVisible = false;
			GameManager.Instance.SecondaryPlayer.ToggleGunRenderers(false);
			GameManager.Instance.SecondaryPlayer.ToggleShadowVisiblity(false);
			GameManager.Instance.SecondaryPlayer.ToggleFollowerRenderers(false);
			GameManager.Instance.SecondaryPlayer.ToggleHandRenderers(false);
			
			GameManager.Instance.SecondaryPlayer.CurrentGun.CurrentAmmo = 0;
			GameManager.Instance.SecondaryPlayer.CurrentGun.CanGainAmmo = false;
			GameManager.Instance.SecondaryPlayer.healthHaver.IsVulnerable = false;
			GameManager.Instance.SecondaryPlayer.SetIsFlying(true, "so they dont fall in pits");
			if (GameManager.Instance.PrimaryPlayer.healthHaver.IsDead)
			{
				GameManager.Instance.StartCoroutine(KillP2());
			}
			var AA = BraveInput.SecondaryPlayerInstance.ActiveActions;
			AA.MapAction.ClearBindings();
			GameManager.Instance.SecondaryPlayer.SetIsStealthed(true, "i dont want them targetable");
			UnityEngine.Object.Destroy(m_stealthVfx.GetValue(GameManager.Instance.SecondaryPlayer) as GameObject);
		}

		public IEnumerator KillP2()
		{
			yield return new WaitForSeconds(0.5f);
			GameManager.Instance.SecondaryPlayer.healthHaver.IsVulnerable = true;
			GameManager.Instance.SecondaryPlayer.healthHaver.ApplyDamage(float.PositiveInfinity, Vector2.zero, "magic powers");
			GameManager.Instance.PrimaryPlayer.OnEnteredCombat -= Module.AddMCP2;
			UnityEngine.Object.Destroy(Module.instance.gameObject.GetComponent<AddMindControlForP2>());
			GameManager.Instance.SecondaryPlayer.IsVisible = true;
			GameManager.Instance.SecondaryPlayer.ToggleGunRenderers(true);
			GameManager.Instance.SecondaryPlayer.ToggleShadowVisiblity(true);
			GameManager.Instance.SecondaryPlayer.ToggleFollowerRenderers(true);
			GameManager.Instance.SecondaryPlayer.ToggleHandRenderers(true);
			var i = GameManager.Instance.SecondaryPlayer.specRigidbody.GetPixelColliders();
			foreach (var col in i)
			{
				col.Enabled = false;
			}
		}
	}
}
