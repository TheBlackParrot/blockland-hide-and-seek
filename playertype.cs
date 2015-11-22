datablock PlayerData(PlayerFrozenArmor : PlayerStandardArmor) {
	airControl = 0;
	jumpForce = 0;
	maxForwardSpeed = 0;
	maxSideSpeed = 0;
	maxBackwardSpeed = 0;
	runForce = 0;
	groundImpactShakeAmp = "0.5 0.5 0.5";
	groundImpactMinSpeed = 13;
	minJetEnergy = 0;
	jetEnergyDrain = 0;
	canJet = 0;
	canRide = 0;
	uiName = "Frozen Player";
	showEnergyBar = false;
	horizMaxSpeed = 0;
	jumpForce = 0;
	JumpSound = -1;
	maxBackwardCrouchSpeed = 0;
	maxForwardCrouchSpeed = 0;
	maxSideCrouchSpeed = 0;
	maxJumpSpeed = 0;
};

datablock PlayerData(PlayerFastArmor : PlayerStandardArmor) {
	maxForwardSpeed = 14;
	maxSideSpeed = 12;
	maxBackwardSpeed = 7;
	canJet = 0;
	uiName = "Fast Player";
};

datablock PlayerData(PlayerSlowArmor : PlayerFastArmor) {
	maxForwardSpeed = 4;
	maxSideSpeed = 3;
	maxBackwardSpeed = 2;
	uiName = "Slow Player";
};