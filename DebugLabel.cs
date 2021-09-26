using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using SGUI;
using ETGGUI;

namespace VersusMod
{
	class DebugLabel : SLabel
	{// Token: 0x06000010 RID: 16 RVA: 0x00002708 File Offset: 0x00000908
		public void Start()
		{
			SGUIRoot.Main.Children.Add(this);
			this.Size = new Vector2(1000f, 40f);
			this.Alignment = TextAnchor.MiddleRight;
			this.OnUpdateStyle = delegate (SElement elem)
			{
				this.LoadFont();
				elem.Font = DebugLabel.font;
				try
				{
					elem.Position = new Vector2((float)Screen.width * this.pos.x, (float)Screen.height * this.pos.y);
				}
				catch
				{
					ETGModConsole.Log("Couldn't set position", false);
				}
				elem.Size = new Vector2(1000f, 40f);
			};
			this.SetText("--");
		}

		// Token: 0x06000011 RID: 17 RVA: 0x00002761 File Offset: 0x00000961
		public void SetText(string text)
		{
			this.Text = text;
		}

		// Token: 0x06000012 RID: 18 RVA: 0x0000276B File Offset: 0x0000096B
		public void ShowLabel()
		{
			this.isEnabled = true;
		}

		// Token: 0x06000013 RID: 19 RVA: 0x00002775 File Offset: 0x00000975
		public void LoadFont()
		{
			DebugLabel.gameFont = (dfFont)GameUIRoot.Instance.Manager.DefaultFont;
			DebugLabel.font = FontConverter.GetFontFromdfFont(DebugLabel.gameFont, 2);
		}

		// Token: 0x06000014 RID: 20 RVA: 0x000027A4 File Offset: 0x000009A4
		private float map(float s, float a1, float a2, float b1, float b2)
		{
			return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
		}

		// Token: 0x06000015 RID: 21 RVA: 0x000027C6 File Offset: 0x000009C6
		public void SetPosition(float x, float y)
		{
			this.pos = new Vector2(x, y);
			this.Position = new Vector2((float)Screen.width * this.pos.x, (float)Screen.height * this.pos.y);
		}

		// Token: 0x0400000D RID: 13
		public bool isEnabled = true;

		// Token: 0x0400000E RID: 14
		public static Font font;

		// Token: 0x0400000F RID: 15
		public static dfFont gameFont;

		// Token: 0x04000010 RID: 16
		private Vector2 pos;
	}
}
