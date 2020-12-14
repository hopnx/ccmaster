let cm = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');

var Player = cc.Class({
    extends: cc.Component,
    statics: {
        url: GLOBAL.Host + '/hubs/player',
        commands: {
            REQUEST_PLAYER_INFO: 'RequestPlayerInfo',
        },
    },
    properties: {
        nodePlayerProfile: cc.Node,
    },
    setPlayerName(name) {
        this.playerId = id;
        if (this.playerInfo && this.playerInfo.id != this.playerId)
            this.loadPlayerInfo();
    },
    onLoad() {
        let nodeTimer = this.node.getChildByName("Timer");
        if (nodeTimer)
            this.timer = nodeTimer.getComponent("Timer");
        let nodeClock = this.node.getChildByName("Clock");
            if (nodeClock)
                this.clock = nodeClock.getComponent("Clock");                
        //
        let listener = [
            {   event: 'ReceivePlayerInfo',handler: this.onReceivePlayerInfo.bind(this)
            },
        ]
        this.connection = cm.createConnection(Player.url, listener, this.loadPlayerInfo.bind(this));
        this.playerProfile = this.nodePlayerProfile.getComponent("PlayerProfile");
        this.node.on('mousedown', this.playerOnClick.bind(this));
    },
    playerOnClick(){
        this.playerProfile.request(this.playerInfo.id)
    },
    onReceivePlayerInfo(response) {
        if (response.ok) {
            this.setPlayerInfo(response.data);
        }
    },
    loadPlayerInfo(playerId) {
        if (!playerId && !this.playerId)
            return;
        this.playerInfo = null;
        let request = {
            id: playerId || this.playerId
        };
        cm.sendRequest(this.connection, Player.commands.REQUEST_PLAYER_INFO, request);
    },
    setTimeInfo(info){
        this.setTimerInfo(info);
        this.setClockInfo(info);
    },
    setTimerInfo(info){
        if (info.isYourTurn)
            this.timer.startTimer(info.remainTime);
        else
            this.timer.stopTimer();
    },
    resetTimer(){
       if (this.timer){
           this.timer.reset();
       }
       if (this.clock){
           this.clock.reset();
       }
    },
    setClockInfo(info){
        if (info.isYourTurn)
            this.clock.startClock(info.remainMoveTime);
        else
            this.clock.stopClock();
    },
    setPlayerInfo(playerInfo) { 
        if (playerInfo)     
            this.playerInfo = {...playerInfo};
        else 
            this.playerInfo = {
                name:"",
                rank:"",
            };
        //update avatar
        let avatar = cc.find("Avatar", this.node);
        if (avatar) {
            let noAvatar = avatar.getChildByName("no-avatar");
            if (noAvatar)
                noAvatar.active = !this.playerInfo.id;
            let image = avatar.getChildByName("image");
            if (image)
                image.active = this.playerInfo.id;
        }
        //update name
        let name = cc.find("Name/label", this.node);
        if (name) {
            name.getComponent(cc.Label).string = this.playerInfo.name || "";
        }
        //update level
        let rank = cc.find("Rank/label", this.node);
        if (rank)
            rank.getComponent(cc.Label).string = this.playerInfo.rank||"";
        //update statistic
        let score = cc.find("Statistic/score/value", this.node);
        if (score)
            score.getComponent(cc.Label).string = this.playerInfo.score;
        let win = cc.find("Statistic/win/value", this.node);
        if (win)
            win.getComponent(cc.Label).string = this.playerInfo.totalWin;
        let draw = cc.find("Statistic/draw/value", this.node);
        if (draw)
            draw.getComponent(cc.Label).string = this.playerInfo.totalDraw;
        let lose = cc.find("Statistic/lose/value", this.node);
        if (lose)
            lose.getComponent(cc.Label).string = this.playerInfo.totalLose;
        //update ready
        let ready = cc.find('IsReady',this.node);
        if (ready){
            ready.active = (this.playerInfo && this.playerInfo.readyToPlay);
        }
        if (this.timer && this.playerInfo){
            if (this.playerInfo)
                this.timer.setStartValue(this.playerInfo.totalTime);
            else 
                this.timer.setStartValue(0);
        } 
    }
});
