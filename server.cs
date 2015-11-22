exec("./support.cs");
exec("./maps.cs");

if($HAS::Init $= "") {
	exec("./playertype.cs");
	datablock AudioProfile(seekerBeep)
	{
		filename = "./sounds/seekerBeep.wav";
		description = AudioClosest3d;
		preload = true;
	};
}


function Player::getPlayerInSight(%this) {
	%eye = vectorScale(%this.getEyeVector(), 10);
	%pos = %this.getEyePoint();
	%mask = $TypeMasks::All;
	%hit = getWord(containerRaycast(%pos, vectorAdd(%pos, %eye), %mask, %this), 0);

	if(isObject(%hit)) {
		if(%hit.getClassName() $= "Player") {
			return %hit;
		}
	}

	return -1;
}

function Player::blinkPlayer(%this) {
	cancel(%this.blinkSched);
	%this.blinkSched = %this.schedule(300, blinkPlayer);

	if(isObject(%this.seeing)) {
		%this.playSound(seekerBeep);
		%this.schedule(150, playSound, seekerBeep);
	} else {
		if(%this.blinkMode) {
			%this.playSound(seekerBeep);
		}
	}

	if(%this.blinkMode) {
		%this.blinkMode = 0;
		%this.setNodeColor("ALL", "1 0 0 1");
	} else {
		%this.blinkMode = 1;
		%this.setNodeColor("ALL", "1 1 1 1");
	}
}

function Player::movementCheck(%this) {
	cancel(%this.movementSched);
	%this.movementSched = %this.schedule(100, movementCheck);

	%pos = %this.getPosition();
	%radius = 4;
	%mask = $TypeMasks::PlayerObjectType;

	initContainerRadiusSearch(%pos, %radius, %mask);
	while(%person = containerSearchNext()) {
		if(%person !$= %this) {
			if(%person.client.team) {
				%this.normalizeNodes();
				return;
			}
		}
	}

	if(%this.lastPosition !$= %this.getPosition() || %this.isVisibleToOthers) {
		%this.normalizeNodes();
	} else {
		%this.hideNode("ALL");
	}

	%this.lastPosition = %this.getPosition();
}

function Player::lookCheck(%this) {
	cancel(%this.lookSched);
	%this.lookSched = %this.schedule(100, lookCheck);

	%target = %this.getPlayerInSight();

	if(isObject(%this.getPlayerInSight())) {
		if(!%target.client.team) {
			%this.seeing = %target;
			%target.beingSeen(%this);
		}
	} else {
		if(isObject(%this.seeing)) {
			%this.seeing.changeDatablock("PlayerFrozenArmor");
			%this.seeing.setDamageFlash(0);
		}
		%this.seeing.seen = 0;
		%this.seeing = "";
	}
}

function Player::beingSeen(%this, %who) {
	%this.seen += 1;
	if(%this.getDatablock().getName() $= "PlayerNoJet") {
		%this.changeDatablock("PlayerFrozenArmor");
		%this.setVelocity("0 0 0");
	}
	
	%this.client.play2D(errorSound);
	%this.setDamageFlash(%this.seen/30);

	if(%this.seen > 30) {
		%this.kill();
		$DefaultMinigame.schedule(33, checkHASLast);
	}
}

function GameConnection::statsLoop(%this) {
	cancel(%this.statsSched);
	%this.statsSched = %this.schedule(1000, statsLoop);

	%remains = mFloor(($HAS::TimingStart + $HAS::TimingDelay/1000) - $Sim::Time);
	%this.bottomPrint("<font:Arial Bold:18>\c3" @ getTimeString(%remains), 2, 1);
}

function MinigameSO::checkHASLast(%this) {
	if(isEventPending(%this.resetSched)) {
		return;
	}

	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];

		if(isObject(%client.player)) {
			switch(%client.team) {
				case 0: %hiders++;
				case 1: %seekers++;
			}
		}
	}
	if(%seekers || %hiders) {
		if(!%seekers) {
			%this.messageAll('MsgAdminForce', "\c3Hiders\c5" SPC "win this round! Resetting in \c310 seconds...");
		}
		if(!%hiders) {
			%this.messageAll('MsgAdminForce', "\c0Seekers\c5" SPC "win this round! Resetting in \c310 seconds...");
		}
	}

	if(!%seekers || !%hiders) {
		%this.resetSched = %this.schedule(10000, reset);
	}
}

function MinigameSO::getSpawns(%this) {
	%this.seekerSpawns = "";
	%this.hiderSpawns = "";

	for(%i=0;%i<BrickGroup_888888.getCount();%i++) {
		%brick = BrickGroup_888888.getObject(%i);
		switch$(%brick.getName()) {
			case "_seekerSpawn":
				%this.seekerSpawns = trim(%this.seekerSpawns SPC %brick);
			case "_hiderSpawn":
				%this.hiderSpawns = trim(%this.hiderSpawns SPC %brick);
		}
	}
}

// O click a brick, take the color
// O if caught, freeze the player
// O must be looked at for 3 continuous seconds
// O do a radius search, don't show players unless seekers are within ~9 studs or if in motion
// O seekers should be faster
// O hiders should be slower after seekers are released, let them be quick beforehand
// O stats

function MinigameSO::startRound(%this) {
	for(%i=0;%i<%this.numMembers;%i++) {
		%client = %this.member[%i];
		if(%client.team) {
			if(isObject(%client.player)) {
				%client.player.setVelocity("0 0 0");

				%spawn = getWord(%this.hiderSpawns, getRandom(0, getWordCount(%this.hiderSpawns)-1));
				%client.player.setTransform(%spawn.getPosition());
			}
		} else {
			if(isObject(%client.player)) {
				%client.player.hideNode("ALL");
				%client.player.movementCheck();
				%client.player.changeDatablock("PlayerSlowArmor");
			}
		}
	}

	%delay = 180000 + (%this.numMembers*15000);
	%this.messageAll('MsgAdminForce', "\c5The \c0seekers \c5have been released!");
	%this.gameEndDelay = %this.schedule(%delay, endRound);

	$HAS::TimingStart = $Sim::Time;
	$HAS::TimingDelay = %delay;
	$HAS::Ongoing = 1;
}

function MinigameSO::endRound(%this) {
	// if we get here, the seekers won

	if(isEventPending(%this.resetSched)) {
		return;
	}

	%this.messageAll('MsgAdminForce', "\c3Hiders\c5" SPC "win this round! Resetting in \c310 seconds...");
	%this.resetSched = %this.schedule(10000, reset);
}

package HideAndSeekPackage {
	function fxDTSBrick::setItem(%this) { return; }

	function MinigameSO::reset(%this) {
		parent::reset(%this);
		%this.rounds++;
		if(%this.rounds % 3 == 1) {
			for(%i=0;%i<%this.numMembers;%i++) {
				%client = %this.member[%i];

				%client.camera.setMode(observer);
				%client.setControlObject(%cl.camera);

				if(isObject(%client.player)) {
					%client.player.delete();
				}
			}

			BrickGroup_888888.chainDeleteCallback = "loadHASMap();";
			BrickGroup_888888.chainDeleteAll();
			return;
		}

		$HAS::Ongoing = 0;
		%this.respawnAll();
		$HAS::TimingStart = $Sim::Time;

		if(!%this.numMembers) {
			return;
		}

		for(%i=0;%i<%this.numMembers;%i++) {
			%this.member[%i].team = 0;
			%this.member[%i].player.setShapeNameDistance(3);
		}

		%seekers = mCeil(%this.numMembers / 6);

		for(%i=0;%i<%seekers;%i++) {
			%client = %this.member[getRandom(0, %this.numMembers-1)];
			while(%client.team == 1) {
				%client = %this.member[getRandom(0, %this.numMembers-1)];
			}
			%client.team = 1;

			%spawn = getWord(%this.seekerSpawns, getRandom(0, getWordCount(%this.seekerSpawns)-1));
			%client.player.changeDatablock(playerFastArmor);
			%client.player.setTransform(%spawn.getPosition());
			%client.player.setShapeNameColor(getColorIDTable(0));
			%client.player.blinkPlayer();
			%client.player.lookCheck();

			%this.messageAll('', "\c3" @ %client.name SPC "\c5is a \c0seeker \c5this round!");
		}

		for(%i=0;%i<%this.numMembers;%i++) {
			%client = %this.member[%i];

			if(!%client.team) {
				%spawn = getWord(%this.hiderSpawns, getRandom(0, getWordCount(%this.hiderSpawns)-1));
				%client.player.setTransform(%spawn.getPosition());
				%client.player.setShapeNameColor(getColorIDTable(14));
				%client.player.changeDatablock(playerFastArmor);
			}
		}

		cancel(%this.gameStartDelay);
		cancel(%this.gameEndDelay);
		%delay = mFloor(40000 + mCeil(%this.numMembers / 6)*15000);
		%this.gameStartDelay = %this.schedule(%delay, startRound);

		$HAS::TimingDelay = %delay;
	}

	function Player::activateStuff(%this) {
		if(!%this.client.team) {
			%eye = vectorScale(%this.getEyeVector(), 10);
			%pos = %this.getEyePoint();
			%mask = $TypeMasks::FxBrickObjectType;
			%hit = getWord(containerRaycast(%pos, vectorAdd(%pos, %eye), %mask, %this), 0);

			if(isObject(%hit)) {
				%this.setNodeColor("ALL", getColorIDTable(%hit.colorID));
				%this.lastColor = getColorIDTable(%hit.colorID);
			}
		}

		return parent::activateStuff(%this);
	}

	function Player::changeDatablock(%this, %block) {
		parent::changeDatablock(%this, %block);

		if(%this.lastColor !$= "") {
			%this.setNodeColor("ALL", %this.lastColor);
		}
	}

	function MinigameSO::checkLastManStanding(%this) { 
		%this.checkHASLast();
		return;
	}

	function GameConnection::spawnPlayer(%this) {
		if(!getBrickCount()) {
			$DefaultMinigame.reset();
			return;
		}

		if(!$HAS::Ongoing) {
			parent::spawnPlayer(%this);
			%this.player.normalizeNodes();
		}

		%this.statsLoop();
	}

	function onServerDestroyed() {
		deleteVariables("$HAS::Init");
		deleteVariables("$HAS::Ongoing");
		return parent::onServerDestroyed();
	}

	function serverCmdSuicide(%client) {
		if($HAS::Ongoing) {
			return parent::serverCmdSuicide(%client);
		}
	}
};
activatePackage(HideAndSeekPackage);

$HAS::Init = 1;