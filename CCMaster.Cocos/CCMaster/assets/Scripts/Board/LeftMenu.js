var BoardLeftMenu = cc.Class({
    extends: cc.Component,
    properties: {
    },
    refreshMenu(boardInfo) {
        // update switch button
        if (!Global || !Global.account || !Global.account.player)
            return;
        let ownerId = Global.account.player.id;
        
        let switchSideButton = this.node.getChildByName("ButtonSwitchSide");
        let canSwitchSideStatus = ["New","Ready"];           
        if (switchSideButton) {
            switchSideButton.active = (
                boardInfo && 
                boardInfo.redPlayer && 
                boardInfo.redPlayer.id == ownerId &&
                canSwitchSideStatus.includes(boardInfo.status));
        }  
        let readyButton = this.node.getChildByName("ButtonReady");
        if (readyButton){
            readyButton.active = canSwitchSideStatus.includes(boardInfo.status);
        }
        let canResignStatus = ["StartGame","Playing"];
        let resignButton = this.node.getChildByName("ButtonResign");
        if (resignButton){
            resignButton.active = canResignStatus.includes(boardInfo.status);
        }
        let drawButton = this.node.getChildByName("ButtonDraw");
        if (drawButton){
            drawButton.active = canResignStatus.includes(boardInfo.status);
        }
    },
});
