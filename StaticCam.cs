using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Dungeonator;
using MonoMod.RuntimeDetour;
using System.Reflection;

namespace VersusMod
{
	class StaticCam : MonoBehaviour
	{
		private void Start()
		{
			StaticCam.Instance = this;
			this.handledRooms = new List<RoomHandler>();
            GameManager.Instance.OnNewLevelFullyLoaded += delegate ()
			{
				this.handledRooms.Clear();
				this.camControl = Camera.main.GetComponent<CameraController>();
			};
			try
			{
				Hook hook = new Hook(typeof(PlayerController).GetMethod("EnteredNewRoom", BindingFlags.Instance | BindingFlags.NonPublic), typeof(StaticCam).GetMethod("EnteredNewRoom"));
			}
			catch (Exception ex)
			{
				ETGModConsole.Log(ex.Message, false);
			}
			//ETGModConsole.Commands.AddUnit("roomsize", new Action<string[]>(this.RoomSize));
		}

		// Token: 0x06000002 RID: 2 RVA: 0x000020F8 File Offset: 0x000002F8
		private void HandleBossFreeze()
		{
			RoomHandler currentRoom = GameManager.Instance.PrimaryPlayer.CurrentRoom;
			bool flag = currentRoom.area.PrototypeRoomCategory == PrototypeDungeonRoom.RoomCategory.BOSS && !currentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
			if (flag)
			{
				for (int i = 0; i < GameManager.Instance.AllPlayers.Length; i++)
				{
					PlayerController playerController = GameManager.Instance.AllPlayers[i];
					playerController.ClearInputOverride("bossKillCam");
				}
			}
		}


		// Token: 0x06000004 RID: 4 RVA: 0x0000221C File Offset: 0x0000041C
		private void Update()
		{
			this.HandleBossFreeze();
			bool flag = this.player == null;
			if (flag)
			{
				this.player = GameManager.Instance.PrimaryPlayer;
			}
			else
			{
				bool flag2 = this.camControl == null;
				if (flag2)
				{
					this.camControl = Camera.main.GetComponent<CameraController>();
				}
				else
				{
					bool flag3 = StaticCam.isActive;
					if (!flag3)
					{
						bool flag4 = GameManager.Instance.GetLastLoadedLevelDefinition().dungeonSceneName.Equals("tt_foyer");
						if (flag4)
						{
							this.camControl.SetManualControl(false, true);
						}
						else
						{
							bool flag5 = !this.camControl.ManualControl && this.player.AcceptingAnyInput && StaticCam.room.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear);
							if (flag5)
							{
								this.Lock();
							}
						}
					}
				}
			}
		}

		// Token: 0x06000005 RID: 5 RVA: 0x000022EC File Offset: 0x000004EC
		public void SetActive(bool isActive)
		{
			StaticCam.isActive = isActive;
			bool flag = !isActive;
			if (flag)
			{
				bool flag2 = this.camControl != null;
				if (flag2)
				{
					this.camControl.OverrideZoomScale = 1f;
					this.zoom = 1f;
					bool flag3 = this.player != null && this.player.AcceptingAnyInput;
					if (flag3)
					{
						this.camControl.SetManualControl(false, true);
					}
				}
			}
			else
			{
				this.HandleNewRoom(this.player.CurrentRoom, this.player.CurrentRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear));
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x0000238C File Offset: 0x0000058C
		private void HandleNewRoom(RoomHandler room, bool shouldLock)
		{
			StaticCam.room = room;
			bool flag = !this.handledRooms.Contains(room);
			if (flag)
			{
				room.OnEnemiesCleared = (Action)Delegate.Combine(room.OnEnemiesCleared, new Action(delegate ()
				{
					this.Unlock();
				}));
				this.handledRooms.Add(room);
			}
			if (shouldLock)
			{
				this.Lock();
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x000023F0 File Offset: 0x000005F0
		// Token: 0x06000008 RID: 8 RVA: 0x00002484 File Offset: 0x00000684
		private void Unlock()
		{
			bool flag = !StaticCam.isActive;
			if (!flag)
			{
				this.camControl.OverrideZoomScale = 1f;
				this.camControl.SetManualControl(false, true);
				this.locked = false;
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x000024C8 File Offset: 0x000006C8
		private void Lock()
		{
			bool flag = !StaticCam.isActive;
			if (!flag)
			{
				IntVector2 dimensions = StaticCam.room.area.dimensions;
				float num = 27f;
				float num2 = ((float)dimensions.y >= num) ? (Mathf.Sqrt((float)dimensions.y - num) / 12f) : 0f;
				float num3 = 0.6f - num2;
				num3 = Mathf.Max(num3, 0.4f);
				bool flag2 = StaticCam.room.AdditionalRoomState == RoomHandler.CustomRoomState.LICH_PHASE_THREE;
				if (flag2)
				{
					num3 = 0.6f;
				}
				bool flag3 = true;
				bool flag4 = dimensions.y >= 40;
				if (flag4)
				{
					flag3 = false;
					num3 = 0.6f;
				}
				this.camControl.OverrideZoomScale = num3;
				this.zoom = num3;
				Vector3 position = this.camControl.Camera.transform.position;
				bool flag5 = flag3;
				if (flag5)
				{
					this.camControl.OverridePosition = new Vector3(StaticCam.room.area.Center.x, StaticCam.room.area.Center.y + 1f, position.z);
					this.camControl.SetManualControl(true, true);
					this.locked = true;
					Vector3 overridePosition = this.camControl.OverridePosition;
					bool flag6 = float.IsNaN(this.camControl.OverridePosition.x) || float.IsNaN(this.camControl.OverridePosition.y);
					if (flag6)
					{
						ETGModConsole.Log("<color=#FF0000>NaNs!</color>", false);
						this.Unlock();
					}
				}
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000265B File Offset: 0x0000085B
		public static void EnteredNewRoom(Action<PlayerController, RoomHandler> orig, PlayerController self, RoomHandler newRoom)
		{
			orig(self, newRoom);
			StaticCam.Instance.HandleNewRoom(newRoom, newRoom.HasActiveEnemies(RoomHandler.ActiveEnemyType.RoomClear));
		}

		// Token: 0x0600000B RID: 11 RVA: 0x0000267C File Offset: 0x0000087C
		public void RoomSize(string[] args)
		{
			IntVector2 dimensions = StaticCam.room.area.dimensions;
			ETGModConsole.Log(string.Format("Room size: {0}, {1}, last zoom: {2}", dimensions.x, dimensions.y, this.zoom), false);
		}

		// Token: 0x04000001 RID: 1
		private static RoomHandler room;

		// Token: 0x04000002 RID: 2
		private CameraController camControl;

		// Token: 0x04000003 RID: 3
		private PlayerController player;

		// Token: 0x04000004 RID: 4
		private static StaticCam Instance;

		// Token: 0x04000005 RID: 5
		private float zoom;

		// Token: 0x04000006 RID: 6
		public static bool isActive = false;

		// Token: 0x04000007 RID: 7
		private List<RoomHandler> handledRooms;

		// Token: 0x0400000C RID: 12
		private bool locked;
	}
}
