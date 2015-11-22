if($HAS::Maps::CurrentMap $= "") {
	$HAS::Maps::CurrentMap = 1;
}

function initHASMaps() {
	$HAS::Maps::InitMaps = 1;
	$HAS::Maps::MapCount = 0;
	%count = 0;

	%pattern = "saves/HideAndSeek/*.bls";
	%filename = findFirstFile(%pattern);

	while(isFile(%filename)) {
		$HAS::Maps::Map[%count] = %filename;
		%count++;

		%filename = findNextFile(%pattern);
	}

	$HAS::Maps::MapCount = %count;
}
initHASMaps();

function loadHasMap() {
	if($HAS::Maps::CurrentMap == $HAS::Maps::MapCount) {
		$HAS::Maps::CurrentMap = 0;
	}

	%filename = $HAS::Maps::Map[$HAS::Maps::CurrentMap];
	serverDirectSaveFileLoad(%filename, 3, "", 2);

	$HAS::Maps::CurrentMap++;
}

package HideAndSeekMapPackage {
	function onServerDestroyed() {
		deleteVariables("$HAS::Maps*");
		return parent::onServerDestroyed();
	}

	function ServerLoadSaveFile_End() {
		parent::ServerLoadSaveFile_End();

		$DefaultMinigame.getSpawns();
		$DefaultMinigame.reset();
	}
};
activatePackage(HideAndSeekMapPackage);