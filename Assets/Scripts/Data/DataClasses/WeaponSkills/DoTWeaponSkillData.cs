using System;
using UnityEngine;
using System.Collections.Generic;

public class DoTWeaponSkillData : BaseWeaponSkillData {
	public int DoTDuration;
	public int DoTStackSize;

	public override void PerformWeaponSkill( GameBoard gameBoard, Battle battle, List<Tile> match ) {
		battle.ApplyDoT( DoTDuration, DoTStackSize );
	}	
}


