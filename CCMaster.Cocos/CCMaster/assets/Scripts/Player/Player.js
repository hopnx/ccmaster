let CM = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');

var Player = cc.Class({
    extends: cc.Component,
    properties: {
    },
    ctor: function () {
        let listener = [
            { event: 'ReceivePlayerInfo', handler: this.onReceivePlayerInfo.bind(this) },
        ]
        this.commands = {
            REQUEST_PLAYER_INFO_BY_ACCOUNT: 'RequestPlayerInfoByAccountId',
            REQUEST_PLAYER_INFO: 'RequestPlayerInfo',
        }
        this.connection = CM.createConnection(GLOBAL.Host + '/hubs/player', listener, this.loadPlayerInfo.bind(this));
    },
    avatarOnClick() {
        if (this.playerProfile)
            this.playerProfile.loadPlayerInfo(this.playerId);
    },
    onLoad() {
        this.nodePlayerProfile = cc.find("Canvas/PlayerProfile");
        if (this.nodePlayerProfile)
            this.playerProfile = this.nodePlayerProfile.getComponent("PlayerProfile");

        this.nodeAvatar = this.node.getChildByName("avatar");
        this.nodeAvatarValue = cc.find("avatar/image", this.node);
        this.nodeName = cc.find("info/name/value", this.node);
  
        this.nodeRankRibbon = cc.find('avatar/rank/ribbon', this.node);
        this.nodeRankLabel = cc.find("avatar/rank/label", this.node);
        this.nodeStar = cc.find("avatar/rank/star",this.node);
        this.nodeElo = cc.find("info/elo/chess/value", this.node);
        this.nodeCoin = cc.find("info/asset/coin", this.node);
        
     
        this.nodeAvatar.on(cc.Node.EventType.TOUCH_START, this.avatarOnClick.bind(this));
        this.nodeAvatar.on(cc.Node.EventType.MOUSE_UP, this.avatarOnClick.bind(this));

        this.clearPlayerInfo();
    },
    loadPlayerInfo(playerId) {
        this.playerId = this.playerId || playerId;
        this.clearPlayerInfo();
        if (!this.playerId)
            return;
        let request = {
            id: this.playerId
        };
        CM.sendRequest(this.connection, this.commands.REQUEST_PLAYER_INFO, request);
    },
    onReceivePlayerInfo(response) {
        if (response.ok) {
            this.setPlayerInfo(response.data);
        }
        else
            this.showMessage(response.message);
    },
    showMessage(message) {
        alert(message);
        //Global.gameManager.showMessage(message);
    },
    clearPlayerInfo() {
        if (this.nodeAvatarValue) {
            this.nodeAvatarValue.color = cc.Color.BLACK;
            this.nodeAvatarValue.opacity = 10;
        }
        if (this.nodeRankLabel)
            this.nodeRankLabel.getComponent(cc.Label).string = "";
        if (this.nodeRankRibbon) {
            this.nodeRankRibbon.active = false;
        }
        if (this.nodeStar){
            for (let i = 0; i < this.nodeStar.children.length; i++) {
                let star = this.nodeStar.children[i];
                star.color = cc.Color.BLACK;
                star.opacity = 180;
            }
        }
    },
    setPlayerInfo(info) {
        this.player = info;
        if (this.nodeAvatarValue) {
            this.nodeAvatarValue.color = cc.Color.WHITE;
            this.nodeAvatarValue.opacity = 255;
        }
        //update name
        if (this.nodeName) {
            let name = this.nodeName.getComponent(cc.Label);
            if (name)
                name.string = info.name;
        }
        if (this.nodeElo) {
            let elo = this.nodeElo.getComponent(cc.Label);
            if (elo)
                elo.string = info.score;
        }
        if (this.nodeCoin) {
            let coin = this.nodeCoin.getComponent(cc.Label);
            if (coin)
                coin.string = Global.gameManager.getCoinString(info.coin);
        }
        if (this.nodeRankLabel) {
            let rankLabel = this.nodeRankLabel.getComponent(cc.Label);
            if (rankLabel)
                rankLabel.string = info.rankLabel;
        }
        if (this.nodeRankRibbon) {
            if (info.rankIndex)
                this.nodeRankRibbon.active = true;
            for (let i = 0; i < this.nodeRankRibbon.children.length; i++) {
                this.nodeRankRibbon.children[i].active = false;
            }
            if (info.rankIndex >= 3)
                this.nodeRankRibbon.getChildByName("red").active = true;
            else
                if (info.rankIndex == 2)
                    this.nodeRankRibbon.getChildByName("orange").active = true;
                else
                    if (info.rankIndex == 1)
                        this.nodeRankRibbon.getChildByName("green").active = true;
        }
        if (this.nodeStar) {            
            if (info.starIndex >= 1) {
                let star1 = this.nodeStar.getChildByName("1");
                if (star1) {
                    star1.color = cc.Color.WHITE;
                    star1.opacity = 255;
                }
            }
            if (info.starIndex >= 2) {
                let star2 = this.nodeStar.getChildByName("2");
                if (star2) {
                    star2.color = cc.Color.WHITE;
                    star2.opacity = 255;
                }
            }
            if (info.starIndex >= 3) {
                let star3 = this.nodeStar.getChildByName("3");
                if (star3) {
                    star3.color = cc.Color.WHITE;
                    star3.opacity = 255;
                }
            }
        }
    }
});
