function Player::playSound(%this, %sound) {
	if(isObject(%sound)) {
		serverPlay3D(%sound, %this.getPosition());
	}
}

function MinigameSO::messageAll(%this, %tag, %msg) {
	for(%i=0;%i<%this.numMembers;%i++) {
		messageClient(%this.member[%i], %tag, %msg);
	}
}

function Player::normalizeNodes(%this) {
	%this.hideNode("ALL");
	%list = "headSkin chest RArm LArm RHand LHand pants LShoe RShoe";
	for(%i=0;%i<getWordCount(%list);%i++) {
		%node = getWord(%list, %i);
		%this.unHideNode(%node);
	}
}