let CM = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');

var PlayerProfile = cc.Class({
    extends: cc.Component,
    statics: {
    },
    properties: {

    },
    ctor:function(){
        this.commands = {
            REQUEST_PROFILE: 'RequestPlayerInfo',
        }
        let listener = [
            { event: 'ReceivePlayerInfo', handler: this.show.bind(this) }
        ];
        this.connection = CM.createConnection(GLOBAL.Host + '/hubs/player', listener);
    },
    onLoad() {
        this.node.active = false;     
    },
    loadPlayerInfo(playerId) {
        let request = {
            id: playerId
        }
        CM.sendRequest(this.connection, this.commands.REQUEST_PROFILE, request);
    },
    show(response) {
        if (!response.ok) {
            Global.gameManager.showMessage(response.message);
        }
        else
            this.setPlayerInfo(response.data);
    },
    setPlayerInfo(info) {
        //update name
        let nodeName = cc.find("info/name/label", this.node);
        if (nodeName) {
            let name = nodeName.getComponent(cc.Label);
            if (name)
                name.string = info.name;
        }
        let nodeElo = cc.find("info/asset/elo", this.node);
        if (nodeElo) {
            let elo = nodeElo.getComponent(cc.Label);
            if (elo)
                elo.string = info.score;
        }
        let nodeCoin = cc.find("info/asset/coin", this.node);
        if (nodeCoin) {
            let coin = nodeCoin.getComponent(cc.Label);
            if (coin)
                coin.string = Global.gameManager.getCoinString(info.coin);
        }
        let nodeRankLabel = cc.find("rank/label", this.node);
        if (nodeRankLabel) {
            let rankLabel = nodeRankLabel.getComponent(cc.Label);
            if (rankLabel)
                rankLabel.string = info.rankLabel;
        }
        let nodeRankRibbon = cc.find('rank/ribbon', this.node);
        if (nodeRankRibbon) {
            for (let i = 0; i < nodeRankRibbon.children.length; i++) {
                nodeRankRibbon.children[i].active = false;
            }
            if (info.rankIndex >= 3)
                nodeRankRibbon.getChildByName("red").active = true;
            else
                if (info.rankIndex == 2)
                    nodeRankRibbon.getChildByName("orange").active = true;
                else
                    nodeRankRibbon.getChildByName("green").active = true;
        }
        let nodeStar = cc.find("rank/star", this.node);
        if (nodeStar) {
            for (let i = 0; i < nodeStar.children.length; i++) {
                let star = nodeStar.children[i];
                star.color = cc.Color.BLACK;
                star.opacity = 180;
            }
            if (info.starIndex >= 1) {
                let star1 = nodeStar.getChildByName("1");
                if (star1) {
                    star1.color = cc.Color.WHITE;
                    star1.opacity = 255;
                }
            }
            if (info.starIndex >= 2) {
                let star2 = nodeStar.getChildByName("2");
                if (star2) {
                    star2.color = cc.Color.WHITE;
                    star2.opacity = 255;
                }
            }
            if (info.starIndex >= 3) {
                let star3 = nodeStar.getChildByName("3");
                if (star3) {
                    star3.color = cc.Color.WHITE;
                    star3.opacity = 255;
                }
            }
        }
        let win  = cc.find("archive/win/value",this.node);
        if (win)
            win.getComponent(cc.Label).string = info.totalWin||0;        
        let draw = cc.find("archive/draw/value",this.node);
        if (draw)
            draw.getComponent(cc.Label).string = info.totalDraw||0;
        let lose = cc.find("archive/lose/value",this.node);
        if (lose)
            lose.getComponent(cc.Label).string = info.totalLose||0;

        this.node.active = true;
    },
    buttonCloseOnClick() {
        this.node.active = false;
    }
});
