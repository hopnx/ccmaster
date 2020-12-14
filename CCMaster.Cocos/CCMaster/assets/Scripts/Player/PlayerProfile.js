let cm = require('ConnectionManager');
let GLOBAL = require('GlobalSetting');

var PlayerProfile = cc.Class({
    extends: cc.Component,
    statics: {
        current:null,
        url: GLOBAL.Host + '/hubs/player',
        commands: {
            REQUEST_PROFILE: 'RequestPlayerInfo',
        }        
    },
    properties: {
       
    },
    ctor: function(){
        this.init();
    },
    onLoad () {
    },
    init(){
        let listener = [
            {
                event: 'ReceivePlayerInfo',
                handler: this.show.bind(this)
            }
        ];
        if (!this.connection){
            this.connection = cm.createConnection(PlayerProfile.url, listener);
        }
    },
    request(playerId){
        let request = {
            id: playerId
        }
        cm.sendRequest(this.connection, PlayerProfile.commands.REQUEST_PROFILE, request);
    },
    show(response){
        if (!response.ok)
        {
            return;
        }
        
        this.playerInfo = {...response.data};
        
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
            rank.getComponent(cc.Label).string = this.playerInfo.rank;
        //update statistic
        let score = cc.find("Score/label", this.node);
        if (score)
            score.getComponent(cc.Label).string = this.playerInfo.score;
      
       let total = cc.find("Statistic/total/value", this.node);
            if (total)
                total.getComponent(cc.Label).string = this.playerInfo.totalGame;
          
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
        this.node.active = true;
    },
    buttonCloseOnClick(){
        this.node.active = false;
    }
});
