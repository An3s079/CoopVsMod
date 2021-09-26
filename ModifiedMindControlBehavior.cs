using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;
using System.Collections;

namespace VersusMod
{
	class ModifiedMindControlBehavior : MonoBehaviour
	{
		public ModifiedMindControlBehavior()
		{
			this.m_attackedThisCycle = true;
		}

		MethodInfo BeginState, EndState, DoTeleport;
		//FieldInfo StateTP;

		private void Start()
		{
			isDashingOrTPing = false;
			this.m_aiActor = GetComponent<AIActor>();
			this.m_behaviorSpeculator = this.m_aiActor.behaviorSpeculator;
			GameObject gameObject = new GameObject("fake target");
			this.m_fakeActor = gameObject.AddComponent<NonActor>();
			this.m_fakeActor.HasShadow = false;
			this.m_fakeTargetRigidbody = gameObject.AddComponent<SpeculativeRigidbody>();
			this.m_fakeTargetRigidbody.PixelColliders = new List<PixelCollider>();
			this.m_fakeTargetRigidbody.CollideWithTileMap = true;
			this.m_fakeTargetRigidbody.CollideWithOthers = true;
			this.m_fakeTargetRigidbody.CanBeCarried = true;
			this.m_fakeTargetRigidbody.CanBePushed = true;
			this.m_fakeTargetRigidbody.CanCarry = true;
			PixelCollider pixelCollider = new PixelCollider();
			pixelCollider.ColliderGenerationMode = PixelCollider.PixelColliderGeneration.Manual;
			pixelCollider.CollisionLayer = CollisionLayer.TileBlocker;
			pixelCollider.ManualWidth = 4;
			pixelCollider.ManualHeight = 4;
			this.m_fakeTargetRigidbody.PixelColliders.Add(pixelCollider);
			this.m_overheadVFX = this.m_aiActor.PlayEffectOnActor((GameObject)ResourceCache.Acquire("Global VFX/VFX_Controller_Status"), new Vector3(0f, this.m_aiActor.specRigidbody.HitboxPixelCollider.UnitDimensions.y, 0f), true, false, true);
			m_aiActor.LocalTimeScale = 1.5f;
			foreach (var behav in m_behaviorSpeculator.MovementBehaviors)
			{
				if(behav is SeekTargetBehavior)
				{
					var i = behav as SeekTargetBehavior;
					i.PathInterval = 0;
				}
				if(behav is MoveErraticallyBehavior)
				{
					var i = behav as MoveErraticallyBehavior;
					i.PathInterval = 0;
				}
				if(behav is FleeTargetBehavior)
				{
					var i = behav as FleeTargetBehavior;
					i.PathInterval = 0;
				}
			}
			
		}

		// Token: 0x06004CC7 RID: 19655 RVA: 0x0019A784 File Offset: 0x00198984
		private Vector2 GetPlayerAimPointController(Vector2 aimBase, Vector2 aimDirection)
		{
			Func<SpeculativeRigidbody, bool> rigidbodyExcluder = (SpeculativeRigidbody otherRigidbody) => otherRigidbody.minorBreakable && !otherRigidbody.minorBreakable.stopsBullets;
			Vector2 result = aimBase + aimDirection * 10f;
			CollisionLayer layer = CollisionLayer.EnemyHitBox;
			int rayMask = CollisionMask.LayerToMask(CollisionLayer.HighObstacle, CollisionLayer.BulletBlocker, layer, CollisionLayer.BulletBreakable);
			RaycastResult raycastResult;
			if (PhysicsEngine.Instance.Raycast(aimBase, aimDirection, 50f, out raycastResult, true, true, rayMask, null, false, rigidbodyExcluder, null))
			{
				result = aimBase + aimDirection * raycastResult.Distance;
			}
			RaycastResult.Pool.Free(ref raycastResult);
			return result;
		}

		// Token: 0x06004CC8 RID: 19656 RVA: 0x0019A818 File Offset: 0x00198A18
		private void UpdateAimTargetPosition()
		{
			PlayerController playerController = this.owner;
			BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(playerController.PlayerIDX);
			GungeonActions activeActions = instanceForPlayer.ActiveActions;
			Vector3 v = m_aiActor.CenterPosition + BraveMathCollege.DegreesToVector(owner.FacingDirection) * 5f;
			if (instanceForPlayer.IsKeyboardAndMouse(false))
			{
				
				this.m_fakeTargetRigidbody.transform.position = v;
			}
			else

			{
				this.m_fakeTargetRigidbody.transform.position = v;
			}
			this.m_fakeTargetRigidbody.Reinitialize();
		}

		// Token: 0x06004CC9 RID: 19657 RVA: 0x0019A8A8 File Offset: 0x00198AA8
		private void Update()
		{
			this.m_fakeActor.specRigidbody = this.m_fakeTargetRigidbody;
			if (this.m_aiActor)
			{
				this.m_aiActor.CanTargetEnemies = false;
				this.m_aiActor.CanTargetPlayers = true;
				this.m_aiActor.PlayerTarget = this.m_fakeActor;
				this.m_aiActor.OverrideTarget = null;
				this.UpdateAimTargetPosition();
				if (this.m_aiActor.aiShooter)
				{
					this.m_aiActor.aiShooter.AimAtPoint(this.m_behaviorSpeculator.PlayerTarget.CenterPosition);
				}
				
			}
			if (this.m_behaviorSpeculator)
			{

				PlayerController playerController = this.owner;
				BraveInput instanceForPlayer = BraveInput.GetInstanceForPlayer(playerController.PlayerIDX);
				GungeonActions activeActions = instanceForPlayer.ActiveActions;
				if (this.m_behaviorSpeculator.AttackCooldown <= 0f)
				{
					if (!this.m_attackedThisCycle && this.m_behaviorSpeculator.ActiveContinuousAttackBehavior != null)
					{
						this.m_attackedThisCycle = true;
					}
					if (this.m_attackedThisCycle && this.m_behaviorSpeculator.ActiveContinuousAttackBehavior == null)
					{
						this.m_behaviorSpeculator.AttackCooldown = float.MaxValue;
						if (dashBehav != null)
						{

						}
						if (TeleBehav != null)
						{
							TeleBehav.RequiresLineOfSight = true;
							TeleBehav.MinRange = 1000f;
							TeleBehav.Range = 0.1f;
						}
					}
				}
				else if (activeActions.ShootAction.WasPressed)
				{
					this.m_attackedThisCycle = false;
					this.m_behaviorSpeculator.AttackCooldown = 0f;
				}
				else if (activeActions.DodgeRollAction.WasPressed)
				{
					this.m_attackedThisCycle = false;
					if (dashBehav != null)
					{
						if (isDashingOrTPing == false)
						{
							dashBehav.RequiresLineOfSight = false;
							dashBehav.MinRange = 3;
							dashBehav.Range = 8f;
							StartCoroutine(DoDash(dashBehav.dashTime));
						}
					}
					if (TeleBehav != null)
					{
						if (isDashingOrTPing == false)
						{
							TeleBehav.RequiresLineOfSight = false;
							TeleBehav.MinRange = 3;
							TeleBehav.Range = 17f;
							StartCoroutine(DoTP());
						}
						
					}
					//this.m_behaviorSpeculator.AttackCooldown = 0f;
				}





				if (this.m_behaviorSpeculator.TargetBehaviors != null && this.m_behaviorSpeculator.TargetBehaviors.Count > 0)
				{
					this.m_behaviorSpeculator.TargetBehaviors.Clear();
				}
				if (this.m_behaviorSpeculator.MovementBehaviors != null && this.m_behaviorSpeculator.MovementBehaviors.Count > 0)
				{
					this.m_behaviorSpeculator.MovementBehaviors.Clear();
				}
				this.m_aiActor.ImpartedVelocity += activeActions.Move.Value * this.m_aiActor.MovementSpeed * m_aiActor.LocalTimeScale;



				if (this.m_behaviorSpeculator.AttackBehaviors != null)
				{
					for (int i = 0; i < this.m_behaviorSpeculator.AttackBehaviors.Count; i++)
					{
						AttackBehaviorBase attack = this.m_behaviorSpeculator.AttackBehaviors[i];
						this.ProcessAttack(attack);
					}
				}
			}
		}

		private IEnumerator DoTP()
		{
			isDashingOrTPing = true;
			if (TeleBehav.teleportOutAnim != null)
			{
				m_aiActor.aiAnimator.PlayUntilFinished(TeleBehav.teleportOutAnim);
				while (m_aiActor.aiAnimator.IsPlaying(TeleBehav.teleportOutAnim))
				{
					yield return null;
				}
			}
			DoTeleport.Invoke(TeleBehav, null);
			if (TeleBehav.teleportInAnim != null)
			{
				m_aiActor.aiAnimator.PlayUntilFinished(TeleBehav.teleportInAnim);
				while (m_aiActor.aiAnimator.IsPlaying(TeleBehav.teleportInAnim))
				{
					yield return null;
				}
			}
			m_aiActor.aiShooter.ToggleGunAndHandRenderers(true, "they wont show up otherwise fot some reason");
			isDashingOrTPing = false;
		}

		private IEnumerator DoDash(float dashtime)
		{
			isDashingOrTPing = true;
			if(dashBehav.chargeAnim != null)
			{
				m_aiActor.aiAnimator.PlayUntilFinished(dashBehav.chargeAnim);
				while (m_aiActor.aiAnimator.IsPlaying(dashBehav.chargeAnim))
				{
					yield return null;
				}

			}
			BeginState.Invoke(dashBehav, State);
			yield return new WaitForSeconds(dashtime);
			EndState.Invoke(dashBehav, State);
			isDashingOrTPing = false;
		}

		

		// Token: 0x06004CCA RID: 19658 RVA: 0x0019AAE8 File Offset: 0x00198CE8
		private void ProcessAttack(AttackBehaviorBase attack)
		{
			if (attack == null)
			{
				return;
			}
			if (attack is BasicAttackBehavior)
			{
				BasicAttackBehavior basicAttackBehavior = attack as BasicAttackBehavior;
				basicAttackBehavior.Cooldown = 0f;
				basicAttackBehavior.RequiresLineOfSight = false;
				basicAttackBehavior.MinRange = -1f;
				basicAttackBehavior.Range = -1f;
				if (attack is TeleportBehavior)
				{
					TeleBehav = basicAttackBehavior as TeleportBehavior;
					basicAttackBehavior.RequiresLineOfSight = true;
					basicAttackBehavior.MinRange = 1000f;
					basicAttackBehavior.Range = 0.1f;
					DoTeleport = typeof(TeleportBehavior).GetMethod("DoTeleport", BindingFlags.Instance | BindingFlags.NonPublic);
					
				}
				if(attack is DashBehavior)
				{
					dashBehav = basicAttackBehavior as DashBehavior;
					basicAttackBehavior.RequiresLineOfSight = true;
					basicAttackBehavior.MinRange = 1000f;
					basicAttackBehavior.Range = 0.1f;
					BeginState = typeof(DashBehavior).GetMethod("BeginState", BindingFlags.Instance | BindingFlags.NonPublic);
					State[0] = typeof(DashBehavior).GetNestedType("DashState", BindingFlags.NonPublic).GetField("Charge").GetValue(dashBehav);
					EndState = typeof(DashBehavior).GetMethod("EndState", BindingFlags.Instance | BindingFlags.NonPublic);
				}
				if (basicAttackBehavior is ShootGunBehavior)
				{
					ShootGunBehavior shootGunBehavior = basicAttackBehavior as ShootGunBehavior;
					shootGunBehavior.LineOfSight = false;
					shootGunBehavior.EmptiesClip = false;
					shootGunBehavior.Cooldown = 0.3f;
					shootGunBehavior.RespectReload = true;
				}
			}
			else if (attack is AttackBehaviorGroup)
			{
				AttackBehaviorGroup attackBehaviorGroup = attack as AttackBehaviorGroup;
				for (int i = 0; i < attackBehaviorGroup.AttackBehaviors.Count; i++)
				{
					this.ProcessAttack(attackBehaviorGroup.AttackBehaviors[i].Behavior);
				}
			}
		}
		object[] State = new object[0];
		DashBehavior dashBehav;
		TeleportBehavior TeleBehav;
		// Token: 0x040042DD RID: 17117
		[NonSerialized]
		public PlayerController owner;

		// Token: 0x040042DE RID: 17118
		private AIActor m_aiActor;

		// Token: 0x040042DF RID: 17119
		private BehaviorSpeculator m_behaviorSpeculator;

		// Token: 0x040042E0 RID: 17120
		private bool m_attackedThisCycle;

		private bool isDashingOrTPing;
		// Token: 0x040042E1 RID: 17121
		private NonActor m_fakeActor;

		// Token: 0x040042E2 RID: 17122
		private SpeculativeRigidbody m_fakeTargetRigidbody;

		// Token: 0x040042E3 RID: 17123
		

		// Token: 0x040042E4 RID: 17124
		private GameObject m_overheadVFX;
	}
}
